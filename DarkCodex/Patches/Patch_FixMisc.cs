using Kingmaker.QA;
using Owlcat.Runtime.Core.Logging;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony | Severity.Hidden, "Fix Misc", "suppress errors", false)]
    [HarmonyPatch]
    public class Patch_FixMisc
    {
        [HarmonyPatch(typeof(AddKineticistInfusionDamageTrigger), nameof(AddKineticistInfusionDamageTrigger.ApplyInternal))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerTool(instructions, generator, original);
            data.ReplaceAllCalls(typeof(LogChannelEx), nameof(LogChannelEx.ErrorWithReport), Empty, new Type[] { typeof(LogChannel), typeof(string), typeof(object[]) });
            return data;
        }

        public static void Empty(LogChannel channel, string msgFormat, params object[] @params) { }
    }
}
