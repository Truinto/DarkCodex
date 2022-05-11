using HarmonyLib;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Fact Selection Parametrized", "what does this do?", false)]
    [HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.CanSelect))]
    public class Patch_FactSelectionParameterized
    {
        public static bool Prefix(BlueprintParametrizedFeature __instance, ref bool __result, UnitDescriptor unit, LevelUpState state, FeatureSelectionState selectionState, IFeatureSelectionItem item)
        {
            if (__instance.ParameterType != FeatureParameterType.Custom)
                return true;

            if (item.Param == null)
                __result = false;
            //else if (__instance.Items.FirstOrDefault(i => i.Feature == item.Feature && i.Param == item.Param) == null)
            //    __result = false;
            else if (unit.GetFeature(__instance, item.Param) != null)
                __result = false;
            else if (item.Param.Blueprint is BlueprintFact fact && !unit.HasFact(fact))
                __result = __instance.HasNoSuchFeature;
            else
                __result = true;

            return false;
        }
    }
}
