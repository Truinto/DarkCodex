using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class AddCasterLevelParametrized : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>
    {
        public ContextValue Bonus;
        public ModifierDescriptor Descriptor = ModifierDescriptor.UntypedStackable;

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (this.Param.SpellSchool != null)
            {
                if ((this.Param.SpellSchool.Value & evt.Spell.School) != 0)
                    evt.AddBonusCasterLevel(this.Bonus.Calculate(this.Context), this.Descriptor);
            }
            else if (this.Param.Blueprint is BlueprintAbility spell)
            {
                if (spell == evt.Spell)
                    evt.AddBonusCasterLevel(this.Bonus.Calculate(this.Context), this.Descriptor);
            }
            else if (this.Param.Blueprint is BlueprintSpellbook book)
            {
                if (book == evt.Spellbook?.Blueprint)
                    evt.AddBonusCasterLevel(this.Bonus.Calculate(this.Context), this.Descriptor);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }
}
