using CodexLib;
using HarmonyLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.RuleSystem.Rules;
using Shared;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_AlwaysAChance
    {
        // OvertipViewPartCombatText.OnCombatMessage
        // UICombatTexts.GetTbmCombatText

        [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.IsSuccessRoll))]
        [HarmonyPostfix]
        public static void Postfix1(int d20, RuleAttackRoll __instance, ref bool __result)
        {
            __result = __result || (d20 == 1 && __instance.Initiator != null && __instance.Initiator.State.Features.AlwaysChance);
        }

        [HarmonyPatch(typeof(RuleCombatManeuver), nameof(RuleCombatManeuver.IsSuccessRoll))]
        [HarmonyPostfix]
        public static void Postfix2(int d20, RuleCombatManeuver __instance, ref bool __result)
        {
            __result = __result || d20 == 20 || (d20 == 1 && __instance.Initiator != null && __instance.Initiator.State.Features.AlwaysChance);
        }

        [HarmonyPatch(typeof(RuleDispelMagic), nameof(RuleDispelMagic.IsSuccessRoll))]
        [HarmonyPostfix]
        public static void Postfix3(int d20, RuleDispelMagic __instance, ref bool __result)
        {
            __result = __result || d20 == 20 || (d20 == 1 && __instance.Initiator != null && __instance.Initiator.State.Features.AlwaysChance);
        }

        [HarmonyPatch(typeof(RuleSkillCheck), nameof(RuleSkillCheck.IsSuccessRoll))]
        [HarmonyPostfix]
        public static void Postfix4(int d20, RuleSkillCheck __instance, ref bool __result)
        {
            __result = __result || d20 == 20 || (d20 == 1 && __instance.Initiator != null && __instance.Initiator.State.Features.AlwaysChance);
        }

        [HarmonyPatch(typeof(RuleSavingThrow), nameof(RuleSavingThrow.IsSuccessRoll))]
        [HarmonyPostfix]
        public static void Postfix5(int d20, RuleSavingThrow __instance, ref bool __result)
        {
            __result = __result || (d20 == 1 && __instance.Initiator != null && __instance.Initiator.State.Features.AlwaysChance);
        }
    }
}
