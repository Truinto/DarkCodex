using Kingmaker.UI.MVVM._PCView.ActionBar;
using Kingmaker.UI.MVVM._VM.ActionBar;

namespace CodexLib.Patches
{
    [HarmonyPatch]
    public class Patch_ActionBarConvert
    {
        [HarmonyPatch(typeof(ActionBarSlotVM), nameof(ActionBarSlotVM.SetMechanicSlot))]
        [HarmonyPostfix]
        public static void SetMechanicSlot(ActionBarSlotVM __instance)
        {
            if (__instance.MechanicActionBarSlot.GetContentData() is UnitFact fact)
            {
                fact.CallComponents<IActionBarConvert>(a =>
                {
                    __instance.HasConvert.Value = true;

                    var icon = a.GetIcon();
                    if (icon)
                        __instance.ForeIcon.Value = icon;
                });
            }
        }

        //[HarmonyPatch(typeof(ActionBarBaseSlotView), nameof(ActionBarBaseSlotView.BindViewImplementation))]
        //[HarmonyPostfix]
        //public static void ModifyCornerSize(ActionBarBaseSlotView __instance) // not working
        //{
        //    __instance.m_ForeIcon.cornerSize.Set(32f, 32f);
        //    AccessTools.Method(typeof(UnityEngine.UI.Graphic), "UpdateGeometry").Invoke(__instance.m_ForeIcon, new object[0]);
        //}

        [HarmonyPatch(typeof(ActionBarSlotVM), nameof(ActionBarSlotVM.OnShowConvertRequest))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static bool OnShowConvertRequest(ActionBarSlotVM __instance)
        {
            if (__instance.ConvertedVm.Value != null && !__instance.ConvertedVm.Value.IsDisposed)
                return true;

            if (__instance.MechanicActionBarSlot.GetContentData() is not UnitFact fact)
                return true;

            fact.CallComponents<IActionBarConvert>(a => __instance.ConvertedVm.Value = new ActionBarConvertedVMAny(__instance, a.GetConverts(), __instance.CloseConvert));

            return false;
        }

        [HarmonyPatch(typeof(ActionBarSlotPCView), "UnityEngine.EventSystems.IEndDragHandler.OnEndDrag")]
        [HarmonyPrefix]
        public static bool DisableDrag(ActionBarSlotPCView __instance)
        {
            if (__instance.ViewModel?.MechanicActionBarSlot is IActionBarDisableDrag)
            {
                // clear drop indicator
                EventBus.RaiseEvent<IActionBarDragHandler>(h => h.EndDrag(__instance.ViewModel, null), true);
                return false;
            }

            return true;
        }
    }
}
