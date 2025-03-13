using UnityEngine;

namespace Components
{
    [CreateAssetMenu(fileName = nameof(ShouldersItemViewConfig), menuName = "CustomConfigs/Items/ShouldersItemViewConfig", order = 0)]
    public class ShouldersItemViewConfig : ItemViewConfig
    {
        public ShouldersItemViewConfig()
        {
            slotID = EquipItemSlotIdentifierMap.Shoulders;
        }
    }

}