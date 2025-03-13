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
    [Documentation(Doc.DrawRule, Doc.Visual, Doc.FX, "this system draw bag to entity to entity by emiting particle, this system should ")]
    public sealed class DrawBagParticleFromEntityToEntitySystem : BaseSystem, IReactCommand<DrawGlobalResourceRewardCommand>, IUpdatable
    {
        [Required]
        public ViewReferenceGameObjectComponent ViewReferenceGameObjectComponent;

        [Required]
        public AdditionalParticleEffectReferenceComponent AdditionalParticleEffectReferenceComponent;

        [Required]
        public BagsDrawRuleVariables BagsDrawRuleVariables;

        [Single]
        public PoolingSystem PoolingSystem;

        [Required]
        public VFXDrawResourceConfigComponent VFXDrawResourceConfigComponent;

        [Required]
        public VFXDrawResourceDrawParticlesContextsComponent VFXDrawResourceDrawParticlesContextsComponent;

        [Required]
        public DrawRuleTagComponent DrawRuleTagComponent;

        private Entity chestEntity;
        private bool IsOpenTriggerSend;

        public override void InitSystem()
        {
        }

        public async void CommandReact(DrawGlobalResourceRewardCommand command)
        {
            if (DrawRuleTagComponent.CounterIdentifierContainers != command.GlobalResourceRewardCommand.CounterID
                || DrawRuleTagComponent.DrawRuleIdentifiers != command.GlobalResourceRewardCommand.DrawRule)
                return;

            chestEntity = Owner.World.GetEntityBySingleComponent<ChestTagComponent>();
            Owner.World.GetEntityBySingleComponent<VisualQueueTagComponent>().GetOrAddComponent<VisualLocalLockComponent>().AddLock();

            var drawTarget = Owner.World.GetFilter<ParticleTargetTagComponent>().
                FirstOrDefault(x => x.GetComponent<ParticleTargetTagComponent>().TargetID == VFXDrawResourceConfigComponent.ParticlesTargetID);

            var particleSystemView = await PoolingSystem.GetViewFromPool(ViewReferenceGameObjectComponent.ViewReference);
            var additionalparticleSystemView = await PoolingSystem.GetViewFromPool(AdditionalParticleEffectReferenceComponent.ViewReference);

            var particleSystem = particleSystemView.GetComponent<ParticlesProviderMonoComponent>().ParticleSystem;
            var additionalParticleSystem = additionalparticleSystemView.GetComponent<ParticlesProviderMonoComponent>().ParticleSystem;

            var emission = particleSystem.emission;
            emission.enabled = false;

            var addEmission = additionalParticleSystem.emission;
            addEmission.enabled = false;

            var module = particleSystem.main;
            module.simulationSpace = ParticleSystemSimulationSpace.Local;

            var neededTransform = command.GlobalResourceRewardCommand.From.Entity.GetTransform();
            particleSystem.transform.SetPositionAndRotation(neededTransform.position, neededTransform.rotation);
            additionalParticleSystem.transform.SetPositionAndRotation(neededTransform.position, neededTransform.rotation);

            var particlesCountToEmit = command.GlobalResourceRewardCommand.Amount > VFXDrawResourceConfigComponent.SpawnParticlesCount ?
                VFXDrawResourceConfigComponent.SpawnParticlesCount : command.GlobalResourceRewardCommand.Amount;

            particleSystem.Play();
            additionalParticleSystem.Play();

            particleSystem.Emit(particlesCountToEmit);
            additionalParticleSystem.Emit(particlesCountToEmit);

            var context = new ResourceParticleSystemContext
            {
                CounterID = command.GlobalResourceRewardCommand.CounterID,
                Target = new AliveEntity(drawTarget),
                TotalRewardAmount = command.GlobalResourceRewardCommand.Amount,
                CurrentParticlesCount = particlesCountToEmit,
                FirstStatePercent = VFXDrawResourceConfigComponent.NonControlStatePercentage,
                ParticleSystem = particleSystem,
                ParticleView = particleSystemView,
                ParticleSystemGravityModifier = particleSystem.main.gravityModifier.constant,

                AdditionalParticleView = additionalparticleSystemView,
                AdditionalParticleSystem = additionalParticleSystem,
                AdditioanlParticles = HECSPooledArray<ParticleSystem.Particle>.GetArray(particlesCountToEmit).Items,

                Particles = HECSPooledArray<ParticleSystem.Particle>.GetArray(particlesCountToEmit).Items,
                ParticleContexts = HECSPooledArray<ParticleContext>.GetArray(particlesCountToEmit).Items,
                ParticleRewards = HECSPooledArray<int>.GetArray(particlesCountToEmit).Items,

                NextRewardIndex = 0,
                LastDiviationCurvesCollectionIndex = -1,
            };

            var particlesCount = particleSystem.GetParticles(context.Particles);
            var addParticlesCount = additionalParticleSystem.GetParticles(context.AdditioanlParticles);

            InitParticlesContexts(ref context, particlesCount);

            CalculateRewardByParticle(ref context);

            VFXDrawResourceDrawParticlesContextsComponent.ResourceParticleContexts.Add(context);

            particleSystem.SetParticles(context.Particles, particlesCount);
            additionalParticleSystem.SetParticles(context.AdditioanlParticles, particlesCount);
        }

        public void UpdateLocal()
        {
            var collection = VFXDrawResourceDrawParticlesContextsComponent.ResourceParticleContexts;

            for (int i = 0; i < collection.Count; i++)
            {
                ref var currentContext = ref collection[i];

                var targetPosition = GetTargetPosition(ref currentContext);

                var particlesCount = currentContext.ParticleSystem.GetParticles(currentContext.Particles);
                var addParticlesCount = currentContext.AdditionalParticleSystem.GetParticles(currentContext.AdditioanlParticles);

                MoveParticles(ref currentContext, targetPosition, particlesCount, addParticlesCount);

                currentContext.ParticleSystem.SetParticles(currentContext.Particles, particlesCount);
                currentContext.AdditionalParticleSystem.SetParticles(currentContext.AdditioanlParticles, particlesCount);

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
                context.AdditioanlParticles[i].randomSeed = (uint)i;
                context.AdditioanlParticles[i].startLifetime = context.Particles[i].startLifetime;
                context.AdditioanlParticles[i].remainingLifetime = context.Particles[i].remainingLifetime;
                ref var particleContext = ref context.ParticleContexts[context.Particles[i].randomSeed];

                particleContext.StartPosition = context.Particles[i].position;
                particleContext.IsInControlState = false;
                particleContext.DiviationCurveIndex = GetDiviationCurveIndex(ref context);
                particleContext.AdditionalParticleID = context.AdditioanlParticles[i].randomSeed;

                context.AdditioanlParticles[i].position = context.Particles[i].position;
            }
        }

        private void MoveParticles(ref ResourceParticleSystemContext currentContext, Vector3 targetPosition, int particlesCount, int additionalParticlesCount)
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

                    if (!chestEntity.TryGetComponent<RollingEquipItemTagComponent>(out var component))
                    {
                        if (finalStateProgress >= BagsDrawRuleVariables.ActivateChestTimer && !IsOpenTriggerSend)
                        {
                            chestEntity.Command(new TriggerAnimationCommand { Index = AnimParametersMap.Open });
                            IsOpenTriggerSend = true;
                        }
                    }
                    else
                    {
                        IsOpenTriggerSend = false;
                    }

                    var lerpDirection = Vector3.LerpUnclamped(particleContext.StartPosition, targetPosition, finalStateProgress);

                    if (VFXDrawResourceConfigComponent.IsControledByCurves)
                    {
                        ChangeDirectionByCurves(finalStateProgress, ref lerpDirection,
                            VFXDrawResourceConfigComponent.DeviationCurvesCollections[particleContext.DiviationCurveIndex]);
                    }

                    particle.position = lerpDirection;

                    if (finalStateProgress >= BagsDrawRuleVariables.ActivateSpeedupTimer)
                    {
                        particle.remainingLifetime -= (Time.deltaTime * BagsDrawRuleVariables.SpeedupModifier);
                    }
                }

                var worldParticlePosition = currentContext.ParticleSystem.transform.TransformPoint(particle.position);
                var localAdditionalParticlePosition = currentContext.AdditionalParticleSystem.transform.InverseTransformPoint(worldParticlePosition);

                ref var additionalParticle = ref GetAdditionalParticle(ref currentContext, additionalParticlesCount, particleContext.AdditionalParticleID);
                additionalParticle.position = localAdditionalParticlePosition;
            }
        }

        private ref ParticleSystem.Particle GetAdditionalParticle(ref ResourceParticleSystemContext currentContext, int particleCount, uint additionalParticleID)
        {
            for (int j = 0; j < particleCount; j++)
            {
                if (currentContext.AdditioanlParticles[j].randomSeed == additionalParticleID)
                {
                    return ref currentContext.AdditioanlParticles[j];
                }
            }

            return ref currentContext.NullableParticle;
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

                if (index == context.LastDiviationCurvesCollectionIndex)
                {
                    if (index + 1 > lastIndex)
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

                if(!chestEntity.TryGetComponent<RollingEquipItemTagComponent>(out var component))
                {
                    chestEntity.Command(new PlayLocalVFXCommand { Enable = true, ID = FXIdentifierMap.ChestShineIdentifier });
                }

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
            Vector3 targetPosition = Vector3.zero;

            if(currentContext.Target.Entity.TryGetComponent<UIParticlesTargetProviderComponent>(out var targetProvider))
            {
                targetPosition = targetProvider.Get.TargetTransform.position;
            }
            else
            {
                targetPosition = currentContext.Target.Entity.GetPosition();
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

            var screenTargetPosition = CanvasCam.WorldToScreenPoint(targetPosition);
            var pos = MainCam.ScreenToWorldPoint(screenTargetPosition);

            return currentContext.ParticleSystem.transform.InverseTransformPoint(pos);
        }

        private void CheckIsAllParticlesDead(ref ResourceParticleSystemContext context)
        {
            if (context.ParticleSystem.particleCount == 0)
            {
                context.IsNeedToRemove = true;

                if (context.TotalRewardAmount >= 0)
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
                    ArrayPool<ParticleSystem.Particle>.Shared.Return(currentContext.AdditioanlParticles);
                    ArrayPool<int>.Shared.Return(currentContext.ParticleRewards);

                    var main = currentContext.ParticleSystem.main;
                    var gravityCurve = currentContext.ParticleSystem.main.gravityModifier;
                    gravityCurve.constant = currentContext.ParticleSystemGravityModifier;
                    main.gravityModifier = gravityCurve;

                    currentContext.ParticleSystem.Stop();
                    currentContext.AdditionalParticleSystem.Stop();

                    PoolingSystem.ReleaseView(currentContext.ParticleView);
                    PoolingSystem.ReleaseView(currentContext.AdditionalParticleView);

                    VFXDrawResourceDrawParticlesContextsComponent.ResourceParticleContexts.RemoveAt(i);

                    Owner.World.GetEntityBySingleComponent<VisualQueueTagComponent>().GetComponent<VisualLocalLockComponent>().Remove();
                }
            }

            if (collection.Count == 0 && IsOpenTriggerSend)
            {
                if (!chestEntity.TryGetComponent<RollingEquipItemTagComponent>(out var component))
                {
                    chestEntity.Command(new TriggerAnimationCommand { Index = AnimParametersMap.Close });
                }

                IsOpenTriggerSend = false;
            }
        }
    }
}