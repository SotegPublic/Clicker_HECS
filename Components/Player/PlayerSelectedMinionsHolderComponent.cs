using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;
using Helpers;

namespace Components
{
    [Serializable][Documentation(Doc.Holder, "here we hold player selected minions model")]
    public sealed class PlayerSelectedMinionsHolderComponent : BaseComponent
    {
        [SerializeField, IdentifierDropDown(nameof(PlayerMinionIdentifier))] private int rightMinionId;
        [SerializeField, IdentifierDropDown(nameof(PlayerMinionIdentifier))] private int leftMinionId;

        public int RightMinionId => rightMinionId;
        public int LeftMinionId => leftMinionId;

        public void SetRightMinion(int minionId)
        {
            rightMinionId = minionId;
        }

        public void SetLEftMinion(int minionId)
        {
            leftMinionId = minionId;
        }
    }
}