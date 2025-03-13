using HECSFramework.Core;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Components
{
    [Serializable][Documentation(Doc.Equipment, Doc.Item, "here we hold item parameters")]
    public sealed class EquipItemParametersComponent : BaseComponent
    {
        public string ItemName;
        public EquipItemQualityIdentifier EquipItemQualityIdentifier;
        public EquipItemSlotIdentifier EquipItemSlotIdentifier;
        public int ItemLevel;
        public int ItemPower;
        public Sprite Icon;
        public AssetReference ViewReference;
        public GameObject MainItemView;
        public GameObject SecondItemView;
        public int ExpCost;
        public int GoldCost;
    }
}