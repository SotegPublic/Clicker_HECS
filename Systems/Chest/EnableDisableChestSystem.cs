using System;
using Commands;
using Components;
using HECSFramework.Core;
using UnityEngine;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Chest, "this system reacts to redraw counters and should be on chest actor")]
    public sealed class EnableDisableChestSystem : BaseSystem, IReactGlobalCommand<UpdateVisualRewardCounterCommand>
    {
        [Required]
        public AnimatorStateComponent animatorState;

        public ChestsCountComponent chestsCount => Owner.World.GetEntityBySingleComponent<PlayerTagComponent>().GetComponent<ChestsCountComponent>();

        public async void CommandGlobalReact(UpdateVisualRewardCounterCommand command)
        {
            if (chestsCount.Value == 0)
            {
                if (animatorState.State.TryGetBool(AnimParametersMap.IsActive, out var result) && result == true)
                {
                    animatorState.State.SetBool(AnimParametersMap.IsActive, false);

                    await new WaitForTagState(AnimParametersMap.Closed, animatorState.Animator).RunJob();
                    Owner.World.GetEntityBySingleComponent<ChestCounterTagComponent>().Command(new BoolAnimationCommand { Value = false, Index = AnimParametersMap.IsActive });
                }
            }
            else if (chestsCount.Value > 0)
            {
                if (animatorState.State.TryGetBool(AnimParametersMap.IsActive, out var result) && result == false)
                {
                    animatorState.State.SetBool(AnimParametersMap.IsActive, true);
                    Owner.World.GetEntityBySingleComponent<ChestCounterTagComponent>().Command(new BoolAnimationCommand { Value = true, Index = AnimParametersMap.IsActive });
                }
                else
                {
                    animatorState.State.SetBool(AnimParametersMap.IsActive, true);
                    Owner.World.GetEntityBySingleComponent<ChestCounterTagComponent>().Command(new BoolAnimationCommand { Value = true, Index = AnimParametersMap.IsActive });
                }
            }
        }

        public override void InitSystem()
        {
        }
    }

    public struct WaitForTagState : IHecsJob
    {
        public int Tag;
        public Animator Animator;

        public WaitForTagState(int tag, Animator animator)
        {
            Tag = tag;
            Animator = animator;
        }

        public bool IsComplete()
        {
            return Animator.GetCurrentAnimatorStateInfo(0).tagHash == Tag;
        }

        public void Run()
        {
        }
    }
}