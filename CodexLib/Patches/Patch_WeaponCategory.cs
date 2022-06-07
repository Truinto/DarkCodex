using HarmonyLib;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Enums;
using Kingmaker.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CodexLib.Patches
{
    /// <summary>
    /// Use Helper.EnumCreateWeaponCategory(..) to add new weapon categories.<br/>
    /// This patch adds them to the selection.
    /// </summary>
    [HarmonyPatch]
    public class Patch_WeaponCategory
    {
        public static List<(WeaponCategory num, LocalizedString name, Sprite icon)> Extention = new();

        [HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.ExtractItemsWeaponCategory))]
        [HarmonyPostfix]
        public static void Postfix(BlueprintParametrizedFeature __instance, ref IEnumerable<FeatureUIData> __result)
        {
            if (Extention == null || Extention.Count == 0)
                return;

            var result = __result.ToList();

            foreach ((WeaponCategory num, LocalizedString name, Sprite icon) in Extention)
            {
                result.Add(new FeatureUIData(__instance, num, name, "", icon, name.Key));
            }

            __result = result;
        }
    }
}
