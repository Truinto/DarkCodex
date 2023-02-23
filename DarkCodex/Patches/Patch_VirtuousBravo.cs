using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_VirtuousBravo
    {
        [HarmonyPatch(typeof(UnitEntityData), nameof(UnitEntityData.CalculateSpeedModifier))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerTool(instructions, generator, original);

            data.Seek(typeof(UnitMechanicFeatures), nameof(UnitMechanicFeatures.TricksterMobilityFastMovement));
            data.Seek(OpCodes.Mul);
            data.InsertBefore(Patch);

            return data.Code;
        }

        public static float Patch(float value, UnitEntityData __instance)
        {
            if (value > 1f)
                return value;
            if (__instance.HasFlag(MechanicFeature.MobilityAtFullSpeed))
                return 1f;
            return value;
        }
    }
}
