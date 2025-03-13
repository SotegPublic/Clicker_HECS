using HECSFramework.Core;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Stats, Doc.Player, "SkillPowerModifiableCounterComponent")]
    public sealed class SkillPowerModifiableCounterComponent : ModifiableFloatCounterComponent
    {
        [SerializeField] private float skillPower = 1;

        public override int Id => CounterIdentifierContainerMap.SkillPower;

        public override float SetupValue => skillPower;
    }
}