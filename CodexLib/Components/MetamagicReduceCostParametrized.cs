using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class MetamagicReduceCostParametrized : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleApplyMetamagic>
    {
        public int Reduction = 1;
        public Metamagic Metamagic;
        public bool ReduceByMostExpensive;

        public void OnEventAboutToTrigger(RuleApplyMetamagic evt)
        {
            var bp = this.Param.Blueprint as BlueprintAbility;
            if (bp != null && bp != evt.Spell && bp != evt.Spell.Parent)
                return;

            if (ReduceByMostExpensive)
            {
                int max = 0;
                foreach (var meta in evt.AppliedMetamagics)
                    max = Math.Max(max, meta.GetCost(evt.Initiator));
                evt.ReduceCost(max);
                return;
            }

            if (Metamagic != 0 && !evt.AppliedMetamagics.Contains(Metamagic))
                return;

            evt.ReduceCost(Reduction);
        }

        public void OnEventDidTrigger(RuleApplyMetamagic evt)
        {
            // modified SpellLevelCost cannot reduce the spells cost; unless mythic metamagic
            if (evt.Result.SpellLevelCost < 0 && !evt.Result.Has(Metamagic.CompletelyNormal))
            {
                evt.Result.SpellLevelCost = 0;
            }
        }
    }
}
