using Commands;
using HECSFramework.Core;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Stats, Doc.Equipment, "here we hold summary player item power")]
    public sealed class SummaryItemPowerComponent : SimpleIntCounterBaseComponent
    {
        [SerializeField] private int itemPower;
        public override int Value { get => itemPower; protected set => itemPower = value; }

        public override int Id => CounterIdentifierContainerMap.ItemPower;

        public override void ChangeValue(int value)
        {
            Value += value;
            Owner.World.Command(new SummaryPowerUpdate());
        }
    }
}


namespace Commands
{
    public struct SummaryPowerUpdate : IGlobalCommand { }
}