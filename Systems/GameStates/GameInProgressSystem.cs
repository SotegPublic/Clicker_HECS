using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;
using Strategies;
using System.Threading.Tasks;

namespace Systems
{
	[Serializable][Documentation(Doc.State, "GameInProgressSystem")]
    public sealed class GameInProgressSystem : BaseGameStateSystem, IReactGlobalCommand<GoToNextChunkCommand>
    {
        [Required]
        private CurrentEnemyComponent currentEnemyComponent;

        protected override int State => GameStateIdentifierMap.GameInProgressStateIdentifier;

        public override void InitSystem()
        {
        }

        protected override void ProcessState(int from, int to)
        {
            SetNewCurrentEnemy();
            Owner.World.GetEntityBySingleComponent<PlayerCharacterTagComponent>().AddComponent<CanAttackTagComponent>();
        }

        private void SetNewCurrentEnemy()
        {
            var sceneVariables = Owner.World.GetEntityBySingleComponent<SceneManagerTagComponent>().GetComponent<SceneVariablesComponent>();
            
            if(sceneVariables.CurrentBiomMoveType == BiomMoveTypeIdentifierMap.Chunk)
            {
                var index = 0;
                var chunksHolder = Owner.World.GetEntityBySingleComponent<ChunkMoveFeatureTagComponent>().GetComponent<ChunksQueueHolderComponent>();

                foreach (var chunk in chunksHolder.Chunks)
                {
                    index++;

                    if (index == 2)
                    {
                        currentEnemyComponent.Enemy = chunk.ChunkEnemy;
                        chunk.ChunkEnemy.Entity.AddComponent<ActiveEnemyTagComponent>();
                        break;
                    }
                }
            }

            if(sceneVariables.CurrentBiomMoveType == BiomMoveTypeIdentifierMap.TravelPrefab || sceneVariables.CurrentBiomMoveType == BiomMoveTypeIdentifierMap.TravelScene)
            {
                var travelBiomVariables = Owner.World.GetEntityBySingleComponent<TravelMoveFeatureTagComponent>().GetComponent<TravelBiomTypeVariablesComponent>();

                currentEnemyComponent.Enemy = travelBiomVariables.EnemiesByIndex[sceneVariables.CurrentBiomLevelIndex];
                currentEnemyComponent.Enemy.AddComponent<ActiveEnemyTagComponent>();
            }
        }

        public void CommandGlobalReact(GoToNextChunkCommand command)
        {
            AwaitVisualScenariosAsync();
        }

        private async void AwaitVisualScenariosAsync()
        {
            var job = new WaitForDrawQueue(Owner.World.GetEntityBySingleComponent<VisualQueueTagComponent>());
            var awaiter = job.RunJob();

            await awaiter;
            Owner.World.GetEntityBySingleComponent<PlayerCharacterTagComponent>().RemoveComponent<CanAttackTagComponent>();
            EndState();
        }
    }
}