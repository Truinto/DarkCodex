using Kingmaker.UnitLogic.Class.Kineticist;

namespace CodexLib
{
    [HarmonyPatch]
    public class Patch_SuppressError
    {
        [HarmonyPatch(typeof(UnitPartKineticist), nameof(UnitPartKineticist.MaxBurn), MethodType.Getter)]
        [HarmonyFinalizer]
        public static Exception Finalizer1() => null;

        [HarmonyPatch(typeof(UnitPartKineticist), nameof(UnitPartKineticist.GatherPowerAbility), MethodType.Getter)]
        [HarmonyFinalizer]
        public static Exception Finalizer2() => null;
    }
}
