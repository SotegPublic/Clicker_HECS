using UnityEngine;

namespace Components
{
    [CreateAssetMenu(fileName = nameof(ShieldItemViewConfig), menuName = "CustomConfigs/Items/ShieldItemViewConfig", order = 0)]
    public class ShieldItemViewConfig : ItemViewConfig
    {
        public ShieldItemViewConfig()
        {
            slotID = EquipItemSlotIdentifierMap.Shield;
        }
    }

}