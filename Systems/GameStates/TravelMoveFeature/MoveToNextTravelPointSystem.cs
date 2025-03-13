using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;
using System.Threading.Tasks;

namespace Systems
{
	[Serializable][Documentation(Doc.State, Doc.Level, "Here we spawn next enemy and move player characters after level end")]
    public sealed class MoveToNextTravelPointSystem : BaseGameStateSystem, IUpdatable
    {
        [Required]
        private TravelBiomTypeVariablesComponent biomVariables;
        [Required]
        private AnimationCurveComponent animationCurveComponent;
        [Required]
        private ProgressComponent progressComponent;
        [Required]
        private TravelPointsHolderComponent travelPointsHolderComponent;

        protected override int State => GameStateIdentifierMap.ChangeTravelPointStateIdentifier;

        private SceneVariablesComponent sceneVariables;
        private BiomConfigsHolderComponent biomConfigsHolderComponent;

        public override void InitSystem()
        {
            var sceneManager = Owner.World.GetEntityBySingleComponent<SceneManagerTagComponent>();
            sceneVariables = sceneManager.GetComponent<SceneVariablesComponent>();
            biomConfigsHolderComponent = sceneManager.GetComponent<BiomConfigsHolderComponent>();
        }

        protected override async void ProcessState(int from, int to)
        {
            if (biomConfigsHolderComponent.TryGetBiomConfig(sceneVariables.CurrentBiomIndex, out var biomConfig))
            {
                if (sceneVariables.CurrentBiomLevelIndex == biomConfig.Levels.Length - 1)
                {
                    Debug.LogWarning("All levels end");
                    Owner.World.Command(new GoToNextBiomCommand());
                    EndState();
                }
                else
                {
                    if (biomVariables.LastLoadedEnemyIndex + 1 <= biomConfig.Levels.Length)
                    {
                        await LoadNextEnemyAsynk(biomConfig);
                    }
                    Owner.AddComponent<MoveToNextLevelTagComponent>();
                    Owner.Command(new StartRunBetweenLevelsCommand());
                }
            }
            else
            {
                throw new Exception($"Get Biom Config error: target biom ID = {sceneVariables.CurrentBiomID}");
            }
        }

        public void UpdateLocal()
        {
            if (Owner.TryGetComponent<MoveToNextLevelTagComponent>(out var tag))
            {
                var speed = animationCurveComponent.AnimationCurve.Evaluate(progressComponent.Value);
                progressComponent.ChangeValue(Time.deltaTime * speed);

                var playerTransform = biomVariables.PlayerCharacter.GetTransform();
                var newPlayerPoint = travelPointsHolderComponent.GetTravelPointByIndex(sceneVariables.CurrentBiomLevelIndex + 1).PlayerTravelPoint.position;
                var pastPlayerPoint = travelPointsHolderComponent.GetTravelPointByIndex(sceneVariables.CurrentBiomLevelIndex).PlayerTravelPoint.position;
                LerpCharacter(newPlayerPoint, pastPlayerPoint, playerTransform);

                var selectedMinionsHolder = Owner.World.GetEntityBySingleComponent<PlayerTagComponent>().GetComponent<PlayerSelectedMinionsHolderComponent>();

                if (selectedMinionsHolder.RightMinionId != 0 && selectedMinionsHolder.RightMinionId != PlayerMinionIdentifierMap.None)
                {
                    var rightMinionTransform = biomVariables.RightMinion.GetTransform();
                    var rightMinionNewPoint = travelPointsHolderComponent.GetTravelPointByIndex(sceneVariables.CurrentBiomLevelIndex + 1).RightMinionTravelPoint.position;
                    var rightMinionPastPoint = travelPointsHolderComponent.GetTravelPointByIndex(sceneVariables.CurrentBiomLevelIndex).RightMinionTravelPoint.position;
                    LerpCharacter(rightMinionNewPoint, rightMinionPastPoint, rightMinionTransform);
                }

                if(selectedMinionsHolder.LeftMinionId != 0 && selectedMinionsHolder.LeftMinionId != PlayerMinionIdentifierMap.None)
                {
                    var leftMinionTransform = biomVariables.LeftMinion.GetTransform();
                    var leftMinionNewPoint = travelPointsHolderComponent.GetTravelPointByIndex(sceneVariables.CurrentBiomLevelIndex + 1).LeftMinionTravelPoint.position;
                    var leftMinionPastPoint = travelPointsHolderComponent.GetTravelPointByIndex(sceneVariables.CurrentBiomLevelIndex).LeftMinionTravelPoint.position;
                    LerpCharacter(leftMinionNewPoint, leftMinionPastPoint, leftMinionTransform);
                }

                if (progressComponent.Value >= 1)
                {
                    progressComponent.SetValue(0);
                    Owner.RemoveComponent<MoveToNextLevelTagComponent>();
                    Owner.Command(new StopRunBetweenLevelsCommand());
                    RemoveLastEnemy();
                    sceneVariables.CurrentBiomLevelIndex++;
                    EndState();
                }
            }
        }

        private void LerpCharacter(Vector3 newPoint, Vector3 pastPoint, Transform characterTransform)
        {
            var direction = Vector3.Lerp(pastPoint, newPoint, progressComponent.Value);

            characterTransform.position = direction;
        }

        private async Task LoadNextEnemyAsynk(BiomConfig loadingBiomConfig)
        {
            var currnetBiomIndex = sceneVariables.CurrentBiomIndex;
            var levelConfig = biomConfigsHolderComponent.GetLevelConfig(currnetBiomIndex, biomVariables.LastLoadedEnemyIndex);
            var enemyContainer = biomConfigsHolderComponent.GetLevelEnemy(currnetBiomIndex, biomVariables.LastLoadedEnemyIndex);
            var spawnPointTransform = travelPointsHolderComponent.GetTravelPointByIndex(biomVariables.LastLoadedEnemyIndex).EnemySpawnPoint;
            var enemyActor = await enemyContainer.GetActor(position: spawnPointTransform.position, transform: spawnPointTransform);
            enemyActor.Init();

            var enemyLevelComponent = enemyActor.Entity.GetComponent<LevelCounterComponent>();
            var levelBonus = sceneVariables.CurrentBiomIndex + levelConfig.EnemyLevelBonus;

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

            biomVariables.EnemiesByIndex.Add(biomVariables.LastLoadedEnemyIndex, enemyActor.Entity);
            biomVariables.LastLoadedEnemyIndex++;
        }

        private void RemoveLastEnemy()
        {
            if (biomVariables.EnemiesByIndex.ContainsKey(sceneVariables.CurrentBiomLevelIndex - 1))
            {
                Owner.World.Command(new DestroyEntityWorldCommand { Entity = biomVariables.EnemiesByIndex[sceneVariables.CurrentBiomLevelIndex - 1] });
                biomVariables.EnemiesByIndex.Remove(sceneVariables.CurrentBiomLevelIndex - 1);
            }
        }
    }
}