using System;
using HECSFramework.Core;
using Components;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace Systems
{
	[Serializable][Documentation(Doc.Load, Doc.Level, "here we warmup biom")]
    public sealed class WarmupSystem : BaseGameStateSystem 
    {
        [Required]
        private BiomConfigsHolderComponent biomConfigsHolderComponent;
        [Required]
        private SceneVariablesComponent sceneVariablesComponent;
        [Required]
        private PlayerCharactersHolderComponent playerCharactersHolder;
        [Required]
        private PlayerMinionsHolderComponent playerMinionsHolder;
        [Required]
        private FXWarmupConfigsHolderComponent fxWarmupConfigsHolderComponent;
        [Required]
        private EquipItemsReferencesHolderComponent equipItemsReferencesHolderComponent;


        [Single]
        private PoolingSystem poolingSystem;

        private List<UniTask>  taskList = new List<UniTask>(100);
        private Dictionary<AssetReference, int> scenarioReferences = new Dictionary<AssetReference, int>(50);

        protected override int State => GameStateIdentifierMap.PreloadStateIdentifier;

        public override void InitSystem()
        {
        }

        protected override async void ProcessState(int from, int to)
        {
            taskList.Clear();

            if(biomConfigsHolderComponent.TryGetBiomConfig(sceneVariablesComponent.CurrentBiomIndex, out var biomConfig))
            {
                WarmupBiom(biomConfig);
            }

            await UniTask.WhenAll(taskList);
            EndState();
        }

        private void WarmupBiom(BiomConfig biomConfig)
        {
            for(int i = 0; i < fxWarmupConfigsHolderComponent.FXWarmupConfigs.Length; i++)
            {
                var fxReference = fxWarmupConfigsHolderComponent.FXWarmupConfigs[i].FxReference;

                taskList.Add(poolingSystem.Warmup(fxReference, fxWarmupConfigsHolderComponent.FXWarmupConfigs[i].WarmupCount));
            }

            for (int i = 0; i < biomConfig.BiomEnemies.Length; i++)
            {
                var enemyViewReference = biomConfig.BiomEnemies[i].GetComponent<ViewReferenceGameObjectComponent>().ViewReference;

                taskList.Add(poolingSystem.Warmup(enemyViewReference, sceneVariablesComponent.PreloadingEnemiesCount));
            }

            if(biomConfig.MoveTypeID == BiomMoveTypeIdentifierMap.Chunk)
            {
                for (int i = 0; i < biomConfig.StandartChunkReferences.Length; i++)
                {
                    taskList.Add(poolingSystem.Warmup(biomConfig.StandartChunkReferences[i], sceneVariablesComponent.PreloadingChunksCount));
                }

                for (int i = 0; i < biomConfig.LargeChunkReferences.Length; i++)
                {
                    taskList.Add(poolingSystem.Warmup(biomConfig.LargeChunkReferences[i], sceneVariablesComponent.PreloadingChunksCount));
                }
            }

            for (int i = 0; i < playerCharactersHolder.GetCollectionLenth(); i++)
            {
                taskList.Add(poolingSystem.Warmup(playerCharactersHolder.GetAssetReferenceByIndex(i), sceneVariablesComponent.PreloadCharactersCount)); 
            }

            for (int i = 0; i < playerMinionsHolder.GetCollectionLenth(); i++)
            {
                taskList.Add(poolingSystem.Warmup(playerMinionsHolder.GetAssetReferenceByIndex(i), sceneVariablesComponent.PreloadMinionsCount));
            }

            for(int i = 0; i < equipItemsReferencesHolderComponent.ItemViewReferences.Length; i++)
            {
                taskList.Add(poolingSystem.Warmup(equipItemsReferencesHolderComponent.ItemViewReferences[i], 2));
            }

            WarmupScenario();
        }

        private void WarmupScenario()
        {
            if(biomConfigsHolderComponent.TryGetBiomScenarioReferences(sceneVariablesComponent.CurrentBiomIndex, ref scenarioReferences))
            {
                foreach (var levelReferences in scenarioReferences)
                {
                    taskList.Add(poolingSystem.Warmup(levelReferences.Key, levelReferences.Value));
                }
            }
        }
    }
}