using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;

namespace Systems
{
	[Serializable][Documentation(Doc.Rewards, "this system create special reward item from config")]
    public sealed class CreateSpecialRewardItemSystem : BaseSystem
    {
        [Required]
        private SpecialItemRewardConfigComponent itemConfig;

        public override void InitSystem()
        {
        }

        public Entity GetItemEntity()
        {
            var itemRulesHolder = Owner.World.GetEntityBySingleComponent<EquipItemsGeneratorTagComponent>().GetComponent<GeneratorRulesHolderComponent>();
            
            var itemEntity = Entity.Get(itemConfig.ItemName);
            var modifiersHolder = itemEntity.GetOrAddComponent<ModifiersHolderComponent>();
            var itemPower = 0f;

            var mainModifierValuesRule = itemRulesHolder.GetModifierValuesRule(itemConfig.MainModifierConfig.ModifierID);
            var mainModifier = GetRuntimeModifier(itemConfig.MainModifierConfig);
            modifiersHolder.SetMainModifier(mainModifier);
            itemPower += mainModifier.GetValue * mainModifierValuesRule.ModifierPointPower;

            for (int i = 0; i < itemConfig.SecondaryModifiersConfigs.Length; i++)
            {
                var valueRule = itemRulesHolder.GetModifierValuesRule(itemConfig.SecondaryModifiersConfigs[i].ModifierID);
                var secondariModifier = GetRuntimeModifier(itemConfig.SecondaryModifiersConfigs[i]);
                modifiersHolder.SecondaryModifiers.Add(secondariModifier);
                itemPower += secondariModifier.GetValue * valueRule.ModifierPointPower;
            }

            var parametersComponent = itemEntity.GetOrAddComponent<EquipItemParametersComponent>();
            parametersComponent.ItemPower = (int)itemPower;
            parametersComponent.EquipItemQualityIdentifier = itemConfig.QualityID;
            parametersComponent.ItemLevel = itemConfig.ItemLevel;
            parametersComponent.EquipItemSlotIdentifier = itemConfig.SlotID;
            parametersComponent.Icon = itemConfig.ItemIcon;
            parametersComponent.ViewReference = itemConfig.ItemView;
            parametersComponent.ItemName = itemEntity.ID;
            parametersComponent.GoldCost = itemConfig.GoldCost;
            parametersComponent.ExpCost = itemConfig.ExpCost;

            return itemEntity;
        }

        public DefaultFloatModifier GetRuntimeModifier(ItemModifierConfig config)
        {
            return new DefaultFloatModifier
            {
                GetCalculationType = ModifierCalculationType.Add,
                GetModifierType = ModifierValueType.Value,
                GetValue = config.Value,
                ID = config.ModifierID,
                ModifierGuid = Guid.NewGuid(),
                ModifierID = config.ModifierID,
            };
        }
    }
}