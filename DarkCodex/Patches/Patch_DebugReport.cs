using HarmonyLib;
using Kingmaker.ElementsSystem;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Debug Report", "fixes error log crashes due to unnamed components", false)]
    [HarmonyPatch(typeof(Element), nameof(Element.AssetGuidShort), MethodType.Getter)]
    public class Patch_DebugReport
    {
        /// <summary>If a component causes an exception, it will try to resolve the asset name.
        /// If that name is null or wrong formated, it will crash the report and hide any meaningful log entries.
        /// This patch fills the missing data with nonsense to prevent that bug.</summary>
        public static void Prefix(Element __instance)
        {
            if (__instance.name == null)
                __instance.name = "$$empty";
        }
    }
}
