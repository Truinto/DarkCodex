using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Kingmaker.Designers.Mechanics.Facts;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Enduring Spells", "allows Enduring Spell to apply to spells from any source", false)]
    [HarmonyPatch(typeof(EnduringSpells), nameof(EnduringSpells.HandleBuffDidAdded))]
    public class Patch_EnduringSpells
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            bool flag = true;

            foreach (var line in instr)
            {
                if (flag && line.opcode == OpCodes.Ret)
                {
                    line.opcode = OpCodes.Nop;
                    flag = false;
                }
                yield return line;
            }

            if (flag)
                throw new Exception("Transpiler patch illegal state.");
        }
    }
}
