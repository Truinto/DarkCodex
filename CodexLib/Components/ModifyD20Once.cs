using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.RuleSystem.Rules.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    // TODO: Kingmaker.Designers.Mechanics.Facts.ModifyD20 has been updated
    public class ModifyD20Once : UnitFactComponentDelegate<ModifyD20Once.ComponentData>, IInitiatorRulebookHandler<RuleRollD20>, IUnitNewCombatRoundHandler
    {
        public AlignmentComponent Alignment;
        public bool AddBonus;
        public bool AddSavingThrowBonus;
        public bool AgainstAlignment;
        public bool DispellOn20;
        public bool DispellOnRerollFinished;
        public bool Replace;
        public bool RerollOnlyIfFailed;
        public bool RerollOnlyIfSuccess;
        public bool SpecificDescriptor;
        public bool SpecificSkill;
        public bool TakeBest;
        public bool TargetAlignment;
        public bool WithChance;
        public ContextValue Bonus;
        public ContextValue Chance;
        public ContextValue RollResult = 1;
        public ContextValue Value;
        public ContextValue ValueToCompareRoll;
        public int RollsAmount;
        public ModifierDescriptor BonusDescriptor;
        public ModifierDescriptor ModifierDescriptor;
        public FlaggedSavingThrowType m_SavingThrowType = FlaggedSavingThrowType.All;
        public ModifyD20.RollConditionType RollCondition;
        public RuleDispelMagic.CheckType DispellMagicCheckType;
        public RuleType Rule;
        public SpellDescriptorWrapper SpellDescriptor;
        public StatType[] Skill;

        public bool OncePerRound;
        public bool OncePerType = true;

        public ModifyD20Once()
        {
        }

        public ModifyD20Once(ModifyD20 md20, bool oncePerRound = true)
        {
            this.Alignment = md20.Alignment;
            this.AddBonus = md20.AddBonus;
            this.AddSavingThrowBonus = md20.AddSavingThrowBonus;
            this.AgainstAlignment = md20.AgainstAlignment;
            this.DispellOn20 = md20.DispellOn20;
            this.DispellOnRerollFinished = md20.DispellOnRerollFinished;
            this.Replace = md20.Replace;
            this.RerollOnlyIfFailed = md20.RerollOnlyIfFailed;
            this.RerollOnlyIfSuccess = md20.RerollOnlyIfSuccess;
            this.SpecificDescriptor = md20.SpecificDescriptor;
            this.SpecificSkill = md20.SpecificSkill;
            this.TakeBest = md20.TakeBest;
            this.TargetAlignment = md20.TargetAlignment;
            this.WithChance = md20.WithChance;
            this.Bonus = md20.Bonus;
            this.Chance = md20.Chance;
            this.RollResult = md20.RollResult;
            this.Value = md20.Value;
            this.ValueToCompareRoll = md20.ValueToCompareRoll;
            this.RollsAmount = md20.RollsAmount;
            this.BonusDescriptor = md20.BonusDescriptor;
            this.ModifierDescriptor = md20.ModifierDescriptor;
            this.m_SavingThrowType = md20.m_SavingThrowType;
            this.RollCondition = md20.RollCondition;
            this.DispellMagicCheckType = md20.DispellMagicCheckType;
            this.Rule = md20.Rule;
            this.SpellDescriptor = md20.SpellDescriptor;
            this.Skill = md20.Skill;

            this.OncePerRound = oncePerRound;
        }

        public void OnEventAboutToTrigger(RuleRollD20 evt)
        {
            var previousEvent = Rulebook.CurrentContext.PreviousEvent;
            if (previousEvent == null || !CheckRule(previousEvent))
                return;

            if (this.AgainstAlignment && !previousEvent.Initiator.Alignment.ValueRaw.HasComponent(this.Alignment))
            {
                if (this.TargetAlignment && !previousEvent.GetRuleTarget().Alignment.ValueRaw.HasComponent(this.Alignment))
                    return;
                if (!this.TargetAlignment && !previousEvent.Initiator.Alignment.ValueRaw.HasComponent(this.Alignment))
                    return;
            }

            if (!ShouldReroll(evt, previousEvent))
                return;

            if (this.WithChance && UnityEngine.Random.Range(1, 101) > this.Chance.Calculate(this.Fact.MaybeContext))
                return;

            var data = this.Data;

            if (this.OncePerRound)
            {
                var previousType = previousEvent.ToRuleType();
                if ((previousType & data.Triggered) != 0)
                    return;

                data.Triggered |= this.OncePerType ? previousType : RuleType.All;
            }

            data.Roll = evt;

            if (this.AddSavingThrowBonus)
            {
                CharacterStats stats = evt.Initiator.Stats;
                int value = this.Value.Calculate(this.Fact.MaybeContext);
                previousEvent.AddTemporaryModifier(stats.SaveWill.AddModifier(value, this.Runtime, this.ModifierDescriptor));
                previousEvent.AddTemporaryModifier(stats.SaveReflex.AddModifier(value, this.Runtime, this.ModifierDescriptor));
                previousEvent.AddTemporaryModifier(stats.SaveFortitude.AddModifier(value, this.Runtime, this.ModifierDescriptor));
            }

            if (this.AddBonus)
            {
                var stats2 = evt.Initiator.Stats;
                int value2 = this.Bonus.Calculate(this.Fact.MaybeContext);
                foreach (var modifiableValue in stats2.AllStats)
                {
                    previousEvent.AddTemporaryModifier(modifiableValue.AddModifier(value2, this.Runtime, this.BonusDescriptor));
                }
            }

            if (this.Replace)
                evt.Override(this.RollResult.Calculate(this.Context));
            else
                evt.AddReroll(this.RollsAmount, this.TakeBest, this.Fact);
        }

        public void OnEventDidTrigger(RuleRollD20 evt)
        {
            var data = this.Data;
            if (data.Roll == evt)
            {
                if ((evt.Result == 20 && this.DispellOn20) || this.DispellOnRerollFinished)
                    this.Owner.RemoveFact(this.Fact);
                data.Roll = null;
            }
        }

        public void HandleNewCombatRound(UnitEntityData unit)
        {
            this.Data.Triggered = 0;
            Helper.PrintDebug("ModifyD20Once new round");
        }

        private static bool IsRollFailed(RuleRollD20 rollRule, ISuccessable evt)
        {
            int d = rollRule.PreRollDice();
            return !evt.IsSuccessRoll(d);
        }

        private bool ShouldReroll(RuleRollD20 rollRule, RulebookEvent previousRule)
        {
            if (previousRule is ISuccessable successable)
            {
                if (this.RerollOnlyIfFailed)
                    return IsRollFailed(rollRule, successable);
                if (this.RerollOnlyIfSuccess)
                    return !IsRollFailed(rollRule, successable);
            }

            switch (this.RollCondition)
            {
                case ModifyD20.RollConditionType.ShouldBeMoreThan:
                    return rollRule.PreRollDice() > this.ValueToCompareRoll.Calculate(this.Context);
                case ModifyD20.RollConditionType.ShouldBeLessThan:
                    return rollRule.PreRollDice() < this.ValueToCompareRoll.Calculate(this.Context);
                case ModifyD20.RollConditionType.ShouldBeLessOrEqualThan:
                    return rollRule.PreRollDice() <= this.ValueToCompareRoll.Calculate(this.Context);
                case ModifyD20.RollConditionType.ShouldBeMoreOrEqualThan:
                    return rollRule.PreRollDice() >= this.ValueToCompareRoll.Calculate(this.Context);
                case ModifyD20.RollConditionType.Equal:
                    return rollRule.PreRollDice() == this.ValueToCompareRoll.Calculate(this.Context);
                default:
                    return true;
            }
        }

        private bool CheckRule(RulebookEvent rule)
        {
            return this.Rule == RuleType.All
                || (this.Rule & RuleType.AttackRoll) != 0 && rule is RuleAttackRoll
                || (this.Rule & RuleType.Concentration) != 0 && rule is RuleCheckConcentration
                || (this.Rule & RuleType.Initiative) != 0 && rule is RuleInitiativeRoll
                || (this.Rule & RuleType.SpellResistance) != 0 && rule is RuleSpellResistanceCheck
                || (this.Rule & RuleType.Maneuver) != 0 && rule is RuleCombatManeuver
                || IsSuitableSkillCheck(rule)
                || IsSuitableSavingThrow(rule)
                || IsSuitableDispelMagic(rule);
        }

        private bool IsSuitableSkillCheck(RulebookEvent rule)
        {
            return (this.Rule & RuleType.SkillCheck) != 0
                && rule is RuleSkillCheck ruleSkillCheck
                && (!this.SpecificSkill || this.Skill.HasItem(ruleSkillCheck.StatType));
        }

        private bool IsSuitableSavingThrow(RulebookEvent rule)
        {
            if ((this.Rule & RuleType.SavingThrow) == 0
                || rule is not RuleSavingThrow ruleSavingThrow)
                return false;

            var descriptor = rule.Reason.Context?.SpellDescriptor ?? Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.None;
            if (this.SpecificDescriptor && !descriptor.Intersects(this.SpellDescriptor))
                return false;

            var innerSavingThrowType = RuleSavingThrow.ConvertToFlaggedSavingThrowType(ruleSavingThrow.Type);
            return (this.m_SavingThrowType & innerSavingThrowType) != 0;
        }

        private bool IsSuitableDispelMagic(RulebookEvent rule)
        {
            return (this.Rule & RuleType.DispelMagic) != 0
                && rule is RuleDispelMagic ruleDispelMagic
                && (ruleDispelMagic.Check == this.DispellMagicCheckType || this.DispellMagicCheckType == RuleDispelMagic.CheckType.None);
        }

        public class ComponentData
        {
            public RuleType Triggered;
            public RuleRollD20 Roll;
        }
    }
}
