using HarmonyLib;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.QA;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Logging;
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

        [HarmonyPatch(typeof(ModifiableValueSkill), nameof(ModifiableValueSkill.RemoveBackgroundSkillSource))]
        [HarmonyPrefix]
        public static bool Prefix2(UnitFact fact, ModifiableValueSkill __instance)
        {
            if (__instance.BackgroundSkillSource != fact)
                return false;
            return true;
        }

        [HarmonyPatch(typeof(ModifiableValueSkill), nameof(ModifiableValueSkill.AddBackgroundSkillSource))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instr)
        {
            var original = AccessTools.Method(typeof(LogChannelEx), nameof(LogChannelEx.ErrorWithReport), new Type[] { typeof(LogChannel), typeof(string), typeof(object[]) });

            foreach (var line in instr)
            {
                if (line.Calls(original))
                    line.ReplaceCall(Empty);
                yield return line;
            }
        }

        [HarmonyPatch(typeof(ModifiableValueSkill), nameof(ModifiableValueSkill.RemoveBackgroundSkillSource))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler2(IEnumerable<CodeInstruction> instr) => Transpiler1(instr);

        public static void Empty(LogChannel channel, string msgFormat, params object[] @params) { }
    }
}
