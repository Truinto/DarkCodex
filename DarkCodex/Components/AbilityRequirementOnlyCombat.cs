using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;


namespace DarkCodex.Components
{
    [AllowedOn(typeof(BlueprintAbility))]
    public class AbilityRequirementOnlyCombat : BlueprintComponent, IAbilityRestriction
    {
        public bool Not;

        public bool IsAbilityRestrictionPassed(AbilityData ability)
        {
            return ability.Caster.Unit.IsInCombat ^ this.Not;
        }

        public string GetAbilityRestrictionUIText()
        {
            return (string)LocalizedTexts.Instance.Reasons.NoRequiredCondition;
        }
    }
}