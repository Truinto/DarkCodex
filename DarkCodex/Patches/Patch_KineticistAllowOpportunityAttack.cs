using Kingmaker.Blueprints;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.FactLogic;
using Shared;
using CodexLib;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Controllers.Combat;
using Kingmaker.UnitLogic.Commands;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Kineticist Allow Opportunity Attack", "allows Attack of Opportunities with anything but standard Kinetic Blade; so that Kinetic Whip works; also allows natural attacks to be used, if Whip isn't available", false)]
    [HarmonyPatch]
    public class Patch_KineticistAllowOpportunityAttack
    {
        private static BlueprintGuid blade_p = BlueprintGuid.Parse("b05a206f6c1133a469b2f7e30dc970ef"); //KineticBlastPhysicalBlade
        private static BlueprintGuid blade_e = BlueprintGuid.Parse("a15b2fb1d5dc4f247882a7148d50afb0"); //KineticBlastEnergyBlade

        public static void Example()
        {
            var CreateAddMechanicsFeature = AccessTools.Method("CodexLib.Helper, CodexLib:CreateAddMechanicsFeature", new[] { Type.GetType("CodexLib.MechanicFeature, CodexLib") });
            var comp = CreateAddMechanicsFeature?.Invoke(null, new object[] { 8 });

            Main.Print($"Patch_KineticistAllowOpportunityAttack.Example {comp != null}");
        }

        /// <summary>
        /// Remove condition DisableAttacksOfOpportunity from kinetic blades.
        /// </summary>
        [HarmonyPatch(typeof(AddKineticistBlade), nameof(AddKineticistBlade.OnActivate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerTool(instructions, generator, original);
            data.ReplaceAllCalls(typeof(UnitState), nameof(UnitState.AddCondition), patch);
            return data;

            void patch(UnitState __instance, UnitCondition condition, Buff source, UnitConditionExceptions exceptions)
            {
            }
        }

        /// <summary>
        /// Modify call to GetThreadHand to return only weapons that allow attacks of opportunity (not kinetic blades).<br/>
        /// This should be a transpiler, but it's wrapped in a compiler generated field.
        /// </summary>
        [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.CanAttackOfOpportunity), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool Prefix4(UnitCombatState __instance, ref bool __result)
        {
            if (__instance.AttackOfOpportunityCount <= 0
                || __instance.Unit.Descriptor.State.HasCondition(UnitCondition.DisableAttacksOfOpportunity)
                || __instance.Unit.Descriptor.State.HasCondition(UnitCondition.Confusion)
                || __instance.Unit.Passive)
            {
                __result = false;
                return false;
            }

            __result = __instance.Unit.CanAttack(f => GetThreatHand_AttackOfOpportunity(f)?.Weapon);
            return false;
        }

        /// <summary>
        /// Modify call to GetThreadHand to return only weapons that allow attacks of opportunity (not kinetic blades).
        /// </summary>
        [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.AttackOfOpportunity))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler5(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerTool(instructions, generator, original);
            data.ReplaceAllCalls(typeof(UnitHelper), nameof(UnitHelper.GetThreatHand), GetThreatHand_AttackOfOpportunity);
            return data;
        }

        [HarmonyPatch(typeof(UnitAttackOfOpportunity), nameof(UnitAttackOfOpportunity.Init))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler6(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) => Transpiler5(instructions, generator, original);

        public static WeaponSlot GetThreatHand_AttackOfOpportunity(UnitEntityData unit)
        {
            foreach (var slot in allSlots())
            {
                if (!slot.HasWeapon)
                    continue;

                var bp = slot.Weapon.Blueprint;
                if ((bp.IsMelee || unit.State.Features.SnapShot)
                    && (!bp.IsUnarmed || unit.Descriptor.State.Features.ImprovedUnarmedStrike)
                    && (bp.Type.Category != WeaponCategory.KineticBlast || unit.HasFlag(MechanicFeature.KineticBladeAttackOfOpportunity)))
                    return slot;
            }
            return null;

            IEnumerable<WeaponSlot> allSlots()
            {
                yield return unit.Body.PrimaryHand;
                yield return unit.Body.SecondaryHand;
                foreach (var slot in unit.Body.AdditionalLimbs)
                    yield return slot;
            }
        }

    }
}
