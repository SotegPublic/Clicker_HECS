using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;
using static UnityEngine.EventSystems.EventTrigger;

namespace Systems
{
	[Serializable][Documentation(Doc.Animation, Doc.Abilities, "this system activate attack animation")]
    public sealed class AttackAnimationAbilitySystem : BaseAbilitySystem, IUpdatable
    {
        [Required]
        private ActionsHolderComponent actionsHolderComponent;

        private float currentClipTime;
        private float clipLenth;

        public override void Execute(Entity owner = null, Entity target = null, bool enable = true)
        {
            actionsHolderComponent.ExecuteAction(ActionIdentifierMap.ExecuteAbilityIdentifier);
            Owner.GetComponent<AbilityOwnerComponent>().AbilityOwner.AddComponent<PlayingAttackAnimationTagComponent>();
        }

        public override void InitSystem()
        {
        }

        public void UpdateLocal()
        {
            var abilityOwner = Owner.GetComponent<AbilityOwnerComponent>().AbilityOwner;

            if (abilityOwner.TryGetComponent<PlayingAttackAnimationTagComponent>(out var inAttackComponent))
            {
                currentClipTime += Time.deltaTime;

                if (clipLenth == 0)
                {
                    var animationCheckOutsHolder = abilityOwner.GetComponent<AnimationCheckOutsHolderComponent>();

                    if (animationCheckOutsHolder.TryGetCheckoutInfo(AnimationEventIdentifierMap.Attack, out var animationCheckOutInfo))
                    {
                        clipLenth = animationCheckOutInfo.ClipLenght;
                    }
                }
                else
                {
                    if (currentClipTime >= clipLenth)
                    {
                        abilityOwner.RemoveComponent<PlayingAttackAnimationTagComponent>();
                        currentClipTime = 0;
                        clipLenth = 0;
                        actionsHolderComponent.ExecuteAction(ActionIdentifierMap.EndAbilityIdentifier);
                    }
                }
            }
        }
    }
}