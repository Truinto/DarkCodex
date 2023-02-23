using HarmonyLib;
using Kingmaker.ElementsSystem;

namespace CodexLib.Patches
{
    /// <summary>
    /// If a component causes an exception, it will try to resolve the asset name.<br/>
    /// If that name is null or wrong formated, it will crash the report and hide any meaningful log entries.<br/>
    /// This patch fills the missing data with nonsense to prevent that bug.
    /// </summary>
    [HarmonyPatch(typeof(Element), nameof(Element.AssetGuidShort), MethodType.Getter)]
    [HarmonyPriority(Priority.LowerThanNormal)]
    public class Patch_DebugReport
    {
        public static void Prefix(Element __instance)
        {
            __instance.name ??= "$$empty";
        }
    }
}
