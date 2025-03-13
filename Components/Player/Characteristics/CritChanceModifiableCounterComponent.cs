using HECSFramework.Core;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Stats, Doc.Player, "CritChanceModifiableCounterComponent")]
    public sealed class CritChanceModifiableCounterComponent : ModifiableFloatCounterComponent
    {
        [SerializeField] private float critChance;

        public override int Id => CounterIdentifierContainerMap.CritChance;

        public override float SetupValue => critChance;
    }
}