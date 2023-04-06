using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Items.Weapons;
using Shared;

namespace DarkCodex
{
    [HarmonyPatch(typeof(TwoWeaponFightingAttackPenalty), nameof(TwoWeaponFightingAttackPenalty.OnEventAboutToTrigger), typeof(RuleCalculateAttackBonusWithoutTarget))]
    public class Patch_ProdigiousTWF
    {
        [HarmonyPatch]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerTool(instructions, generator, original);

            // set variable to true, if weapon should be considered light
            data.Seek(typeof(BlueprintItemWeapon), nameof(BlueprintItemWeapon.IsLight));
            data.InsertAfter(Patch);

            return data.Code;
        }

        public static bool Patch(bool __stack, TwoWeaponFightingAttackPenalty __instance)
        {
            return __stack || __instance.Owner.HasFlag(MechanicFeature.ProdigiousTWF);
        }
    }
}
