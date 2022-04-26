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

namespace DarkCodex.Components
{

    [AllowedOn(typeof(BlueprintUnitFact), false)]
    public class BleedBuff : UnitBuffComponentDelegate, ITickEachRound, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleHealDamage>, IRulebookHandler<RuleHealDamage>
    {
        public override void OnActivate()
        {
        }

        public override void OnDeactivate()
        {
        }

        public void OnNewRound()
        {
            if (this.Value == null)
                return;

            var damage = this.Value.GetDirect();
            var ruleDamage = new RuleDealDamage(this.Owner, this.Owner, damage);
            base.Context.TriggerRule(ruleDamage);

            Helper.PrintDebug($"BleedBuff.OnNewRound {Value.Dice}d{(int)Value.DiceType}+{Value.Bonus} result={ruleDamage.Result}");

            if (!this.Owner.IsInCombat)
                this.Owner.Buffs.RemoveFact(this.Buff);
        }

        public void OnEventAboutToTrigger(RuleHealDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleHealDamage evt)
        {
            if (evt.Value > 0 && evt.HealFormula != DiceFormula.Zero)
                this.Owner.Buffs.RemoveFact(this.Buff);
        }

        [JsonProperty]
        public DiceValue Value;
    }

    public class ContextActionIncreaseBleed : ContextAction
    {
        public override string GetCaption()
        {
            return "";
        }

        public override void RunAction()
        {
            var context = ContextData<MechanicsContext.Data>.Current?.Context;
            if (context == null)
                return;

            //if (context.SourceAbility.IsSpell) return;

            Buff buff = this.Target.Unit.Buffs.GetBuff(Resource.Cache.BuffBleed);
            if (buff == null)
                buff = this.Target.Unit.Descriptor.AddBuff(Resource.Cache.BuffBleed, context);

            var bleed = buff.GetComponent<BleedBuff>();
            if (bleed == null)
                return;

            ApplyBleed(buff, bleed);
            //buff.Owner.Buffs.UpdateNextEvent();
        }

        public void ApplyBleed(Buff buff, BleedBuff bleed)
        {
            var caster = this.Context.MaybeCaster;
            var target = this.Target.Unit;

            //this.Context[AbilityRankType.Default] = caster.Stats.SneakAttack.ModifiedValue;
            int rank = this.Context[AbilityRankType.Default];
            int sneak = caster.Stats.SneakAttack.ModifiedValue;

            if (bleed.Value == null)
                bleed.Value = DiceValue.Get(this.Value, this.Context);
            else if (this.IsStacking)
                bleed.Value.Increase(this.Value, this.Context);

            var damage = bleed.Value.GetDirect(); // maybe use physical, if still conflicting with energy attacks
            var ruleDamage = new RuleDealDamage(caster, target, damage);
            this.Context.TriggerRule(ruleDamage);

            Helper.PrintDebug($"ContextActionIncreaseBleed.ApplyBleed {Value.DiceCountValue}d{(int)Value.DiceType}+{Value.BonusValue} rank={rank} sneak={sneak} result={ruleDamage.Result}");

            if (this.IsFlensing)
            {
                var ac = target.Stats.GetStat(StatType.AC);
                int natural = ac.GetDescriptorBonus(ModifierDescriptor.NaturalArmor);
                natural += ac.GetDescriptorBonus(ModifierDescriptor.NaturalArmorEnhancement);
                natural += ac.GetDescriptorBonus(ModifierDescriptor.NaturalArmorForm);
                natural = Math.Max(0, natural);
                int value = this.Value.Calculate(this.Context);
                int malus = Math.Min(natural, value);

                Helper.PrintDebug($" -flensing natural={natural} value={value} malus={malus}");

                if (malus > 0)
                {
                    var modifier = buff.m_StoredMods?.FirstOrDefault();
                    if (modifier == null)
                    {
                        modifier = ac.AddModifier(-malus, ModifierDescriptor.NaturalArmor);
                        modifier.StackMode = ModifiableValue.StackMode.ForceStack;
                        buff.StoreModifier(modifier);
                    }
                    else
                    {
                        modifier.ModValue -= malus;
                    }
                }
            }
        }

        public ContextActionIncreaseBleed()
        {
        }

        public ContextActionIncreaseBleed(bool flensing)
        {
            this.IsFlensing = flensing;
            this.Value = new ContextDiceValue() { 
                DiceCountValue = 0,
                DiceType = DiceType.Zero,
                BonusValue = Helper.CreateContextValue(AbilityRankType.Default)
            };
        }

        internal static void CreateBleedBuff()
        {
            var buff = Helper.CreateBlueprintBuff(
                "BleedVariableBuff",
                "Bleed",
                "This creature takes hit point damage each turn. Bleeding can be stopped through the application of any spell that cures hit point damage.",
                "e12fafba433448f8b71208b0162061fb",
                Helper.StealIcon("75039846c3d85d940aa96c249b97e562")
                ).SetComponents(
                new BleedBuff(),
                Helper.CreateSpellDescriptorComponent(SpellDescriptor.Bleed)
                );

            Resource.Cache.BuffBleed.SetReference(buff);
        }

        public ContextDiceValue Value;
        public bool IsFlensing; // reduce natural armor by dice count
        public bool IsStacking; // false = apply higher value; true = add value
    }
}
