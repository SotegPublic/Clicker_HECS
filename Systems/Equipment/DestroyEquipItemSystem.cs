using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;

namespace Systems
{
	[Serializable][Documentation(Doc.Equipment, "DestroyEquipItemSystem")]
    public sealed class DestroyEquipItemSystem : BaseSystem, IReactGlobalCommand<DestroyEquipItemCommand>
    {
        [Single]
        private PoolingSystem poolingSystem;

        public override void InitSystem()
        {
        }

        public void CommandGlobalReact(DestroyEquipItemCommand command)
        {
            if(command.IsEquipted)
            {
                DestroyEquiptedItemView(command.ItemEntity);
            }

            DestroyItem(command.ItemEntity);
        }

        private void DestroyItem(Entity itemEntity)
        {
            var player = Owner.World.GetEntityBySingleComponent<PlayerTagComponent>();
            var playerExp = player.GetComponent<ExperienceComponent>().Value;

            Owner.World.Command(new GlobalResourceRewardCommand
            {
                CounterID = CounterIdentifierContainerMap.Gold,
                Amount = itemEntity.GetComponent<EquipItemParametersComponent>().GoldCost,
                From = new AliveEntity(Owner),
                To = new AliveEntity(player),
                DrawRule = DrawRuleIdentifierMap.GoldRewardForEquip
            });

            Owner.World.Command(new GlobalResourceRewardCommand
            {
                CounterID = CounterIdentifierContainerMap.Experience,
                Amount = itemEntity.GetComponent<EquipItemParametersComponent>().ExpCost,
                From = new AliveEntity(Owner),
                To = new AliveEntity(player),
                DrawRule = DrawRuleIdentifierMap.ExpRewardForEquip
            });

            itemEntity.GetComponent<ModifiersHolderComponent>().ClearModifiers();

            Owner.World.Command(new DestroyEntityWorldCommand { Entity = itemEntity });
        }

        private void DestroyEquiptedItemView(Entity itemEntity)
        {
            var mainItemView = itemEntity.GetComponent<EquipItemParametersComponent>().MainItemView;
            var secondItemView = itemEntity.GetComponent<EquipItemParametersComponent>().SecondItemView;

            if (mainItemView != null)
            {
                poolingSystem.ReleaseView(mainItemView);
            }

            if (secondItemView != null)
            {
                poolingSystem.ReleaseView(secondItemView);
            }
        }
    }
}