using Kingmaker.UI;
using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UI.UnitSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    internal class ActivatableVariants : UnitFactComponentDelegate<ActivatableVariantsData>, IActionBarConvert
    {
        public BlueprintUnitFact[] Facts;

        public List<MechanicActionBarSlot> GetConverts() // UnitFact? BlueprintUnitFact?
        {
            //return this.Facts;
            throw new NotImplementedException();
        }
    }

    internal class ActivatableVariantsData
    {

    }

    [HarmonyPatch]
    internal class Patch_ActionBarConvert
    {
        [HarmonyPatch(typeof(ActionBarSlotVM), nameof(ActionBarSlotVM.SetMechanicSlot))]
        [HarmonyPostfix]
        public static void SetMechanicSlot(ActionBarSlotVM __instance)
        {
            if (__instance.MechanicActionBarSlot.GetContentData() is UnitFact fact && fact.Blueprint.GetComponent<IActionBarConvert>() != null)
                __instance.HasConvert.Value = true;
        }

        [HarmonyPatch(typeof(ActionBarSlotVM), nameof(ActionBarSlotVM.OnShowConvertRequest))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static bool OnShowConvertRequest(ActionBarSlotVM __instance)
        {
            if (__instance.ConvertedVm.Value != null && !__instance.ConvertedVm.Value.IsDisposed)
                return true;

            if (__instance.MechanicActionBarSlot.GetContentData() is not UnitFact fact)
                return true;

            var convert = fact.Blueprint.GetComponent<IActionBarConvert>();
            if (convert == null)
                return true;

            __instance.ConvertedVm.Value = new ActionBarConvertedVMAny(__instance, convert.GetConverts(), __instance.CloseConvert);
            return false;
        }
    }
}
