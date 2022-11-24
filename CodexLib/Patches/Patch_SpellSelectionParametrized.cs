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
    /// <summary>
    /// Custom logic for parametrized feature. Allows selection of any spells/abilities.
    /// </summary>
    [HarmonyPatch]
    public class Patch_SpellSelectionParametrized
    {
        //[HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.ExtractItemsFromSpellbooks))]
        //[HarmonyPrefix]
        public static bool old_Prefix1(UnitDescriptor unit, BlueprintParametrizedFeature __instance, ref IEnumerable<FeatureUIData> __result)
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

        // TODO: check what this does
        //[HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.CanSelect))]
        //[HarmonyPrefix]
        public static bool old_Prefix2(BlueprintParametrizedFeature __instance, ref bool __result, UnitDescriptor unit, LevelUpState state, FeatureSelectionState selectionState, IFeatureSelectionItem item)
        {
            if (__instance.ParameterType != FeatureParameterType.Custom)
                return true;

            if (item.Param == null)
                __result = false;
            else if (__instance.Items.FirstOrDefault(i => i.Feature == item.Feature && i.Param == item.Param) == null)
                __result = false;
            else if (unit.GetFeature(__instance, item.Param) != null)
                __result = false;
            else if (item.Param.Blueprint is BlueprintFact fact && !unit.HasFact(fact))
                __result = !__instance.RequireProficiency;
            else
                __result = !__instance.HasNoSuchFeature;

            return false;
        }




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
            foreach (var v in ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints)
            {
                if (v.Value.Blueprint is not BlueprintAbility spell
                    || spell.Hidden
                    || spell.m_Parent != null
                    || spell.GetComponent<SpellListComponent>() == null
                    || spell.m_DisplayName.IsEmptyKey())
                    continue;

                int min = 10;
                int max = 0;

                foreach (var a in spell.GetComponents<SpellListComponent>())
                {
                    if (a.SpellLevel < min) min = a.SpellLevel;
                    if (a.SpellLevel > max) max = a.SpellLevel;
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

        [HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.GetGroup))]
        [HarmonyPostfix]
        public static void SelectionGroup(BlueprintParametrizedFeature __instance, ref FeatureGroup __result)
        {
            if (IsCustom(__instance))
                __result = FeatureGroup.MythicAdditionalProgressions;
        }

        public static void X()
        {
            var x = new BlueprintParametrizedFeature();

            x.ParameterType = FeatureParameterType.Custom;
            x.Groups = new FeatureGroup[0];
            x.CustomParameterVariants = new AnyBlueprintReference[0];
            x.IgnoreParameterFeaturePrerequisites = false; // only used with BlueprintFeature; if true CanSelect will not check BlueprintFeature's prerequisites
            x.SpecificSpellLevel = false;
            x.SpellLevel = 0;
            x.SpellLevelPenalty = 0;
            x.m_SpellcasterClass = null;
            x.m_SpellList = null;
        }

        // GetFullSelectionItems(): unit independent full selection of items; item must exist for CanSelect and IFeatureSelection logic
        // ExtractSelectionItems(): unit dependent full selection of items
        // CanSelect(): whenever the item is selectable
        // 
        // checklist:
        // - any spell selection (if ParameterType=FeatureParameterType.Custom & Group=ParameterizedAbilitySelection)
        // - specific spell list (if m_SpellList is not null; this list doesn't need to be in assets)
        // - spell level range min-max (SpellLevelPenalty=min SpellLevel=max)
        // - any known spell selection (if Group=KnownSpell)
        // - any known ability (if Group=KnownAbility)
        // - test if patching BlueprintParametrizedFeature.GetGroup() with FeatureGroup.MythicAdditionalProgressions puts it after spell selection
        // 
        // remarks:
        // - m_SpellList could be set to a pseudo blueprint (no guid)
    }
}
