using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{

    public class KineticMastery : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (evt.Weapon == null || evt.Weapon.Blueprint.Category != WeaponCategory.KineticBlast)
                return;

            if (evt.Weapon.Blueprint.AttackType.IsTouch())
                evt.AddModifier(evt.Initiator.Progression.MythicLevel / 2, this.Fact, ModifierDescriptor.UntypedStackable);
            else
                evt.AddModifier(evt.Initiator.Progression.MythicLevel, this.Fact, ModifierDescriptor.UntypedStackable);
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
        }
    }
}
