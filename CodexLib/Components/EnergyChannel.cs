using Kingmaker.UnitLogic.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class EnergyChannel : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleAttackWithWeaponResolve>
    {
        public void OnEventAboutToTrigger(RuleAttackWithWeaponResolve evt)
        {
            if (this.Fact is not Buff buff)
                return;

            if (!evt.AttackRoll.IsHit)
                return;

            int dice = this.Context[AbilitySharedValue.Damage];
            int bonus = this.Context[AbilitySharedValue.DamageBonus];
            var element = (DamageEnergyType)this.Context[AbilitySharedValue.StatBonus];

            evt.Damage.m_DamageBundle.m_Chunks.Add(new EnergyDamage(new DiceFormula(dice, DiceType.D6), bonus, element) { SourceFact = this.Fact });

            buff.ReduceDuration(10.Rounds().Seconds);
        }

        public void OnEventDidTrigger(RuleAttackWithWeaponResolve evt)
        {
        }
    }
}
