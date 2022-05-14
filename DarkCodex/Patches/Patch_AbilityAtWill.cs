using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using CodexLib;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Ability At Will", "provides logic for at will spells")]
    [HarmonyPatch]
    public class Patch_AbilityAtWill
    {
        [HarmonyPatch(typeof(Spellbook), nameof(Spellbook.SpendInternal))]
        [HarmonyPriority(410)]
        [HarmonyPrefix]
        public static bool SpendInternal(BlueprintAbility blueprint, ref bool __result)
        {
            if (blueprint.GetComponent<AbilityAtWill>())
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Spellbook), nameof(Spellbook.GetAvailableForCastSpellCount))]
        [HarmonyPriority(410)]
        [HarmonyPrefix]
        public static bool GetAvailableForCastSpellCount(AbilityData spell, ref int __result)
        {
            if (spell.Blueprint.GetComponent<AbilityAtWill>())
            {
                __result = -1;
                return false;
            }
            return true;
        }
    }
}
