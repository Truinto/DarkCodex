using HarmonyLib;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.EntitySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_FixItemEnchantments
    {
        [HarmonyPatch(typeof(EntityFactsProcessor<ItemEnchantment>), nameof(EntityFactsProcessor<ItemEnchantment>.RemoveFact), typeof(EntityFact))]
        [HarmonyPostfix]
        public static void Postfix1(EntityFact fact) // todo figure out why temporary entchantments from Magic Weapon get lost when saving
        {
            Main.PrintDebug($"ItemEnchantment remove fact: {fact.Name}\n{Environment.StackTrace}");
        }

        [HarmonyPatch(typeof(EntityFactsProcessor<ItemEnchantment>), nameof(EntityFactsProcessor<ItemEnchantment>.RemoveFact), typeof(BlueprintFact))]
        [HarmonyPostfix]
        public static void Postfix2(BlueprintFact blueprint)
        {
            Main.PrintDebug($"ItemEnchantment remove fact 2: {blueprint.name}\n{Environment.StackTrace}");
        }
    }
}
