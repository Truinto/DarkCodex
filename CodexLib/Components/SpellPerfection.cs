using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
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
using Kingmaker.Utility;
using Kingmaker.UnitLogic.Parts;

namespace CodexLib
{
    public class SpellPerfection : UnitFactComponentDelegate, IAfterRule, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookHandler<RuleSpellResistanceCheck>, IInitiatorRulebookHandler<RuleAttackRoll>
    {
        //public static BlueprintFeatureReference SpellFocus = Helper.ToRef<BlueprintFeatureReference>("16fa59cc9a72a6043b566b49184f53fe");
        //public static BlueprintFeatureReference SpellFocusGreater = Helper.ToRef<BlueprintFeatureReference>("5b04b45b228461c43bad768eb0f7c7bf");
        //public static BlueprintFeatureReference SpellFocusMythic = Helper.ToRef<BlueprintFeatureReference>("41fa2470ab50ff441b4cfbb2fc725109");

        //public static BlueprintFeatureReference WeaponFocus = Helper.ToRef<BlueprintFeatureReference>("1e1f627d26ad36f43bbd26cc2bf8ac7e");
        //public static BlueprintFeatureReference WeaponFocusGreater = Helper.ToRef<BlueprintFeatureReference>("09c9e82965fb4334b984a1e9df3bd088");
        //public static BlueprintFeatureReference WeaponFocusMythic = Helper.ToRef<BlueprintFeatureReference>("74eb201774bccb9428ba5ac8440bf990");

        //public static BlueprintFeatureReference SpellPenetration = Helper.ToRef<BlueprintFeatureReference>("ee7dc126939e4d9438357fbd5980d459");
        //public static BlueprintFeatureReference SpellPenetrationGreater = Helper.ToRef<BlueprintFeatureReference>("1978c3f91cfbbc24b9c9b0d017f4beec");
        //public static BlueprintFeatureReference SpellPenetrationMythic = Helper.ToRef<BlueprintFeatureReference>("51b6b22ff184eef46a675449e837365d");

        //public const SpellDescriptor AnyElement = SpellDescriptor.Fire | SpellDescriptor.Acid | SpellDescriptor.Cold | SpellDescriptor.Electricity | SpellDescriptor.Sonic;
        //public static BlueprintFeatureReference ElementalFocusAcid = Helper.ToRef<BlueprintFeatureReference>("52135eada006e9045a848cd659749608");
        //public static BlueprintFeatureReference ElementalFocusAcidGreater = Helper.ToRef<BlueprintFeatureReference>("49926dc94aca16145b6a608277b6f31c");
        //public static BlueprintFeatureReference ElementalFocusCold = Helper.ToRef<BlueprintFeatureReference>("2ed9d8bf76412ba4a8afe38fa9925fca");
        //public static BlueprintFeatureReference ElementalFocusColdGreater = Helper.ToRef<BlueprintFeatureReference>("f37a210a77d769c4ea2b23c22c07b83a");
        //public static BlueprintFeatureReference ElementalFocusFire = Helper.ToRef<BlueprintFeatureReference>("13bdf8d542811ac4ca228a53aa108145");
        //public static BlueprintFeatureReference ElementalFocusFireGreater = Helper.ToRef<BlueprintFeatureReference>("7a722c3e782aa5349a867c3516a2a4cf");
        //public static BlueprintFeatureReference ElementalFocusElectricity = Helper.ToRef<BlueprintFeatureReference>("d439691f37d17804890bd9c263ae1e80");
        //public static BlueprintFeatureReference ElementalFocusElectricityGreater = Helper.ToRef<BlueprintFeatureReference>("6a3be3df06f555d44a2b9dbfbcc2df23");

        //public static BlueprintFeatureReference SchoolMastery = Helper.ToRef<BlueprintFeatureReference>("ac830015569352b458efcdfae00a948c");

        public BlueprintAbility GetEligible(BlueprintAbility spell)
        {
            var bp = this.Param.Blueprint as BlueprintAbility;
            if (bp == null)
                return null;
            if (bp == spell)
                return spell;
            if (bp == spell.Parent)
                return spell.Parent;
            return null;
        }

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            var spell = GetEligible(evt.Spell);
            if (spell == null)
                return;

            int sum = 0;
            foreach (var mod in evt.m_BonusCasterLevel.Modifiers)
                if (mod.Descriptor == ModifierDescriptor.Feat)
                    sum += mod.Value;
            evt.AddBonusCasterLevel(sum);

            sum = 0;
            foreach (var mod in evt.m_BonusDC.Modifiers)
                if (mod.Descriptor == ModifierDescriptor.Feat)
                    sum += mod.Value;
            evt.AddBonusDC(sum);

            evt.m_BonusConcentration *= 2;


            //var spell = GetEligible(evt.Spell);
            //if (spell == null)
            //    return;
            //var unit = evt.Initiator;

            ////Spell Specialization
            //if (unit.HasParam<SpellSpecializationParametrized>(spell))
            //    evt.AddBonusCasterLevel(2);

