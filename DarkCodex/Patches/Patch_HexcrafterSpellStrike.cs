using HarmonyLib;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Hexcrafter Spell Strike", "hexes with touch range can be used with Spell Strike")]
    [HarmonyPatch(typeof(UnitPartMagus), nameof(UnitPartMagus.IsSpellFromMagusSpellList))]
    public class Patch_HexcrafterSpellStrike
    {
        public static void Postfix(AbilityData spell, ref bool __result)
        {
            __result = __result || Resource.Cache.AccursedStrike.Contains(spell.Blueprint);
        }
    }
}
