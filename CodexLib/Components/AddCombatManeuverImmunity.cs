using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class AddCombatManeuverImmunity : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCombatManeuver>
    {
        public CombatManeuver CombatManeuver;

        public AddCombatManeuverImmunity(CombatManeuver combatManeuver)
        {
            this.CombatManeuver = combatManeuver;
        }

        public void OnEventAboutToTrigger(RuleCombatManeuver evt)
        {
            if (evt.Type == this.CombatManeuver)
                evt.AutoFailure = true;
        }

        public void OnEventDidTrigger(RuleCombatManeuver evt)
        {
        }
    }
}
