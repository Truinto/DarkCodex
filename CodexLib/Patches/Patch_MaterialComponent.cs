using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib.Patches
{
    [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.RequireMaterialComponent), MethodType.Getter)]
    public class Patch_MaterialComponent
    {
        public static void Postfix(AbilityData __instance, ref bool __result)
        {
            __result = __result && !__instance.Caster.Unit.HasFlag(MechanicFeature.NoMaterialComponent);
        }
    }
}
