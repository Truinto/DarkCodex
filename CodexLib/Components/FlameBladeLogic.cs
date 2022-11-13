using Kingmaker.Blueprints.Items.Ecnchantments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class FlameBladeLogic : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookHandler<RuleCalculateDamage>
    {
        public BlueprintUnitFactReference FlameBladeDervish;
        public DamageTypeMix Type;

        /// <param name="flameBladeDervish">type: <b>BlueprintUnitFact</b></param>
        public FlameBladeLogic(BlueprintUnitFactReference flameBladeDervish, DamageTypeMix type, ContextValue amount = null)
        {
            this.FlameBladeDervish = flameBladeDervish;
            this.Type = type;
        }

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt.Weapon != this.Owner)
                return;

            if (evt.Initiator.HasFact(this.FlameBladeDervish))
            {
                evt.OverrideDamageBonusStat(evt.Initiator.GetStat(StatType.Intelligence, StatType.Wisdom, StatType.Charisma).Type);
                evt.OverrideDamageBonusStatMultiplier(1f);
            }
            
            int bonus = this.Owner.GetCasterLevel() / 2;
            evt.AddDamageModifier(bonus, this.Fact);
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt.Weapon != this.Owner)
                return;

            // remove strength bonus
            var dmg = evt.DamageDescription[0];
            var mod = dmg.m_Modifiers.m_Modifiers.FirstOrDefault(f => f.Stat == StatType.Strength && f.Fact == null);
            if (mod.Value != 0)
            {
                dmg.m_Modifiers.m_Modifiers.Remove(mod);
                dmg.Bonus -= mod.Value;
            }
        }

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
            if (evt.Reason.Item != this.Owner)
                return;

            foreach (var damage in evt.CalculatedDamage)
            {
                var source = damage.Source;
                if (source.Type >= DamageType.Direct)
                    continue;

                if ((source.ToDamageTypeMix() & this.Type) == 0)
                    continue;

                int reduction = Math.Min(source.ReductionBecauseResistance.TotalValue, evt.Target.Descriptor.IsUndead ? 30 : 10);
                if (reduction <= 0)
                    continue;

                // reduce DR
                source.ReductionBecauseResistance.Add(new Modifier(-reduction, this.Fact, ModifierDescriptor.UntypedStackable));
            }
        }
    }
}
