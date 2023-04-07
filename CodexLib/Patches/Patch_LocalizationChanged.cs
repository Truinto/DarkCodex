using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib.Patches
{
    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.OnLocaleChanged))]
    public class Patch_LocalizationChanged
    {
        [HarmonyPatch]
        [HarmonyPrefix]
        public static void Prefix()
        {
            Helper.Print($"OnLocaleChanged to {LocalizationManager.s_CurrentLocale}");
            Helper.ClearStringMaps();
        }
    }
}
