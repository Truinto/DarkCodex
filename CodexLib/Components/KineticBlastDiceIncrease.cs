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
                switch (dmg.m_Dice)
                {
                    case DiceType.D6:
                        dmg.m_Dice = DiceType.D8;
                        break;
                    case DiceType.D8:
                        dmg.m_Dice = DiceType.D10;
                        break;
                    case DiceType.D10:
                        dmg.m_Dice = DiceType.D12;
                        break;
                    default:
                        continue;
                }
                bundles[i].ReplaceDice(dmg);
            }
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
        }
    }
}
