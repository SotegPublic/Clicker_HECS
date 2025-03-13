using HECSFramework.Core;
using UnityEngine;

namespace Systems
{
    public struct CompareItemsWindowContext
    {
        public string ItemName;
        public Sprite ItemIcon;
        public DefaultFloatModifier[] ItemsModifiers;
        public int ModifiersCount;
        public string QualityName;
        public int QualityID;
        public int ItemPower;
    }
}