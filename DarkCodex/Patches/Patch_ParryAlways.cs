using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony | Severity.DefaultOff, "Patch: Parry Always", "use parry even if attack would have missed anyway", false)]
    [HarmonyPatch]
    public class Patch_ParryAlways
    {
        [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.OnTrigger))]
        [HarmonyPriority(390)]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerTool(instructions, generator, original);

            //data.Seek(true, f => f.Calls(typeof(RuleAttackRoll), nameof(RuleAttackRoll.Parry)), f => f.Is(OpCodes.Brfalse_S));
            data.Last();
            data.Rewind(typeof(RuleAttackRoll.ParryData), nameof(RuleAttackRoll.ParryData.Trigger));
            data.Rewind(typeof(RuleAttackRoll), nameof(RuleAttackRoll.IsHit));
            data.NextJumpNever();

            return data.Code;
        }
    }
}
