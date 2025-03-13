using Commands;
using Components;
using HECSFramework.Core;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Systems
{
    [Serializable][Documentation(Doc.Level, Doc.Load, "here we load biomView and set parameters for following states")]
    public sealed class LoadBiomSystem : BaseGameStateSystem
    {
        [Required]
        private BiomConfigsHolderComponent biomConfigsHolderComponent;
        [Required]
        private SceneVariablesComponent sceneVariables;

        [Single]
        private PoolingSystem poolingSystem;

        protected override int State { get; } = GameStateIdentifierMap.LoadBiomStateIdentifier;

        public override void InitSystem()
        {
        }

        protected override async void ProcessState(int from, int to)
        {
            if(sceneVariables.CurrentBiomIndex <= biomConfigsHolderComponent.Bioms.Length - 1)
            {
                await LoadBiomAsync(sceneVariables.CurrentBiomIndex);
                EndState();
            }
            else
            {
                Debug.LogWarning("Ошибка загрузки следующего уровня");
            }
        }

        private async Task LoadBiomAsync(int biomConfigIndex)
        {
            var loadingBiomConfig = biomConfigsHolderComponent.Bioms[biomConfigIndex];
            Owner.World.Command(new LoadBiomCommand
            {
                BiomConfig = loadingBiomConfig
            });

            var job = new WaitFor<BiomReadyTagComponent>(Owner);
            var awaiter = job.RunJob();

            await awaiter;

            Owner.RemoveComponent<BiomReadyTagComponent>();
        }
    }
}