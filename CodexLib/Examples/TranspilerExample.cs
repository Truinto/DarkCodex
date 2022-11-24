using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib.Examples
{
    public class TranspilerExample
    {
        // Excellent Transpiler Example
        [HarmonyPatch(typeof(object), nameof(object.ToString))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerData(instructions, generator, original);

            // stuff here

            return data.Code;
        }
    }
}
