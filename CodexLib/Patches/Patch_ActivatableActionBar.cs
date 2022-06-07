using HarmonyLib;
using Kingmaker.UI.UnitSettings;

namespace CodexLib.Patches
{
    /// <summary>
    /// Adds logic for automatic-only activatable. Set WeightInGroup to restrict use of Activatable.
    /// </summary>
    //[PatchInfo(Severity.Harmony, "Patch: Activatable ActionBar", "adds logic for automatic-only activatable", false)]
    [HarmonyPatch]
    public static class Patch_ActivatableActionBar
    {
        [HarmonyPatch(typeof(MechanicActionBarSlotActivableAbility), nameof(MechanicActionBarSlotActivableAbility.OnClick))]
        [HarmonyPrefix]
        public static bool Prefix(MechanicActionBarSlotActivableAbility __instance)
        {
            if (!__instance.ActivatableAbility.IsOn && __instance.ActivatableAbility.Blueprint.WeightInGroup == Const.NoManualOn)
            {
                return false;
            }
            if (__instance.ActivatableAbility.IsOn && __instance.ActivatableAbility.Blueprint.WeightInGroup == Const.NoManualOff)
            {
                return false;
            }
            if (__instance.ActivatableAbility.Blueprint.WeightInGroup == Const.NoManualAny)
            {
                return false;
            }
            return true;
        }
    }
}
