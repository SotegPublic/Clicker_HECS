using System;
using Commands;
using Components;
using HECSFramework.Core;
using UnityEngine;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Equipment, "this system generate equip item")]
    public sealed class EquipItemsGeneratorSystem : BaseSystem, IReactCommand<GenerateEquipItemCommand>
    {
        [Required]
        private EquipItemsGeneratorTagComponent equipItemsGeneratorTagComponent;
        [Required]
        private GeneratorRulesHolderComponent generatorRulesHolderComponent;
        [Required]
        private ItemViewsHolderComponent itemViewsHolderComponent;
        [Required]
        private NewItemHolderComponent newItemHolder;

        private int[] secondaryModifiersIDs = new int[16];
        private DefaultFloatModifier[] itemModifiers = new DefaultFloatModifier[16];

        public override void InitSystem()
        {
        }

        public void CommandReact(GenerateEquipItemCommand command)
        {
            Owner.GetOrAddComponent<VisualLocalLockComponent>().AddLock();

            var slotID = command.SlotID;
            var qualityID = generatorRulesHolderComponent.GetItemQuality(command.ItemLevel);
            var secondaryModifiersCount = generatorRulesHolderComponent.GetSecondaryModifiers(slotID, qualityID, ref secondaryModifiersIDs);
            var itemViewConfig = itemViewsHolderComponent.GetViewConfig(slotID, qualityID, command.ItemLevel);
            var mainModifierID = generatorRulesHolderComponent.GetMainModifier(slotID);
            var mainModifierValuesRule = generatorRulesHolderComponent.GetModifierValuesRule(mainModifierID);

            var itemPower = 0f;

            var mainModifier = GetRuntimeModifier(command.ItemLevel, mainModifierID, mainModifierValuesRule, qualityID, true);
            itemPower += mainModifier.GetValue * mainModifierValuesRule.ModifierPointPower;

            for (int i = 0; i < secondaryModifiersCount; i++)
            {
                var valueRule = generatorRulesHolderComponent.GetModifierValuesRule(secondaryModifiersIDs[i]);
                itemModifiers[i] = GetRuntimeModifier(command.ItemLevel, secondaryModifiersIDs[i], valueRule, qualityID, false);
                itemPower += itemModifiers[i].GetValue * valueRule.ModifierPointPower;
            }

            var itemEntity = CreateEquipItemEntity(slotID, qualityID, command.ItemLevel, secondaryModifiersCount, mainModifier, itemViewConfig, itemPower);
            newItemHolder.NewEquipItem = itemEntity;
            
            Owner.GetOrAddComponent<VisualLocalLockComponent>().Remove();
        }

        private Entity CreateEquipItemEntity(EquipItemSlotIdentifier slotID, EquipItemQualityIdentifier qualityID, int itemLevel, int secondaryModifiersCount,
            DefaultFloatModifier mainModifier, ItemViewConfig itemViewConfig, float itemPower)
        {
            var itemEntity = Entity.Get("TestItem " + slotID.name); // todo - create name generator
            var modifiersHolder = itemEntity.GetOrAddComponent<ModifiersHolderComponent>();

            modifiersHolder.SetMainModifier(mainModifier);

            for (int i = 0; i < secondaryModifiersCount; i++)
            {
                modifiersHolder.SecondaryModifiers.Add(itemModifiers[i]);
            }

            var parametersComponent = itemEntity.GetOrAddComponent<EquipItemParametersComponent>();
            parametersComponent.ItemPower = (int)itemPower;
            parametersComponent.EquipItemQualityIdentifier = qualityID;
            parametersComponent.ItemLevel = itemLevel;
            parametersComponent.EquipItemSlotIdentifier = slotID;
            parametersComponent.Icon = itemViewConfig.Icon;
            parametersComponent.ViewReference = itemViewConfig.ViewAssetReference;
            parametersComponent.ItemName = itemEntity.ID;
            parametersComponent.GoldCost = generatorRulesHolderComponent.GetGoldCost(itemLevel);
            parametersComponent.ExpCost = generatorRulesHolderComponent.GetExpCost(itemLevel);

            return itemEntity;
        }

        public DefaultFloatModifier GetRuntimeModifier(int itemLevel, int modifierID, ModifierValuesRule valueRule, EquipItemQualityIdentifier qualityID, bool isMain)
        {
            float qualityModifier = 0f;
            float mainModifier = isMain ? valueRule.MainSlotModifier : 0f;
            float randomBonus = UnityEngine.Random.Range(0, (valueRule.PerItemLevelBonus * (valueRule.PerItemLevelBonusModifier.Evaluate(itemLevel) * 0.01f)) * valueRule.RndPercentFromStep);
            
            float itemLevelBonus = 0;

            for(int i = 0; i < itemLevel; i++)
            {
                itemLevelBonus += valueRule.PerItemLevelBonus * (valueRule.PerItemLevelBonusModifier.Evaluate(itemLevel) * 0.01f);
            }

            for (int i = 0; i < valueRule.QualityBonusRules.Length; i++)
            {
                if (valueRule.QualityBonusRules[i].QualityID == qualityID)
                {
                    qualityModifier = valueRule.QualityBonusRules[i].QualityBonusPercent;
                }
            }

            var value = ((valueRule.BaseValue + itemLevelBonus) * (1 + mainModifier) * (1 + qualityModifier)) + randomBonus;

            return new DefaultFloatModifier
            {
                GetCalculationType = ModifierCalculationType.Add,
                GetModifierType = ModifierValueType.Value,
                GetValue = value,
                ID = valueRule.ModifiableCounterID,
                ModifierGuid = Guid.NewGuid(),
                ModifierID = modifierID,
            };
        }
    }
}