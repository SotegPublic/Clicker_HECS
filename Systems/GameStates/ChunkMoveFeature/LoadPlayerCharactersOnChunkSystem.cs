using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Systems
{
	[Serializable][Documentation(Doc.Spawn, Doc.Load, "here we load player characters and minions")]
    public sealed class LoadPlayerCharactersOnChunkSystem : BaseGameStateSystem 
    {
        [Single]
        private VisualEquipmentSystem visualEquipmentSystem;

        private PlayerCharactersHolderComponent playerCharactersHolderComponent;
        private PlayerMinionsHolderComponent minionsHolderComponent;
        private List<Task> taskList = new List<Task>(10);
        private Entity[] minionSpawnPoints = new Entity[10];

        protected override int State => GameStateIdentifierMap.SpawnPlayerCharactersOnChunkStateIdentifier;

        public override void InitSystem()
        {
            var sceneManager = Owner.World.GetEntityBySingleComponent<SceneManagerTagComponent>();
            playerCharactersHolderComponent = sceneManager.GetComponent<PlayerCharactersHolderComponent>();
            minionsHolderComponent = sceneManager.GetComponent<PlayerMinionsHolderComponent>();
        }

        protected override async void ProcessState(int from, int to)
        {
            taskList.Clear();

            SpawnPlayerCharacter();
            SpawnPlayerMinions();

            await Task.WhenAll(taskList);

            VisualEquipCharacterItems();

            EndState();
        }

        private void SpawnPlayerCharacter()
        {
            var spawnPointTransform = Owner.World.GetEntityBySingleComponent<PlayerSpawnPointTagComponent>().GetComponent<UnityTransformComponent>().Transform;

            if(playerCharactersHolderComponent.TryGetEntityByCharacterIdentifier(PlayerCharacterIdentifierMap.Slime, out var container))
            {
                taskList.Add(GetAndInitActor(container, spawnPointTransform));
            }
        }

        private void VisualEquipCharacterItems()
        {
            var inventory = Owner.World.GetEntityBySingleComponent<PlayerTagComponent>().GetComponent<EquipItemsHolderComponent>();

            foreach(var item in inventory.EquipItems)
            {
                visualEquipmentSystem.VisualEquipItem(item.Value, item.Key);
            }
        }

        private void SpawnPlayerMinions()
        {
            minionSpawnPoints = Owner.World.GetFilter<MinionSpawnPointTagComponent>().ToArray();
            var selectedMinionsHolder = Owner.World.GetEntityBySingleComponent<PlayerTagComponent>().GetComponent<PlayerSelectedMinionsHolderComponent>();



            for(int i = 0; i < minionSpawnPoints.Length; i++)
            {
                var spawnPointTransform = minionSpawnPoints[i].GetComponent<UnityTransformComponent>().Transform;
                var spawnPointId = minionSpawnPoints[i].GetComponent<MinionSpawnPointTagComponent>().MinionSpawnPointID;
                var minionID = 0;

                if(spawnPointId == MinionSpawnPointIdentifierMap.RightMinionSpawnPoint)
                {
                    if(selectedMinionsHolder.RightMinionId != 0 && selectedMinionsHolder.RightMinionId != PlayerMinionIdentifierMap.None)
                    {
                        minionID = selectedMinionsHolder.RightMinionId;
                    }
                }
                else if(spawnPointId == MinionSpawnPointIdentifierMap.LeftMinionSpawnPoint)
                {
                    if (selectedMinionsHolder.LeftMinionId != 0 && selectedMinionsHolder.LeftMinionId != PlayerMinionIdentifierMap.None)
                    {
                        minionID = selectedMinionsHolder.RightMinionId;
                    }
                }

                if (minionsHolderComponent.TryGetEntityByMinionIdentifier(minionID, out var container))
                {
                    taskList.Add(GetAndInitActor(container, spawnPointTransform));
                }
            }
        }

        private async Task GetAndInitActor(EntityContainer container, Transform spawnPointTransform)
        {
            var actor = await container.GetActor(position: spawnPointTransform.position).AsTask();

            actor.Init();

            var job = new WaitFor<ViewReadyTagComponent>(actor.Entity);
            var viewSpawnAwaiter = job.RunJob();

            await viewSpawnAwaiter;
        }
    }
}