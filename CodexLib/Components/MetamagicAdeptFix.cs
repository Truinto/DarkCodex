using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
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
        public override void OnActivate() // TODO: Metamagic Adept
        {
            //https://www.d20pfsrd.com/classes/core-classes/sorcerer/bloodlines/bloodlines-from-paizo/arcane-bloodline/
            this.Owner.AddFlags(MechanicFeature.SpontaneousMetamagicNoFullRound);
        }

        public override void OnDeactivate()
        {
            this.Owner.RemoveFlags(MechanicFeature.SpontaneousMetamagicNoFullRound);
        }

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {
            this.Buff.Remove();
        }
    }
}
