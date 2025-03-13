using HECSFramework.Unity;
using Helpers;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(ModifierValuesRule), menuName = "CustomConfigs/ModifiersValuesRule", order = 8)]
public class ModifierValuesRule : ScriptableObject
{
    [SerializeField, IdentifierDropDown(nameof(ModifierIdentifier))] private int modifier;
    [SerializeField, IdentifierDropDown(nameof(CounterIdentifierContainer))] private int modifiableCounterID;
    [SerializeField] private float modifierPointPower;
    [SerializeField] private float baseValue;
    [SerializeField] private float perItemLevelBonus;
    [SerializeField] private AnimationCurve perItemLevelBonusModifier;
    [SerializeField][Range(0f, 1f)] private float mainSlotModifier = 0f;
    [SerializeField][Range(0f, 1f)] private float randomBonusPercent = 0f;
    [SerializeField] private QualityBonusForValueRule[] qualityBonusRules;

    public int Modifier => modifier;
    public float ModifierPointPower => modifierPointPower;
    public float BaseValue => baseValue;
    public float PerItemLevelBonus => perItemLevelBonus;
    public float MainSlotModifier => mainSlotModifier;
    public float RndPercentFromStep => randomBonusPercent;
    public QualityBonusForValueRule[] QualityBonusRules => qualityBonusRules;
    public int ModifiableCounterID => modifiableCounterID;
    public AnimationCurve PerItemLevelBonusModifier => perItemLevelBonusModifier;
}
