using HarmonyLib;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Background Checks", "prevents identical background skills from causing an error", false)]
    [HarmonyPatch]
    public class Patch_BackgroundChecks
    {
        [HarmonyPatch(typeof(ModifiableValueSkill), nameof(ModifiableValueSkill.AddBackgroundSkillSource))]
        [HarmonyPrefix]
        public static bool Prefix1(UnitFact fact, ModifiableValueSkill __instance)
        {
            if (__instance.BackgroundSkillSource != null)
                return false;
            return true;
        }

        [HarmonyPatch(typeof(ModifiableValueSkill), nameof(ModifiableValueSkill.AddBackgroundSkillSource))]
        [HarmonyPrefix]
        public static bool Prefix2(UnitFact fact, ModifiableValueSkill __instance)
        {
            if (__instance.BackgroundSkillSource != fact)
                return false;
            return true;
        }
    }
}
