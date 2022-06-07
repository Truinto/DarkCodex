using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// increase caster level, up to HD limit
    /// </summary>
    public class AddCasterLevelLimit : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>
    {
        public ContextValue Bonus;

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            //evt.AddBonusCasterLevel(this.Bonus.Calculate(this.Context), this.Descriptor);
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
            int bonus = Helper.MinMax(evt.Initiator.Progression.CharacterLevel - evt.Result.CasterLevel, 0, this.Bonus.Calculate(this.Context));
            evt.Result.CasterLevel += bonus;
        }
    }
}
