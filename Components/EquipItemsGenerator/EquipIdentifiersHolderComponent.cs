using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.Item, Doc.Equipment, "here we hold equipment modifiers for item generation system")]
    public sealed class EquipIdentifiersHolderComponent : BaseComponent
    {
        [SerializeField] private EquipItemSlotIdentifier[] equipItemSlotIdentifiers;

        public EquipItemSlotIdentifier GetRandomSlotIdentifier()
        {
            return equipItemSlotIdentifiers[UnityEngine.Random.Range(0, equipItemSlotIdentifiers.Length)];
        }
    }
}