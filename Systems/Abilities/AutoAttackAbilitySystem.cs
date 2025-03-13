using System;
using Commands;
using Components;
using HECSFramework.Core;
using UnityEngine;
using HECSFramework.Unity;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Abilities, "Auto Attack Ability System")]
    public sealed class AutoAttackAbilitySystem : BaseAbilitySystem, IUpdatable
    {
        [Required]
        private ActionsHolderComponent actionsHolderComponent;

        [Required]
        private AnimationSpeedComponent animationSpeedComponent;

        [Required]
        private ParticleIdentifierHolderComponent particleIdentifierHolderComponent;

        [Required]
        private ProjectileHolderComponent projectileHolderComponent;

        [Required]
        private DamageComponent damageComponent;

        private float currentClipTime;
        private float clipLenth;
        private float attackTiming;
        private AliveEntity abilityTarget;
        private bool isSecondStageStarted;
        private bool isProjectileSpawned;

        public override void Execute(Entity owner = null, Entity target = null, bool enable = true)
        {
            AbilityOwner.Command(new FloatAnimationCommand { Index = AnimParametersMap.AttackAnimationSpeed, Value = animationSpeedComponent.Value });

            actionsHolderComponent.ExecuteAction(ActionIdentifierMap.ExecuteAbilityIdentifier);
            AbilityOwner.AddComponent<PlayingAttackAnimationTagComponent>();
            abilityTarget = new AliveEntity(target);
        }

        public override void InitSystem()
        {
        }

        public void UpdateLocal()
        {
            var abilityOwner = Owner.GetComponent<AbilityOwnerComponent>().AbilityOwner;

            if (abilityOwner.TryGetComponent<PlayingAttackAnimationTagComponent>(out var inAttackComponent))
            {
                currentClipTime += Time.deltaTime;

                if (clipLenth == 0)
                {
                    var animationCheckOutsHolder = abilityOwner.GetComponent<AnimationCheckOutsHolderComponent>();

                    var atkAnimationSpeed = abilityOwner.GetComponent<AnimatorStateComponent>().Animator.GetFloat(nameof(AnimParametersMap.AttackAnimationSpeed));

                    if (animationCheckOutsHolder.TryGetCheckoutInfo(AnimationEventIdentifierMap.Attack, out var animationCheckOutInfo))
                    {
                        clipLenth = animationCheckOutInfo.ClipLenght / atkAnimationSpeed;
                        attackTiming = animationCheckOutInfo.Timing / atkAnimationSpeed;
                    }
                }
                else
                {
                    if (currentClipTime >= attackTiming && !isSecondStageStarted)
                    {
                        GetProjectileFromPoolAcync();
                        actionsHolderComponent.ExecuteAction(ActionIdentifierMap.OnAnimationEventIdentifier);
                        isSecondStageStarted = true;
                    }

                    if (currentClipTime >= clipLenth && isProjectileSpawned)
                    {
                        abilityOwner.RemoveComponent<PlayingAttackAnimationTagComponent>();
                        ResetSystem();
                    }
                }
            }
        }

        private void ResetSystem()
        {
            currentClipTime = 0;
            attackTiming = 0;
            clipLenth = 0;
            isSecondStageStarted = false;
            isProjectileSpawned = false;
        }

        private async void GetProjectileFromPoolAcync()
        {
            var spawnPositions = Owner.GetComponent<AbilityOwnerComponent>().AbilityOwner.AsActor().GetComponentsInChildren<SpawnParticlePositionMonoComponent>();
            Vector3 spawnPosition = Vector3.zero;

            for (int i = 0; i < spawnPositions.Length; i++)
            {
                if (spawnPositions[i].ParticleIdentifier == particleIdentifierHolderComponent.ParticleIdentifier)
                {
                    spawnPosition = spawnPositions[i].Transform.position;
                    break;
                }
            }

            var projectile = await projectileHolderComponent.projectileContainer.GetActor(position: spawnPosition);

            var playerAttack = Owner.World.GetEntityBySingleComponent<PlayerTagComponent>().GetComponent<AttackPowerModifiableCounterComponent>();
            projectile.GetHECSComponent<StartPositionHolderComponent>().StartPosition = spawnPosition;
            projectile.GetHECSComponent<TargetHolderComponent>().Target = abilityTarget.Entity;
            projectile.GetHECSComponent<DamageComponent>().SetValue(damageComponent.Value + playerAttack.Value);

            projectile.Init();

            isProjectileSpawned = true;
        }
    }
}