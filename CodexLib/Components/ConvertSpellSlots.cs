using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Consumes spell slots in place of item charges.
    /// </summary>
    public class ConvertSpellSlots : EntityFactComponentDelegate, IInitiatorRulebookHandler<RuleSpendCharge>
    {
        public BlueprintSpellbook Spellbook;
        public BlueprintBuff Buff;

        public void OnEventAboutToTrigger(RuleSpendCharge evt)
        {
            var item = evt.Spell.SourceItem;
            var caster = evt.Initiator;
            if (item == null || item.IsSpendCharges || !caster.Buffs.HasFact(this.Buff))
                return;

            var spellbook = caster.GetSpellbook(Spellbook);
            if (spellbook == null)
                return;

            for (int i = 3; i < spellbook.m_SpontaneousSlots.Length; i++)
            {
                if (spellbook.m_SpontaneousSlots[i] > 0)
                {
                    spellbook.m_SpontaneousSlots[i]--;
                    evt.ShouldSpend = false;
                    evt.ShouldConsumeMaterial = false;
                    return;
                }
            }
        }

        public void OnEventDidTrigger(RuleSpendCharge evt)
        {
        }
    }
}
