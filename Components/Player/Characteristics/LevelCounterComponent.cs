using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Player, "LevelCounterComponent")]
    public sealed class LevelCounterComponent : SimpleIntCounterBaseComponent
    {
        [SerializeField] private int level;

        public override int Value { get => level; protected set => level = value; }

        public override int Id => CounterIdentifierContainerMap.Level;
    }
}