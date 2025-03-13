using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.Stats, Doc.Holder, Doc.Visual, "here we hold the amount of experience to move to the next level")]
    public sealed class ExperienceToNextLevelCounter : SimpleFloatCounterBaseComponent
    {
        [SerializeField] private float expToNextLevelCount;

        public override float Value { get => expToNextLevelCount; protected set => expToNextLevelCount = value; }

        public override int Id => CounterIdentifierContainerMap.ExperienceToNextLevel;
    }
}