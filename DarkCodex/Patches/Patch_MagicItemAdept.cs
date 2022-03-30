using HarmonyLib;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Magic Item Adept", "patches for Magic Item Adept", true)]
    [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.GetParamsFromItem))]
    public class Patch_MagicItemAdept
    {
        public static void Postfix(AbilityData __instance, AbilityParams __result)
        {
            if (__instance.Caster == null)
                return;

            if (!__instance.Caster.Unit.Descriptor.HasFact(Resource.Cache.FeatureMagicItemAdept))
                return;

            __result.CasterLevel = __instance.Caster.Progression.CharacterLevel;
        }
    }
}
