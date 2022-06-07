using HarmonyLib;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System.Collections.Generic;

namespace CodexLib.Patches
{
    /// <summary>
    /// BlueprintParametrizedFeature of type SpellSpecialization won't work if no prerequisite is required.<br/>
    /// This patch fixes that.
    /// </summary>
    [HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.ExtractItemsFromSpellbooks))]
    public class Patch_SpellSelectionParametrized
    {
        public static bool Prefix(UnitDescriptor unit, BlueprintParametrizedFeature __instance, ref IEnumerable<FeatureUIData> __result)
        {
            if (__instance.Prerequisite != null)
                return true;

            var list = new List<FeatureUIData>();

            foreach (Spellbook spellbook in unit.Spellbooks)
            {
                foreach (SpellLevelList spellLevel in spellbook.Blueprint.SpellList.SpellsByLevel)
                {
                    if (spellLevel.SpellLevel <= spellbook.MaxSpellLevel)
                    {
                        foreach (BlueprintAbility blueprintAbility in spellLevel.SpellsFiltered)
                        {
                            list.Add(new FeatureUIData(__instance, blueprintAbility, blueprintAbility.Name, blueprintAbility.Description, blueprintAbility.Icon, blueprintAbility.name));
                        }
                    }
                }
            }
            __result = list;
            return false;
        }
    }
}
