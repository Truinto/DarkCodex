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
        /// <summary>
        /// Returns dummy SpellSlots for spontaneous metamagic. m_CachedName starts with "Spontaneous: "
        /// Returns dummy SpellSlots for temporary learned spells. m_CachedName starts with "Temporary: "
        /// This is solely for ActionBarVM.CollectSpells.
        /// </summary>
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

                var slot = new SpellSlot(spellLevel, SpellSlotType.Common, -1)
                {
                    Spell = spell,
                    Available = true
                };

                list.Add(slot);
            }

            var temporary = __instance.Owner.Unit.Get<UnitPartTemporarySpellsKnown>();
            if (temporary != null)
            {
                foreach (var entry in temporary.m_Entries)
                {
                    foreach (var spell in entry.Spells)
                    {
                        if (spell.SpellLevel != spellLevel)
                            continue;
                        if (spell.Spellbook != __instance)
                            continue;
                        if (list.Any(a => a.Spell.Blueprint == spell.Blueprint))
                            continue;

                        spell.m_CachedName = "Temporary: " + spell.Name;

                        var slot = new SpellSlot(spellLevel, SpellSlotType.Common, -1)
                        {
                            Spell = spell,
                            Available = true
                        };

                        list.Add(slot);
                    }
                }
            }

            __result = list;
        }

        /// <summary>
        /// Returns dummy spell count for dummy spells.
        /// </summary>
        [HarmonyPatch(typeof(Spellbook), nameof(Spellbook.GetAvailableForCastSpellCount))]
        [HarmonyPostfix]
        public static void Postfix2(AbilityData spell, Spellbook __instance, ref int __result)
        {
            if (__result == 0 && __instance.Blueprint.IsArcanist)
            {
                if (spell.m_CachedName?.StartsWith("Spontaneous: ") == true
                    || spell.m_CachedName?.StartsWith("Temporary: ") == true)
                {
                    __result = __instance.GetSpontaneousSlots(__instance.GetSpellLevel(spell));
                }
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
        [HarmonyPostfix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        public static void Postfix4(AbilityData __instance, ref CommandType __result)
        {
            if (IsForceFullRound(__instance))
                __result = CommandType.Standard;
        }

        /// <summary>
        /// Allow arcanist to memorize the same spell any number. Needed because metamagic is ignored and only blueprints are compared. This is a base game issue.
        /// </summary>
        [HarmonyPatch(typeof(Spellbook), nameof(Spellbook.Memorize))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler5(IEnumerable<CodeInstruction> instr)
        {
            var original = AccessTools.Field(typeof(BlueprintSpellbook), nameof(BlueprintSpellbook.IsArcanist));

            foreach (var line in instr)
            {
                if (line.Calls(original))
                    line.ReplaceCall(Helper.FakeAlwaysFalse);
                yield return line;
            }
        }

        /// <summary>
        /// Returns dummy SpellSlot.
        /// This is solely for AbilityData.GetConversions for ArcanistMagicalSupremacy.
        /// </summary>
        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.SpellSlot), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool Prefix6(AbilityData __instance, ref SpellSlot __result)
        {
            if (__result != null)
                return true;

            if (__instance.m_CachedName == null)
                return true;

            if (!__instance.m_CachedName.StartsWith("Spontaneous: ") && !__instance.m_CachedName.StartsWith("Temporary: "))
                return true;

            __result = new SpellSlot(0, SpellSlotType.Common, -1)
            {
                Spell = __instance,
                Available = true
            };

            return false;
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

        // returns list of all memorized spell-blueprints that have NO metamagic
        public static List<BlueprintAbility> GetAllMemorizedBlueprints(Spellbook spellbook)
        {
            var list = new List<BlueprintAbility>();
            foreach (var level in spellbook.m_MemorizedSpells)
            {
                foreach (var slot in level)
                {
                    var memSpell = slot.Spell;
                    if (memSpell != null
                        && memSpell.MetamagicData?.NotEmpty != true     // metamagic has to be empty
                        && slot.Available
                        && !list.Contains(memSpell.Blueprint))
                        list.Add(memSpell.Blueprint);
                }
            }
            return list;
        }
    }
}
