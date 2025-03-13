using System;
using HECSFramework.Core;
using Components;
using Commands;
using UnityEngine;

namespace Systems
{
	[Serializable][Documentation(Doc.Enemy, "this system killing enemy")]
    public sealed class EnemyDieSystem : BaseSystem, IReactCommand<IsDeadCommand>, IUpdatable
    {
        [Single]
        public PlayerTagComponent PlayerTagComponent;

        [Required]
        private RewardsLocalHolderComponent rewardsLocalHolderComponent;

        private bool isAwaitingDie;
        private float currentClipTime;
        private float clipLenth;
        private AnimationCheckOutsHolderComponent animationCheckOutsHolderComponent;

        public void CommandReact(IsDeadCommand command)
        {
            var target = PlayerTagComponent.Owner;

            rewardsLocalHolderComponent.ExecuteRewards(
            new ExecuteReward
            {
                Owner = Owner,
                Target = target
            });

            EnemyDie();
        }

        public override void InitSystem()
        {
        }

        public void UpdateLocal()
        {
            if(isAwaitingDie)
            {
                currentClipTime += Time.deltaTime;

                if (clipLenth == 0)
                {
                    if (animationCheckOutsHolderComponent.TryGetCheckoutInfo(AnimationEventIdentifierMap.Die, out var animationCheckOutInfo))
                    {
                        clipLenth = animationCheckOutInfo.ClipLenght;
                    }
                }
                else
                {
                    if (currentClipTime >= clipLenth)
                    {
                        Owner.World.GetEntityBySingleComponent<VisualQueueTagComponent>().GetComponent<VisualLocalLockComponent>().Remove();
                        currentClipTime = 0;
                        clipLenth = 0;
                    }
                }
            }
        }

        private void EnemyDie()
        {
            Owner.Command(new TriggerAnimationCommand { Index = AnimParametersMap.Die });
            Owner.RemoveComponent<ActiveEnemyTagComponent>();

            if (Owner.TryGetComponent<AnimationCheckOutsHolderComponent>(out var component))
            {
                Owner.World.GetEntityBySingleComponent<VisualQueueTagComponent>().GetOrAddComponent<VisualLocalLockComponent>().AddLock();
                animationCheckOutsHolderComponent = component;
                isAwaitingDie = true;
            }

            Owner.World.Command(new GoToNextChunkCommand());
        }
    }
}