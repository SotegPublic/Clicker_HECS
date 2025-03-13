using HECSFramework.Unity;
using Helpers;
using System;
using UnityEngine;

[Serializable]
public class ItemModifierGenerationContext
{
    [SerializeField, IdentifierDropDown(nameof(CounterIdentifierContainer))] private int modifiableCounterID;
    [SerializeField] private int maxItemLevelForRule;
    [SerializeField] private float minModifierValue;
    [SerializeField] private float maxModifierValue;

    public int ModifiableCounterID => modifiableCounterID;
    public int MaxItemLevelForRule => maxItemLevelForRule;
    public float MinModifierValue => minModifierValue;
    public float MaxModifierValue => maxModifierValue;
}
