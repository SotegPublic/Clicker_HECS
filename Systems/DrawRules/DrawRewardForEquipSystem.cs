using System;
using HECSFramework.Core;
using Components;
using Commands;

namespace Systems
{
	[Serializable][Documentation(Doc.Rewards, Doc.Equipment, "this system draw reward when we destroy equip item")]
    public sealed class DrawRewardForEquipSystem : BaseSystem, IReactCommand<DrawGlobalResourceRewardCommand>
    {
        [Required]
        public DrawRuleTagComponent DrawRuleTagComponent;

        public void CommandReact(DrawGlobalResourceRewardCommand command)
        {
            Owner.World.Command(new UpdateVisualRewardCounterCommand
            {
                Amount = command.GlobalResourceRewardCommand.Amount,
                CounterID = DrawRuleTagComponent.CounterIdentifierContainers
            });
        }

        public override void InitSystem()
        {
        }
    }
}