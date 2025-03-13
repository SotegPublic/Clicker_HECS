using UnityEngine;

namespace Components
{
    [CreateAssetMenu(fileName = nameof(BootsItemViewConfig), menuName = "CustomConfigs/Items/BootsItemViewConfig", order = 0)]
    public class BootsItemViewConfig : ItemViewConfig
    {
        public BootsItemViewConfig()
        {
            slotID = EquipItemSlotIdentifierMap.Boots;
        }
    }

}