

namespace CodexLib.Patches
{
    [HarmonyPatch(typeof(MechanicsContext), nameof(MechanicsContext.Recalculate))]
    public class Patch_ContextRankBonus
    {
        public static void Postfix(MechanicsContext __instance)
        {
            if (__instance.MaybeCaster == null)
                return;

            foreach (var comp in __instance.AssociatedBlueprint.GetComponents<IContextBonus>())
                comp.Apply(__instance);
        }
    }
}
