using System;
using HECSFramework.Core;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Stats, Doc.Player, "AttackPowerModifiableCounterComponent")]
    public sealed class AttackPowerModifiableCounterComponent : ModifiableFloatCounterComponent
    {
        [SerializeField] private float attackPower;

        public override int Id => CounterIdentifierContainerMap.AttackPower;

        public override float SetupValue => attackPower;
    }
}