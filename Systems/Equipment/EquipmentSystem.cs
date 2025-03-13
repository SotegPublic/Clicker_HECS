using System;
using HECSFramework.Core;
using Components;
using Commands;

namespace Systems
{
	[Serializable][Documentation(Doc.Equipment, "this system is responsible for working with the logic part of the equipment")]
    public sealed class EquipmentSystem : BaseSystem 
    {
        [Required]
        private EquipItemsHolderComponent equipItemsHolderComponent;
        [Required]
        private CountersHolderComponent countersHolderComponent;

        [Single]
        private VisualEquipmentSystem visualEquipmentSystem;

        public override void InitSystem()
        {
        }

        public void EquipItem(Entity newItem, int slotID)
        {
            var inventory = Owner.World.GetEntityBySingleComponent<PlayerTagComponent>().GetComponent<EquipItemsHolderComponent>();
            var itemPowerCounter = countersHolderComponent.GetCounter<ICounter<int>>(CounterIdentifierContainerMap.ItemPower);

            if (inventory.TryGetEquiptedItem(slotID, out var oldItem))
            {
                var modifiers = oldItem.GetComponent<ModifiersHolderComponent>();

                var mainItemCounter = countersHolderComponent.GetCounter<ModifiableFloatCounterComponent>(modifiers.MainModifier.ModifierID);
                mainItemCounter.RemoveModifier(modifiers.MainModifier.ModifierGuid);

                for (int i = 0; i < modifiers.SecondaryModifiers.Count; i++)
                {
                    var counter = countersHolderComponent.GetCounter<ModifiableFloatCounterComponent>(modifiers.SecondaryModifiers[i].ModifierID);
                    counter.RemoveModifier(modifiers.SecondaryModifiers[i].ModifierGuid);
                }

                itemPowerCounter.ChangeValue(-oldItem.GetComponent<EquipItemParametersComponent>().ItemPower);

                Owner.World.Command(new DestroyEquipItemCommand { ItemEntity = oldItem, IsEquipted = true });
            }


            if (equipItemsHolderComponent.EquipItems.ContainsKey(slotID))
            {
                equipItemsHolderComponent.EquipItems[slotID] = newItem;
            }
            else
            {
                equipItemsHolderComponent.EquipItems.Add(slotID, newItem);
            }

            var newModifiers = newItem.GetComponent<ModifiersHolderComponent>();

            var mainNewItemCounter = countersHolderComponent.GetCounter<ModifiableFloatCounterComponent>(newModifiers.MainModifier.ModifierID);
            mainNewItemCounter.AddModifier(newModifiers.MainModifier.ModifierGuid, newModifiers.MainModifier);

            for (int i = 0; i < newModifiers.SecondaryModifiers.Count; i++)
            {
                var counter = countersHolderComponent.GetCounter<ModifiableFloatCounterComponent>(newModifiers.SecondaryModifiers[i].ModifierID);
                counter.AddModifier(newModifiers.SecondaryModifiers[i].ModifierGuid, newModifiers.SecondaryModifiers[i]);
            }

            itemPowerCounter.ChangeValue(newItem.GetComponent<EquipItemParametersComponent>().ItemPower);

            visualEquipmentSystem.VisualEquipItem(newItem, slotID);
        }
    }
}