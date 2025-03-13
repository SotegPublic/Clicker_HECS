using HECSFramework.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.Modifiers, Doc.Holder, "here we hold modifiers")]
    public sealed class ModifiersHolderComponent : BaseComponent
    {
        [SerializeField] private DefaultFloatModifier mainModifier;
        [SerializeField] private List<DefaultFloatModifier> secondaryModifiers = new List<DefaultFloatModifier>(8);

        public DefaultFloatModifier MainModifier => mainModifier;
        public List<DefaultFloatModifier> SecondaryModifiers => secondaryModifiers;

        public void SetMainModifier(DefaultFloatModifier modifier)
        {
            mainModifier = modifier;
        }

        public void ClearModifiers()
        {
            secondaryModifiers.Clear();
            mainModifier = null;
        }
    }
}