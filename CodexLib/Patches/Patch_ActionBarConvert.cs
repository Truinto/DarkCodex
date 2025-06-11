using Kingmaker.UI.MVVM._PCView.ActionBar;
using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UI.UnitSettings;

namespace CodexLib.Patches
{
    [HarmonyPatch]
    public class Patch_ActionBarConvert
    {
        [HarmonyPatch(typeof(ActionBarSlotVM), nameof(ActionBarSlotVM.UpdateResource))]
        [HarmonyPostfix]
        public static void ShowUnfoldButton(ActionBarSlotVM __instance)
        {
            try
            {
                if (__instance.MechanicActionBarSlot == null || __instance.MechanicActionBarSlot.IsBad())
                    return;

                if (__instance.MechanicActionBarSlot is IMechanicGroup)
                {
                    __instance.HasConvert.Value = true;
                    __instance.Icon.Value = __instance.MechanicActionBarSlot.GetIcon();
                    return;
                }

                object obj = __instance.MechanicActionBarSlot.GetContentData();
                if (obj is AbilityData ability)
                    obj = ability.m_Fact;

                if (obj is UnitFact fact)
                {
                    fact.CallComponents<IActionBarConvert>(a =>
                    {
                        __instance.HasConvert.Value = true;

                        var icon = a.GetIcon();
                        if (icon)
                            __instance.ForeIcon.Value = icon;
                    });
                }
            } catch (Exception e) { Helper.PrintDebug(e.ToString()); }
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

            if (__instance.MechanicActionBarSlot is IMechanicGroup group)
            {
                for (int i = group.Slots.Count - 1; i >= 0; i--)
                {
                    if (group.Slots[i].IsBad())
                        group.Slots.RemoveAt(i);
                }

                __instance.ConvertedVm.Value = new ActionBarConvertedVMAny(__instance, group.Slots, __instance.CloseConvert); // if null is used, it won't close; possible useful for nesting
                return false;
            }

            object obj = __instance.MechanicActionBarSlot.GetContentData();
            if (obj is AbilityData ability)
                obj = ability.m_Fact;

            if (obj is not UnitFact fact)
                return true;

            bool hasConvert = false;
            fact.CallComponents<IActionBarConvert>(a =>
            {
                if (!hasConvert)
                    __instance.ConvertedVm.Value = new ActionBarConvertedVMAny(__instance, a.GetConverts(), __instance.CloseConvert);
                hasConvert = true;
            });

            return !hasConvert;
        }

        [HarmonyPatch(typeof(UnitUISettings), nameof(UnitUISettings.GetBadSlotReplacement))]
        [HarmonyPrefix]
        private static bool BadReplacement(MechanicActionBarSlot slot, ref MechanicActionBarSlot __result)
        {
            if (slot is MechanicActionBarSlotGroup or MechanicActionBarSlotSpellGroup or MechanicActionBarSlotPlaceholder)
            {
                __result = null;
                Helper.PrintDebug($"BadReplacement {slot} to null");
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(ActionBarSlotPCView), "UnityEngine.EventSystems.IEndDragHandler.OnEndDrag")]
        [HarmonyPrefix]
        public static bool DisableDrag(ActionBarSlotPCView __instance) // TODO: ensure controller input can't circumvent this
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
