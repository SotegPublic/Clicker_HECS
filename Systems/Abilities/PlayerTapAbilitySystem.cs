using System;
using Commands;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Abilities, "Player Tap Ability System")]
    public sealed class PlayerTapAbilitySystem : BaseAbilitySystem, IUpdatable
    {
        [Required]
        private ActionsHolderComponent actionsHolderComponent;

        [Required]
        private TapAbilityDamageModifierComponent tapAbilityDamageModifierComponent;

        [Required]
        private ParticleIdentifierHolderComponent particleIdentifierHolderComponent;

        [Required]
        private ProjectileHolderComponent projectileHolderComponent;

        [Required]
        private DamageComponent damageComponent;

        private float timeOut;

        private Transform spawn;
        private Animator animator;

        public override void Execute(Entity owner = null, Entity target = null, bool enable = true)
        {
            timeOut = 1f;
            animator.SetBool(AnimParametersMap.Open, true);
            actionsHolderComponent.ExecuteAction(ActionIdentifierMap.ExecuteAbilityIdentifier);
            GetProjectileFromPoolAcync(target);
        }

        public override void InitSystem()
        {
            using var localElements = AbilityOwner.GetComponent<VFXElementsHolderComponent>().GetVFXComponentsByID(FXIdentifierMap.SpawnMissileFX);

            if (localElements.Count > 0)
            {
                animator = localElements.Items[0].GetComponentInChildren<Animator>();
                spawn = localElements.Items[0].transform;
            }
        }

        public void UpdateLocal()
        {
            if (timeOut > 0)
            {
                timeOut -= Time.deltaTime;

                if (timeOut <= 0)
                {
                    animator.SetBool(AnimParametersMap.Open, false);
                }
            }
        }

        private async void GetProjectileFromPoolAcync(Entity target)
        {
            var projectile = await projectileHolderComponent.projectileContainer.GetActor(position: spawn.transform.position);

            AbilityOwner.Command(new PlayLocalVFXCommand { Enable = true, ID = FXIdentifierMap.AbilityEffect });
            var playerAttack = Owner.World.GetEntityBySingleComponent<PlayerTagComponent>().GetComponent<AttackPowerModifiableCounterComponent>();
            projectile.Entity.GetOrAddComponent<StartPositionHolderComponent>().StartPosition = spawn.transform.position;
            projectile.Entity.GetOrAddComponent<TargetHolderComponent>().Target = target;
            projectile.Entity.GetOrAddComponent<DamageComponent>().SetValue((damageComponent.Value + playerAttack.Value) * tapAbilityDamageModifierComponent.AbilityDamageModifier);

            projectile.Init();
        }
    }
}