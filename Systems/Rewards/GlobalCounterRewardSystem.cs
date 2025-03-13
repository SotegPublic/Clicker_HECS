using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Rewards, Doc.Counters, "this system procces counter rewards and execute visual parts of rewards")]
    public sealed class GlobalCounterRewardSystem : BaseSystem, IReactGlobalCommand<GlobalResourceRewardCommand>
    {
        [Required]
        public DrawRuleEntitiesHolderComponent DrawRuleEntitiesHolderComponent;

        public override void InitSystem()
        {
        }

        public void CommandGlobalReact(GlobalResourceRewardCommand command)
        {
            var counter = command.To.Entity.GetComponent<CountersHolderComponent>()
                .GetCounter<ICounter<int>>(command.CounterID);

            var previousValue = counter.Value;
            counter.ChangeValue(command.Amount);
            var currentValue = counter.Value;

            DrawRuleEntitiesHolderComponent.Draw(new DrawGlobalResourceRewardCommand
            {
                CurrentValue = currentValue,
                GlobalResourceRewardCommand = command,
                PreviousValue = previousValue
            });
        }
    }
}