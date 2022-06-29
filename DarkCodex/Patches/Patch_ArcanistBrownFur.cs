using HarmonyLib;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_ArcanistBrownFur
    {
        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.IsArcanistSpell), MethodType.Getter)]
        public static bool Prefix(AbilityData __instance, ref bool __result)
        {
            if (__instance.Blueprint.Type == AbilityType.Spell)
                __result = true;
            return false;
        }
    }
}
