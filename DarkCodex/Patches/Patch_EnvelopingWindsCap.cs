using System;
using HarmonyLib;
using Kingmaker.RuleSystem.Rules;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Enveloping Winds Cap", "removes 50% evasion cap for Hurricane Queen", false)]
    [HarmonyPatch(typeof(MissChance), nameof(MissChance.ClampMissChance))]
    public class Patch_EnvelopingWindsCap
    {
        //this.MissChance = Math.Min(50, Math.Max(this.MissChance, value));
        public static bool Prefix(int missChance, ref int __result)
        {
            __result = Math.Min(100, missChance);
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
