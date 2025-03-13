using Commands;
using HECSFramework.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Systems;

namespace Components
{
    [Serializable][Documentation(Doc.Equipment, "here we hold player equip items")]
    public sealed class EquipItemsHolderComponent : BaseComponent
    {
        private Dictionary<int, Entity> equipItems = new Dictionary<int, Entity>();

        public Dictionary<int, Entity> EquipItems => equipItems;

        public bool TryGetEquiptedItem(int slotID, out Entity equipItem)
        {
            if(equipItems.ContainsKey(slotID))
            {
                equipItem = equipItems[slotID];
                return true;
            }
          
            equipItem = null;
            return false;
        }
    }
}