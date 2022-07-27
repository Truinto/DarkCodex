using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Ability Range", "bonus spell range equal to 5 feet per 2 caster levels", true)]
    [HarmonyPatch(typeof(BlueprintAbility), nameof(BlueprintAbility.GetRange))]
    public class Patch_AbilityRange
    {
        public static void Postfix(bool reach, AbilityData abilityData, BlueprintAbility __instance, ref Feet __result)
        {
            if (abilityData?.Spellbook == null)
                return;

            if (!abilityData.Caster.IsPlayerFaction)
                return;

            var abilityRange = abilityData.Range;
            if (abilityRange > AbilityRange.Long || (!reach && abilityRange == AbilityRange.Touch))
                return;

            __result.m_Value += 5f * (abilityData.Spellbook.CasterLevel / 2).MinMax(0, 10);
        }
    }
}
