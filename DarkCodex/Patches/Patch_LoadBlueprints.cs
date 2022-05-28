using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using CodexLib;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony | Severity.Hidden, "Patch: Load Blueprints", "needed to load blueprints of other mods", false)]
    [HarmonyPatch]
    public class Patch_LoadBlueprints
    {
        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.AddCachedBlueprint))]
        [HarmonyPostfix]
        public static void OnAddCachedBlueprint(SimpleBlueprint bp)
        {
            try
            {
                if (bp is BlueprintAbility ability && !Resource.Cache.Ability.Contains(ability))
                    Resource.Cache.Ability.Add(ability);
                else if (bp is BlueprintBuff buff && !Resource.Cache.Buff.Contains(buff))
                    Resource.Cache.Buff.Add(buff);
                else if (bp is BlueprintFeature feature && !Resource.Cache.Feature.Contains(feature))
                    Resource.Cache.Feature.Add(feature);
                else if (bp is BlueprintActivatableAbility activatable && !Resource.Cache.Activatable.Contains(activatable))
                    Resource.Cache.Activatable.Add(activatable);
                else if (bp is BlueprintItem item && !Resource.Cache.Item.Contains(item))
                    Resource.Cache.Item.Add(item);
                else if (bp is BlueprintItemEnchantment enchantment && !Resource.Cache.Enchantment.Contains(enchantment))
                    Resource.Cache.Enchantment.Add(enchantment);
            }
            catch (Exception e)
            {
                Main.PrintException(e);
            }
        }
    }
}
