using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;

namespace Systems
{
	[Serializable][Documentation(Doc.Abilities, "this system control base projectile behavior")]
    public sealed class BaseProjectileSystem : BaseSystem, IUpdatable
    {
        [Required]
        private ProgressComponent progressComponent;
        [Required]
        private DamageComponent damageComponent;
        [Required]
        private TargetHolderComponent targetHolderComponent;
        [Required]
        private StartPositionHolderComponent startPositionHolderComponent;
        [Required]
        private AnimationCurveComponent animationCurveComponent;

        private Transform transform;
        private Vector3 targetPosition;

        public override void InitSystem()
        {
            transform = Owner.GetComponent<UnityTransformComponent>().Transform;
            targetPosition = targetHolderComponent.Target.AsActor().GetComponentInChildren<TargetMonoComponent>().Transform.position;
        }

        public void UpdateLocal()
        {
            var speed = animationCurveComponent.AnimationCurve.Evaluate(progressComponent.Value);
            progressComponent.ChangeValue(Time.deltaTime * speed);

            var direction = Vector3.Lerp(startPositionHolderComponent.StartPosition, targetPosition, progressComponent.Value);

            transform.position = direction;

            if (progressComponent.Value >= 1)
            {
                progressComponent.SetValue(0);

                targetHolderComponent.Target.Command(new DamageCommand<float> { DamageValue = damageComponent.Value });

                Owner.World.Command(new DestroyEntityWorldCommand { Entity = Owner });
            }
        }
    }
}