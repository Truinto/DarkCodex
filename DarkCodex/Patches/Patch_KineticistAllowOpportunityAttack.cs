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

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Kineticist Allow Opportunity Attack", "allows Attack of Opportunities with anything but standard Kinetic Blade; so that Kinetic Whip works; also allows natural attacks to be used, if Whip isn't available", false)]
    [HarmonyPatch]
    public class Patch_KineticistAllowOpportunityAttack
    {
        private static BlueprintGuid blade_p = BlueprintGuid.Parse("b05a206f6c1133a469b2f7e30dc970ef"); //KineticBlastPhysicalBlade
        private static BlueprintGuid blade_e = BlueprintGuid.Parse("a15b2fb1d5dc4f247882a7148d50afb0"); //KineticBlastEnergyBlade

        [HarmonyPatch(typeof(AddKineticistBlade), nameof(AddKineticistBlade.OnActivate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instr)
        {
            List<CodeInstruction> list = instr.ToList();
            var original = AccessTools.Method(typeof(UnitState), nameof(UnitState.AddCondition));

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Calls(original))
                {
                    Helper.PrintDebug("Patched at " + i);
                    list[i] = CodeInstruction.Call(typeof(Patch_KineticistAllowOpportunityAttack), nameof(NullReplacement));
                }
            }

            return list;
        }
        public static void NullReplacement(UnitState state, UnitCondition condition, Buff sourceBuff, UnitConditionExceptions exceptions)
        {
        }

        [HarmonyPatch(typeof(UnitHelper), nameof(UnitHelper.IsThreatHand))]
        [HarmonyPrefix]
        public static bool Prefix2(UnitEntityData unit, WeaponSlot hand, ref bool __result)
        {
            if (!hand.HasWeapon)
                __result = false;

            else if (!hand.Weapon.Blueprint.IsMelee && !unit.State.Features.SnapShot)
                __result = false;

            else if (hand.Weapon.Blueprint.IsUnarmed && !unit.Descriptor.State.Features.ImprovedUnarmedStrike)
                __result = false;

            else if ((hand.Weapon.Blueprint.Type.AssetGuid == blade_p || hand.Weapon.Blueprint.Type.AssetGuid == blade_e)
                     && unit.Buffs.GetBuff(Resource.Cache.BuffKineticWhip) == null)
                __result = false;

            else
                __result = true;

            return false;
        }

    }

    //[HarmonyPatch(typeof(KineticistController), nameof(KineticistController.TryRunKineticBladeActivationAction))]
    //public class Patch_KineticistWhipReach
    //{
    //    public static void Postfix(UnitPartKineticist kineticist, UnitCommand cmd, bool __result)
    //    {
    //        if (!__result)
    //            return;
    //        if (kineticist.Owner.Buffs.GetBuff(Patch_KineticistAllowOpportunityAttack2.whip_buff) == null) 
    //            return;
    //        cmd.ApproachRadius += 5f * 0.3048f;
    //    }
    //}
}
