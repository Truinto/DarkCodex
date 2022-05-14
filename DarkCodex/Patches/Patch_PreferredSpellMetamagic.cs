using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.UI.Common;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Shared;
using CodexLib;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Preferred Spell Metamagic", "necessary patches for Preferred Spell", false, Requirement: typeof(Patch_SpellSelectionParametrized))]
    [HarmonyPatch]
    public class Patch_PreferredSpellMetamagic
    {
        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.GetConversions))]
        [HarmonyPostfix]
        public static void Postfix1(AbilityData __instance, ref IEnumerable<AbilityData> __result)
        {
            if (__instance.Spellbook == null)
                return;

            var list = __result.ToList();

            try
            {
                foreach (var conlist in __instance.Spellbook.m_SpellConversionLists)
                {
                    if (!conlist.Key.StartsWith("PreferredSpell#"))
                        continue;

                    var targetspell = conlist.Value.Last();

                    list.FirstOrDefault(f => f.Blueprint == targetspell)?.MetamagicData?.Clear(); // clear metamagic copied from donor spell

                    var metamagics = __instance.Spellbook.GetCustomSpells(__instance.SpellLevel).Where(w => w.Blueprint == targetspell); //__instance.Spellbook.GetSpellLevel(__instance)
                    foreach (var metamagic in metamagics)
                    {
                        var variants = targetspell.GetComponent<AbilityVariants>()?.Variants;
                        if (variants == null)
                        {
                            list.Add(new AbilityData(targetspell, __instance.Caster)
                            {
                                m_ConvertedFrom = __instance,
                                MetamagicData = metamagic.MetamagicData?.Clone(),
                                DecorationBorderNumber = metamagic.DecorationBorderNumber,
                                DecorationColorNumber = metamagic.DecorationColorNumber
                            });
                        }
                        else
                        {
                            foreach (var variant in variants)
                            {
                                list.Add(new AbilityData(variant, __instance.Caster)
                                {
                                    m_ConvertedFrom = __instance,
                                    MetamagicData = metamagic.MetamagicData?.Clone(),
                                    DecorationBorderNumber = metamagic.DecorationBorderNumber,
                                    DecorationColorNumber = metamagic.DecorationColorNumber
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Main.PrintException(e);
            }

            __result = list;
        }

        [HarmonyPatch(typeof(MechanicActionBarSlotSpontaneusConvertedSpell), nameof(MechanicActionBarSlotSpontaneusConvertedSpell.GetDecorationSprite))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        public static void Postfix2(MechanicActionBarSlotSpontaneusConvertedSpell __instance, ref Sprite __result)
        {
            if (__instance.Spell?.ConvertedFrom != null)
                __result = UIUtility.GetDecorationBorderByIndex(__instance.Spell.DecorationBorderNumber);
        }

        [HarmonyPatch(typeof(MechanicActionBarSlotSpontaneusConvertedSpell), nameof(MechanicActionBarSlotSpontaneusConvertedSpell.GetDecorationColor))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        public static void Postfix3(MechanicActionBarSlotSpontaneusConvertedSpell __instance, ref Color __result)
        {
            if (__instance.Spell?.ConvertedFrom != null)
                __result = UIUtility.GetDecorationColorByIndex(__instance.Spell.DecorationColorNumber);
        }
    }
}
