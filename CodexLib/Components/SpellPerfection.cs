using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class SpellPerfection : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookHandler<RuleSpellResistanceCheck>, IInitiatorRulebookHandler<RuleAttackRoll>
    {
        public static BlueprintFeatureReference SpellFocus = Helper.ToRef<BlueprintFeatureReference>("16fa59cc9a72a6043b566b49184f53fe");
        public static BlueprintFeatureReference SpellFocusGreater = Helper.ToRef<BlueprintFeatureReference>("5b04b45b228461c43bad768eb0f7c7bf");
        public static BlueprintFeatureReference SpellFocusMythic = Helper.ToRef<BlueprintFeatureReference>("41fa2470ab50ff441b4cfbb2fc725109");

        public static BlueprintFeatureReference WeaponFocus = Helper.ToRef<BlueprintFeatureReference>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
        public static BlueprintFeatureReference WeaponFocusGreater = Helper.ToRef<BlueprintFeatureReference>("09c9e82965fb4334b984a1e9df3bd088");
        public static BlueprintFeatureReference WeaponFocusMythic = Helper.ToRef<BlueprintFeatureReference>("74eb201774bccb9428ba5ac8440bf990");

        public static BlueprintFeatureReference SpellPenetration = Helper.ToRef<BlueprintFeatureReference>("ee7dc126939e4d9438357fbd5980d459");
        public static BlueprintFeatureReference SpellPenetrationGreater = Helper.ToRef<BlueprintFeatureReference>("1978c3f91cfbbc24b9c9b0d017f4beec");
        public static BlueprintFeatureReference SpellPenetrationMythic = Helper.ToRef<BlueprintFeatureReference>("51b6b22ff184eef46a675449e837365d");


        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            var bp = this.Param.Blueprint as BlueprintAbility;
            if (bp == null || bp != evt.Spell)
                return;
            var unit = evt.Initiator;

            if (unit.GetFeature(SpellFocusGreater, bp) != null)
                evt.AddBonusDC(unit.HasFact(SpellFocusMythic) ? 4 : 2);
            else if (unit.GetFeature(SpellFocus, bp) != null)
                evt.AddBonusDC(unit.HasFact(SpellFocusMythic) ? 2 : 1);
        }

        public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
        {
            var bp = this.Param.Blueprint as BlueprintAbility;
            if (bp != null && bp != evt.Ability)
                return;
            var unit = evt.Initiator;

            if (unit.HasFact(SpellPenetrationGreater))
                evt.AddModifier(unit.HasFact(SpellPenetrationMythic) ? unit.Progression.MythicLevel + 4 : 4, this.Fact);
            else if (unit.HasFact(SpellPenetration))
                evt.AddModifier(unit.HasFact(SpellPenetrationMythic) ? unit.Progression.MythicLevel / 2 + 2 : 2, this.Fact);
        }

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            var bp = this.Param.Blueprint as BlueprintAbility;
            if (bp != null && bp != this.Context.SourceAbility)
                return;
            var unit = evt.Initiator;

            if (unit.GetFeature(WeaponFocusGreater, WeaponCategory.Touch) != null || unit.GetFeature(WeaponFocusGreater, WeaponCategory.Ray) != null)
                evt.AddModifier(unit.HasFact(WeaponFocusMythic) ? 4 : 2, this.Fact);
            else if (unit.GetFeature(WeaponFocus, WeaponCategory.Touch) != null || unit.GetFeature(WeaponFocus, WeaponCategory.Ray) != null)
                evt.AddModifier(unit.HasFact(WeaponFocusMythic) ? 2 : 1, this.Fact);
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }

        public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
        {
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }
}
