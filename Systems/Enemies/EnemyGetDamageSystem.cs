using System;
using Commands;
using Components;
using HECSFramework.Core;


namespace Systems
{
	[Serializable][Documentation(Doc.Enemy, Doc.Damage, "Enemy Get Damage System")]
    public sealed class EnemyGetDamageSystem : BaseSystem, IReactCommand<DamageCommand<float>>
    {
        [Required]
        private ActionsHolderComponent actionsHolderComponent;

        public void CommandReact(DamageCommand<float> command)
        {
            if(Owner.IsAliveAndNotDead())
            {
                actionsHolderComponent.ExecuteAction(ActionIdentifierMap.EnemyGetDamageIdentifier);
            }
        }

        public override void InitSystem()
        {
        }
    }
}