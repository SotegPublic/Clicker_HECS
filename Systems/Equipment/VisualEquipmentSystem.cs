using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;

namespace Systems
{
	[Serializable][Documentation(Doc.Equipment, Doc.Visual, "this system is responsible for working with the visual part of the equipment")]
    public sealed class VisualEquipmentSystem : BaseSystem
    {
        [Required]
        private EquipItemsHolderComponent equipItemsHolderComponent;

        [Single]
        private PoolingSystem poolingSystem;

        public override void InitSystem()
        {
        }

        public void VisualEquipItem(Entity item, int slotID)
        {
            var sprite = item.GetComponent<EquipItemParametersComponent>().Icon;

            if (slotID == EquipItemSlotIdentifierMap.Head || slotID == EquipItemSlotIdentifierMap.Shield || slotID == EquipItemSlotIdentifierMap.Weapon
                || slotID == EquipItemSlotIdentifierMap.Shoulders)
            {
                SpawnItemView(item, slotID);
            }

            Owner.World.Command(new UpdateInventoryUISlotCommand { SlotID = slotID, Sprite = sprite });
        }

        private void SpawnItemView(Entity item, int slotID)
        {
            var playerCharacter = Owner.World.GetEntityBySingleComponent<PlayerCharacterTagComponent>();
            var constrantsHolder = playerCharacter.GetComponent<ConstrantsHolderComponent>();

            if (slotID == EquipItemSlotIdentifierMap.Shoulders)
            {
                var mainSlot = ConstrantIdentifierMap.RightShoulder;
                var secondSlot = ConstrantIdentifierMap.LeftShoulder;

                SpawnViewIntoConstrant(item, mainSlot, constrantsHolder);
                SpawnViewIntoConstrant(item, secondSlot, constrantsHolder);
            }
            else
            {
                SpawnViewIntoConstrant(item, slotID, constrantsHolder);
            }


        }

        private async void SpawnViewIntoConstrant(Entity item, int constrantSlotID, ConstrantsHolderComponent constrantsHolder)
        {
            if (constrantsHolder.TryGetConstrantTransform(constrantSlotID, out var constrantTransform))
            {
                var assetReference = item.GetComponent<EquipItemParametersComponent>().ViewReference;
                var itemView = await poolingSystem.GetViewFromPool(assetReference);

                if (constrantSlotID == ConstrantIdentifierMap.LeftShoulder)
                {
                    item.GetComponent<EquipItemParametersComponent>().SecondItemView = itemView;
                }
                else
                {
                    item.GetComponent<EquipItemParametersComponent>().MainItemView = itemView;
                }

                itemView.transform.position = constrantTransform.position;
                itemView.transform.rotation = constrantTransform.rotation;
                itemView.transform.SetParent(constrantTransform);
            }
        }
    }
}