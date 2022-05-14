using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ScalingCMBonus : WeaponEnchantmentLogic, ISubscriber, IInitiatorRulebookSubscriber, ITargetRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateCMB>, IRulebookHandler<RuleCalculateCMB>
    {
        public void OnEventAboutToTrigger(RuleCalculateCMB evt)
        {
            if (evt.Type == this.Type)
            {
                int bonus = GameHelper.GetItemEnhancementBonus(this.Owner);
                evt.AddModifier(bonus, base.Fact, this.Descriptor);
            }
        }

        public void OnEventDidTrigger(RuleCalculateCMB evt)
        {
        }

        public CombatManeuver Type;

        public ModifierDescriptor Descriptor = ModifierDescriptor.UntypedStackable;
    }
}
