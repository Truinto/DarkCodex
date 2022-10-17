using CodexLib;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_ChangeSpellElement
    {
        public const SpellDescriptor AnyElement = SpellDescriptor.Fire | SpellDescriptor.Acid | SpellDescriptor.Cold | SpellDescriptor.Electricity | SpellDescriptor.Sonic;

        [HarmonyPatch(typeof(IncreaseSpellDescriptorDC), nameof(IncreaseSpellDescriptorDC.OnEventAboutToTrigger))]
        [HarmonyPriority(Priority.High)]
        [HarmonyPrefix]
        public static bool Prefix1(RuleCalculateAbilityParams evt, IncreaseSpellDescriptorDC __instance)
        {
            if (evt.Spell == null)
                return false;

            if (__instance.SpellsOnly && evt.Spellbook == null && evt.Spell.Type != AbilityType.Spell && evt.Spell.Type != AbilityType.SpellLike)
                return false;

            if (!evt.Spell.SpellDescriptor.HasAnyFlag(AnyElement))
                return false;

            if (evt.TryGetCustomData<SpellDescriptor>(Const.KeyChangeElement, out var descriptor))
                descriptor = (evt.Spell.SpellDescriptor & ~AnyElement) | descriptor;
            else
                descriptor = evt.Spell.SpellDescriptor;

            if (descriptor.HasAnyFlag(__instance.Descriptor))
                evt.AddBonusDC(__instance.BonusDC, __instance.ModifierDescriptor);

            return false;
        }

        [HarmonyPatch(typeof(IncreaseSpellContextDescriptorDC), nameof(IncreaseSpellContextDescriptorDC.OnEventAboutToTrigger))]
        [HarmonyPriority(Priority.High)]
        [HarmonyPrefix]
        public static bool Prefix2(RuleCalculateAbilityParams evt, IncreaseSpellContextDescriptorDC __instance)
        {
            if (evt.Spell == null)
                return false;

            if (__instance.SpellsOnly && evt.Spellbook == null && evt.Spell.Type != AbilityType.Spell && evt.Spell.Type != AbilityType.SpellLike)
                return false;

            if (!evt.Spell.SpellDescriptor.HasAnyFlag(AnyElement))
                return false;

            if (evt.TryGetCustomData<SpellDescriptor>(Const.KeyChangeElement, out var descriptor))
                descriptor = (evt.Spell.SpellDescriptor & ~AnyElement) | descriptor;
            else
                descriptor = evt.Spell.SpellDescriptor;

            if (descriptor.HasAnyFlag(__instance.Descriptor))
                evt.AddBonusDC(__instance.Value.Calculate(__instance.Context), __instance.ModifierDescriptor);

            return false;
        }

        [HarmonyPatch(typeof(IncreaseSpellDescriptorCasterLevel), nameof(IncreaseSpellDescriptorCasterLevel.OnEventAboutToTrigger))]
        [HarmonyPriority(Priority.High)]
        [HarmonyPrefix]
        public static bool Prefix3(RuleCalculateAbilityParams evt, IncreaseSpellDescriptorCasterLevel __instance)
        {
            if (!evt.Spell.SpellDescriptor.HasAnyFlag(AnyElement))
                return true;

            if (!evt.TryGetCustomData<SpellDescriptor>(Const.KeyChangeElement, out var descriptor))
                return true;

            descriptor = (evt.Spell.SpellDescriptor & ~AnyElement) | descriptor;

            if (descriptor.HasAnyFlag(__instance.Descriptor))
                evt.AddBonusCasterLevel(__instance.BonusCasterLevel, __instance.ModifierDescriptor);

            return false;
        }
    }
}