            ////Elemental Focus
            //if (evt.TryGetCustomData<SpellDescriptor>(Const.KeyChangeElement, out var descriptor))
            //    descriptor = (spell.SpellDescriptor & ~AnyElement) | descriptor;
            //else
            //    descriptor = spell.SpellDescriptor;
            //if ((descriptor & SpellDescriptor.Fire) != 0 && unit.HasFact(ElementalFocusFire))
            //    evt.AddBonusDC(unit.HasFact(ElementalFocusFireGreater) ? 2 : 1);
            //else if ((descriptor & SpellDescriptor.Acid) != 0 && unit.HasFact(ElementalFocusAcid))
            //    evt.AddBonusDC(unit.HasFact(ElementalFocusAcidGreater) ? 2 : 1);
            //else if ((descriptor & SpellDescriptor.Cold) != 0 && unit.HasFact(ElementalFocusCold))
            //    evt.AddBonusDC(unit.HasFact(ElementalFocusColdGreater) ? 2 : 1);
            //else if ((descriptor & SpellDescriptor.Electricity) != 0 && unit.HasFact(ElementalFocusElectricity))
            //    evt.AddBonusDC(unit.HasFact(ElementalFocusElectricityGreater) ? 2 : 1);

            //// Expanded Arsenal
            //var school = spell.GetComponent<SpellComponent>()?.School ?? SpellSchool.None;
            //var arsenal = unit.Get<UnitPartExpandedArsenal>();
            //if (arsenal != null)
            //    foreach (var entry in arsenal.Entries)
            //        school |= entry.School;
            //if (school == SpellSchool.None)
            //    return;

            //// Spell Focus
            //if (unit.HasParam(SpellFocusGreater, school))
            //    evt.AddBonusDC(unit.HasFact(SpellFocusMythic) ? 4 : 2);
            //else if (unit.GetFeature(SpellFocus, school) != null)
            //    evt.AddBonusDC(unit.HasFact(SpellFocusMythic) ? 2 : 1);

            //// School Mastery
            //if (unit.HasParam<SchoolMasteryParametrized>(school))
            //    evt.AddBonusCasterLevel(1);
        }

        public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
        {
            var spell = GetEligible(evt.Ability);
            if (spell == null)
                return;

            int sum = 0;
            foreach (var mod in evt.m_AdditionalSpellPenetration.Modifiers)
                if (mod.Descriptor == ModifierDescriptor.Feat)
                    sum += mod.Value;
            evt.AddSpellPenetration(sum);

            //var spell = GetEligible(evt.Ability);
            //if (spell == null)
            //    return;
            //var unit = evt.Initiator;

            //// Spell Penetration
            //if (unit.HasFact(SpellPenetrationGreater))
            //    evt.AddModifier(unit.HasFact(SpellPenetrationMythic) ? unit.Progression.MythicLevel + 4 : 4, this.Fact);
            //else if (unit.HasFact(SpellPenetration))
            //    evt.AddModifier(unit.HasFact(SpellPenetrationMythic) ? unit.Progression.MythicLevel / 2 + 2 : 2, this.Fact);
        }

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            var spell = GetEligible(this.Context.SourceAbility);
            if (spell == null)
                return;

            int sum = 0;
            foreach (var mod in evt.m_ModifiableBonus.Modifiers)
                if (mod.Descriptor == ModifierDescriptor.Feat)
                    sum += mod.Value;
            evt.AddModifier(sum, this.Fact);

            //var spell = GetEligible(this.Context.SourceAbility);
            //if (spell == null)
            //    return;
            //var unit = evt.Initiator;

            //// Weapon Focus
            //if (unit.GetFeature(WeaponFocusGreater, WeaponCategory.Touch) != null || unit.GetFeature(WeaponFocusGreater, WeaponCategory.Ray) != null)
            //    evt.AddModifier(unit.HasFact(WeaponFocusMythic) ? 4 : 2, this.Fact);
            //else if (unit.GetFeature(WeaponFocus, WeaponCategory.Touch) != null || unit.GetFeature(WeaponFocus, WeaponCategory.Ray) != null)
            //    evt.AddModifier(unit.HasFact(WeaponFocusMythic) ? 2 : 1, this.Fact);
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
            //var spell = GetEligible(evt.Spell);
            //if (spell == null)
            //    return;

            //int sum = 0;
            //foreach (var mod in evt.m_BonusCasterLevel.Modifiers)
            //    if (mod.Descriptor == ModifierDescriptor.Feat)
            //        sum += mod.Value;
            //evt.Result.CasterLevel += sum;

            //sum = 0;
            //foreach (var mod in evt.m_BonusDC.Modifiers)
            //    if (mod.Descriptor == ModifierDescriptor.Feat)
            //        sum += mod.Value;
            //evt.Result.DC += sum;

            //evt.Result.Concentration += evt.m_BonusConcentration;
        }

        public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
        {
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }
}
