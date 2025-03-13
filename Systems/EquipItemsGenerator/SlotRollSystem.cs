using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;
using Helpers;
using Cysharp.Threading.Tasks;

namespace Systems
{
	[Serializable][Documentation(Doc.Equipment, "this system roll new item slot and visualizes rolling process")]
    public sealed class SlotRollSystem : BaseSystem, IReactGlobalCommand<RollNewItemCommand>, IUpdatable
    {
        [Required]
        private EquipIdentifiersHolderComponent equipIdentifiersHolderComponent;
        [Required]
        private SlotRollSystemParametersComponent parameters;
        [Required]
        private GeneratorRulesHolderComponent generatorRulesHolderComponent;

        [Single]
        public UISystem UISystem;

        private bool isActive;
        private bool isItemGenerated;
        private bool isCompareWindowShowed;
        private CompareItemsWindowContext newItemContext;
        private CompareItemsWindowContext oldItemContext;
        private Entity chestEntity;
        private ChestItemSpriteMonoComponent chestItemSpriteMonoComponent;
        private float currentTime;

        public void CommandGlobalReact(RollNewItemCommand command)
        {
            var specialRewardHolder = Owner.World.GetSingleComponent<SpecialRewardHolderComponent>();
            if (specialRewardHolder.TryGetSpecialReward(out var reward))
            {
                var specialRewardEntity = reward.GetEntity(Owner.World).Init();

                specialRewardEntity.Command(new GetSpecialRewardCommand());
            }
            else
            {
                chestEntity = Owner.World.GetEntityBySingleComponent<ChestTagComponent>();
                chestItemSpriteMonoComponent = chestEntity.AsActor().GetComponent<ChestItemSpriteMonoComponent>();
                var slotID = equipIdentifiersHolderComponent.GetRandomSlotIdentifier();

                GenerateItem(command.PlayerLevel, slotID);
                isActive = true;
            }
        }


        public override void InitSystem()
        {
        }

        public void UpdateLocal()
        {
            if(isActive)
            {
                if(!isCompareWindowShowed && isItemGenerated && chestEntity.TryGetComponent<ReadyForShowingItemTagComponent>(out var component))
                {
                    currentTime += Time.deltaTime;
                    var progress = Math.Clamp(currentTime, 0, parameters.SlotRollTime) / parameters.SlotRollTime;
                    var currentSpriteScale = parameters.ScaleCurve.AnimationCurve.Evaluate(progress);
                    var currentSpritePositionModifier = parameters.TransformCurve.AnimationCurve.Evaluate(progress);

                    chestItemSpriteMonoComponent.SetScale(currentSpriteScale);
                    chestItemSpriteMonoComponent.SetPositionYModifier(currentSpritePositionModifier);
                    
                    if(currentTime > parameters.SlotRollTime)
                    {
                        ShowComparisonWindow();
                        chestEntity.Command(new TriggerAnimationCommand { Index = AnimParametersMap.Close });
                        chestEntity.RemoveComponent<ReadyForShowingItemTagComponent>();
                        chestEntity.RemoveComponent<RollingEquipItemTagComponent>();
                        isItemGenerated = false;
                        currentTime = 0;
                        isCompareWindowShowed = true;
                    }
                }

                if(isCompareWindowShowed && !chestEntity.TryGetComponent<RollAndComparingEquipItemTagComponent>(out var tag))
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

        private async void GenerateItem(int itemLevel, EquipItemSlotIdentifier slotID)
        {
            var generator = Owner.World.GetEntityBySingleComponent<EquipItemsGeneratorTagComponent>();
            generator.Command(new GenerateEquipItemCommand { ItemLevel = itemLevel, SlotID = slotID });

            var job = new WaitForDrawQueue(generator);
            var awaiter = job.RunJob();

            await awaiter;

            var itemParameters = Owner.World.GetEntityBySingleComponent<EquipItemsGeneratorTagComponent>()
                .GetComponent<NewItemHolderComponent>().NewEquipItem.GetComponent<EquipItemParametersComponent>();
            var itemSprite = itemParameters.Icon;
            var itemQuality = itemParameters.EquipItemQualityIdentifier;
            var itemColor = generatorRulesHolderComponent.GetQualityColor(itemQuality);

            chestItemSpriteMonoComponent.SetSprite(itemSprite, parameters.SlotSpriteSize);
            chestItemSpriteMonoComponent.SetZeroScale();
            chestItemSpriteMonoComponent.ShowSprite();
            chestItemSpriteMonoComponent.SetParticleColor(itemColor);
            chestItemSpriteMonoComponent.PlayParticleSystem();
            isItemGenerated = true;
        }


        private void ShowComparisonWindow()
        {
            Entity equiptedItem = null;
            bool isNoEquiptedItem = false;
            var newItem = Owner.World.GetEntityBySingleComponent<EquipItemsGeneratorTagComponent>().GetComponent<NewItemHolderComponent>().NewEquipItem;
            var slotID = newItem.GetComponent<EquipItemParametersComponent>().EquipItemSlotIdentifier;
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

            CreateAndFillItemCompareContext(newItem, ref newItemContext);

            UISystem.ShowUI(new ShowEquipItemsComparePopupCommand
            {
                SlotID = slotID,
                IsNoEquiptedItem = isNoEquiptedItem,
                NewItem = newItem,
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