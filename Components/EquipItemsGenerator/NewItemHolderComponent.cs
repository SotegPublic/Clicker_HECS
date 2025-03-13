using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.Holder, Doc.Equipment, "here we hold new equip item for comparison and decision making")]
    public sealed class NewItemHolderComponent : BaseComponent
    {
        [SerializeField] public Entity NewEquipItem;
    }
}