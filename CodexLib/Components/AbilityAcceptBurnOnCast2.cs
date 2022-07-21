
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Class.Kineticist;

namespace CodexLib
{
    public class AbilityAcceptBurnOnCast2 : BlueprintComponent, IAbilityRestriction, IAbilityOnCastLogic
    {
        public int BurnValue = 1;

        public bool IsFree(UnitEntityData unit)
        {
            return ElementalEmbodiment != null && unit.HasFact(ElementalEmbodiment);
        }

        public bool IsAbilityRestrictionPassed(AbilityData ability)
        {
            var unitPartKineticist = ability.Caster.Get<UnitPartKineticist>();
            return unitPartKineticist != null && this.BurnValue <= unitPartKineticist.LeftBurnThisRound || IsFree(ability.Caster.Unit);
        }

        public string GetAbilityRestrictionUIText()
        {
            return LocalizedTexts.Instance.Reasons.KineticNotEnoughBurnLeft;
        }

        public void OnCast(AbilityExecutionContext context)
        {
            if (IsFree(context.Caster))
                return;
            var unitPartKineticist = context.Caster.Get<UnitPartKineticist>();
            if (unitPartKineticist == null)
                return;
            unitPartKineticist.AcceptBurn(this.BurnValue, context.Ability);
        }

        public static BlueprintFeature ElementalEmbodiment;
    }
}
