using Kingmaker.Controllers.Optimization;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    public class DuplicateSpell : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCastSpell>   // DublicateSpellComponent
    {
        public int Radius = 30;
        public Func<AbilityData, bool> AbilityCheck;

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {
            if (evt.IsDuplicateSpellApplied || !evt.Success || AbilityCheck != null && AbilityCheck(evt.Spell))
                return;

            AbilityData spell = evt.Spell;
            UnitEntityData newTarget = this.GetNewTarget(spell, evt.SpellTarget.Unit);
            if (newTarget == null)
                return;
            Rulebook.Trigger(new RuleCastSpell(spell, newTarget)
            {
                IsDuplicateSpellApplied = true
            });
        }

        private UnitEntityData GetNewTarget(AbilityData data, UnitEntityData baseTarget)
        {
            List<UnitEntityData> list = EntityBoundsHelper.FindUnitsInRange(baseTarget.Position, this.Radius.Feet().Meters);
            list.Remove(baseTarget);
            list.Remove(base.Owner);
            list.RemoveAll((UnitEntityData x) => x.Faction != baseTarget.Faction || !data.CanTarget(x));
            if (list.Count <= 0)
            {
                return null;
            }
            return list.GetRandomElements(1, new Random())[0];
        }
    }
}
