using System;
using Commands;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using TMPro;

namespace Systems
{
    [Serializable][Documentation(Doc.UI, Doc.Counters, Doc.Chest, Doc.Rewards, "this systems update UI counters by command")]
    public sealed class GlobalUICountersSystem : BaseSystem, IReactGlobalCommand<UpdateVisualRewardCounterCommand>, IReactCommand<AfterCommand<UpdateVisualRewardCounterCommand>>
    {
        public void CommandGlobalReact(UpdateVisualRewardCounterCommand command)
        {
            var targetUICounter = Owner.World.GetFilter<CounterUITagComponent>().
                FirstOrDefault(x => x.GetComponent<CounterUITagComponent>().CounterIdentifierContainer == command.CounterID);

            if(targetUICounter == null ) return;
            
            var counter = targetUICounter.GetComponent<CountersHolderComponent>().GetOrAddIntCounter(command.CounterID);
            counter.ChangeValue(command.Amount);

            CommandReact(new AfterCommand<UpdateVisualRewardCounterCommand> { Value = command });
        }

        public void CommandReact(AfterCommand<UpdateVisualRewardCounterCommand> command)
        {
            var targetUICounter = Owner.World.GetFilter<CounterUITagComponent>().
                FirstOrDefault(x => x.GetComponent<CounterUITagComponent>().CounterIdentifierContainer == command.Value.CounterID);

            if (command.Value.CounterID == CounterIdentifierContainerMap.Chests)
            {
                var text = targetUICounter.AsActor().GetComponent<UIAccessMonoComponent>().GetGenericComponent<TextMeshPro>(UIAccessIdentifierMap.Text);
                var counter = targetUICounter.GetComponent<CountersHolderComponent>().GetOrAddIntCounter(command.Value.CounterID);
                text.text = counter.Value.ToString();
            }
            else
            {
                var text = targetUICounter.AsActor().GetComponent<UIAccessMonoComponent>().GetTextMeshProUGUI(UIAccessIdentifierMap.Text);
                var counter = targetUICounter.GetComponent<CountersHolderComponent>().GetOrAddIntCounter(command.Value.CounterID);
                text.text = counter.Value.ToString();
            }
        }

        public override void InitSystem()
        {
        }
    }
}