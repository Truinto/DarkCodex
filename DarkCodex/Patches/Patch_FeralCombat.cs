using HarmonyLib;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Shared;
using CodexLib;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Feral Combat", "collection of patches for Feral Combat Training", false)]
    [HarmonyPatch]
    public class Patch_FeralCombat
    {
        /* 
         * Glossary:
         * - BA = base attack bonus attacks
         * - claw = claw attack (overlaps with weapon slots)
         * - EX = bite, hoves, tail, tentacle, gore, ...
         * - 1H = one handed weapon
         * - 1H2 = one handed weapon in two hands
         * - 2H = two handed (either hand)
         * - 1HM, 2HM = one or two handed monk weapon
         * - TWF = two weapon fighting
         * - IUS = improved unarmed strike (never overlaps)
         * - FB = flurry of blows (equals BA+1; never combines with EX nor TWF)
         * - FC = feral combat
         * 
         * Possible attack pattern by RAW   : Chosen when (in theory)
         * - claw, claw, EX                 : no weapons
         * - 1H (+BA), claw, EX             : one weapon + OneHandedToggleAbility
         * - 1H2 (+BA), EX                  : one weapon
         * - 1H (+BA), 1H (+TWF), EX        : two weapons
         * - 2H (+BA), EX                   : one two-handed weapon
         * - 1HM (FB)                       : FlurryToggleAbility
         * - 2HM (FB)                       : FlurryToggleAbility
         * - claw or EX (FB only with FC)   : no weapons + FlurryToggleAbility
         * 
         * Note:
         * 1) Owlcat does not implement this logic correctly. Flurry simply grants one extra attack (two at level 11), if no armor is worn and only monk weapons are wielded.
         *    That means monks can do additional natural attacks (any but claws) during flurry, which is illegal.
         * 
         * https://github.com/Vek17/TabletopTweaks-Base/blob/master/TabletopTweaks-Base/NewContent/BaseAbilities/OneHandedToggleAbility.cs
         */

        //search for: ImprovedUnarmedStrike
        //skip: PummelingCharge, Patch_DeflectArrows_CheckRestriction_Patch
        //OK: AddInitiatorAttackWithWeaponTrigger		AddInitiatorAttackWithWeaponTriggerOrFeralTraining
        //OK: AbilityCasterMainWeaponCheck			AbilityCasterMainWeaponCheckOrFeralCombat
        //OK: MonkNoArmorAndMonkWeaponFeatureUnlock	MonkNoArmorAndMonkWeaponOrFeralCombatFeatureUnlock
        //OK: AdditionalStatBonusOnAttackDamage		AdditionalStatBonusOnAttackDamageOrFeralCombat

        [HarmonyPatch(typeof(AddInitiatorAttackWithWeaponTrigger), nameof(AddInitiatorAttackWithWeaponTrigger.IsSuitable), typeof(RuleAttackWithWeapon))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) // general & styles strikes
        {
            var data = new TranspilerTool(instructions, generator, original);
            data.Seek(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.Category));
            data.InsertAfter(patch1);
            data.First().Seek(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.Type));
            data.InsertAfter(patch2);
            return data.Code;

            static WeaponCategory patch1(WeaponCategory category, AddInitiatorAttackWithWeaponTrigger __instance, RuleAttackWithWeapon evt)
            {
                if (__instance.Category == WeaponCategory.UnarmedStrike && evt.Weapon.Blueprint.IsNatural && evt.Initiator.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat))
                    return WeaponCategory.UnarmedStrike;
                return category;
            }

            static BlueprintWeaponType patch2(BlueprintWeaponType weaponType, AddInitiatorAttackWithWeaponTrigger __instance, RuleAttackWithWeapon evt)
            {
                if (__instance.WeaponType == Resource.Cache.WeaponTypeUnarmed.Get() && evt.Weapon.Blueprint.IsNatural && evt.Initiator.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat))
                    return Resource.Cache.WeaponTypeUnarmed.Get();
                return weaponType;
            }
        }


        [HarmonyPatch(typeof(AbilityCasterMainWeaponCheck), nameof(AbilityCasterMainWeaponCheck.IsCasterRestrictionPassed))]
        [HarmonyPostfix]
        public static void Postfix2(UnitEntityData caster, AbilityCasterMainWeaponCheck __instance, ref bool __result) // stunning fist & co
        {
            __result = __result ||
                caster.Body.PrimaryHand.Weapon.Blueprint.IsNatural
                && __instance.Category.Contains(WeaponCategory.UnarmedStrike)
                && caster.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat);
        }


        [HarmonyPatch(typeof(MonkNoArmorAndMonkWeaponFeatureUnlock), nameof(MonkNoArmorAndMonkWeaponFeatureUnlock.CheckEligibility))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler3(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) // flurry of blows
        {
            var data = new TranspilerTool(instructions, generator, original);
            data.ReplaceAllCalls(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.IsMonk), patch);
            return data.Code;

            static bool patch(BlueprintItemWeapon weapon, MonkNoArmorAndMonkWeaponFeatureUnlock __instance)
            {
                return weapon.IsMonk || (weapon.IsNatural && __instance.Owner.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat));
            }
        }


        [HarmonyPatch(typeof(AdditionalStatBonusOnAttackDamage), nameof(AdditionalStatBonusOnAttackDamage.CheckConditions), typeof(RuleCalculateWeaponStats))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler4(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) // dragon style
        {
            var data = new TranspilerTool(instructions, generator, original);
            data.Seek(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.Category));
            data.InsertAfter(patch);
            return data.Code;

            static WeaponCategory patch(WeaponCategory category, AdditionalStatBonusOnAttackDamage __instance, RuleCalculateWeaponStats evt)
            {
                if (__instance.Category == WeaponCategory.UnarmedStrike && evt.Weapon.Blueprint.IsNatural && evt.Initiator.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat))
                    return WeaponCategory.UnarmedStrike;
                return category;
            }
        }


        [HarmonyPatch(typeof(AdditionalDiceOnAttack), nameof(AdditionalDiceOnAttack.CheckCondition), typeof(RuleAttackRoll))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler5(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) // elemental fist
        {
            var data = new TranspilerTool(instructions, generator, original);
            data.Seek(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.Category));
            data.InsertAfter(patch);
            return data.Code;

            static WeaponCategory patch(WeaponCategory category, AdditionalDiceOnAttack __instance, RuleAttackRoll evt)
            {
                if (__instance.Category == WeaponCategory.UnarmedStrike && evt.Weapon.Blueprint.IsNatural && evt.Initiator.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat))
                    return WeaponCategory.UnarmedStrike;
                return category;
            }
        }


        [HarmonyPatch(typeof(AdditionalDiceOnAttack), nameof(AdditionalDiceOnAttack.CheckCondition), typeof(RuleAttackWithWeapon))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler6(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) // elemental fist
        {
            var data = new TranspilerTool(instructions, generator, original);
            data.Seek(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.Category));
            data.InsertAfter(patch);
            return data.Code;

            static WeaponCategory patch(WeaponCategory category, AdditionalDiceOnAttack __instance, RuleAttackWithWeapon evt)
            {
                if (__instance.Category == WeaponCategory.UnarmedStrike && evt.Weapon.Blueprint.IsNatural && evt.Initiator.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat))
                    return WeaponCategory.UnarmedStrike;
                return category;
            }
        }


        [HarmonyPatch(typeof(AddOutgoingPhysicalDamageProperty), nameof(AddOutgoingPhysicalDamageProperty.OnEventAboutToTrigger), typeof(RulePrepareDamage))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler7(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) // Ki Strike
        {
            var data = new TranspilerTool(instructions, generator, original);
            data.Seek(typeof(AddOutgoingPhysicalDamageProperty), nameof(AddOutgoingPhysicalDamageProperty.CheckWeaponType));
            data.Seek(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.Type));
            data.InsertAfter(patch);
            return data.Code;

            static BlueprintWeaponType patch(BlueprintWeaponType weaponType, AddOutgoingPhysicalDamageProperty __instance, RulePrepareDamage evt)
            {
                if (__instance.WeaponType == Resource.Cache.WeaponTypeUnarmed.Get()
                    && evt.DamageBundle.Weapon.Blueprint.IsNatural
                    && evt.Initiator.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat))
                    return Resource.Cache.WeaponTypeUnarmed.Get();
                return weaponType;
            }
        }


        [HarmonyPatch(typeof(IgnoreDamageReductionOnAttack), nameof(IgnoreDamageReductionOnAttack.CheckCondition), typeof(RuleAttackWithWeapon))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler8(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) // Shattering Punch
        {
            var data = new TranspilerTool(instructions, generator, original);
            data.Seek(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.Type));
            data.InsertAfter(patch);
            return data.Code;

            static BlueprintWeaponType patch(BlueprintWeaponType weaponType, IgnoreDamageReductionOnAttack __instance, RuleAttackWithWeapon evt)
            {
                if (__instance.WeaponType == Resource.Cache.WeaponTypeUnarmed.Get()
                    && evt.Weapon.Blueprint.IsNatural
                    && evt.Initiator.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat))
                    return Resource.Cache.WeaponTypeUnarmed.Get();
                return weaponType;
            }
        }


        [HarmonyPatch(typeof(MythicUnarmedStrike), nameof(MythicUnarmedStrike.OnEventAboutToTrigger), typeof(RuleCalculateWeaponStats))]
        [HarmonyPostfix]
        public static void Postfix9(RuleCalculateWeaponStats evt, MythicUnarmedStrike __instance) // Mythic Improved Unarmed Strike
        {
            if (!evt.Weapon.Blueprint.IsUnarmed
                && evt.Weapon.Blueprint.IsNatural
                && evt.Initiator.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat))
                evt.AddDamageModifier(evt.Initiator.Progression.MythicLevel / 2, __instance.Fact);
        }
    }

}
