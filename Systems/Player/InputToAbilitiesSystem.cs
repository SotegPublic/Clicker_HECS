using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Abilities, Doc.Input, Doc.Character, "we listen here input and execute abilities by input")]
    public sealed class InputToAbilitiesSystem : BaseSystem, IReactCommand<InputPerformedCommand>
    {

        public void CommandReact(InputPerformedCommand command)
        {

            if (command.Index == InputIdentifierMap.Fire)
            {
                var target = Owner.World.GetSingleComponent<CurrentEnemyComponent>().Enemy;
                Owner.Command(new ExecuteAbilityByIDCommand { Enable = true, AbilityIndex = AdditionalAbilityIdentifierMap.TapAbilityIdentifier, Owner = Owner, Target = target });
            }
        }

        public override void InitSystem()
        {
        }
    }
}