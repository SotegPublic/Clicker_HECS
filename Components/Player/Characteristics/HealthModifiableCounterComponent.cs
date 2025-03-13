using HECSFramework.Core;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Stats, Doc.Player, "HealthModifiableCounterComponent")]
    public sealed class HealthModifiableCounterComponent : ModifiableFloatCounterComponent
    {
        [SerializeField] private float health;

        public override int Id => CounterIdentifierContainerMap.Health;

        public override float SetupValue => health;
    }
}