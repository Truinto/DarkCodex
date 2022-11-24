using Kingmaker.UI.MVVM._VM.CharGen;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony | Severity.DefaultOff, "Patch: Partial Respec", "allows respec to be finished at any point", false)]
    [HarmonyPatch]
    public class Patch_RespecPartially
    {
        [HarmonyPatch(typeof(RespecWindowVM), nameof(RespecWindowVM.UpdateProperties))]
        [HarmonyPostfix]
        public static void UnlockButton(RespecWindowVM __instance)
        {
            __instance.IsFinished.Value = true;
        }

        [HarmonyPatch(typeof(RespecWindowVM), nameof(RespecWindowVM.Complete))]
        [HarmonyPrefix]
        public static bool ForceCompletion(RespecWindowVM __instance)
        {
            __instance.m_EndAction?.Invoke();
            return false;
        }
    }
}
