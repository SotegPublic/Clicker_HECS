using HECSFramework.Core;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Stats, Doc.Player, "CritDamageModifiableCounterComponent")]
    public sealed class CritDamageModifiableCounterComponent : ModifiableFloatCounterComponent
    {
        [SerializeField] private float critDamage;

        public override int Id => CounterIdentifierContainerMap.CritDamage;

        public override float SetupValue => critDamage;
    }
}