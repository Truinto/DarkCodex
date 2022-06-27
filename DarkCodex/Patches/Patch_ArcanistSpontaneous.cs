using CodexLib;
using HarmonyLib;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace DarkCodex
{
    /// <summary>
    /// Patches AbilityData.GetConversions to return all metamagic spells of the original spell's level, if that metamagic spell isn't already prepared.
    /// </summary>
    [PatchInfo(Severity.Harmony, "Patch: Fix Arcanist Spontaneous Metamagic", "allows arcanist to use non memorized metamagic, but increases casting time", false)]
    [HarmonyPatch]
    public class Patch_ArcanistSpontaneous
    {
        [HarmonyPatch(typeof(Spellbook), nameof(Spellbook.GetMemorizedSpells))]
        [HarmonyPostfix]
        public static void Postfix1(int spellLevel, Spellbook __instance, ref IEnumerable<SpellSlot> __result)
        {
            if (!__instance.Blueprint.IsArcanist)
                return;

            var memorizedBlueprints = GetAllMemorizedBlueprints(__instance);

            var list = __result as List<SpellSlot> ?? __result.ToList();

            foreach (var meta in __instance.GetCustomSpells(spellLevel))
            {
                // skip if metamagic is already prepared
                if (__instance.m_MemorizedSpells[spellLevel].Any(a => a.Spell?.Equals(meta) == true))
                    continue;

                // skip if basic version is not memorized
                if (!memorizedBlueprints.Contains(meta.Blueprint))
                    continue;

                if (meta.SpellLevelInSpellbook == null)
                    meta.SpellLevelInSpellbook = __instance.GetMinSpellLevel(meta.Blueprint);

                var spell = new AbilityData(meta.Blueprint, __instance, meta.SpellLevelInSpellbook.Value);
                spell.MetamagicData = meta.MetamagicData?.Clone();
                spell.DecorationBorderNumber = meta.DecorationBorderNumber;
                spell.DecorationColorNumber = meta.DecorationColorNumber;
                spell.m_CachedName = "Spontaneous: " + spell.Name;

                var slot = new SpellSlot(spellLevel, SpellSlotType.Common, -1);
                slot.Spell = spell;
                slot.Available = true;

                list.Add(slot);
            }

            var temporary = __instance.Owner.Unit.Get<UnitPartTemporarySpellsKnown>();
            if (temporary != null)
            {
                foreach (var entry in temporary.m_Entries)
                {
                    foreach (var spell in entry.Spells)
                    {
                        if (spell.Spellbook != __instance)
                            continue;
                        if (list.Any(a => a.Spell.Blueprint == spell.Blueprint))
                            continue;

                        var slot = new SpellSlot(spellLevel, SpellSlotType.Common, -1);
                        slot.Spell = spell;
                        slot.Available = true;

                        list.Add(slot);
                    }
                }
            }

            __result = list;
        }

        [HarmonyPatch(typeof(Spellbook), nameof(Spellbook.GetAvailableForCastSpellCount))]
        [HarmonyPostfix]
        public static void Postfix2(AbilityData spell, Spellbook __instance, ref int __result)
        {
            if (__result == 0 && __instance.Blueprint.IsArcanist && spell.m_CachedName?.StartsWith("Spontaneous: ") == true)
            {
                __result = __instance.GetSpontaneousSlots(__instance.GetSpellLevel(spell));
            }
        }

        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.RequireFullRoundAction), MethodType.Getter)]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        public static void Postfix3(AbilityData __instance, ref bool __result)
        {
            if (IsForceFullRound(__instance))
                __result = true;
        }

        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.GetDefaultActionType))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        public static void Postfix4(AbilityData __instance, ref CommandType __result)
        {
            if (IsForceFullRound(__instance))
                __result = CommandType.Standard;
        }



        public static bool IsForceFullRound(AbilityData __instance)
        {
            if (!__instance.IsArcanist)
                return false;

            if (__instance.HasMetamagic(Metamagic.Quicken))
                return false;

            return __instance.m_CachedName?.StartsWith("Spontaneous: ") == true;

            //int spellLevel = __instance.Spellbook.GetSpellLevel(__instance);
            //foreach (var memorized in __instance.Spellbook.m_MemorizedSpells[spellLevel])
            //{
            //    if (memorized.Spell.Equals(__instance))
            //        return false;
            //}
            //return true;
        }

        public static bool IsMemorized(Spellbook spellbook, BlueprintAbility spell)
        {
            foreach (var level in spellbook.m_MemorizedSpells)
            {
                foreach (var slot in level)
                {
                    var memSpell = slot.Spell;
                    if (memSpell != null
                        && memSpell.Blueprint == spell
                        && memSpell.MetamagicData?.NotEmpty != true
                        && slot.Available)
                        return true;
                }
            }
            return false;
        }

        public static List<BlueprintAbility> GetAllMemorizedBlueprints(Spellbook spellbook)
        {
            var list = new List<BlueprintAbility>();
            foreach (var level in spellbook.m_MemorizedSpells)
            {
                foreach (var slot in level)
                {
                    var memSpell = slot.Spell;
                    if (memSpell != null
                        && memSpell.MetamagicData?.NotEmpty != true
                        && slot.Available
                        && !list.Contains(memSpell.Blueprint))
                        list.Add(memSpell.Blueprint);
                }
            }
            return list;
        }
    }
}
