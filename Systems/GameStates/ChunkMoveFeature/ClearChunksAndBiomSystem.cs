using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable][Documentation(Doc.Level, Doc.Global, "this system clear chunk biom before the next one has been loaded")]
    public sealed class ClearChunksAndBiomSystem : BaseGameStateSystem 
    {
        [Single]
        private PoolingSystem poolingSystem;

        [Required]
        private ChunksQueueHolderComponent chunksHolder;

        private SceneVariablesComponent sceneVariablesComponent;

        protected override int State => GameStateIdentifierMap.ClearChunksAndBiomStateIdentifier;

        public override void InitSystem()
        {
            sceneVariablesComponent = Owner.World.GetEntityBySingleComponent<SceneManagerTagComponent>().GetComponent<SceneVariablesComponent>();
        }

        protected override void ProcessState(int from, int to)
        {
            poolingSystem.ReleaseView(sceneVariablesComponent.CurrentBiomView);

            for(int i = chunksHolder.Chunks.Count - 1; i >= 0; i--)
            {
                var chunk = chunksHolder.Chunks.Dequeue();

                if (chunk.ChunkEnemy.IsAlive)
                {
                    Owner.World.Command(new DestroyEntityWorldCommand { Entity = chunk.ChunkEnemy });
                }

                chunk.Clear();
                poolingSystem.ReleaseView(chunk);
            }

            var playerCharacter = Owner.World.GetEntityBySingleComponent<PlayerCharacterTagComponent>();
            var playerMinions = Owner.World.GetEntitiesByComponent<PlayerMinionTagComponent>();

            playerCharacter.HecsDestroy();

            foreach (var minion in playerMinions)
            {
                minion.HecsDestroy();
            }

            sceneVariablesComponent.CurrentBiomIndex++;
            EndState();
        }
    }
}