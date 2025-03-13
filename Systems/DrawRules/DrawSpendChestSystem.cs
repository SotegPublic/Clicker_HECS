using System;
using Commands;
using Components;
using HECSFramework.Core;
using UnityEngine;

namespace Systems
{
    [Serializable][Documentation(Doc.DrawRule, Doc.Resources, "this system draw equip item roll scenario")]
    public sealed class DrawSpendChestSystem : BaseSystem, IReactCommand<DrawSpendResourceCommand>, IUpdatable
    {
        [Required]
        private AnimationCheckOutsHolderComponent animationCheckOutsHolder;
        [Required]
        private SpendChestRuleVariablesComponent variablesComponent;

        private float clipLenth;
        private float currentClipTime;
        private bool isActivated;
        private Entity chestEntity;
        public void CommandReact(DrawSpendResourceCommand command)
        {
            chestEntity = Owner.World.GetEntityBySingleComponent<ChestTagComponent>();
            chestEntity.AddComponent<RollingEquipItemTagComponent>();

            chestEntity.Command(new TriggerAnimationCommand { Index = AnimParametersMap.RollItem });
            chestEntity.Command(new PlayLocalVFXCommand { Enable = true, ID = FXIdentifierMap.ChestRollFx });
            isActivated = true;
        }

        public override void InitSystem()
        {
        }

        public void UpdateLocal()
        {
            if(isActivated)
            {
                if(clipLenth == 0)
                {
                    if (animationCheckOutsHolder.TryGetCheckoutInfo(AnimationEventIdentifierMap.Roll, out var checkoutInfo))
                    {
                        var chestAnimator = chestEntity.GetComponent<AnimatorStateComponent>().Animator;
                        if (chestAnimator.GetFloat("RollSpeed") != variablesComponent.ChestRollAnimationSpeedModifier)
                        {
                            chestEntity.Command(new FloatAnimationCommand { Index = AnimParametersMap.RollSpeed, Value = variablesComponent.ChestRollAnimationSpeedModifier });
                        }
                        
                        clipLenth = checkoutInfo.ClipLenght / variablesComponent.ChestRollAnimationSpeedModifier;
                    }
                }
                else
                {
                    currentClipTime += Time.deltaTime;

                    if(currentClipTime >= clipLenth)
                    {
                        chestEntity.Command(new PlayLocalVFXCommand { Enable = true, ID = FXIdentifierMap.ChestShineIdentifier });
                        chestEntity.AddComponent<ReadyForShowingItemTagComponent>();
                        isActivated = false;
                        clipLenth = 0;
                        currentClipTime = 0;
                    }
                }
            }
        }
    }
}