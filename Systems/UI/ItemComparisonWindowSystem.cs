using System;
using System.Buffers;
using Commands;
using Components;
using HECSFramework.Core;
using Helpers;
using UnityEngine;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.UI, "this system operates item comparison ui")]
    public sealed class ItemComparisonWindowSystem : BaseSystem, IAfterEntityInit, IReactCommand<ShowEquipItemsComparePopupCommand>, IReactCommand<AfterCommand<HideUICommand>>
    {
        [Required]
        public UIAccessProviderComponent UIAccessProviderComponent;

        [Required]
        public UIAccessPrfbProviderComponent prefabProvider;

        [Required]
        public CompareWindowSpriteHolderComponent CompareWindowSpriteHolderComponent;

        [Required]
        private QualityColorRulesHolderComponent qualityColorRulesHolderComponent;

        [Single]
        private EquipmentSystem equipmentSystem;
        [Single]
        private CritCalculationSystem critCalculationSystem;

        private HECSSyncPool<UIAccessMonoComponent> modifiersRows;
        private int slotID;
        private Entity newItem;

        public void AfterEntityInit()
        {
            modifiersRows = new HECSSyncPool<UIAccessMonoComponent>(prefabProvider.Get.GetPrefab(UIAccessIdentifierMap.EquipmentModifier));

            var acceptButton = UIAccessProviderComponent.Get.GetButton(UIAccessIdentifierMap.AcceptButton);
            acceptButton.onClick.AddListener(EquipItem);

            var declineButton = UIAccessProviderComponent.Get.GetButton(UIAccessIdentifierMap.CancelButton);
            declineButton.onClick.AddListener(DestroyNewItem);
        }

        public override void InitSystem()
        {
        }

        public void CommandReact(AfterCommand<HideUICommand> command)
        {
            modifiersRows.ReleaseAll();
            UIAccessProviderComponent.Get.GetUIAccess(UIAccessIdentifierMap.OldEquipmentCompare).gameObject.SetActive(true);
        }

        public void CommandReact(ShowEquipItemsComparePopupCommand command)
        {
            slotID = command.SlotID;
            newItem = command.NewItem;

            DrawItems(ref command.NewItemContext, ref command.OldItemContext, command.IsNoEquiptedItem);

            ArrayPool<DefaultFloatModifier>.Shared.Return(command.NewItemContext.ItemsModifiers);

            if(!command.IsNoEquiptedItem)
            {
                ArrayPool<DefaultFloatModifier>.Shared.Return(command.OldItemContext.ItemsModifiers);
            }
        }

        private void DrawItems(ref CompareItemsWindowContext newItemContext, ref CompareItemsWindowContext oldItemContext, bool isNoEquiptedItem)
        {
            var newItemPanel = UIAccessProviderComponent.Get.GetUIAccess(UIAccessIdentifierMap.NewEquipmentCompare);
            newItemPanel.GetImage(UIAccessIdentifierMap.Icon).sprite = newItemContext.ItemIcon;
            newItemPanel.GetTextMeshProUGUI(UIAccessIdentifierMap.Name).text = $"[{newItemContext.QualityName}] " + newItemContext.ItemName;
            newItemPanel.GetTextMeshProUGUI(UIAccessIdentifierMap.Name).color = qualityColorRulesHolderComponent.GetQualityColor(newItemContext.QualityID);
            newItemPanel.GetTextMeshProUGUI(UIAccessIdentifierMap.PowerText).text = newItemContext.ItemPower.ToString();

            if (isNoEquiptedItem)
            {
                UIAccessProviderComponent.Get.GetUIAccess(UIAccessIdentifierMap.OldEquipmentCompare).gameObject.SetActive(false);
                newItemPanel.GetImage(UIAccessIdentifierMap.CompareImage).sprite = CompareWindowSpriteHolderComponent.NoArrowSprite;
                DrawModifiers(ref newItemContext, newItemPanel, true);

            }
            else
            {
                var oldItemPanel = UIAccessProviderComponent.Get.GetUIAccess(UIAccessIdentifierMap.OldEquipmentCompare);
                oldItemPanel.GetImage(UIAccessIdentifierMap.Icon).sprite = oldItemContext.ItemIcon;
                oldItemPanel.GetTextMeshProUGUI(UIAccessIdentifierMap.Name).text = $"[{oldItemContext.QualityName}] " + oldItemContext.ItemName;
                oldItemPanel.GetTextMeshProUGUI(UIAccessIdentifierMap.Name).color = qualityColorRulesHolderComponent.GetQualityColor(oldItemContext.QualityID);
                oldItemPanel.GetTextMeshProUGUI(UIAccessIdentifierMap.PowerText).text = oldItemContext.ItemPower.ToString();
                oldItemPanel.GetImage(UIAccessIdentifierMap.CompareImage).sprite = CompareWindowSpriteHolderComponent.NoArrowSprite;
                DrawModifiers(ref oldItemContext, oldItemPanel, false);
                DrawModifiers(ref newItemContext, newItemPanel, true, oldItemContext.ItemsModifiers, oldItemContext.ModifiersCount, false);

                newItemPanel.GetImage(UIAccessIdentifierMap.CompareImage).sprite = oldItemContext.ItemPower > newItemContext.ItemPower ?
                    CompareWindowSpriteHolderComponent.ArrowDownSprite : CompareWindowSpriteHolderComponent.ArrowUpSprite;

            }
        }

        private void DrawModifiers(ref CompareItemsWindowContext context, UIAccessMonoComponent itemPanel, bool isNewItem,
            DefaultFloatModifier[] oldItemModifiers = null, int oldItemModifiersCount = 0, bool isNoEquiptedItem = true)
        {
            var modifiersRoot = itemPanel.GetRectTransform(UIAccessIdentifierMap.ModifiersGroup);

            for (int i = context.ModifiersCount - 1; i >= 0; i--)
            {
                var newModPanel = modifiersRows.Get(modifiersRoot);
                newModPanel.transform.localScale = Vector3.one;

                if (i == 0)
                {
                    newModPanel.GetTextMeshProUGUI(UIAccessIdentifierMap.Name).text = "<b><u>" + IdentifierToStringMap.IntToString[context.ItemsModifiers[i].ModifierID] + "</u></b>";
                    if(context.ItemsModifiers[i].ModifierID == ModifierIdentifierMap.CritChance || context.ItemsModifiers[i].ModifierID == ModifierIdentifierMap.CritDamage)
                    {
                        var percent = 0f;

                        switch (context.ItemsModifiers[i].ModifierID)
                        {
                            case ModifierIdentifierMap.CritChance:
                                percent = critCalculationSystem.GetCritChancePercent(context.ItemsModifiers[i].GetValue);
                                break;
                            case ModifierIdentifierMap.CritDamage:
                                percent = critCalculationSystem.GetCritDamagePercent(context.ItemsModifiers[i].GetValue);
                                break;
                            default:
                                break;
                        }

                        newModPanel.GetTextMeshProUGUI(UIAccessIdentifierMap.Value).text = "<b><u>" + Math.Round(percent, 2).ToString() + "%</u></b>";
                    }
                    else
                    {
                        newModPanel.GetTextMeshProUGUI(UIAccessIdentifierMap.Value).text = "<b><u>" + Math.Round(context.ItemsModifiers[i].GetValue, 1).ToString() + "</u></b>";
                    }
                    
                }
                else
                {
                    newModPanel.GetTextMeshProUGUI(UIAccessIdentifierMap.Name).text = IdentifierToStringMap.IntToString[context.ItemsModifiers[i].ModifierID];
                    if(context.ItemsModifiers[i].ModifierID == ModifierIdentifierMap.CritChance || context.ItemsModifiers[i].ModifierID == ModifierIdentifierMap.CritDamage)
                    {
                        var percent = 0f;

                        switch (context.ItemsModifiers[i].ModifierID)
                        {
                            case ModifierIdentifierMap.CritChance:
                                percent = critCalculationSystem.GetCritChancePercent(context.ItemsModifiers[i].GetValue);
                                break;
                            case ModifierIdentifierMap.CritDamage:
                                percent = critCalculationSystem.GetCritDamagePercent(context.ItemsModifiers[i].GetValue);
                                break;
                            default:
                                break;
                        }

                        newModPanel.GetTextMeshProUGUI(UIAccessIdentifierMap.Value).text = Math.Round(percent, 2).ToString() + "%";
                    }
                    else
                    {
                        newModPanel.GetTextMeshProUGUI(UIAccessIdentifierMap.Value).text = Math.Round(context.ItemsModifiers[i].GetValue, 1).ToString();
                    }
                }

                newModPanel.GetImage(UIAccessIdentifierMap.CompareImage).sprite = CompareWindowSpriteHolderComponent.NoArrowSprite;

                if (isNewItem && !isNoEquiptedItem)
                {
                    for (int j = 0; j < oldItemModifiersCount; j++)
                    {
                        if (oldItemModifiers[j].ModifierID != context.ItemsModifiers[i].ModifierID) continue;

                        if (oldItemModifiers[j].GetValue != context.ItemsModifiers[i].GetValue)
                        {
                            newModPanel.GetImage(UIAccessIdentifierMap.CompareImage).sprite = oldItemModifiers[j].GetValue > context.ItemsModifiers[i].GetValue ?
                                CompareWindowSpriteHolderComponent.ArrowDownSprite : CompareWindowSpriteHolderComponent.ArrowUpSprite;
                        }
                    }
                }

                newModPanel.transform.SetAsFirstSibling();
            }
        }

        private void EquipItem()
        {
            equipmentSystem.EquipItem(newItem, slotID);
            Owner.World.Command(new HideUICommand { UIViewType = UIIdentifierMap.ItemComparisonWindow_UIIdentifier });

            var chestEntity = Owner.World.GetEntityBySingleComponent<ChestTagComponent>();
            chestEntity.RemoveComponent<RollAndComparingEquipItemTagComponent>();
        }

        private void DestroyNewItem()
        {
            Owner.World.Command(new DestroyEquipItemCommand { ItemEntity = newItem, IsEquipted = false });
            Owner.World.Command(new HideUICommand { UIViewType = UIIdentifierMap.ItemComparisonWindow_UIIdentifier });

            var chestEntity = Owner.World.GetEntityBySingleComponent<ChestTagComponent>();
            chestEntity.RemoveComponent<RollAndComparingEquipItemTagComponent>();
        }

        public override void BeforeDispose()
        {
            base.BeforeDispose();

            var acceptButton = UIAccessProviderComponent.Get.GetButton(UIAccessIdentifierMap.AcceptButton);
            acceptButton.onClick.RemoveListener(EquipItem);

            var declineButton = UIAccessProviderComponent.Get.GetButton(UIAccessIdentifierMap.CancelButton);
            declineButton.onClick.RemoveListener(DestroyNewItem);

        }

    }
}