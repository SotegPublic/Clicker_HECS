using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;

namespace Systems
{
	[Serializable][Documentation(Doc.Level, Doc.Load, "here we load chunk type biom and set parameters for following states")]
    public sealed class LoadChunkBiomSubsystem : BaseSystem, IReactGlobalCommand<LoadBiomCommand>
    {
        [Required]
        private SceneVariablesComponent sceneVariables;

        [Single]
        private PoolingSystem poolingSystem;

        public async void CommandGlobalReact(LoadBiomCommand command)
        {
            if (command.BiomConfig.MoveTypeID == BiomMoveTypeIdentifierMap.Chunk)
            {
                var loadingBiomConfig = command.BiomConfig;

                var biomView = await poolingSystem.GetViewFromPool(loadingBiomConfig.BiomReference);

                sceneVariables.CurrentBiomID = loadingBiomConfig.BiomID;
                sceneVariables.CurrentBiomView = biomView;
                sceneVariables.CurrentBiomLevelIndex = 0;

                sceneVariables.CurrentBiomMoveType = BiomMoveTypeIdentifierMap.Chunk;

                var chunkBiomVariables = Owner.World.GetEntityBySingleComponent<ChunkMoveFeatureTagComponent>().GetComponent<ChunkBiomTypeVariablesComponent>();
                chunkBiomVariables.CurrentBiomLevelIndexForLoad = 0;
                chunkBiomVariables.CurrentChunksLenth = 0;

                Owner.AddComponent<BiomReadyTagComponent>();
            }
        }

        public override void InitSystem()
        {
        }
    }
}