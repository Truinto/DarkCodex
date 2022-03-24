using HarmonyLib;
using Kingmaker.UnitLogic.Buffs;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Polymorph", "allows debug flags to keep inventory or model during polymorph", false)]
    public class Patch_Polymorph
    {
        [HarmonyPatch(typeof(Polymorph), nameof(Polymorph.OnActivate))]
        [HarmonyPrefix]
        public static void KeepSlots(Polymorph __instance)
        {
            __instance.m_KeepSlots = __instance.m_KeepSlots || Settings.StateManager.State.polymorphKeepInventory;
        }

        [HarmonyPatch(typeof(Polymorph), nameof(Polymorph.TryReplaceView))]
        [HarmonyPrefix]
        public static bool KeepModel(Polymorph __instance)
        {
            return !Settings.StateManager.State.polymorphKeepModel;
        }
    }
}
