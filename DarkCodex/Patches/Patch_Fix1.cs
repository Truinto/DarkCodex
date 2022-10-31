using Kingmaker.UnitLogic.Class.Kineticist;

namespace DarkCodex
{
    [HarmonyPatch(typeof(UnitPartKineticist), nameof(UnitPartKineticist.MaxBurn), MethodType.Getter)]
    public class Patch_Fix1
    {
        public static Exception Finalizer() => null;
    }
}
