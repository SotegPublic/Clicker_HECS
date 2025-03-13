using Commands;
using Helpers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Components
{
    public class ItemViewConfig: ScriptableObject
    {
        [SerializeField, ReadOnly]protected int slotID;
        [SerializeField] protected AssetReference viewAssetReference;
        [SerializeField] protected Sprite icon;
        [SerializeField] protected EquipItemQualityIdentifier[] qualities;
        [SerializeField] protected int minimumItemLevel;

        public int SlotID => slotID;
        public AssetReference ViewAssetReference => viewAssetReference;
        public Sprite Icon => icon;
        public EquipItemQualityIdentifier[] Qualities => qualities;
        public int MinimumItemLevel => minimumItemLevel;
    }

}