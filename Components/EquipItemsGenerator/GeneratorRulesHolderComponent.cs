using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Inventory, Doc.Item, "in this component we hold rules for equip item generation")]
    public sealed class GeneratorRulesHolderComponent : BaseComponent
    {
        [SerializeField] private SlotModifiersRule[] slotModifiersRules;
        [SerializeField] private ModifierValuesRule[] modifierValuesRules;
        [SerializeField] private QualitiesByItemLevelRule[] qualityRules;
        [SerializeField] private QualityColorRule[] qualityColorsRules;
        [SerializeField] private ModifiersCountRules modifiersCountRules;
        [SerializeField] private ExperianseAndGoldCostConfig costConfig;

        public EquipItemQualityIdentifier GetItemQuality(int itemLevel)
        {
            for (int i = 0; i < qualityRules.Length; i++)
            {
                var isLevelItemLowerThanMax = i == 0 ? itemLevel <= qualityRules[i].MaxItemLevelValue :
                    itemLevel > qualityRules[i - 1].MaxItemLevelValue &&
                    itemLevel <= qualityRules[i].MaxItemLevelValue;

                if (isLevelItemLowerThanMax)
                {
                    return qualityRules[i].QualitiesSet[UnityEngine.Random.Range(0, qualityRules[i].QualitiesSet.Length)];
                }
            }

            return null;
        }

        public int GetMainModifier(int slotID)
        {
            for (int i = 0; i < slotModifiersRules.Length; i++)
            {
                if (slotModifiersRules[i].SlotID == slotID)
                {
                    return slotModifiersRules[i].MainSlotModifier;
                }
            }
            return 0;
        }

        public int GetSecondaryModifiers(int slotID, int qualityID, ref int[] secondaryModifiers)
        {
            var modifiersCount = modifiersCountRules.GetModifiersCount(qualityID) - 1;

            for (int i = 0; i < slotModifiersRules.Length; i++)
            {
                if (slotModifiersRules[i].SlotID == slotID)
                {
                    var random = new System.Random();
                    var secondaryModifiersCount = Math.Min(slotModifiersRules[i].SecondaryModifiers.Length, modifiersCount);

                    for (int j = 0; j < secondaryModifiersCount; j++)
                    {
                        var rndIndex = random.Next(0, slotModifiersRules[i].SecondaryModifiers.Length);
                        secondaryModifiers[j] = slotModifiersRules[i].SecondaryModifiers[rndIndex];
                    }

                    return secondaryModifiersCount;
                }
            }

            return 0;
        }

        public ModifierValuesRule GetModifierValuesRule(int modifierID)
        {
            for (int i = 0; i < modifierValuesRules.Length; i++)
            {
                if (modifierValuesRules[i].Modifier == modifierID)
                {
                    return modifierValuesRules[i];
                }
            }

            return null;
        }

        public int GetGoldCost(float itemLevel)
        {
            return (int)(itemLevel * costConfig.GoldCostModifierPerLevel);
        }

        public int GetExpCost(float itemLevel)
        {
            return (int)(itemLevel * costConfig.ExpCostModifierPerLevel);
        }

        public Color GetQualityColor(int qualityID)
        {
            for(int i = 0; i < qualityColorsRules.Length; i++)
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