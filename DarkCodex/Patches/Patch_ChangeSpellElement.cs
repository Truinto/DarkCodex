using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.RuleSystem.Rules.Abilities;
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
        [HarmonyPatch(typeof(IncreaseSpellDescriptorDC), nameof(IncreaseSpellDescriptorDC.OnEventAboutToTrigger))]
        [HarmonyPrefix]
        public static bool Prefix1(RuleCalculateAbilityParams evt, IncreaseSpellDescriptorDC __instance)
        {
            if (__instance.SpellsOnly && evt.Spellbook == null)
                return false;

            var context = __instance.Context;
            if (context.SpellDescriptor == 0)
                context.AddSpellDescriptor(evt.Spell.SpellDescriptor);

            if (context.SpellDescriptor.HasAnyFlag(__instance.Descriptor))
                evt.AddBonusDC(__instance.BonusDC, __instance.ModifierDescriptor);
            return false;
        }

        [HarmonyPatch(typeof(IncreaseSpellContextDescriptorDC), nameof(IncreaseSpellContextDescriptorDC.OnEventAboutToTrigger))]
        [HarmonyPrefix]
        public static bool Prefix2(RuleCalculateAbilityParams evt, IncreaseSpellContextDescriptorDC __instance)
        {
            if (__instance.SpellsOnly && evt.Spellbook == null)
                return false;

            var context = __instance.Context;
            if (context.SpellDescriptor == 0)
                context.AddSpellDescriptor(evt.Spell.SpellDescriptor);

            if (context.SpellDescriptor.HasAnyFlag(__instance.Descriptor))
                evt.AddBonusDC(__instance.Value.Calculate(context), __instance.ModifierDescriptor);
            return false;
        }

        [HarmonyPatch(typeof(IncreaseSpellDescriptorCasterLevel), nameof(IncreaseSpellDescriptorCasterLevel.OnEventAboutToTrigger))]
        [HarmonyPrefix]
        public static bool Prefix3(RuleCalculateAbilityParams evt, IncreaseSpellDescriptorCasterLevel __instance)
        {
            var context = __instance.Context;
            if (context.SpellDescriptor == 0)
                context.AddSpellDescriptor(evt.Spell.SpellDescriptor);

            if (context.SpellDescriptor.HasAnyFlag(__instance.Descriptor))
                evt.AddBonusCasterLevel(__instance.BonusCasterLevel, __instance.ModifierDescriptor);
            return false;
        }
    }
}
