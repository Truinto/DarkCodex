using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;

namespace CodexLib
{
    // only really useful for checking move action
    [AllowedOn(typeof(BlueprintAbility))]
    public class AbilityRequirementActionAvailable : BlueprintComponent, IAbilityRestriction
    {
        public bool Not;
        public ActionType Action;
        public float Amount = 3f;   // note: move action 3f = 1 move action, 6f = 2 move actions

        public bool IsAbilityRestrictionPassed(AbilityData ability)
        {
            var cooldown = ability.Caster.Unit.CombatState.Cooldown;

            switch (Action)
            {
                case ActionType.Free:
                    return true ^ Not;
                case ActionType.Swift:
                    return (cooldown.SwiftAction + this.Amount <= 6f) ^ Not;
                case ActionType.Move:
                    return (cooldown.MoveAction + this.Amount <= 6f) ^ Not;
                case ActionType.Standard:
                    return (cooldown.StandardAction + this.Amount <= 6f) ^ Not;
                case ActionType.FullRound:
                    return (cooldown.SwiftAction + this.Amount <= 6f) ^ Not;
                default:
                    return true;
            }
        }

        public string GetAbilityRestrictionUIText()
        {
            return LocalizedTexts.Instance.Reasons.NoRequiredCondition;
        }
    }

    public enum ActionType
    {
        Free = UnitCommand.CommandType.Free,
        Swift = UnitCommand.CommandType.Swift,
        Move = UnitCommand.CommandType.Move,
        Standard = UnitCommand.CommandType.Standard,
        FullRound = 4
    }
}
