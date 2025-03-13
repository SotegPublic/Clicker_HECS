using System;
using System.Buffers;
using Commands;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using Helpers;
using UnityEngine;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.DrawRule, Doc.Visual, Doc.FX, "this system draw resource to entity to entity by emiting particle, this system should ")]
    public sealed class DrawResourceByParticleFromEntityToEntitySystem : BaseSystem, IReactCommand<DrawGlobalResourceRewardCommand>, IUpdatable
    {
        [Required]
        public ViewReferenceGameObjectComponent ViewReferenceGameObjectComponent;

        [Single]
        public PoolingSystem PoolingSystem;

        [Required]
        public VFXDrawResourceConfigComponent VFXDrawResourceConfigComponent;

        [Required]
        public VFXDrawResourceDrawParticlesContextsComponent VFXDrawResourceDrawParticlesContextsComponent;

        [Required]
        public DrawRuleTagComponent DrawRuleTagComponent;

        public override void InitSystem()
        {
        }

        public async void CommandReact(DrawGlobalResourceRewardCommand command)
        {
            if (DrawRuleTagComponent.CounterIdentifierContainers != command.GlobalResourceRewardCommand.CounterID
                || DrawRuleTagComponent.DrawRuleIdentifiers != command.GlobalResourceRewardCommand.DrawRule)
                return;

            Owner.World.GetEntityBySingleComponent<VisualQueueTagComponent>().GetOrAddComponent<VisualLocalLockComponent>().AddLock();

            var drawTarget = Owner.World.GetFilter<ParticleTargetTagComponent>().
                FirstOrDefault(x => x.GetComponent<ParticleTargetTagComponent>().TargetID == VFXDrawResourceConfigComponent.ParticlesTargetID);


            var needed = await PoolingSystem.GetViewFromPool(ViewReferenceGameObjectComponent.ViewReference);
            var particleSystem = needed.GetComponent<ParticlesProviderMonoComponent>().ParticleSystem;

            var emission = particleSystem.emission;
            emission.enabled = false;

            var module = particleSystem.main;
            module.simulationSpace = ParticleSystemSimulationSpace.Local;

            var neededTransform = command.GlobalResourceRewardCommand.From.Entity.GetTransform();
            particleSystem.transform.SetPositionAndRotation(neededTransform.position, neededTransform.rotation);

            var particlesCountToEmit = command.GlobalResourceRewardCommand.Amount > VFXDrawResourceConfigComponent.SpawnParticlesCount ?
                VFXDrawResourceConfigComponent.SpawnParticlesCount : command.GlobalResourceRewardCommand.Amount;

            particleSystem.Play();
            particleSystem.Emit(particlesCountToEmit);

            var context = new ResourceParticleSystemContext
            {
                CounterID = command.GlobalResourceRewardCommand.CounterID,
                Target = new AliveEntity(drawTarget),
                TotalRewardAmount = command.GlobalResourceRewardCommand.Amount,
                CurrentParticlesCount = particlesCountToEmit,
                FirstStatePercent = VFXDrawResourceConfigComponent.NonControlStatePercentage,
                ParticleSystem = particleSystem,
                ParticleView = needed,
                ParticleSystemGravityModifier = particleSystem.main.gravityModifier.constant,

                Particles = HECSPooledArray<ParticleSystem.Particle>.GetArray(particlesCountToEmit).Items,
                ParticleContexts = HECSPooledArray<ParticleContext>.GetArray(particlesCountToEmit).Items,
                ParticleRewards = HECSPooledArray<int>.GetArray(particlesCountToEmit).Items,

                NextRewardIndex = 0,
                LastDiviationCurvesCollectionIndex = -1,
            };

            var particlesCount = particleSystem.GetParticles(context.Particles);

            InitParticlesContexts(ref context, particlesCount);

            CalculateRewardByParticle(ref context);

            VFXDrawResourceDrawParticlesContextsComponent.ResourceParticleContexts.Add(context);
            particleSystem.SetParticles(context.Particles, particlesCount);
        }

        public void UpdateLocal()
        {
            var collection = VFXDrawResourceDrawParticlesContextsComponent.ResourceParticleContexts;

            for (int i = 0; i < collection.Count; i++)
            {
                ref var currentContext = ref collection[i];

                var targetPosition = GetTargetPosition(ref currentContext);

                var particlesCount = currentContext.ParticleSystem.GetParticles(currentContext.Particles);

                MoveParticles(ref currentContext, targetPosition, particlesCount);

                currentContext.ParticleSystem.SetParticles(currentContext.Particles, particlesCount);

                SendReward(ref currentContext);

                CheckIsAllParticlesDead(ref currentContext);
            }

            ClearContextCollection(collection);
        }

        private void InitParticlesContexts(ref ResourceParticleSystemContext context, int particlesCount)
        {
            for (int i = 0; i < particlesCount; i++)
            {
                context.Particles[i].randomSeed = (uint)i;
                ref var particleContext = ref context.ParticleContexts[context.Particles[i].randomSeed];

                particleContext.StartPosition = context.Particles[i].position;
                particleContext.IsInControlState = false;
                particleContext.DiviationCurveIndex = GetDiviationCurveIndex(ref context);
            }
        }

        private void MoveParticles(ref ResourceParticleSystemContext currentContext, Vector3 targetPosition, int particlesCount)
        {
            for (int i = 0; i < particlesCount; i++)
            {
                ref var particle = ref currentContext.Particles[i];
                ref var particleContext = ref currentContext.ParticleContexts[particle.randomSeed];

                if (!particleContext.IsInControlState)
                {
                    var mainProgress = 1 - (particle.remainingLifetime / particle.startLifetime);

                    if (mainProgress >= currentContext.FirstStatePercent)
                    {
                        particleContext.IsInControlState = true;
                        particleContext.StartPosition = particle.position;
                        particleContext.ControledStateLifeTime = particle.remainingLifetime;

                        particle.velocity = Vector3.zero;

                        var main = currentContext.ParticleSystem.main;
                        var gravityCurve = currentContext.ParticleSystem.main.gravityModifier;
                        gravityCurve.constant = 0;
                        main.gravityModifier = gravityCurve;
                    }
                }
                else
                {
                    var finalStateProgress = 1 - (particle.remainingLifetime / particleContext.ControledStateLifeTime);

                    var lerpDirection = Vector3.LerpUnclamped(particleContext.StartPosition, targetPosition, finalStateProgress);
                    
                    if(VFXDrawResourceConfigComponent.IsControledByCurves)
                    {
                        ChangeDirectionByCurves(finalStateProgress, ref lerpDirection,
                            VFXDrawResourceConfigComponent.DeviationCurvesCollections[particleContext.DiviationCurveIndex]);
                    }

                    particle.position = lerpDirection;
                }
            }
        }

        private void ChangeDirectionByCurves(float finalStateProgress, ref Vector3 lerpDirection, DeviationCurvesCollection diviationCurvesCollection)
        {
            lerpDirection.x *= diviationCurvesCollection.XDeviationCurve.Evaluate(finalStateProgress);
            lerpDirection.y *= diviationCurvesCollection.YDeviationCurve.Evaluate(finalStateProgress);
            lerpDirection.z *= diviationCurvesCollection.ZDeviationCurve.Evaluate(finalStateProgress);
        }

        private int GetDiviationCurveIndex(ref ResourceParticleSystemContext context)
        {
            var lenth = VFXDrawResourceConfigComponent.DeviationCurvesCollections.Length;
            var lastIndex = lenth - 1;

            if (lenth > 1)
            {
                var index = UnityEngine.Random.Range(0, lenth);

                if(index == context.LastDiviationCurvesCollectionIndex)
                {
                    if(index + 1 > lastIndex)
                    {
                        index--;
                    }
                    else
                    {
                        index++;
                    }
                }

                context.LastDiviationCurvesCollectionIndex = index;
                return index;
            }

            return lastIndex;
        }

        private void SendReward(ref ResourceParticleSystemContext currentContext)
        {
            if (currentContext.CurrentParticlesCount > currentContext.ParticleSystem.particleCount)
            {
                var delta = currentContext.CurrentParticlesCount - currentContext.ParticleSystem.particleCount;

                for (int i = 0; i < delta; i++)
                {
                    currentContext.Target.Entity.Command(new ImpactCommand { TypeOfImpact = currentContext.CounterID });
                    var reward = currentContext.ParticleRewards[currentContext.NextRewardIndex];

                    Owner.World.Command(new UpdateVisualRewardCounterCommand
                    {
                        Amount = reward,
                        CounterID = currentContext.CounterID
                    });
                    currentContext.TotalRewardAmount -= reward;
                    currentContext.NextRewardIndex++;
                }
                currentContext.CurrentParticlesCount -= delta;
            }
        }

        private void CalculateRewardByParticle(ref ResourceParticleSystemContext context)
        {
            var mod = context.TotalRewardAmount % context.CurrentParticlesCount;
            var rewardAmount = (context.TotalRewardAmount - mod) / context.CurrentParticlesCount;

            var stepsWithGreaterReward = context.TotalRewardAmount - (context.CurrentParticlesCount * rewardAmount);
            for (int i = 0; i < context.CurrentParticlesCount; i++)
            {
                if (i < stepsWithGreaterReward)
                {
                    context.ParticleRewards[i] = rewardAmount + 1;
                }
                else
                {
                    context.ParticleRewards[i] = rewardAmount;
                }
            }
        }

        private Vector3 GetTargetPosition(ref ResourceParticleSystemContext currentContext)
        {
            Transform targetTransform = default;

            if (currentContext.Target.Entity.TryGetComponent<UIParticlesTargetProviderComponent>(out var targetProvider))
            {
                targetTransform = targetProvider.Get.TargetTransform;
            }
            else
            {
                targetTransform = currentContext.Target.Entity.GetComponent<UnityTransformComponent>().Transform;
            }

            var cameraFilter = Owner.World.GetFilter<CameraTagComponent>();

            var MainCam = cameraFilter.
                FirstOrDefault(x =>
                    x.GetComponent<CameraTagComponent>().CameraIdentifier == CameraIdentifierMap.MainCameraIdentifier)
                    .AsActor().GetComponent<Camera>();
            var CanvasCam = cameraFilter.
                FirstOrDefault(x =>
                    x.GetComponent<CameraTagComponent>().CameraIdentifier == CameraIdentifierMap.CanvasCameraIdentifier)
                    .AsActor().GetComponent<Camera>();

            var targetPosInWorld = targetTransform.TransformPoint(targetTransform.position);
            var screenTargetPosition = CanvasCam.WorldToScreenPoint(targetPosInWorld);

            var targetPosition = MainCam.ScreenToWorldPoint(screenTargetPosition);

            return currentContext.ParticleSystem.transform.InverseTransformPoint(targetPosition);
        }

        private void CheckIsAllParticlesDead(ref ResourceParticleSystemContext context)
        {
            if (context.ParticleSystem.particleCount == 0)
            {
                context.IsNeedToRemove = true;

                if(context.TotalRewardAmount >= 0)
                {
                    context.Target.Entity.Command(new ImpactCommand { TypeOfImpact = context.CounterID });
                    Owner.World.Command(new UpdateVisualRewardCounterCommand
                    {
                        Amount = context.TotalRewardAmount,
                        CounterID = context.CounterID
                    });
                    context.TotalRewardAmount = 0;
                    context.CurrentParticlesCount = 0;
                }
            }
        }

        private void ClearContextCollection(HECSList<ResourceParticleSystemContext> collection)
        {
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                ref var currentContext = ref collection[i];

                if (currentContext.IsNeedToRemove)
                {
                    ArrayPool<ParticleSystem.Particle>.Shared.Return(currentContext.Particles);
                    ArrayPool<ParticleContext>.Shared.Return(currentContext.ParticleContexts);
                    ArrayPool<int>.Shared.Return(currentContext.ParticleRewards);

                    var main = currentContext.ParticleSystem.main;
                    var gravityCurve = currentContext.ParticleSystem.main.gravityModifier;
                    gravityCurve.constant = currentContext.ParticleSystemGravityModifier;
                    main.gravityModifier = gravityCurve;

                    currentContext.ParticleSystem.Stop();
                    PoolingSystem.ReleaseView(currentContext.ParticleView);

                    VFXDrawResourceDrawParticlesContextsComponent.ResourceParticleContexts.RemoveAt(i);
                    Owner.World.GetEntityBySingleComponent<VisualQueueTagComponent>().GetComponent<VisualLocalLockComponent>().Remove();
                }
            }
        }
    }
}