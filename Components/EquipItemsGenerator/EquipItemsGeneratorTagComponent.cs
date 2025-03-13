using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.Tag, Doc.Equipment, "by this tag we mark equip items generator")]
    public sealed class EquipItemsGeneratorTagComponent : BaseComponent, IWorldSingleComponent
    {
    }
}