using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;
using System.Collections.Generic;
using Helpers;
using System.Buffers;

namespace Systems
{
	[Serializable][Documentation(Doc.Visual, Doc.UI, Doc.VFX, "this system spawn bags into chest and send rewards command")]
    public sealed class ChestGettingBagsSystem : BaseSystem, IReactCommand<PrepearChestForGettingRewardCommand>, IReactCommand<SpawnBagInChestCommand>, IInitAfterView, IUpdatable
    {
        [Required]
        private GettingBagSystemVariablesComponent variablesComponent;
        

        public override void InitSystem()
        {
        }

        public void InitAfterView()
        {
            variablesComponent.ChestParticles = Owner.AsActor().GetComponent<ChestParticlesMonocomponent>();
        }

        public void Reset()
        {
        }

        public void CommandReact(PrepearChestForGettingRewardCommand command)
        {
            variablesComponent.BagsCount = command.BagsCount;
            variablesComponent.ChestParticlesEmitType = variablesComponent.BagsCount switch
            {
                1 => ChestParticlesEmitType.Center,
                2 => ChestParticlesEmitType.LeftRight,
                >2 => ChestParticlesEmitType.Random,
                <=0 => throw new Exception("expected number of bags in GettingBagsSystem is less than or equal to zero")
            };

            Owner.Command(new TriggerAnimationCommand { Index = AnimParametersMap.Open });
            variablesComponent.ChestParticles.ActivateParticlesSystems();
            variablesComponent.IsAwaitingBags = true;

            Owner.World.GetEntityBySingleComponent<VisualQueueTagComponent>().GetOrAddComponent<VisualLocalLockComponent>().AddLock();
        }

        public void CommandReact(SpawnBagInChestCommand command)
        {
            switch (variablesComponent.ChestParticlesEmitType)
            {
                case ChestParticlesEmitType.Center:

                    var centerContext = variablesComponent.ChestParticles.GetCenterParticleSystem();
                    ExecuteCommand(command, centerContext);

                    break;
                case ChestParticlesEmitType.LeftRight:

                    var sideContext = variablesComponent.ChestParticles.GetRightOrLeftParticleSystem();
                    ExecuteCommand(command, sideContext);

                    break;
                case ChestParticlesEmitType.Random:

                    if (variablesComponent.ChestParticles.TryGetFreeParticleSystem(out var emiterContext))
                    {
                        ExecuteCommand(command, emiterContext);
                    }
                    else
                    {
                        variablesComponent.CommandsQueue.Enqueue(command);
                    }

                    break;
                default:
                case ChestParticlesEmitType.None:
                    break;
            }
        }

        public void UpdateLocal()
        {
            if (!variablesComponent.IsAwaitingBags) return;

            if (variablesComponent.CommandsQueue.Count > 0)
            {
                if (variablesComponent.ChestParticles.TryGetFreeParticleSystem(out var emiterContext))
                {
                    var command = variablesComponent.CommandsQueue.Dequeue();
                    ExecuteCommand(command, emiterContext);
                }
            }

            for(int i = 0; i < variablesComponent.BagParticleContexts.Count; i++)
            {
                ref var context = ref variablesComponent.BagParticleContexts[i];
                var particlesCount = context.emiterContext.particleSystem.GetParticles(context.Particles);

                if(particlesCount == 0)
                {
                    context.IsNeedToRemove = true;

                    Owner.World.Command(new UpdateVisualRewardCounterCommand
                    {
                        Amount = context.RewardAmount,
                        CounterID = CounterIdentifierContainerMap.Chests
                    });
                }
            }

            ClearContextCollection();

            if(variablesComponent.BagsCount == 0)
            {
                Owner.World.GetEntityBySingleComponent<VisualQueueTagComponent>().GetComponent<VisualLocalLockComponent>().Remove();
                Owner.Command(new TriggerAnimationCommand { Index = AnimParametersMap.Close });
                variablesComponent.IsAwaitingBags = false;
            }
        }

        private void ClearContextCollection()
        {
            for(int i = variablesComponent.BagParticleContexts.Count - 1; i >= 0; i--)
            {
                ref var context = ref variablesComponent.BagParticleContexts[i];

                if(context.IsNeedToRemove)
                {
                    ArrayPool<ParticleSystem.Particle>.Shared.Return(context.Particles);

                    context.emiterContext.IsBusy = false;
                    variablesComponent.BagsCount--;

                    variablesComponent.BagParticleContexts.Remove(context);
                }
            }
        }

        private void ExecuteCommand(SpawnBagInChestCommand command, ChestParticleEmiterContext emiterContext)
        {
            emiterContext.particleSystem.Emit(variablesComponent.ParticlesCountToEmit);
            emiterContext.particleSubSystem.Emit(variablesComponent.ParticlesCountToEmit);

            var context = new BagParticleContext
            {
                emiterContext = emiterContext,
                RewardAmount = command.BagRewardAmount,
                Particles = HECSPooledArray<ParticleSystem.Particle>.GetArray(variablesComponent.ParticlesCountToEmit).Items
            };

            emiterContext.IsBusy = true;
            emiterContext.particleSystem.GetParticles(context.Particles, variablesComponent.ParticlesCountToEmit);

            variablesComponent.BagParticleContexts.Add(context);
        }
    }
}