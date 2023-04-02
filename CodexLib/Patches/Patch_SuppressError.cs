using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UI.UnitSettings;
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

        [HarmonyPatch(typeof(UnitEntityData), nameof(UnitEntityData.EnsureFactProcessors))]
        [HarmonyPostfix]
        public static void ClearInvalidFacts(UnitEntityData __instance)
        {
            __instance.Facts.m_Facts.RemoveAll(f => f.Blueprint == null);
        }

        /// <summary>
        /// This happens specifically because of VariantSelectionApplyEffect when it overrides AbilityData.OverridenResourceLogic for unkown reasons.
        /// </summary>
        [HarmonyPatch(typeof(BlueprintComponent), nameof(BlueprintComponent.OnDeserialized))]
        [HarmonyPrefix]
        public static bool FixNullExceptionDeserializeComponents()
        {
            if (Kingmaker.Blueprints.JsonSystem.Json.BlueprintBeingRead == null)
                return false;
            return true;
        }
    }
}
