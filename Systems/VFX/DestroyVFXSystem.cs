using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;

namespace Systems
{
    [Serializable][Documentation(Doc.Visual, "this system destroy particles after it's end")]
    public sealed class DestroyVFXSystem : BaseSystem, IUpdatable, IInitAfterView
    {
        [Single]
        private PoolingSystem poolingSystem;

        private ParticleSystem particles;
        private float duration;
        private float currentTime;

        public void InitAfterView()
        {
            var particlesMono = Owner.AsActor().GetComponentInChildren<ParticlesProviderMonoComponent>();
            particles = particlesMono.ParticleSystem;
            duration = particles.main.duration;
        }

        public override void InitSystem()
        {
        }

        public void Reset()
        {
        }

        public void UpdateLocal()
        {
            if(Owner.TryGetComponent<ViewReadyTagComponent>(out var component))
            {
                currentTime += Time.deltaTime;

                if (currentTime >= duration)
                {
                    currentTime = 0;
                    poolingSystem.Release(Owner.AsActor());
                }
            }
        }
    }
}
