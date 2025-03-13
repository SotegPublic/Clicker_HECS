using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;

namespace Systems
{
	[Serializable][Documentation(Doc.Player, Doc.Load, "LoadPlayerSystem")]
    public sealed class LoadPlayerSystem : BaseGameStateSystem 
    {
        protected override int State => GameStateIdentifierMap.LoadPlayerStateIdentifier;

        public override void InitSystem()
        {
        }

        protected override void ProcessState(int from, int to)
        {
            // todo: load player save data and calculation item power

            var player = Owner.World.GetEntityBySingleComponent<PlayerTagComponent>();
            var attackCounter = player.GetComponent<AttackPowerModifiableCounterComponent>();
            var itemPowerCounter = player.GetComponent<SummaryItemPowerComponent>();

            attackCounter.Setup(20);
            itemPowerCounter.SetValue(20);

            //

            EndState();
        }
    }
}