using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;

namespace Systems
{
	[Serializable][Documentation(Doc.Player, "this system calculate player level")]
    public sealed class ExperienceSystem : BaseSystem, IReactGlobalCommand<CheckPlayerLevelCommand>
    {
        [Required]
        private ExperienceConfigComponent experienceConfig;
        [Required]
        private ExperienceToNextLevelCounter experienceToNextLevelCounter;

        public void CommandGlobalReact(CheckPlayerLevelCommand command)
        {
            if(command.SummaryPlayerExperience > experienceToNextLevelCounter.Value)
            {
                var levelCounter = Owner.World.GetEntityBySingleComponent<PlayerTagComponent>().GetComponent<LevelCounterComponent>();
                levelCounter.ChangeValue(1);

                CalcExpCountForNextLevel(levelCounter.Value);
            }
        }

        public override void InitSystem()
        {
            var playerLevel = Owner.World.GetEntityBySingleComponent<PlayerTagComponent>().GetComponent<LevelCounterComponent>().Value;

            CalcExpCountForNextLevel(playerLevel);
        }


        private void CalcExpCountForNextLevel(int level)
        {
            if(level == 1)
            {
                experienceToNextLevelCounter.SetValue(experienceConfig.StartExpValue);
            }

            if(level > 1)
            {
                float lastLevelExpCount = experienceConfig.StartExpValue;

                for (int i = 2; i <= level; i++)
                {
                    var modifier = experienceConfig.GetExpModifier(i);
                    lastLevelExpCount += lastLevelExpCount * modifier;
                }

                experienceToNextLevelCounter.ChangeValue(lastLevelExpCount);
            }
        }
    }
}