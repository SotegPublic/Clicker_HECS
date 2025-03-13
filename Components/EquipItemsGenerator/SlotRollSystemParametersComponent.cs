using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.Variables, "here we set parameters for slot roll system")]
    public sealed class SlotRollSystemParametersComponent : BaseComponent
    {
        [SerializeField] private float slotRollTime;
        [SerializeField] private float slotSpriteSize;
        [SerializeField] private AnimationCurveComponent scaleCurve;
        [SerializeField] private AnimationCurveComponent transformCurve;

        public float SlotRollTime => slotRollTime;
        public float SlotSpriteSize => slotSpriteSize;
        public AnimationCurveComponent ScaleCurve => scaleCurve;
        public AnimationCurveComponent TransformCurve => transformCurve;
    }
}