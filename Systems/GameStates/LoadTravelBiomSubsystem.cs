using System;
using Commands;
using Components;
using HECSFramework.Core;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Systems
{
    [Serializable][Documentation(Doc.Level, Doc.Load, "here we load chunk type biom and set parameters for following states")]
    public sealed class LoadTravelBiomSubsystem : BaseSystem, IReactGlobalCommand<LoadBiomCommand>
    {
        [Required]
        private SceneVariablesComponent sceneVariables;

        [Single]
        private PoolingSystem poolingSystem;

        public async void CommandGlobalReact(LoadBiomCommand command)
        {
            if (command.BiomConfig.MoveTypeID == BiomMoveTypeIdentifierMap.TravelPrefab)
            {
                var loadingBiomConfig = command.BiomConfig;

                var biomView = await poolingSystem.GetViewFromPool(loadingBiomConfig.BiomReference);

                sceneVariables.CurrentBiomID = loadingBiomConfig.BiomID;
                sceneVariables.CurrentBiomView = biomView;
                sceneVariables.CurrentBiomLevelIndex = 0;

                sceneVariables.CurrentBiomMoveType = BiomMoveTypeIdentifierMap.TravelPrefab;
                var biomPointsHolder = biomView.GetComponent<TravelPointsHolderMonoComponent>();
                var travelfeature = Owner.World.GetEntityBySingleComponent<TravelMoveFeatureTagComponent>();
                var travelHolder = travelfeature.GetComponent<TravelPointsHolderComponent>();
                var travelBiomVariables = travelfeature.GetComponent<TravelBiomTypeVariablesComponent>();

                travelHolder.AddTravelPoints(biomPointsHolder.TravelPoints);
                travelBiomVariables.LastLoadedEnemyIndex = 0;

                Owner.AddComponent<BiomReadyTagComponent>();
            }

            if(command.BiomConfig.MoveTypeID == BiomMoveTypeIdentifierMap.TravelScene)
            {
                var loadingBiomConfig = command.BiomConfig;

                var biomSceneInstance = await Addressables.LoadSceneAsync(loadingBiomConfig.BiomReference, UnityEngine.SceneManagement.LoadSceneMode.Additive).Task;

                sceneVariables.CurrentBiomID = loadingBiomConfig.BiomID;
                sceneVariables.CurrentBiomSceneInstance = biomSceneInstance;
                sceneVariables.CurrentBiomLevelIndex = 0;
                SceneManager.SetActiveScene(biomSceneInstance.Scene);

                var travelfeature = Owner.World.GetEntityBySingleComponent<TravelMoveFeatureTagComponent>();
                var travelHolder = travelfeature.GetComponent<TravelPointsHolderComponent>();
                var travelBiomVariables = travelfeature.GetComponent<TravelBiomTypeVariablesComponent>();
                travelBiomVariables.LastLoadedEnemyIndex = 0;


                var rootSceneObjects = biomSceneInstance.Scene.GetRootGameObjects();

                for(int i = 0; i < rootSceneObjects.Length; i++)
                {
                    if (rootSceneObjects[i].TryGetComponent<TravelPointsHolderMonoComponent>(out var component))
                    {
                        travelHolder.AddTravelPoints(component.TravelPoints);
                        break;
                    }
                }

                sceneVariables.CurrentBiomMoveType = BiomMoveTypeIdentifierMap.TravelScene;

                Owner.AddComponent<BiomReadyTagComponent>();
            }
        }

        public override void InitSystem()
        {
        }
    }
}