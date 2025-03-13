using HECSFramework.Core;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Stats, Doc.Player, "DefenceModifiableCounterComponent")]
    public sealed class DefenceModifiableCounterComponent : ModifiableFloatCounterComponent
    {
        [SerializeField] private float defence;

        public override int Id => CounterIdentifierContainerMap.Defence;

        public override float SetupValue => defence;
    }
}