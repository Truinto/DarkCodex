using HarmonyLib;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using System.Collections.Generic;

namespace CodexLib.Patches
{

    // GetFullSelectionItems(): unit independent full selection of items; item must exist for CanSelect and IFeatureSelection logic
    // ExtractSelectionItems(): unit dependent full selection of items
    // CanSelect(): whenever the item is selectable
    // 
    // remarks:
    // - any spell selection (if ParameterType=FeatureParameterType.Custom & Group=ParameterizedAbilitySelection)
    // - specific spell list (if m_SpellList is not null)
    // - spell level range min-max (SpellLevelPenalty=min SpellLevel=max)
    // - any known spell selection (if Group=KnownSpell)
    // - any known ability (if Group=KnownAbility)
    /// <summary>
    /// Custom logic for parametrized feature. Allows selection of any spells/abilities.
    /// </summary>
    [HarmonyPatch]
    public class Patch_SpellSelectionParametrized
    {
        public static bool IsCustom(BlueprintParametrizedFeature __instance)
        {
            return __instance != null && __instance.ParameterType == FeatureParameterType.Custom && __instance.HasGroup(Const.ParameterizedAbilitySelection);
        }

        /// <summary>
        /// Returns 'empty' item collection, since we don't actually need it.
        /// </summary>
        [HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.Items), MethodType.Getter)]
        [HarmonyPriority(Priority.High)]
        [HarmonyPrefix]
        public static bool GetAllItems(BlueprintParametrizedFeature __instance, ref IEnumerable<IFeatureSelectionItem> __result)
        {
            if (!IsCustom(__instance)) return true;
            __result = new FeatureUIData[] { new FeatureUIData(__instance, __instance) };
            return false;
        }

        /// <summary>
        /// Custom selection logic.
        /// </summary>
        [HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.ExtractSelectionItems))]
        [HarmonyPriority(Priority.High)]
        [HarmonyPrefix]
        public static bool GetItemsForUnit(UnitDescriptor beforeLevelUpUnit, UnitDescriptor previewUnit, BlueprintParametrizedFeature __instance, ref IEnumerable<IFeatureSelectionItem> __result)
        {
            if (!IsCustom(__instance)) return true;

            bool knownSpell = __instance.HasGroup(Const.KnownSpell);
            bool knownAbility = __instance.HasGroup(Const.KnownAbility);
            int minLevel = Math.Max(1, __instance.SpellLevelPenalty);
            int maxLevel = __instance.SpellLevel < 1 ? 10 : Math.Min(__instance.SpellLevel, 10);
            var spellList = __instance.SpellList;

            if (knownAbility)
            {
                __result = __instance.ExtractItemsFromBlueprints(previewUnit.Abilities.Enumerable.Select(s => s.Blueprint));
                return false;
            }

            var list = new List<FeatureUIData>();

            // if known spell flag
            if (knownSpell)
            {
                foreach (var book in previewUnit.Spellbooks)
                    for (int i = minLevel; i < maxLevel; i++)
                        foreach (var abilityData in book.GetKnownSpells(i))
                            if (!list.Any(a => a.Param == abilityData.Blueprint))
                                list.Add(new FeatureUIData(__instance, abilityData.Blueprint, abilityData.Blueprint.Name,
                                    abilityData.Blueprint.Description, abilityData.Blueprint.Icon, abilityData.Blueprint.name));
                __result = list;
                return false;

            }

            // if spell list
            if (spellList != null)
            {
                for (int i = minLevel; i < maxLevel; i++)
                    list.AddRange(spellList.GetSpells(i).Select(s => new FeatureUIData(__instance, s, s.Name, s.Description, s.Icon, s.name)));
                __result = list;
                return false;
            }

            // if cached, return that
            if (__instance.m_CachedItems != null)
            {
                __result = __instance.m_CachedItems;
                return false;
            }

            // use custom blueprints, if any
            if (__instance.CustomParameterVariants != null && __instance.CustomParameterVariants.Length > 0)
            {
                foreach (var bp in __instance.CustomParameterVariants)
                    if (bp.Get() is BlueprintUnitFact fact && !list.Any(a => a.Param == fact))
                        list.Add(new FeatureUIData(__instance, fact, fact.Name, fact.Description, fact.Icon, fact.name));

                __result = __instance.m_CachedItems = list.ToArray();
                return false;
            }

            // if none of the above matches, filter for all spells; these are stored in cache
            foreach (var spell in BpCache.Get<BlueprintAbility>())
            {
                //Helper.PrintDebug($"parametrized name={spell.name} hidden={spell.Hidden} parent={spell.m_Parent?.Get()?.name} comp={spell.GetComponent<SpellListComponent>() == null} key={spell.m_DisplayName.IsEmptyKey()} dn={spell.m_DisplayName}");

                if (spell.Hidden
                    || spell.m_Parent.NotEmpty()
                    || spell.GetComponent<SpellListComponent>() == null
                    || spell.m_DisplayName.IsEmptyKey())
                {
                    continue;
                }

                int min = 10;
                int max = 0;

                foreach (var spellComp in spell.GetComponents<SpellListComponent>())
                {
                    if (spellComp.SpellLevel < min) min = spellComp.SpellLevel;
                    if (spellComp.SpellLevel > max) max = spellComp.SpellLevel;
                }

                if (minLevel <= max && maxLevel >= min)
                    list.Add(new FeatureUIData(__instance, spell, spell.Name, spell.Description, spell.Icon, spell.name));
            }
            __result = __instance.m_CachedItems = list.ToArray();
            return false;
        }

        /// <summary>
        /// Can select any item that's not already picked.
        /// </summary>
        [HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.CanSelect))]
        [HarmonyPriority(Priority.High)]
        [HarmonyPrefix]
        public static bool CanSelectFeature(UnitDescriptor unit, LevelUpState state, FeatureSelectionState selectionState, IFeatureSelectionItem item, BlueprintParametrizedFeature __instance, ref bool __result)
        {
            if (!IsCustom(__instance)) return true;

            if (item.Param?.Blueprint == __instance) // this happens only if we returned an 'empty' item collection
            {
                __result = true;
                return false;
            }

            __result = unit.GetFeature(__instance, item.Param) == null;
            return false;
        }

        /// <summary>
        /// Fix call that usually tries to get generic view and replace the list with unit specific version.
        /// </summary>
        [HarmonyPatch(typeof(FeatureSelectionState), nameof(FeatureSelectionState.SetupViewState))]
        [HarmonyPriority(Priority.High)]
        [HarmonyPrefix]
        public static bool FixView(FeatureSelectionState __instance, ref List<FeatureSelectionViewState> __result)
        {
            if (__instance.Selection is not BlueprintParametrizedFeature feature
                || !IsCustom(feature))
                return true;

            var state = Game.Instance.LevelUpController;
            if (state == null)
                return true;

            __result = new List<FeatureSelectionViewState>();

            foreach (IFeatureSelectionItem item in feature.ExtractSelectionItems(state.Unit, state.Preview))
            {
                __result.Add(new FeatureSelectionViewState(__instance, feature, item));
            }

            return false;
        }

        /// <summary>
        /// Fix for 'empty' Items collection.
        /// </summary>
        [HarmonyPatch(typeof(SelectFeature), nameof(SelectFeature.PostLoad))]
        [HarmonyPostfix]
        public static void LevelUpActionPostLoad(SelectFeature __instance)
        {
            if (__instance.m_ItemFeature is not BlueprintParametrizedFeature feature
                || !IsCustom(feature)
                || __instance.m_ItemParam is not FeatureParam param)
                return;

            __instance.Item ??= new FeatureUIData(feature, param, feature.Name, feature.Description, feature.Icon, feature.name);
        }
    }
}
