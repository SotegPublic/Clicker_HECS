using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Systems
{
	[Serializable][Documentation(Doc.State, Doc.Level, Doc.Enemy, "here we spawn enemies in travel biom")]
    public sealed class LoadEnemiesInTravelBiomSystem : BaseGameStateSystem
    {
        [Required]
        private TravelBiomTypeVariablesComponent variablesComponent;
        [Required]
        private TravelPointsHolderComponent travelPointsHolderComponent;


        private SceneVariablesComponent sceneVariables;
        private BiomConfigsHolderComponent biomConfigsHolderComponent;
        private List<Task> taskList = new List<Task>(8);

        protected override int State => GameStateIdentifierMap.SpawnEnemiesInTravelBiomStateIdentifier;

        public override void InitSystem()
        {
            var sceneManager = Owner.World.GetEntityBySingleComponent<SceneManagerTagComponent>();
            sceneVariables = sceneManager.GetComponent<SceneVariablesComponent>();
            biomConfigsHolderComponent = sceneManager.GetComponent<BiomConfigsHolderComponent>();
        }

        protected override async void ProcessState(int from, int to)
        {
            var spawnPointsCount = travelPointsHolderComponent.GetEnemiesSpawnPointsCount();
            var enemiesLeft = spawnPointsCount - sceneVariables.CurrentBiomLevelIndex;
            var preloadEnemiesCount = enemiesLeft >= variablesComponent.PreloadEnemiesCount ? variablesComponent.PreloadEnemiesCount : enemiesLeft;


            for (int i = sceneVariables.CurrentBiomLevelIndex; i < sceneVariables.CurrentBiomLevelIndex + preloadEnemiesCount; i++)
            {
                taskList.Add(SpawnEnemy(i));
            }

            await Task.WhenAll(taskList);

            EndState();
        }

        private async Task SpawnEnemy(int enemyIndex)
        {
            var currnetBiomIndex = sceneVariables.CurrentBiomIndex;
            var levelConfig = biomConfigsHolderComponent.GetLevelConfig(currnetBiomIndex, enemyIndex);
            var enemyContainer = biomConfigsHolderComponent.GetLevelEnemy(currnetBiomIndex, enemyIndex);
            var spawnPointTransform = travelPointsHolderComponent.GetTravelPointByIndex(enemyIndex).EnemySpawnPoint;
            var enemyActor = await enemyContainer.GetActor(position: spawnPointTransform.position);
            enemyActor.Init();

            var enemyLevelComponent = enemyActor.Entity.GetComponent<LevelCounterComponent>();
            var levelBonus = currnetBiomIndex + levelConfig.EnemyLevelBonus;

            enemyLevelComponent.ChangeValue(levelBonus);

            var enemyTypeID = enemyActor.Entity.GetComponent<EnemyTagComponent>().EnemyTypeID;
            var enemiesHealthProgression = Owner.World.GetEntityBySingleComponent<EnemiesProgressionTagComponent>().GetComponent<EnemiesHealthProgressionConfigsHolderComponent>();

            var healthBonus = enemiesHealthProgression.GetHealthBonus(enemyLevelComponent.Value, enemyTypeID);

            var healthModifier = new DefaultFloatModifier
            {
                ID = CounterIdentifierContainerMap.Health,
                GetCalculationType = ModifierCalculationType.Add,
                GetModifierType = ModifierValueType.Value,
                GetValue = healthBonus,
                ModifierGuid = Guid.NewGuid(),
                ModifierID = ModifierIdentifierMap.Health
            };

            var enemyHealth = enemyActor.Entity.GetComponent<HealthComponent>();
            enemyHealth.AddModifier(healthModifier.ModifierGuid, healthModifier);

            if (levelConfig.TryGetModifiers(out var modifiers))
            {
                for (int i = 0; i < modifiers.Count; i++)
                {
                    switch (modifiers[i].ModifierID)
                    {
                        case ModifierIdentifierMap.Health:

                            enemyHealth.AddModifier(modifiers[i].ModifierGuid, modifiers[i]);

                            break;
                        default:
                            break;
                    }
                }
            }

            if (levelConfig.TryGetSpecialRewards(out var specialReward))
            {
                var rewardsHolder = enemyActor.Entity.GetComponent<RewardsLocalHolderComponent>();

                for (int i = 0; i < specialReward.Count; i++)
                {
                    rewardsHolder.AddReward(specialReward[i]);
                }
            }

            variablesComponent.LastLoadedEnemyIndex++;
            variablesComponent.EnemiesByIndex.Add(enemyIndex, enemyActor.Entity);
        }
    }
}