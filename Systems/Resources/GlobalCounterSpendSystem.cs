using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;

namespace Systems
{
	[Serializable][Documentation(Doc.Resources, "this system procces spend resources commands")]
    public sealed class GlobalCounterSpendSystem : BaseSystem, IReactGlobalCommand<GlobalSpendResourceCommand>
    {
        [Required]
        public DrawRuleEntitiesHolderComponent DrawRuleEntitiesHolderComponent;

        public void CommandGlobalReact(GlobalSpendResourceCommand command)
        {
            var counter = command.From.Entity.GetComponent<CountersHolderComponent>().GetCounter<ICounter<int>>(command.CounterID);

            if (counter.Value >= command.Amount)
            {
                counter.ChangeValue(-command.Amount);

                Owner.World.Command(new UpdateVisualRewardCounterCommand
                {
                    Amount = -command.Amount,
                    CounterID = command.CounterID
                });

                DrawRuleEntitiesHolderComponent.Draw(new DrawSpendResourceCommand
                {
                    DrawRule = command.DrawRule,
                    CounterID = command.CounterID,
                    From = command.From,
                    To = command.To
                });
            }
        }

        public override void InitSystem()
        {
        }
    }
}