using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System;

namespace CodexLib
{
    /// <summary>
    /// Only the first buff found will check against the condition.
    /// </summary>
    [AllowedOn(typeof(BlueprintAbility))]
    public class AbilityRequirementHasBuffTimed : BlueprintComponent, IAbilityRestriction
    {
        public BlueprintBuff[] Buffs;
        public CompareType Compare;
        public TimeSpan TimeLeft;

        public bool IsAbilityRestrictionPassed(AbilityData ability)
        {
            Buff buff = null;
            foreach (var sub in Buffs)
            {
                buff = ability.Caster.Buffs.GetBuff(sub);
                if (buff != null) break;
            }

            if (buff == null)
                return Compare == CompareType.LessOrEqual || Compare == CompareType.LessThan;     // if there is no buff return true if the buff should have been less than TimeSpan

            switch(Compare)
            {
                case CompareType.GreaterThan:
                    if (buff.TimeLeft > TimeLeft)
                        return true;
                    break;
                case CompareType.GreaterOrEqual:
                    if (buff.TimeLeft >= TimeLeft)
                        return true;
                    break;
                case CompareType.LessThan:
                    if (buff.TimeLeft < TimeLeft)
                        return true;
                    break;
                case CompareType.LessOrEqual:
                    if (buff.TimeLeft <= TimeLeft)
                        return true;
                    break;
            }

            return false;
        }

        public string GetAbilityRestrictionUIText()
        {
            return (string)LocalizedTexts.Instance.Reasons.NoRequiredCondition;
        }
    }

    public enum CompareType
    {
        GreaterThan,
        GreaterOrEqual,
        LessThan,
        LessOrEqual
    }
}
