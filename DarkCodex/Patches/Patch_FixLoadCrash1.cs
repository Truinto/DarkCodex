using HarmonyLib;
using Kingmaker.EntitySystem.Persistence;
using System.Text.RegularExpressions;
using Shared;

namespace DarkCodex
{
    [PatchInfo(Severity.Hidden | Severity.Harmony, "Patch: Fix Load Crash", "removes trap data during save load to prevent a specific crash; toggle with debug flag 2", false)]
    [HarmonyPatch(typeof(AreaDataStash), nameof(AreaDataStash.GetJsonForArea))]
    public class Patch_FixLoadCrash1
    {
        public static void Postfix(ref string __result)
        {
            if (!Settings.State.debug_2)
                return;

            if (__result == null || __result == "")
                return;

            //Helper.Print(__result);
            __result = Regex.Replace(__result, @"{""\$id"":""[0-9]+"",""\$type"":""Kingmaker\.View\.MapObjects\.Traps\.Simple\.SimpleTrapObjectData, Assembly-CSharp"",.*?""UniqueId"":.*?},?", "");
        }
    }
}
