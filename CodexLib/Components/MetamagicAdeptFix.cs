using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    [AllowedOn(typeof(BlueprintBuff), true)]
    public class MetamagicAdeptFix : UnitBuffComponentDelegate, IInitiatorRulebookHandler<RuleCastSpell>
    {
        public BlueprintAbilityResourceReference Resource;

        public MetamagicAdeptFix(BlueprintAbilityResourceReference resource)
        {
            this.Resource = resource;
        }

        public override void OnActivate()
        {
            this.Owner.Retain(MechanicFeature.SpontaneousMetamagicNoFullRound);
        }

        public override void OnDeactivate()
        {
            this.Owner.Release(MechanicFeature.SpontaneousMetamagicNoFullRound);
        }

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {
            var spell = evt.Spell;
            if (spell.IsSpontaneous
                && spell.MetamagicData?.NotEmpty == true
                && !spell.Blueprint.IsFullRoundAction
                && !spell.HasMetamagic(Metamagic.Quicken))
            {
                int num = this.Owner.Resources.GetResourceAmount(this.Resource);
                if (num > 0)
                    this.Owner.Resources.Spend(this.Resource, 1);
                if (num <= 1)
                    this.Buff.Remove();
            }
        }
    }
}
