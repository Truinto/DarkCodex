using HarmonyLib;
using Kingmaker.UI.UnitSettings;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Activatable ActionBar", "adds logic for automatic-only activatable", false)]
    [HarmonyPatch(typeof(MechanicActionBarSlotActivableAbility), nameof(MechanicActionBarSlotActivableAbility.OnClick))]
    public static class Patch_ActivatableActionBar
    {
        public static readonly int NoManualOn = 788704819;
        public static readonly int NoManualOff = 788704820;
        public static readonly int NoManualAny = 788704821;

        public static bool Prefix(MechanicActionBarSlotActivableAbility __instance)
        {
            if (!__instance.ActivatableAbility.IsOn && __instance.ActivatableAbility.Blueprint.WeightInGroup == NoManualOn)
            {
                return false;
            }
            if (__instance.ActivatableAbility.IsOn && __instance.ActivatableAbility.Blueprint.WeightInGroup == NoManualOff)
            {
                return false;
            }
            if (__instance.ActivatableAbility.Blueprint.WeightInGroup == NoManualAny)
            {
                return false;
            }
            return true;
        }
    }
}
