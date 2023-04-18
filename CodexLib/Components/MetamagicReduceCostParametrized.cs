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
    /// <summary>
    /// Reduces metamagic cost. Can reduce cost by fixed amount or make the most expensive metamagic free. Can be filtered by metamagic.<br/>
    /// Filters by spell, if attached to a <see cref="BlueprintParametrizedFeature"/>.
    /// </summary>
    public class MetamagicReduceCostParametrized : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleApplyMetamagic>
    {
        /// <summary>Amount to reduce cost by.</summary>
        public int Reduction;
        /// <summary>Reduce cost by most expensive. Overrides Reduction.</summary>
        public bool ReduceByMostExpensive;
        /// <summary>Check spell has this specific metamagic.</summary>
        public Metamagic Metamagic;

        [Obsolete]
        private MetamagicReduceCostParametrized()
        {
            Reduction = 1;
        }

        /// <inheritdoc cref="MetamagicReduceCostParametrized"/>
        public MetamagicReduceCostParametrized(int reduction = 1, bool reduceByMostExpensive = false, Metamagic metamagic = 0)
        {
            this.Reduction = reduction;
            this.ReduceByMostExpensive = reduceByMostExpensive;
            this.Metamagic = metamagic;
        }

        /// <summary></summary>
        public void OnEventAboutToTrigger(RuleApplyMetamagic evt)
        {
            if (this.Param?.Blueprint is BlueprintAbility bp && bp != evt.Spell && bp != evt.Spell.Parent)
                return;

            if (Metamagic != 0 && !evt.AppliedMetamagics.Contains(Metamagic))
                return;

            if (ReduceByMostExpensive)
            {
                int max = 0;
                foreach (var meta in evt.AppliedMetamagics)
                    max = Math.Max(max, meta.GetCost(evt.Initiator));
                evt.ReduceCost(max);
                return;
            }

            evt.ReduceCost(Reduction);
        }

        /// <summary></summary>
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
