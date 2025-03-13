using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Components;
using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using HECSFramework.Unity;
using Strategies;
using UnityEngine;

namespace Systems
{
	[Serializable][Documentation(Doc.State, Doc.Player, Doc.Spawn, "this system spawn player characters into travel type biom")]
    public sealed class LoadPlayerCharacterInTravelBiomSystem : BaseGameStateSystem 
    {
        [Single]
        private VisualEquipmentSystem visualEquipmentSystem;

        [Required]
        private TravelBiomTypeVariablesComponent variablesComponent;
        [Required]
        private TravelPointsHolderComponent travelPointsHolderComponent;

        private PlayerCharactersHolderComponent playerCharactersHolderComponent;
        private PlayerMinionsHolderComponent minionsHolderComponent;
        private SceneVariablesComponent sceneVariables;
        private List<Task> taskList = new List<Task>(10);

        protected override int State => GameStateIdentifierMap.SpawnPlayerCharactersInTravelBiomStateIdentifier;

        public override void InitSystem()
        {
            var sceneManager = Owner.World.GetEntityBySingleComponent<SceneManagerTagComponent>();
            playerCharactersHolderComponent = sceneManager.GetComponent<PlayerCharactersHolderComponent>();
            minionsHolderComponent = sceneManager.GetComponent<PlayerMinionsHolderComponent>();
            sceneVariables = sceneManager.GetComponent<SceneVariablesComponent>();
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
            var spawnPointTransform = travelPointsHolderComponent.GetTravelPointByIndex(sceneVariables.CurrentBiomLevelIndex);

            if (playerCharactersHolderComponent.TryGetEntityByCharacterIdentifier(PlayerCharacterIdentifierMap.Slime, out var container))
            {
                taskList.Add(GetPlayer(container, spawnPointTransform.PlayerTravelPoint));
            }
        }

        private void SpawnPlayerMinions()
        {
            var selectedMinionsHolder = Owner.World.GetEntityBySingleComponent<PlayerTagComponent>().GetComponent<PlayerSelectedMinionsHolderComponent>();
            var minionID = 0;

            if (selectedMinionsHolder.RightMinionId != 0 && selectedMinionsHolder.RightMinionId != PlayerMinionIdentifierMap.None)
            {
                minionID = selectedMinionsHolder.RightMinionId;
                var rightSpawnPoint = travelPointsHolderComponent.GetTravelPointByIndex(sceneVariables.CurrentBiomLevelIndex).RightMinionTravelPoint;

                if (minionsHolderComponent.TryGetEntityByMinionIdentifier(minionID, out var container))
                {
                    taskList.Add(GetMinion(container, rightSpawnPoint, true));
                }
            }

            if (selectedMinionsHolder.LeftMinionId != 0 && selectedMinionsHolder.LeftMinionId != PlayerMinionIdentifierMap.None)
            {
                minionID = selectedMinionsHolder.RightMinionId;
                var leftSpawnPoint = travelPointsHolderComponent.GetTravelPointByIndex(sceneVariables.CurrentBiomLevelIndex).LeftMinionTravelPoint;

                if (minionsHolderComponent.TryGetEntityByMinionIdentifier(minionID, out var container))
                {
                    taskList.Add(GetMinion(container, leftSpawnPoint, false));
                }

            }
        }

        private void VisualEquipCharacterItems()
        {
            var inventory = Owner.World.GetEntityBySingleComponent<PlayerTagComponent>().GetComponent<EquipItemsHolderComponent>();

            foreach (var item in inventory.EquipItems)
            {
                visualEquipmentSystem.VisualEquipItem(item.Value, item.Key);
            }
        }
        private async Task GetPlayer(EntityContainer container, Transform spawnPointTransform)
        {
            var actor = await GetAndInitActor(container, spawnPointTransform);

            var job = new WaitFor<ViewReadyTagComponent>(actor.Entity);

            await job.RunJob();

            var camera = Owner.World.GetEntityBySingleComponent<MainCameraComponent>();
            var offsets = camera.GetComponent<CameraOffsetsComponent>();

            var playerCameraPosition = Owner.World.GetEntityBySingleComponent<PlayerCameraPositionComponent>();
            camera.GetTransform().position = playerCameraPosition.GetTransform().position;
            camera.GetTransform().rotation = playerCameraPosition.GetTransform().rotation;

            variablesComponent.PlayerCharacter = actor.Entity;

            offsets.PositionZOffset = camera.GetPosition().z - actor.Entity.GetPosition().z;
            offsets.PositionYOffset = camera.GetPosition().y - actor.Entity.GetPosition().y;
            offsets.PositionXOffset = camera.GetPosition().x - actor.Entity.GetPosition().x;
        }

        private async Task GetMinion(EntityContainer container, Transform spawnPointTransform, bool isRightMinion)
        {
            var actor = await GetAndInitActor(container, spawnPointTransform);

            if(isRightMinion)
            {
                variablesComponent.RightMinion = actor.Entity;
            }
            else
            {
                variablesComponent.LeftMinion = actor.Entity;
            }
        }

        private async Task<Actor> GetAndInitActor(EntityContainer container, Transform spawnPointTransform)
        {
            var actor = await container.GetActor(position: spawnPointTransform.position).AsTask();

            actor.Init();

            var job = new WaitFor<ViewReadyTagComponent>(actor.Entity);
            var viewSpawnAwaiter = job.RunJob();

            await viewSpawnAwaiter;

            return actor;
        }
    }
}