using Kingmaker.UnitLogic.Class.Kineticist;

namespace CodexLib
{
    [HarmonyPatch]
    public class Patch_SuppressError
    {
        [HarmonyPatch(typeof(UnitPartKineticist), nameof(UnitPartKineticist.MaxBurn), MethodType.Getter)]
        public static Exception Finalizer() => null;
    }
}
