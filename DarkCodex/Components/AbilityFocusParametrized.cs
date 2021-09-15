using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    [AllowedOn(typeof(BlueprintParametrizedFeature), false)]
    public class AbilityFocusParametrized : UnitFactComponentDelegate, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>
    {
        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            var isAbility = evt.Blueprint as BlueprintAbility;
            if (isAbility == null)
                return;

            if (isAbility != this.Param.Blueprint)
                return;

            evt.AddBonusDC(2);
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }
}
