using HarmonyLib;
using Kingmaker;
using Kingmaker.Modding;
using Shared;

namespace DarkCodex
{
    /// <remarks>
    /// Must be manually patched; it crashes with PatchAll
    /// </remarks>
    [PatchInfo(Severity.Harmony, "Patch: Allow Achievements", "clears the 'has used mods before' flag and also pretends that no mods are active", false)]
    [HarmonyPatch(typeof(OwlcatModificationsManager), nameof(OwlcatModificationsManager.IsAnyModActive), MethodType.Getter)]
    public class Patch_AllowAchievements
    {
        public static bool Patched;
        public static bool Prefix(ref bool __result)
        {
            if (!Settings.State.allowAchievements)
                return true;

            if (Game.Instance?.Player != null)
                Game.Instance.Player.ModsUser = false;
            __result = false;
            return false;
        }
    }
}
