using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodexLib
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class BleedBuff : UnitBuffComponentDelegate<BleedBuff.RuntimeData>, ITickEachRound, ITargetRulebookHandler<RuleHealDamage>
    {
        public override void OnActivate()
        {
        }

        public override void OnDeactivate()
        {
        }

        public void OnNewRound()
        {
            var damage = this.Data.Value?.GetDirect();
            if (damage == null)
                return;

            var ruleDamage = new RuleDealDamage(this.Owner, this.Owner, damage);
            Rulebook.Trigger(ruleDamage);

            Helper.PrintDebug($"BleedBuff.OnNewRound result={ruleDamage.Result}");

            if (!this.Owner.IsInCombat)
                this.Owner.Buffs.RemoveFact(this.Buff);
        }

        public void OnEventAboutToTrigger(RuleHealDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleHealDamage evt)
        {
            if (evt.Value > 0 && evt.HealFormula.BaseFormula != DiceFormula.Zero)
                this.Owner.Buffs.RemoveFact(this.Buff);
        }

        public void Apply(UnitEntityData caster, UnitEntityData target, DiceValue dice, bool isStacking, bool isFlensing)
        {
            if (caster == null || target == null)
                return;

            DiceValue bleedValue;
            var data = this.Data;
            if (data.Value == null)
                bleedValue = data.Value = dice;
            else if (isStacking)
                bleedValue = data.Value.Increase(dice);
            else
                bleedValue = data.Value.Max(dice);

            var damage = bleedValue.GetDirect(); // maybe use physical, if still conflicting with energy attacks
            var ruleDamage = new RuleDealDamage(caster, target, damage);
            Rulebook.Trigger(ruleDamage);

            Helper.PrintDebug($"BleedBuff.Apply {dice} result={ruleDamage.Result}");

            if (isFlensing)
            {
                var ac = target.Stats.GetStat(StatType.AC);
                int natural = ac.GetDescriptorBonus(ModifierDescriptor.NaturalArmor);
                natural += ac.GetDescriptorBonus(ModifierDescriptor.NaturalArmorEnhancement);
                natural += ac.GetDescriptorBonus(ModifierDescriptor.NaturalArmorForm);
                natural = Math.Max(0, natural);
                int value = dice.Roll();
                int malus = Math.Min(natural, value);

                Helper.PrintDebug($" -flensing natural={natural} value={value} malus={malus}");

                if (malus > 0)
                {
                    var modifier = this.Buff.m_StoredMods?.FirstOrDefault();
                    if (modifier == null)
                    {
                        this.Buff.StoreModifier(ac.AddModifier(new ModifiableValue.Modifier
                        {
                            ModValue = -malus,
                            ModDescriptor = ModifierDescriptor.NaturalArmor,
                            StackMode = ModifiableValue.StackMode.ForceStack,
                            AppliedTo = ac,
                            Source = this.Fact,
                            SourceComponent = this.name
                        }));
                    }
                    else
                    {
                        modifier.ModValue -= malus;
                        ac.UpdateValue();
                    }
                }
            }
        }

        public class RuntimeData
        {
            public DiceValue Value;
        }

        /// <summary>type: <b>BlueprintBuff</b></summary>
        public static AnyRef BuffBleed = "e12fafba433448f8b71208b0162061fb";

        public static void Create()
        {
            if (BuffBleed.Cached != null)
                return;

            Helper.ForceOverwriteGuid("e12fafba433448f8b71208b0162061fb");
            var result = Helper.CreateBlueprintBuff(
                "BleedVariableBuff",
                "Bleed",
                "This creature takes hit point damage each turn. Bleeding can be stopped through the application of any spell that cures hit point damage.",
                Helper.StealIcon("75039846c3d85d940aa96c249b97e562")
                ).SetComponents(
                new BleedBuff(),
                Helper.CreateSpellDescriptorComponent(SpellDescriptor.Bleed)
                );

            BuffBleed.Set(result);
        }

    }

    public class ContextActionIncreaseBleed : ContextAction
    {
        public ContextDiceValue Value;
        public bool IsFlensing; // reduce natural armor by dice count
        public bool IsStacking; // false = apply higher value; true = add value

        public ContextActionIncreaseBleed()
        {
        }

        public ContextActionIncreaseBleed(bool flensing = false)
        {
            this.IsFlensing = flensing;
            this.Value = new ContextDiceValue()
            {
                DiceCountValue = 0,
                DiceType = DiceType.Zero,
                BonusValue = Helper.CreateContextValue(AbilityRankType.Default)
            };
        }

        public override string GetCaption() => nameof(ContextActionIncreaseBleed);

        public override void RunAction()
        {
            Buff buff = this.Target.Unit.Buffs.GetBuff(BleedBuff.BuffBleed);
            buff ??= this.Target.Unit.Descriptor.AddBuff(BleedBuff.BuffBleed, this.Context);
            if (buff == null)
                return;

            Helper.PrintDebug($"ContextActionIncreaseBleed {this.Value} rank={this.Context[AbilityRankType.Default]} sneak={this.Context.MaybeCaster.Stats.SneakAttack.ModifiedValue}");

            buff.CallComponents<BleedBuff>(c => c.Apply(this.Context.MaybeCaster, this.Target.Unit, this.Value, this.IsStacking, this.IsFlensing));

            //ApplyBleed(buff, bleed);
            //buff.Owner.Buffs.UpdateNextEvent();
        }

        //[Obsolete]
        //public void ApplyBleed(Buff buff, BleedBuff bleed)
        //{
        //    var caster = this.Context.MaybeCaster;
        //    var target = this.Target.Unit;

        //    //this.Context[AbilityRankType.Default] = caster.Stats.SneakAttack.ModifiedValue;
        //    int rank = this.Context[AbilityRankType.Default];
        //    int sneak = caster.Stats.SneakAttack.ModifiedValue;

        //    DiceValue bleedValue;
        //    if (bleed.Value == null)
        //        bleedValue = bleed.Value = DiceValue.Get(this.Value, this.Context);
        //    else if (this.IsStacking)
        //        bleedValue = bleed.Value.Increase(this.Value, this.Context);
        //    else
        //        bleedValue = bleed.Value.Max(this.Value, this.Context);

        //    var damage = bleedValue.GetDirect(); // maybe use physical, if still conflicting with energy attacks
        //    var ruleDamage = new RuleDealDamage(caster, target, damage);
        //    this.Context.TriggerRule(ruleDamage);

        //    Helper.PrintDebug($"ContextActionIncreaseBleed.ApplyBleed {Value.DiceCountValue}d{(int)Value.DiceType}+{Value.BonusValue} rank={rank} sneak={sneak} result={ruleDamage.Result}");

        //    if (this.IsFlensing)
        //    {
        //        var ac = target.Stats.GetStat(StatType.AC);
        //        int natural = ac.GetDescriptorBonus(ModifierDescriptor.NaturalArmor);
        //        natural += ac.GetDescriptorBonus(ModifierDescriptor.NaturalArmorEnhancement);
        //        natural += ac.GetDescriptorBonus(ModifierDescriptor.NaturalArmorForm);
        //        natural = Math.Max(0, natural);
        //        int value = this.Value.Calculate(this.Context);
        //        int malus = Math.Min(natural, value);

        //        Helper.PrintDebug($" -flensing natural={natural} value={value} malus={malus}");

        //        if (malus > 0)
        //        {
        //            var modifier = buff.m_StoredMods?.FirstOrDefault();
        //            if (modifier == null)
        //            {
        //                modifier = ac.AddModifier(-malus, ModifierDescriptor.NaturalArmor);
        //                modifier.StackMode = ModifiableValue.StackMode.ForceStack;
        //                buff.StoreModifier(modifier);
        //            }
        //            else
        //            {
        //                modifier.ModValue -= malus;
        //                ac.UpdateValue();
        //            }
        //        }
        //    }
        //}
    }
}
