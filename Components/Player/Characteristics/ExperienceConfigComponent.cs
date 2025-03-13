using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.Player, "here we hold experience config")]
    public sealed class ExperienceConfigComponent : BaseComponent
    {
        [SerializeField] private int maxLevels;
        [SerializeField] private int startExpValue;
        [SerializeField] private List<ExperienceModifierModel> experienceModifierModels = new List<ExperienceModifierModel>();

        public int StartExpValue => startExpValue;

        public float GetExpModifier(int level)
        {
            for(int i = experienceModifierModels.Count - 1; i >= 0; i--)
            {
                var modifierStartLevel = experienceModifierModels[i].Level;
                if (modifierStartLevel <= level)
                {
                    return experienceModifierModels[i].Modifier;
                }
            }

            throw new Exception("no modifiers on level: " + level.ToString());
        }
    }
}