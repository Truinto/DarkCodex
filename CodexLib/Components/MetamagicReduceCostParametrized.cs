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

        public void OnEventAboutToTrigger(RuleApplyMetamagic evt)
        {
            var bp = this.Param.Blueprint as BlueprintAbility;

            if ((bp == null || bp == evt.Spell) && (Metamagic == 0 || evt.AppliedMetamagics.Contains(Metamagic)))
            {
                evt.ReduceCost(Reduction);
            }
        }

        public void OnEventDidTrigger(RuleApplyMetamagic evt)
        {
            // modified SpellLevelCost cannot reduce the spells cost; unless mythic metamagic
            if (evt.Result.SpellLevelCost < 0 && !evt.Result.Has(Metamagic.CompletelyNormal))
            {
                evt.Result.SpellLevelCost = 0;
            }
        }

        private static int GetCost(Metamagic metamagic, UnitEntityData unit, BlueprintAbility spell)
        {
            var state = unit.State.Features;
            int cost = metamagic.DefaultCost();

            if ((metamagic == Metamagic.Empower && state.FavoriteMetamagicEmpower)
                || (metamagic == Metamagic.Maximize && state.FavoriteMetamagicMaximize)
                || (metamagic == Metamagic.Quicken && state.FavoriteMetamagicQuicken)
                || (metamagic == Metamagic.Reach && state.FavoriteMetamagicReach)
                || (metamagic == Metamagic.Extend && state.FavoriteMetamagicExtend)
                || (metamagic == Metamagic.Selective && state.FavoriteMetamagicSelective)
                || (metamagic == Metamagic.Bolstered && state.FavoriteMetamagicBolstered))
                cost--;

            return cost;
        }
    }
}
