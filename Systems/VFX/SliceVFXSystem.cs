using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;

namespace Systems
{
	[Serializable][Documentation(Doc.VFX, "SliceVFXSystem")]
    public sealed class SliceVFXSystem : BaseSystem, IInitAfterView, IUpdatable
    {
        [Single]
        private PoolingSystem poolingSystem;
        [Required]
        private VFXScaleHolderComponent ScaleHolder;

        private float baseDuration;
        private float duration;
        private float currentTime;
        private ParticlesProviderMonoComponent particleProvider;

        public void InitAfterView()
        {
            var atkAnimationSpeed = Owner.World.GetEntityBySingleComponent<PlayerCharacterTagComponent>().GetComponent<AnimatorStateComponent>().Animator.GetFloat(nameof(AnimParametersMap.AttackAnimationSpeed));
            particleProvider = Owner.AsActor().GetComponentInChildren<ParticlesProviderMonoComponent>();
            particleProvider.ParticleSystem.transform.localScale = ScaleHolder.Scale;
            var particleModule = particleProvider.ParticleSystem.main;

            baseDuration = particleModule.duration;
            particleModule.duration /= atkAnimationSpeed;

            particleProvider.ParticleSystem.Play();

            duration = particleModule.duration;
        }

        public override void InitSystem()
        {
        }

        public void Reset()
        {
            
        }

        public void UpdateLocal()
        {
            if (Owner.TryGetComponent<ViewReadyTagComponent>(out var component))
            {
                currentTime += Time.deltaTime;

                if (currentTime >= duration)
                {
                    currentTime = 0;
                    particleProvider.ParticleSystem.Stop();

                    var particleModule = particleProvider.ParticleSystem.main;
                    particleModule.duration = baseDuration;
                    particleProvider.ParticleSystem.transform.localScale = Vector3.one;

                    Owner.World.Command(new DestroyEntityWorldCommand { Entity = Owner });
                }
            }
        }
    }
}