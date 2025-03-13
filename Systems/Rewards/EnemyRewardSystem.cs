using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using HECSFramework.Rewards;
using Commands;
using Random = UnityEngine.Random;

namespace Systems
{
	[Serializable][Documentation(Doc.Rewards, Doc.Enemy, "this system send rewards comand when enemy die")]
    public sealed class EnemyRewardSystem : BaseSystem, IRewardSystem
    {
        [Required]
        private RewardConfigComponent rewardConfigComponent;

        [Single]
        private EnemyGivenExperienceSystem enemyGivenExperienceSystem;

        public void CommandReact(ExecuteReward command)
        {
            if(Owner.TryGetComponent<SpecialRewardTagComponent>(out var component))
            {
                if(component.SpecialRewardID != SpecialRewardTypeIdentifierMap.Gold && component.SpecialRewardID != SpecialRewardTypeIdentifierMap.Expirience)
                {
                    var specialRewardHolder = Owner.World.GetSingleComponent<SpecialRewardHolderComponent>();
                    specialRewardHolder.AddSpecialReward(component.SpecialRewardConstructor);
                }
            }

            var amount = rewardConfigComponent.IsCountBetweenValues ? Random.Range(rewardConfigComponent.MinRewardCount, rewardConfigComponent.MaxRewardCount + 1) : rewardConfigComponent.RewardCount;

            if(rewardConfigComponent.CounterID == CounterIdentifierContainerMap.Experience)
            {
                var enemyLevel = command.Owner.GetComponent<LevelCounterComponent>().Value;
                var enemyType = command.Owner.GetComponent<EnemyTagComponent>().EnemyTypeID;
                var expBonus = enemyGivenExperienceSystem.GetExperienceBonus(enemyType, enemyLevel, amount);

                amount += (int)Math.Round(expBonus, 0);
            }


            Owner.World.Command(new GlobalResourceRewardCommand
            {
                CounterID = rewardConfigComponent.CounterID,
                Amount = amount,
                From = new AliveEntity(command.Owner),
                To = new AliveEntity(command.Target),
                DrawRule = rewardConfigComponent.DrawRuleID
            });
        }

        public override void InitSystem()
        {
        }
    }
}