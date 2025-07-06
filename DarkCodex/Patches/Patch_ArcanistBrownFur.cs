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
        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        public static bool Prefix1(AbilityData __instance, ref bool __result)
        {
            __result = __instance.Blueprint.Type is AbilityType.SpellLike or AbilityType.Spell;
            return false;
        }

        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.ArcanistShareTransmutation), MethodType.Getter)]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        public static bool Prefix2(AbilityData __instance, ref bool __result)
        {
            __result = __instance.Blueprint.Type is AbilityType.SpellLike or AbilityType.Spell && __instance.Caster != null && __instance.Caster.State.Features.ShareTransmutation;
            return false;
        }
    }
}
