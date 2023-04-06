using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_FixSpellStrike
    {
        [HarmonyPatch(typeof(UnitEntityData), nameof(UnitEntityData.PreparedSpellStrike))]
        [HarmonyPrefix]
        public static bool Prefix(UnitEntityData __instance, ref bool __result)
        {
            var unitPartMagus = __instance.Get<UnitPartMagus>();
            if (unitPartMagus == null || !unitPartMagus.Spellstrike.Active)
                return false;

            var abilityData = __instance.Get<UnitPartTouch>()?.Ability.Data;
            if (abilityData == null)
                return true;

            __result = unitPartMagus.IsSpellFromMagusSpellList(abilityData);
            return false;
        }
    }
}
