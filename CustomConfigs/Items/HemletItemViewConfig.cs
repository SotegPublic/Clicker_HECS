using UnityEngine;

namespace Components
{
    [CreateAssetMenu(fileName = nameof(HemletItemViewConfig), menuName = "CustomConfigs/Items/HemletItemViewConfig", order = 0)]
    public class HemletItemViewConfig : ItemViewConfig
    {
        public HemletItemViewConfig()
        {
            slotID = EquipItemSlotIdentifierMap.Head;
        }
    }

}