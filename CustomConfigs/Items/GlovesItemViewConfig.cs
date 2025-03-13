using UnityEngine;

namespace Components
{
    [CreateAssetMenu(fileName = nameof(GlovesItemViewConfig), menuName = "CustomConfigs/Items/GlovesItemViewConfig", order = 0)]
    public class GlovesItemViewConfig : ItemViewConfig
    {
        public GlovesItemViewConfig()
        {
            slotID = EquipItemSlotIdentifierMap.Hands;
        }
    }

}