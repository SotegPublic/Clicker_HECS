using Components;
using HECSFramework.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Systems
{
    [Serializable][Documentation(Doc.Level, Doc.Load, "here we load initial chunks for start game")]
    public sealed class LoadInitialChunksSystem : BaseGameStateSystem
    {
        [Required]
        private ChunkBiomTypeVariablesComponent biomVariables;
        [Required]
        private ChunksQueueHolderComponent chunksHolder;

        [Single]
        private PoolingSystem poolingSystem;

        private SceneVariablesComponent sceneVariablesComponent;
        private BiomConfigsHolderComponent biomConfigsHolderComponent;

        protected override int State => GameStateIdentifierMap.LoadInitialChunksStateIdentifier;

        public override void InitSystem()
        {
            var sceneManager = Owner.World.GetEntityBySingleComponent<SceneManagerTagComponent>();
            sceneVariablesComponent = sceneManager.GetComponent<SceneVariablesComponent>();
            biomConfigsHolderComponent = sceneManager.GetComponent<BiomConfigsHolderComponent>();
        }

        protected override async void ProcessState(int from, int to)
        {
            if (biomConfigsHolderComponent.TryGetBiomConfig(sceneVariablesComponent.CurrentBiomIndex, out var biomConfig))
            {
                for (int i = 0; i < biomVariables.ChunksCountOnScene; i++)
                {
                    await LoadChunkAsynk(biomConfig);
                }
            }

            EndState();
        }

        private async Task LoadChunkAsynk(BiomConfig loadingBiomConfig)
        {
            var chunksHolderTransform = Owner.World.GetEntityBySingleComponent<ChunksSpawnPointTagComponent>().GetComponent<UnityTransformComponent>().Transform;
            var levelIndex = chunksHolder.Chunks.Count == 0 ? 0 : chunksHolder.Chunks.Count - 1;

            var chunk = await poolingSystem.GetViewFromPool(loadingBiomConfig.Levels[levelIndex].ChunkReference);
            var chunkComponent = chunk.GetComponent<ChunkMonoComponent>();

            chunkComponent.Transform.parent = chunksHolderTransform;
            var halfSizeZ = chunkComponent.MeshRenderer.bounds.size.z * 0.5f;

            if (chunksHolder.Chunks.Count == 0)
            {
                chunkComponent.IsEnemyKilled = true;
            }


            if (chunksHolder.Chunks.Count == 1)
            {
                var firstChank = chunksHolder.Chunks.Peek();
                var firstChankHalfSizeZ = firstChank.MeshRenderer.bounds.size.z * 0.5f;
                firstChank.transform.position = new Vector3(0, 0, 0 - (firstChankHalfSizeZ + halfSizeZ));
                firstChank.CurrentPosition = firstChank.transform.position;
                biomVariables.CurrentChunksLenth = -halfSizeZ;
            }

            if(chunksHolder.Chunks.Count > 0)
            {
                if(loadingBiomConfig.Levels[levelIndex].SpecialRewards.Length > 0)
                {
                    chunkComponent.AddSpecialRewards(loadingBiomConfig.Levels[levelIndex].SpecialRewards);
                }

                if(loadingBiomConfig.Levels[levelIndex].ParametersModifiers.Length > 0)
                {
                    chunkComponent.AddChunkModifiers(loadingBiomConfig.Levels[levelIndex].ParametersModifiers);
                }

                chunkComponent.AddChunkLevelBonus(loadingBiomConfig.Levels[levelIndex].EnemyLevelBonus);
            }

            chunkComponent.Transform.position = new Vector3(0, 0, biomVariables.CurrentChunksLenth + halfSizeZ);
            chunkComponent.CurrentPosition = chunkComponent.Transform.position;
            biomVariables.CurrentChunksLenth += halfSizeZ * 2;
            chunksHolder.Chunks.Enqueue(chunkComponent);
        }
    }
}