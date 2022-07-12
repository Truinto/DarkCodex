using HarmonyLib;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_SpellPerfection
    {
        [HarmonyPatch(typeof(SchoolMasteryParametrized), nameof(SchoolMasteryParametrized.OnEventAboutToTrigger))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instr)
        {
            foreach (var line in instr)
            {
                if (line.opcode == OpCodes.Ldc_I4_S && Convert.ToInt32(line.operand) == 25)
                    line.operand = 30;

                yield return line;
            }
        }

        [HarmonyPatch(typeof(SpellPenetrationMythicBonus), nameof(SpellPenetrationMythicBonus.OnEventAboutToTrigger))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler2(IEnumerable<CodeInstruction> instr) => Transpiler1(instr);
    }
}
