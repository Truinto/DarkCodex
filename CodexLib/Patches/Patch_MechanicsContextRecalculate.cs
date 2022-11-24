

namespace CodexLib.Patches
{
    [HarmonyPatch(typeof(MechanicsContext), nameof(MechanicsContext.Recalculate))]
    public class Patch_MechanicsContextRecalculate
    {
        public static void Prefix(MechanicsContext __instance, out IEnumerable<IMechanicRecalculate> __state)
        {
            __state = null;
            if (__instance.MaybeCaster == null)
                return;

            __state = __instance.AssociatedBlueprint.GetComponents<IMechanicRecalculate>().OrderByDescending(o => o.Priority);

            foreach (var comp in __state)
                comp.PreCalculate(__instance);
        }

        public static void Postfix(MechanicsContext __instance, IEnumerable<IMechanicRecalculate> __state)
        {
            if (__state == null)
                return;

            bool sharedRecalc = false;

            foreach (var comp in __state)
            {
                if (comp.Priority >= 400)
                    sharedRecalc = true;
                else if (sharedRecalc && comp.Priority == 300)
                {
                    sharedRecalc = false;
                    __instance.RecalculateSharedValues();
                }

                comp.PostCalculate(__instance);
            }
        }
    }
}
