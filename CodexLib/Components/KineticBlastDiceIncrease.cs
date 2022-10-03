using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.Kineticist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class KineticBlastDiceIncrease : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>
    {
        public bool OnlySimple;

        public KineticBlastDiceIncrease(bool onlySimple = false)
        {
            this.OnlySimple = onlySimple;
        }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            var kin = evt.SourceAbility?.GetComponent<AbilityKineticist>();
            if (kin == null)
                return;

            if (kin.WildTalentBurnCost > 0 || this.OnlySimple && kin.BlastBurnCost > 0)
                return;

            var bundles = evt.m_DamageBundle.m_Chunks;
            for (int i = 0; i < bundles.Count; i++)
            {
                var dmg = bundles[i].Dice;
                var baseValue = dmg.ModifiedValue;
                switch (baseValue.Dice)
                {
                    case DiceType.D6:
                        dmg.Modify(new(baseValue.Rolls, DiceType.D8), this.Fact);
                        break;
                    case DiceType.D8:
                        dmg.Modify(new(baseValue.Rolls, DiceType.D10), this.Fact);
                        break;
                    case DiceType.D10:
                        dmg.Modify(new(baseValue.Rolls, DiceType.D12), this.Fact);
                        break;
                    default:
                        continue;
                }
            }
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
        }
    }
}
