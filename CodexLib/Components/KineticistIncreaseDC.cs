using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Class.Kineticist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class KineticistIncreaseDC : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>
    {
        public int Amount;

        public KineticistIncreaseDC(int amount)
        {
            this.Amount = amount;
        }

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            var spell = this.Context.SourceAbility;
            if (spell == null || spell.GetComponent<AbilityKineticist>() == null)
                return;

            evt.AddBonusDC(this.Amount * this.Fact.GetRank());
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }
}
