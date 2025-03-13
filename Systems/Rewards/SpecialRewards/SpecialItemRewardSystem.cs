using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;
using Cysharp.Threading.Tasks;
using Helpers;

namespace Systems
{
	[Serializable][Documentation(Doc.Rewards, "This system is responsible for the creation and issuance of equip item special reward.")]
    public sealed class SpecialItemRewardSystem : BaseSystem, IReactCommand<GetSpecialRewardCommand>, IUpdatable
    {
        [Required]
        private SlotRollSystemParametersComponent parameters;

        [Single]
        public UISystem UISystem;

        private bool isActive;
        private bool isCompareWindowShowed;
        private CompareItemsWindowContext newItemContext;
        private CompareItemsWindowContext oldItemContext;
        private Entity chestEntity;
        private ChestItemSpriteMonoComponent chestItemSpriteMonoComponent;
        private float currentTime;
        private Entity specialItem;

        public override void InitSystem()
        {
        }

        public void CommandReact(GetSpecialRewardCommand command)
        {
            chestEntity = Owner.World.GetEntityBySingleComponent<ChestTagComponent>();
            chestItemSpriteMonoComponent = chestEntity.AsActor().GetComponent<ChestItemSpriteMonoComponent>();
            specialItem = Owner.GetSystem<CreateSpecialRewardItemSystem>().GetItemEntity();

            SetSpriteParameters(specialItem);

            isActive = true;
        }

        private void SetSpriteParameters(Entity specialItem)
        {
            var itemRulesHolder = Owner.World.GetEntityBySingleComponent<EquipItemsGeneratorTagComponent>().GetComponent<GeneratorRulesHolderComponent>();
            var itemParameters = specialItem.GetComponent<EquipItemParametersComponent>();
            var itemSprite = itemParameters.Icon;
            var itemQuality = itemParameters.EquipItemQualityIdentifier;
            var itemColor = itemRulesHolder.GetQualityColor(itemQuality);

            chestItemSpriteMonoComponent.SetSprite(itemSprite, parameters.SlotSpriteSize);
            chestItemSpriteMonoComponent.SetZeroScale();
            chestItemSpriteMonoComponent.ShowSprite();
            chestItemSpriteMonoComponent.SetParticleColor(itemColor);
            chestItemSpriteMonoComponent.PlayParticleSystem();
        }

        public void UpdateLocal()
        {
            if (isActive)
            {
                if (!isCompareWindowShowed && chestEntity.TryGetComponent<ReadyForShowingItemTagComponent>(out var component))
                {
                    currentTime += Time.deltaTime;
                    var progress = Math.Clamp(currentTime, 0, parameters.SlotRollTime) / parameters.SlotRollTime;
                    var currentSpriteScale = parameters.ScaleCurve.AnimationCurve.Evaluate(progress);
                    var currentSpritePositionModifier = parameters.TransformCurve.AnimationCurve.Evaluate(progress);

                    chestItemSpriteMonoComponent.SetScale(currentSpriteScale);
                    chestItemSpriteMonoComponent.SetPositionYModifier(currentSpritePositionModifier);

                    if (currentTime > parameters.SlotRollTime)
                    {
                        ShowComparisonWindow();
                        chestEntity.Command(new TriggerAnimationCommand { Index = AnimParametersMap.Close });
                        chestEntity.RemoveComponent<ReadyForShowingItemTagComponent>();
                        chestEntity.RemoveComponent<RollingEquipItemTagComponent>();
                        currentTime = 0;
                        isCompareWindowShowed = true;
                    }
                }

                if (isCompareWindowShowed && !chestEntity.TryGetComponent<RollAndComparingEquipItemTagComponent>(out var tag))
                {
                    HideRolledSlotSprite();
                    isActive = false;
                    isCompareWindowShowed = false;
                }
            }
        }

        private void HideRolledSlotSprite()
        {
            chestItemSpriteMonoComponent.HideSprite();
            chestItemSpriteMonoComponent.SetSprite(null, parameters.SlotSpriteSize);
            chestItemSpriteMonoComponent.SetZeroScale();
            chestItemSpriteMonoComponent.SetParticleColor(Color.white);
            chestItemSpriteMonoComponent.StopParticleSystem();
        }

        private void ShowComparisonWindow()
        {
            Entity equiptedItem = null;
            bool isNoEquiptedItem = false;
            var slotID = specialItem.GetComponent<EquipItemParametersComponent>().EquipItemSlotIdentifier;
            var inventory = Owner.World.GetEntityBySingleComponent<PlayerTagComponent>().GetComponent<EquipItemsHolderComponent>();

            if (inventory.TryGetEquiptedItem(slotID, out var item))
            {
                equiptedItem = item;

                CreateAndFillItemCompareContext(equiptedItem, ref oldItemContext);
            }
            else
            {
                isNoEquiptedItem = true;
            }

            CreateAndFillItemCompareContext(specialItem, ref newItemContext);

            UISystem.ShowUI(new ShowEquipItemsComparePopupCommand
            {
                SlotID = slotID,
                IsNoEquiptedItem = isNoEquiptedItem,
                NewItem = specialItem,
                NewItemContext = newItemContext,
                EquiptedItem = equiptedItem,
                OldItemContext = oldItemContext
            }, UIIdentifierMap.ItemComparisonWindow_UIIdentifier).Forget();
        }

        private void CreateAndFillItemCompareContext(Entity itemEntity, ref CompareItemsWindowContext ItemContext)
        {
            var itemModifiersCount = itemEntity.GetComponent<ModifiersHolderComponent>().SecondaryModifiers.Count + 1;
            var itemParameters = itemEntity.GetComponent<EquipItemParametersComponent>();

            ItemContext = new CompareItemsWindowContext
            {
                ItemName = itemParameters.ItemName,
                ItemsModifiers = HECSPooledArray<DefaultFloatModifier>.GetArray(itemModifiersCount).Items,
                ModifiersCount = itemModifiersCount,
                ItemIcon = itemParameters.Icon,
                QualityName = IdentifierToStringMap.IntToString[itemParameters.EquipItemQualityIdentifier],
                QualityID = itemParameters.EquipItemQualityIdentifier,
                ItemPower = itemParameters.ItemPower
            };

            ItemContext.ItemsModifiers[0] = itemEntity.GetComponent<ModifiersHolderComponent>().MainModifier;

            for (int i = 1; i < itemModifiersCount; i++)
            {
                ItemContext.ItemsModifiers[i] = itemEntity.GetComponent<ModifiersHolderComponent>().SecondaryModifiers[i - 1];
            }

        }
    }
}