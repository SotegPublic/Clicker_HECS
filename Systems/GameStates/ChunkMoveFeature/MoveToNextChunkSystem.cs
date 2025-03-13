using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using System.Threading.Tasks;
using System.Linq;
using Commands;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.State, Doc.Level, "Here we spawn and move chanks after level end")]
    public sealed class MoveToNextChunkSystem : BaseGameStateSystem, IUpdatable
    {
        [Single]
        private PoolingSystem poolingSystem;

        [Required]
        private ChunkBiomTypeVariablesComponent biomVariables;
        [Required]
        private ChunksQueueHolderComponent chunksHolder;
        [Required]
        private AnimationCurveComponent animationCurveComponent;
        [Required]
        private ProgressComponent progressComponent;

        private SceneVariablesComponent sceneVariablesComponent;
        private BiomConfigsHolderComponent biomConfigsHolderComponent;

        protected override int State => GameStateIdentifierMap.ChangeChunkStateIdentifier;

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
                if(sceneVariablesComponent.CurrentBiomLevelIndex == biomConfig.Levels.Length - 1)
                {
                    Debug.LogWarning("All levels end");
                    Owner.World.Command(new GoToNextBiomCommand());
                    EndState();
                }
                else
                {
                    if(biomVariables.CurrentBiomLevelIndexForLoad + 1 <= biomConfig.Levels.Length)
                    {
                        await LoadNextChunkAsynk(biomConfig);
                    }
                    Owner.AddComponent<MoveToNextLevelTagComponent>();
                    Owner.Command(new StartRunBetweenLevelsCommand());
                }
            }
            else
            {
                throw new Exception($"Get Biom Config error: target biom ID = {sceneVariablesComponent.CurrentBiomID}");
            }
        }

        public void UpdateLocal()
        {
            if (Owner.TryGetComponent<MoveToNextLevelTagComponent>(out var tag))
            {
                var speed = animationCurveComponent.AnimationCurve.Evaluate(progressComponent.Value);
                progressComponent.ChangeValue(Time.deltaTime * speed);

                float offsetSize = 0f;
                int chunkIndex = 0;

                foreach (var chunk in chunksHolder.Chunks)
                {
                    if(chunkIndex == 1 || chunkIndex == 2)
                    {
                        var halfSizeZ = chunk.MeshRenderer.bounds.size.z * 0.5f;
                        offsetSize+= halfSizeZ;
                    }
                    chunkIndex++;
                }

                foreach (var chunk in chunksHolder.Chunks)
                {
                    var newPosition = new Vector3(chunk.CurrentPosition.x, chunk.CurrentPosition.y, chunk.CurrentPosition.z - offsetSize);

                    var direction = Vector3.Lerp(chunk.CurrentPosition, newPosition, progressComponent.Value);

                    chunk.Transform.position = direction;

                    if(progressComponent.Value >= 1)
                    {
                        chunk.CurrentPosition = chunk.Transform.position;
                    }
                }

                if (progressComponent.Value >= 1)
                {
                    progressComponent.SetValue(0);
                    Owner.RemoveComponent<MoveToNextLevelTagComponent>();
                    Owner.Command(new StopRunBetweenLevelsCommand());
                    RemoveFirstChunck();
                    sceneVariablesComponent.CurrentBiomLevelIndex++;
                    biomVariables.CurrentChunksLenth -= offsetSize;
                    EndState();
                }
            }
        }


        private async Task LoadNextChunkAsynk(BiomConfig loadingBiomConfig)
        {
            var chunksHolderTransform = Owner.World.GetEntityBySingleComponent<ChunksSpawnPointTagComponent>().GetComponent<UnityTransformComponent>().Transform;
            var levelIndex = biomVariables.CurrentBiomLevelIndexForLoad;

            var chunk = await poolingSystem.GetViewFromPool(loadingBiomConfig.Levels[levelIndex].ChunkReference);
            var chunkComponent = chunk.GetComponent<ChunkMonoComponent>();
            chunkComponent.Transform.parent = chunksHolderTransform;

            if(loadingBiomConfig.Levels[levelIndex].SpecialRewards.Length > 0)
            {
                chunkComponent.AddSpecialRewards(loadingBiomConfig.Levels[levelIndex].SpecialRewards);
            }

            var halfSizeZ = chunkComponent.MeshRenderer.bounds.size.z * 0.5f;

            chunkComponent.Transform.position = new Vector3(0, 0, biomVariables.CurrentChunksLenth + halfSizeZ);
            chunkComponent.CurrentPosition = chunkComponent.Transform.position;
            biomVariables.CurrentChunksLenth += halfSizeZ * 2;
            chunksHolder.Chunks.Enqueue(chunkComponent);
        }

        private void RemoveFirstChunck()
        {
            var firstChunk = chunksHolder.Chunks.Dequeue();

            if(firstChunk.ChunkEnemy.IsAlive)
            {
                Owner.World.Command(new DestroyEntityWorldCommand { Entity = firstChunk.ChunkEnemy });
            }
            firstChunk.Clear();
            poolingSystem.ReleaseView(firstChunk);
        }
    }
}