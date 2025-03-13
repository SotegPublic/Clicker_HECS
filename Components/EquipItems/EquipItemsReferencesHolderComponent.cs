using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Components
{
    [Serializable][Documentation(Doc.Holder, Doc.Equipment, "here we hold items asset references for warmup system")]
    public sealed class EquipItemsReferencesHolderComponent : BaseComponent
    {
        [SerializeField] private AssetReference[] itemViewReferences;

        public AssetReference[] ItemViewReferences => itemViewReferences;
    }
}