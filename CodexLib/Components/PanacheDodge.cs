using Kingmaker.UnitLogic.Commands.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Runs after an attack already hit. Will check if the bonus would prevent it and change the result post factum.
    /// </summary>
    public class PanacheDodge : UnitFactComponentDelegate, IAttackHandler, IBeforeRule
    {
        public BlueprintAbilityResourceReference Resource;

        public PanacheDodge(BlueprintAbilityResourceReference resource)
        {
            this.Resource = resource;
        }

        public void HandleAttackHitRoll(RuleAttackRoll attack)
        {
            if (attack.IsFake || !attack.Initiator.IsInGame || !attack.Target.IsInGame || attack.AutoHit)
                return;
            if (attack.Result != AttackResult.Hit || attack.Target != this.Owner || this.Resource.IsEmpty())
                return;

            int bonus = this.Owner.Stats.Charisma.Bonus;
            if (attack.Roll + attack.AttackBonus >= attack.TargetAC + bonus)
                return;

            if (!this.Owner.Resources.HasEnoughResource(this.Resource, 1))
                return;

            if (this.Owner.CombatState.HasCooldownForCommand(UnitCommand.CommandType.Swift))
                return;

            this.Owner.Resources.Spend(this.Resource, 1);
            this.Owner.SpendAction(UnitCommand.CommandType.Swift, false, 0f);

            attack.ACRule.AddModifier(bonus, this.Fact, ModifierDescriptor.UntypedStackable);
            attack.TargetAC += bonus;
            attack.Result = AttackResult.Miss;
        }
    }
}
