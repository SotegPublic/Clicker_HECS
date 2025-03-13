using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.Player, Doc.Stats, "here we hold crit chance and damage calculation parameters")]
    public sealed class CritCalculationComponent : BaseComponent
    {
        [SerializeField] private float critChanceRatingCap;
        [SerializeField] private float critDamageRatingCap;

        public float CritChanceRatingCap => critChanceRatingCap;
        public float CritDamageRatingCap => critDamageRatingCap;
    }
}