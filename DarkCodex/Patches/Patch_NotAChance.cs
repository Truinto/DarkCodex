using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_NotAChance
    {
        [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.IsSuccessRoll))]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPostfix]
        public static void Postfix1(int d20, RuleAttackRoll __instance, ref bool __result)
        {
            __result = __result && !(__instance.AttackBonus < __instance.TargetAC && __instance.Target.HasFlag(MechanicFeature.NotAChance));
        }

        [HarmonyPatch(typeof(RuleCombatManeuver), nameof(RuleCombatManeuver.IsSuccessRoll))]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPostfix]
        public static void Postfix2(int d20, RuleCombatManeuver __instance, ref bool __result)
        {
            __result = __result && !(__instance.InitiatorCMB < __instance.TargetCMD && __instance.Target.HasFlag(MechanicFeature.NotAChance));
        }
    }
}
