using UnityEngine;

namespace Components
{
    [CreateAssetMenu(fileName = nameof(WeaponItemViewConfig), menuName = "CustomConfigs/Items/WeaponItemViewConfig", order = 0)]
    public class WeaponItemViewConfig : ItemViewConfig
    {
        public WeaponItemViewConfig()
        {
            slotID = EquipItemSlotIdentifierMap.Weapon;
        }
    }

}