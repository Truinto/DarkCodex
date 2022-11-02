using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_ProdigiousTWF
    {
        [HarmonyPatch(typeof(TwoWeaponFightingAttackPenalty), nameof(TwoWeaponFightingAttackPenalty.OnEventAboutToTrigger), typeof(RuleCalculateAttackBonusWithoutTarget))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerData(instructions, generator, original);

            // find last access to a local boolean variable
            data.Last().Rewind(f => f.IsStloc(typeof(bool)));

            // set variable to true, if weapon should be considered light
            data.EditLocal(Patch);

            return data.Code;
        }

        public static void Patch(TwoWeaponFightingAttackPenalty __instance, ref bool flag)
        {
            flag = flag || __instance.Owner.HasFlag(MechanicFeature.ProdigiousTWF);
        }
    }
}
