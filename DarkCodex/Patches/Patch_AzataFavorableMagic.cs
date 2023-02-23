using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Azata Favorable Magic", "include saving throws from auras (does not work with TableTopTweaks Azata.FavorableMagic enabled)", true)]
    [HarmonyPatch]
    public class Patch_AzataFavorableMagic
    {
        [HarmonyPatch(typeof(AzataFavorableMagic), nameof(AzataFavorableMagic.CheckReroll), typeof(RuleSavingThrow), typeof(RuleRollD20))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerTool(instructions, generator, original);

            data.Seek(typeof(RuleReason), nameof(RuleReason.Ability));
            data.NextJumpNever();

            return data.Code;
        }
    }
}
