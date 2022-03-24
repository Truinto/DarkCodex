using HarmonyLib;
using Kingmaker.UnitLogic.ActivatableAbilities;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Activatable TryStart", "fixes activatable not starting the second time, while being outside of combat", false)]
    [HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.TryStart))]
    public static class Patch_ActivatableTryStart
    {
        public static void Prefix(ActivatableAbility __instance)
        {
            if (!__instance.Owner.Unit.IsInCombat)
            {
                __instance.Owner.Unit.CombatState.Cooldown.SwiftAction = 0f;
                __instance.Owner.Unit.CombatState.Cooldown.MoveAction = 0f;
            }

        }
    }
}
