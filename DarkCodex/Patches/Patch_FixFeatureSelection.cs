using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    /// <summary>
    /// Makes it so m_AllFeature is returned instead of m_Feature.
    /// </summary>
    [PatchInfo(Severity.Harmony, "Fix Feature Selection", "fixes issue #189", false)]
    [HarmonyPatch(typeof(BlueprintFeatureSelection), nameof(BlueprintFeatureSelection.Features), MethodType.Getter)]
    [HarmonyPriority(Priority.VeryLow)]
    public class Patch_FixFeatureSelection
    {
        public static bool Prefix(BlueprintFeatureSelection __instance, ref ReferenceArrayProxy<BlueprintFeature, BlueprintFeatureReference> __result)
        {
            __result = __instance.m_AllFeatures;
            return false;
        }
    }
}
