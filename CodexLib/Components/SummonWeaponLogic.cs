using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Craft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Logic for Flame Blade, Divine Trident, Produce Flame, and any other spell that grants a magical touch weapon.
    /// </summary>
    public class SummonWeaponLogic : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookHandler<RuleCalculateDamage>, IInitiatorRulebookHandler<RuleAttackRoll>
    {
        public BlueprintUnitFactReference FlameBladeDervish;
        public ContextValue DRReduction;
        public int Step;
        public int Max;

        /// <inheritdoc cref="SummonWeaponLogic"/>
        /// <param name="flameBladeDervish">type: <b>BlueprintUnitFact</b></param>
        /// <param name="drReduction">Amount of DR ignored, if creature has flameBladeDervish.</param>
        /// <param name="step">Bonus damage per caster level.</param>
        /// <param name="max">Maximal bonus damage.</param>
        public SummonWeaponLogic(AnyRef flameBladeDervish, ContextValue drReduction = null, int step = 2, int max = 20)
        {
            this.FlameBladeDervish = flameBladeDervish;
            this.DRReduction = drReduction;
            this.Step = step;
            this.Max = max;
        }

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt.Weapon != this.Owner)
                return;

            int max = this.Max;

            var metamagic = this.Owner.Get<CraftedItemPart>()?.MetamagicData;
            if (metamagic != null)
            {
                if (metamagic.Has(Const.Intensified))
                    max += 5;
            }

            int bonus = Math.Min(max, this.Owner.GetCasterLevel() / this.Step);
            evt.AddDamageModifier(bonus, this.Fact);

            if (this.FlameBladeDervish.NotEmpty() && evt.Initiator.HasFact(this.FlameBladeDervish))
            {
                evt.OverrideDamageBonusStat(evt.Initiator.GetStat(StatType.Intelligence, StatType.Wisdom, StatType.Charisma).Type);
                evt.OverrideDamageBonusStatMultiplier(1f);
            }

            evt.DoNotScaleDamage = true;
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt.Weapon != this.Owner)
                return;

            // remove strength bonus
            var dmg = evt.DamageDescription[0];
            var mod = dmg.m_Modifiers.m_Modifiers.FirstOrDefault(f => f.Stat == StatType.Strength && f.Fact == null);
            if (mod.Stat != 0)
            {
                dmg.m_Modifiers.m_Modifiers.Remove(mod);
                dmg.Bonus -= mod.Value;
            }
        }

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            if (evt.Reason.Item != this.Owner)
                return;

            var metamagic = this.Owner.Get<CraftedItemPart>()?.MetamagicData;
            if (metamagic == null)
                return;

            if (metamagic.Has(Metamagic.Maximize))
                evt.DamageBundle.First().CalculationType.Set(DamageCalculationType.Maximized, Metamagic.Maximize);

            if (metamagic.Has(Metamagic.Empower))
                evt.DamageBundle.First().EmpowerBonus.Set(1.5f, Metamagic.Empower);
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
            if (evt.Reason.Item != this.Owner)
                return;

            // reduce elemental DR
            if (this.FlameBladeDervish.NotEmpty() && evt.Initiator.HasFact(this.FlameBladeDervish) && this.DRReduction != null)
            {
                var damage = evt.CalculatedDamage.First();
                var source = damage.Source;

                int reduction;
                if (evt.Target.Descriptor.IsUndead)
                    reduction = this.DRReduction.Calculate(this.Context) * 3;
                else
                    reduction = this.DRReduction.Calculate(this.Context);
                reduction = Math.Min(source.ReductionBecauseResistance.TotalValue, reduction);
                if (reduction > 0)
                    source.ReductionBecauseResistance.Add(new Modifier(-reduction, this.Fact, ModifierDescriptor.UntypedStackable));
            }
        }

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt.Weapon != this.Owner)
                return;

            var ruleSpellResistance = new RuleSpellResistanceCheck(this.Context, evt.Target);
            ruleSpellResistance.DisableBattleLogSelf = false;
            bool spellResisted = Rulebook.Trigger(ruleSpellResistance).IsSpellResisted;
            if (spellResisted)
                evt.AutoMiss = true;
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }
}
