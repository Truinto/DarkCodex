using Kingmaker.Blueprints.Items.Ecnchantments;
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
    public class ButcheringAxeLogic : WeaponEnchantmentLogic, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
	{
		public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
		{
			if (evt.Weapon == base.Owner && evt.Initiator.Stats.Strength.ModifiedValue < 19)
			{
				evt.AddModifier(-2, base.Fact, ModifierDescriptor.UntypedStackable);
			}
		}

		public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
		{
		}
	}
}
