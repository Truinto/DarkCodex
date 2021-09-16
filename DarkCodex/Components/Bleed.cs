using Kingmaker.Blueprints;
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
    public class BleedBuff : UnitBuffComponentDelegate<BuffDamageStackingDiceData>, ITickEachRound, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleHealDamage>, IRulebookHandler<RuleHealDamage>
    {
        public override void OnActivate()
        {
        }

        public override void OnDeactivate()
        {
        }

        public void OnNewRound()
        {
            if (!this.Owner.IsInCombat)
                this.Owner.Buffs.RemoveFact(this.Buff);

            if (this.Data.Value == null)
                return;

            var damage = this.Data.Value.GetDirect();
            base.Context.TriggerRule(new RuleDealDamage(this.Owner, this.Owner, damage));
        }

        public void OnEventAboutToTrigger(RuleHealDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleHealDamage evt)
        {
            if (evt.Value > 0)
                this.Owner.Buffs.RemoveFact(this.Buff);
        }

        public void ApplyBleed(ContextDiceValue value, bool stacking, bool flensing)
        {
            if (this.Data.Value == null)
                this.Data.Value = DiceValue.Get(value, this.Context);
            else if (stacking)
                this.Data.Value.Increase(value, this.Context);

            var damage = this.Data.Value.GetDirect();
            base.Context.TriggerRule(new RuleDealDamage(this.Owner, this.Owner, damage));

            if (flensing)
            {
                var ac = this.Owner.Stats.GetStat(StatType.AC);
                int natural = ac.GetDescriptorBonus(ModifierDescriptor.NaturalArmor);
                natural += ac.GetDescriptorBonus(ModifierDescriptor.NaturalArmorEnhancement);
                natural += ac.GetDescriptorBonus(ModifierDescriptor.NaturalArmorForm);
                natural = Math.Max(0, natural);
                int malus = Math.Min(natural, value.DiceCountValue.Calculate(this.Context));

                if (malus > 0)
                {
                    var modifier = this.Owner.Stats.GetStat(StatType.AC).AddModifier(-malus, ModifierDescriptor.NaturalArmor);
                    modifier.StackMode = ModifiableValue.StackMode.ForceStack;
                    this.Buff.StoreModifier(modifier);
                }
            }
        }
    }

    public class BuffDamageStackingDiceData
    {
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

            Buff buff = this.Target.Unit.Buffs.GetBuff(BleedBlueprint);
            if (buff == null)
                buff = this.Target.Unit.Descriptor.AddBuff(BleedBlueprint, context);

            var bleed = buff.GetComponent<BleedBuff>();
            if (bleed == null)
                return;

            bleed.ApplyBleed(this.Value, this.IsStacking, this.IsFlensing);
            buff.Owner.Buffs.UpdateNextEvent();
        }

        public ContextActionIncreaseBleed()
        {
        }

        public ContextActionIncreaseBleed(bool flensing)
        {
            this.IsFlensing = flensing;
            Value = new ContextDiceValue() { 
                DiceCountValue = Helper.CreateContextValue(AbilityRankType.Default),
                DiceType = DiceType.D6,
                BonusValue = 0 };
        }

        public static BlueprintBuff createBleedBuff()
        {
            return Helper.CreateBlueprintBuff(
                "BleedVariableBuff",
                "Bleed",
                "This creature takes hit point damage each turn. Bleeding can be stopped through the application of any spell that cures hit point damage.",
                "e12fafba433448f8b71208b0162061fb",
                ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("75039846c3d85d940aa96c249b97e562").Icon
                ).SetComponents(
                new BleedBuff()
                );
        }

        public ContextDiceValue Value;
        public bool IsFlensing; // reduce natural armor by dice count
        public bool IsStacking; // false = apply higher value; true = add value
        public static readonly BlueprintBuff BleedBlueprint = createBleedBuff();
    }
}
