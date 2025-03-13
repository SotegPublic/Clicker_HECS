using HECSFramework.Unity;
using Helpers;
using System;
using UnityEngine;

[Serializable]
public class ModifierRule
{
    [SerializeField, IdentifierDropDown(nameof(ModifierIdentifier))] private int modifierIdentifier;

    [SerializeField] private ItemModifierGenerationContext[] itemModifierGenerationContexts;

    public int ModifierIdentifier => modifierIdentifier;
    public ItemModifierGenerationContext[] ItemModifierGenerationContexts => itemModifierGenerationContexts;
}
