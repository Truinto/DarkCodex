using HarmonyLib;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Shared;
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
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerTool(instructions, generator, original);
            data.ReplaceAllConstant(ModifierDescriptor.UntypedStackable, ModifierDescriptor.Feat);
            return data;
        }

        [HarmonyPatch(typeof(SpellPenetrationMythicBonus), nameof(SpellPenetrationMythicBonus.OnEventAboutToTrigger))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler2(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) => Transpiler1(instructions, generator, original);

        [HarmonyPatch(typeof(SpellSpecializationParametrized), nameof(SpellSpecializationParametrized.OnEventAboutToTrigger))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler3(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original) => Transpiler1(instructions, generator, original);
    }
}
