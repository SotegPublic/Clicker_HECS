using HECSFramework.Core;
using HECSFramework.Unity;
using Helpers;
using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.Holder, Doc.Visual, "here we hold views configs")]
    public sealed class ItemViewsHolderComponent : BaseComponent
    {
        [SerializeField] private HemletItemViewConfig[] helmetsViewConfigs;
        [SerializeField] private ShouldersItemViewConfig[] shouldersViewConfigs;
        [SerializeField] private GlovesItemViewConfig[] handsViewConfigs;
        [SerializeField] private BootsItemViewConfig[] bootsViewConfigs;
        [SerializeField] private WeaponItemViewConfig[] weaponsViewConfigs;
        [SerializeField] private ShieldItemViewConfig[] shieldsViewConfigs;

        private Dictionary<int, Dictionary<int, List<ItemViewConfig>>> configsBySlotAndQuality = new Dictionary<int, Dictionary<int, List<ItemViewConfig>>>();
        private ItemViewConfig[] tmpArray;
        public override void Init()
        {
            base.Init();

            GetConfigsIntoDictionary(helmetsViewConfigs);
            GetConfigsIntoDictionary(shouldersViewConfigs);
            GetConfigsIntoDictionary(handsViewConfigs);
            GetConfigsIntoDictionary(bootsViewConfigs);
            GetConfigsIntoDictionary(weaponsViewConfigs);
            GetConfigsIntoDictionary(shieldsViewConfigs);
        }

        private void GetConfigsIntoDictionary(ItemViewConfig[] configs)
        {
            for (int i = 0; i < configs.Length; i++)
            {
                var config = configs[i];

                if (!configsBySlotAndQuality.ContainsKey(configs[i].SlotID))
                {
                    configsBySlotAndQuality.Add(configs[i].SlotID, new Dictionary<int, List<ItemViewConfig>>());
                }

                for(int j = 0; j < configs[i].Qualities.Length; j++)
                {
                    var qualityID = config.Qualities[j];

                    if (!configsBySlotAndQuality[config.SlotID].ContainsKey(qualityID))
                    {
                        configsBySlotAndQuality[config.SlotID].Add(qualityID, new List<ItemViewConfig>());
                    }

                    configsBySlotAndQuality[config.SlotID][qualityID].Add(config);
                }
            }
        }

        public ItemViewConfig GetViewConfig(int slotID, int qualityID, int itemLevel)
        {
            if(configsBySlotAndQuality.ContainsKey(slotID))
            {
                if (configsBySlotAndQuality[slotID].ContainsKey(qualityID))
                {
                    return GetRandomConfig(configsBySlotAndQuality[slotID][qualityID], itemLevel);
                }
            }

            ArrayPool<ItemViewConfig>.Shared.Return(tmpArray);
            return null;
        }

        private ItemViewConfig GetRandomConfig(List<ItemViewConfig> configs, int itemLevel)
        {
            tmpArray = HECSPooledArray<ItemViewConfig>.GetArray().Items;
            var tmpArrayLenth = 0;

            for (int i = 0; i < configs.Count; i++)
            {
                if (configs[i].MinimumItemLevel <= itemLevel)
                {
                    tmpArray[tmpArrayLenth] = configs[i];
                    tmpArrayLenth++;
                }
            }

            return tmpArray[UnityEngine.Random.Range(0, tmpArrayLenth)];
        }
    }
}