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

        //[HarmonyPatch(typeof(AddInitiatorAttackWithWeaponTrigger), nameof(AddInitiatorAttackWithWeaponTrigger.IsSuitable), typeof(RuleAttackWithWeapon))]
        //[HarmonyTranspiler]
        //public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instr)
        //{
        //    List<CodeInstruction> list = instr.ToList();
        //    MethodInfo reference = AccessTools.PropertyGetter(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.Category));
        //    int index = list.FindIndex(f => f.Calls(reference)) + 2; //68
        //    int label = index - 1;
        //    //for (int i = 64; i <= 70; i++) Helper.PrintInstruction(list[i], i.ToString());
        //    Main.Print("Patching at " + index);
        //    list.Insert(index++, new CodeInstruction(OpCodes.Ldarg_0));
        //    list.Insert(index++, new CodeInstruction(OpCodes.Ldarg_1));
        //    list.Insert(index++, CodeInstruction.Call(typeof(Patch_FeralCombat), nameof(Patch1)));
        //    list.Insert(index++, new CodeInstruction(OpCodes.Brtrue_S, list[label].operand)); //label=11
        //    return list;
        //}
        //public static bool Patch1(AddInitiatorAttackWithWeaponTrigger instance, RuleAttackWithWeapon evt)
        //{
        //    return instance.Category == WeaponCategory.UnarmedStrike && evt.Weapon.Blueprint.IsNatural && evt.Initiator.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat);
        //}
        [HarmonyPatch(typeof(AddInitiatorAttackWithWeaponTrigger), nameof(AddInitiatorAttackWithWeaponTrigger.IsSuitable), typeof(RuleAttackWithWeapon))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerData(instructions, generator, original);
            data.Seek(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.Category));
            data.InsertAfter(Patch1);
            return data.Code;
        }
        public static WeaponCategory Patch1(WeaponCategory category, AddInitiatorAttackWithWeaponTrigger __instance, RuleAttackWithWeapon evt)
        {
            if (__instance.Category == WeaponCategory.UnarmedStrike && evt.Weapon.Blueprint.IsNatural && evt.Initiator.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat))
                return WeaponCategory.UnarmedStrike;
            return category;
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


        //[HarmonyPatch(typeof(MonkNoArmorAndMonkWeaponFeatureUnlock), nameof(MonkNoArmorAndMonkWeaponFeatureUnlock.CheckEligibility))]
        //[HarmonyPrefix]
        //public static bool Prefix3(MonkNoArmorAndMonkWeaponFeatureUnlock __instance) // flurry of blows
        //{
        //    if (!__instance.Owner.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat))
        //        return true;
        //    if (__instance.IsZenArcher)
        //        return true;
        //    var body = __instance.Owner.Body;
        //    if (__instance.IsSohei)
        //    {
        //        if (!body.SecondaryHand.HasShield
        //        && (!body.Armor.HasArmor || body.Armor.Armor.Blueprint.ProficiencyGroup == ArmorProficiencyGroup.Light)
        //        && (body.PrimaryHand.Weapon.Blueprint.IsMonk || body.PrimaryHand.Weapon.Blueprint.IsNatural) || (bool)__instance.Owner.Get<UnitPartWeaponTraining>()?.IsSuitableWeapon(body.PrimaryHand.MaybeWeapon))
        //            __instance.AddFact();
        //        else
        //            __instance.RemoveFact();
        //        return false;
        //    }
        //    if (!body.SecondaryHand.HasShield
        //    && (!body.Armor.HasArmor || !body.Armor.Armor.Blueprint.IsArmor)
        //    && (body.PrimaryHand.Weapon.Blueprint.IsMonk || body.PrimaryHand.Weapon.Blueprint.IsNatural))
        //        __instance.AddFact();
        //    else
        //        __instance.RemoveFact();
        //    return false;
        //}
        [HarmonyPatch(typeof(MonkNoArmorAndMonkWeaponFeatureUnlock), nameof(MonkNoArmorAndMonkWeaponFeatureUnlock.CheckEligibility))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler3(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerData(instructions, generator, original);

            while (!data.IsLast)
            {
                if (data.Calls(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.IsMonk)))
                {
                    data.InsertBefore(OpCodes.Ldarg_0);
                    data.ReplaceCall(Patch3);
                }
                data++;
            }

            return data.Code;
        }
        public static bool Patch3(BlueprintItemWeapon weapon, MonkNoArmorAndMonkWeaponFeatureUnlock __instance)
        {
            return weapon.IsMonk || (weapon.IsNatural && __instance.Owner.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat));
        }


        [HarmonyPatch(typeof(AdditionalStatBonusOnAttackDamage), nameof(AdditionalStatBonusOnAttackDamage.CheckConditions), typeof(RuleCalculateWeaponStats))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler4(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) // dragon style
        {
            var data = new TranspilerData(instructions, generator, original);
            data.Seek(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.Category));
            data.InsertAfter(Patch4);
            return data.Code;
        }
        public static WeaponCategory Patch4(WeaponCategory category, AdditionalStatBonusOnAttackDamage __instance, RuleCalculateWeaponStats evt)
        {
            if (__instance.Category == WeaponCategory.UnarmedStrike && evt.Weapon.Blueprint.IsNatural && evt.Initiator.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat))
                return WeaponCategory.UnarmedStrike;
            return category;
        }


        [HarmonyPatch(typeof(AdditionalDiceOnAttack), nameof(AdditionalDiceOnAttack.CheckCondition), typeof(RuleAttackRoll))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler5(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) // elemental fist
        {
            var data = new TranspilerData(instructions, generator, original);
            data.Seek(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.Category));
            data.InsertAfter(Patch5);
            return data.Code;
        }
        public static WeaponCategory Patch5(WeaponCategory category, AdditionalDiceOnAttack __instance, RuleAttackRoll evt)
        {
            if (__instance.Category == WeaponCategory.UnarmedStrike && evt.Weapon.Blueprint.IsNatural && evt.Initiator.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat))
                return WeaponCategory.UnarmedStrike;
            return category;
        }


        [HarmonyPatch(typeof(AdditionalDiceOnAttack), nameof(AdditionalDiceOnAttack.CheckCondition), typeof(RuleAttackWithWeapon))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler6(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) // elemental fist
        {
            var data = new TranspilerData(instructions, generator, original);
            data.Seek(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.Category));
            data.InsertAfter(Patch6);
            return data.Code;
        }
        public static WeaponCategory Patch6(WeaponCategory category, AdditionalDiceOnAttack __instance, RuleAttackWithWeapon evt)
        {
            if (__instance.Category == WeaponCategory.UnarmedStrike && evt.Weapon.Blueprint.IsNatural && evt.Initiator.Descriptor.HasFact(Resource.Cache.FeatureFeralCombat))
                return WeaponCategory.UnarmedStrike;
            return category;
        }

    }

}
