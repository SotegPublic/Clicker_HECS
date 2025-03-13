using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    [Serializable][Documentation(Doc.Spawn, Doc.Enemy, Doc.Level, "here we spawn enemies on chanks")]
    public sealed class LoadChunksEnemiesSystem : BaseGameStateSystem
    {
        [Required]
        private ChunkBiomTypeVariablesComponent biomVariables;
        [Required]
        private ChunksQueueHolderComponent chunksHolder;

        private SceneVariablesComponent sceneVariables;
        private BiomConfigsHolderComponent biomConfigsHolderComponent;

        protected override int State => GameStateIdentifierMap.SpawnChunksEnemiesStateIdentifier;

        private List<Task> taskList = new List<Task>(5);

        public override void InitSystem()
        {
            var sceneManager = Owner.World.GetEntityBySingleComponent<SceneManagerTagComponent>();
            sceneVariables = sceneManager.GetComponent<SceneVariablesComponent>();
            biomConfigsHolderComponent = sceneManager.GetComponent<BiomConfigsHolderComponent>();
        }

        protected override async void ProcessState(int from, int to)
        {
            taskList.Clear();

            foreach (var chunk in chunksHolder.Chunks)
            {
                if(!chunk.IsEnemyPlaced && !chunk.IsEnemyKilled)
                {
                    taskList.Add(SpawnEnemy(chunk, biomVariables.CurrentBiomLevelIndexForLoad));
                    chunk.IsEnemyPlaced = true;
                    biomVariables.CurrentBiomLevelIndexForLoad++;
                }
            }
            
            await Task.WhenAll(taskList);

            EndState();
        }

        private async Task SpawnEnemy(ChunkMonoComponent chunk, int lastLoadedLevelIndex)
        {
            var currnetBiomIndex = sceneVariables.CurrentBiomIndex;
            var enemyContainer = biomConfigsHolderComponent.GetLevelEnemy(currnetBiomIndex, lastLoadedLevelIndex);
            var enemyActor = await enemyContainer.GetActor(position: chunk.SpawnPointTransform.position, transform: chunk.SpawnPointTransform);
            enemyActor.Init();

            var enemyLevelComponent = enemyActor.Entity.GetComponent<LevelCounterComponent>();
            var levelBonus = currnetBiomIndex + chunk.ChunkLevelBonus;

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

            if(chunk.TryGetModifiers(out var modifiers))
            {
                for(int i = 0; i < modifiers.Count; i++)
                {
                    switch(modifiers[i].ModifierID)
                    {
                        case ModifierIdentifierMap.Health:

                            enemyHealth.AddModifier(modifiers[i].ModifierGuid, modifiers[i]);

                            break;
                        default:
                            break;
                    }
                }
            }

            chunk.ChunkEnemy = enemyActor.Entity.GetAliveEntity();
            
            if(chunk.TryGetSpecialRewards(out var specialReward))
            {
                var rewardsHolder = enemyActor.Entity.GetComponent<RewardsLocalHolderComponent>();
                
                for(int i = 0; i < specialReward.Count; i++)
                {
                    rewardsHolder.AddReward(specialReward[i]);
                }
            }
        }
    }
}