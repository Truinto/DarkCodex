using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class KineticBlastEnhancement : WeaponEnchantmentLogic, IInitiatorRulebookSubscriber,
        IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IRulebookHandler<RuleCalculateAttackBonusWithoutTarget>,
        IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>,
        IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>
    {
        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (evt.Weapon == this.Owner)
                return;
            if (evt.Weapon.Blueprint.Category == WeaponCategory.KineticBlast)
                evt.AddModifier(CalculateBonus(), this.Fact);
        }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            if (evt.SourceAbility == null || !evt.SourceAbility.GetComponent<AbilityKineticist>())
                return;

            evt.DamageBundle.First?.AddModifier(CalculateBonus(), this.Fact);
        }

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (evt.Spell == null || !evt.Spell.GetComponent<AbilityKineticist>())
                return;
            int bonus = CalculateBonus();
            evt.AddBonusDC(bonus);
            evt.AddBonusCasterLevel(bonus);
        }

        public int CalculateBonus()
        {
            var all = this.Owner.Enchantments.SelectMany(p => p.Blueprint.GetComponents<WeaponEnhancementBonus>()).ToList();
            var stacking = all.Where(p => p.Stack).ToList();
            var notstacking = all.Where(p => !p.Stack).ToList();

            int total = !notstacking.Any() ? 0 : notstacking.Max(p => p.EnhancementBonus);
            int sum = !stacking.Any() ? 0 : stacking.Sum(p => p.EnhancementBonus);
            if (total >= 5)
                return total;
            return Math.Min(total + sum, 5);
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
        }
        public void OnEventDidTrigger(RuleDealDamage evt)
        {
        }
        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }
}
