using System;
using HECSFramework.Core;
using Components;
using Commands;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Systems
{
	[Serializable][Documentation(Doc.Level, Doc.Global, "this system clear travel biom before the next one has been loaded")]
    public sealed class ClearTravelBiomSystem : BaseGameStateSystem
    {
        [Required]
        private TravelBiomTypeVariablesComponent biomVariables;

        [Required]
        private TravelPointsHolderComponent travelPointsHolderComponent;

        [Single]
        private PoolingSystem poolingSystem;

        protected override int State => GameStateIdentifierMap.ClearTravelBiomStateIdentifier;

        private SceneVariablesComponent sceneVariablesComponent;

        public override void InitSystem()
        {
            sceneVariablesComponent = Owner.World.GetEntityBySingleComponent<SceneManagerTagComponent>().GetComponent<SceneVariablesComponent>();
        }

        protected override async void ProcessState(int from, int to)
        {
            foreach(var enemy in biomVariables.EnemiesByIndex.Values)
            {
                if(enemy.IsAlive)
                {
                    Owner.World.Command(new DestroyEntityWorldCommand { Entity = enemy });
                }
            }
            biomVariables.EnemiesByIndex.Clear();

            Owner.World.Command(new DestroyEntityWorldCommand { Entity = biomVariables.PlayerCharacter });

            var playerMinions = Owner.World.GetEntitiesByComponent<PlayerMinionTagComponent>();

            foreach (var minion in playerMinions)
            {
                Owner.World.Command(new DestroyEntityWorldCommand { Entity = minion });
            }

            biomVariables.PlayerCharacter = null;
            biomVariables.LeftMinion = null;
            biomVariables.RightMinion = null;

            travelPointsHolderComponent.Clear();

            var camera = Owner.World.GetEntityBySingleComponent<MainCameraComponent>();
            var cameraStartPosition = camera.GetComponent<CameraStartPositionHolderComponent>().StartPosition;

            camera.GetTransform().position = cameraStartPosition;

            if(sceneVariablesComponent.CurrentBiomMoveType == BiomMoveTypeIdentifierMap.TravelPrefab)
            {
                poolingSystem.ReleaseView(sceneVariablesComponent.CurrentBiomView);
            }

            if (sceneVariablesComponent.CurrentBiomMoveType == BiomMoveTypeIdentifierMap.TravelScene)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(0));
                await Addressables.UnloadSceneAsync(sceneVariablesComponent.CurrentBiomSceneInstance).Task;
            }


            sceneVariablesComponent.CurrentBiomIndex++;
            EndState();
        }
    }
}