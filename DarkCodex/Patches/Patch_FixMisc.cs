using Kingmaker.QA;
using Owlcat.Runtime.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_FixMisc
    {
        [HarmonyPatch(typeof(AddKineticistInfusionDamageTrigger), nameof(AddKineticistInfusionDamageTrigger.ApplyInternal))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instr)
        {
            var list = instr as List<CodeInstruction> ?? instr.ToList();
            Helper.RemoveMethods(list, typeof(LogChannelEx), nameof(LogChannelEx.ErrorWithReport), new Type[] { typeof(LogChannel), typeof(string), typeof(object[]) });
            return list;
        }
    }
}
