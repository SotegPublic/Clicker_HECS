using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.Equipment, Doc.Holder, "here we hold actor constrants")]
    public sealed class ConstrantsHolderComponent : BaseComponent
    {
        private Dictionary<int, Transform> constrantsTransforms = new Dictionary<int, Transform>();

        public void AddConstrantTransform(ConstrantMonoComponent constrant)
        {
            constrantsTransforms.Add(constrant.Identifier, constrant.transform);
        }

        public bool TryGetConstrantTransform(int identifier, out Transform transform)
        {
            if (constrantsTransforms.ContainsKey(identifier))
            {
                transform = constrantsTransforms[identifier];
                return true;
            }
            transform = null;
            return false;
        }
    }
}