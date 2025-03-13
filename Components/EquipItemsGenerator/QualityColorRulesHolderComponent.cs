using HECSFramework.Core;
using System;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.Holder, Doc.Visual, "here we hold quality color rules")]
    public sealed class QualityColorRulesHolderComponent : BaseComponent
    {
        [SerializeField] private QualityColorRule[] qualityColorsRules;

        public Color GetQualityColor(int qualityID)
        {
            for (int i = 0; i < qualityColorsRules.Length; i++)
            {
                if (qualityColorsRules[i].QualityID == qualityID)
                {
                    return qualityColorsRules[i].QualityColor;
                }
            }

            return Color.white;
        }
    }
}