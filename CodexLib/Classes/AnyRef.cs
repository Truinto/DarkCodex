using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Wrapper class that combines common references into one. Will implicitly cast into most types. Does not throw, but may return null.
    /// </summary>
    public class AnyRef : BlueprintReferenceBase
    {
        public T Get<T>() where T : SimpleBlueprint
        {
            var bp = ResourcesLibrary.TryGetBlueprint(this.deserializedGuid);

            if (bp == null)
                Helper.PrintError($"AnyRef could not resolve {this.deserializedGuid}");
            else if (bp is not T)
                Helper.PrintError($"AnyRef {this.deserializedGuid} is not {typeof(T)}, it is {bp.GetType()}");

            return bp as T;
        }

        public T To<T>() where T : BlueprintReferenceBase, new()
        {
            return new T() { deserializedGuid = this.deserializedGuid };
        }

        public static T To<T>(AnyRef bp) where T : BlueprintReferenceBase, new()
        {
            if (bp == null || bp.deserializedGuid == BlueprintGuid.Empty)
                return null;
            return new T() { deserializedGuid = bp.deserializedGuid };
        }

        public static AnyRef Get(BlueprintReferenceBase bp)
        {
            if (bp == null || bp.deserializedGuid == BlueprintGuid.Empty)
                return null;
            return new AnyRef() { deserializedGuid = bp.deserializedGuid };
        }

        public static AnyRef Get(object obj)
        {
            if (obj is string str)
                return new AnyRef() { deserializedGuid = BlueprintGuid.Parse(str) };
            else if (obj is BlueprintReferenceBase bp)
                return new AnyRef() { deserializedGuid = bp.deserializedGuid };
            else if (obj is SimpleBlueprint sb)
                return new AnyRef() { deserializedGuid = sb.AssetGuid };
            Helper.PrintError($"AnyRef could not resolve type {obj.GetType()}");
            return null;
        }

        public override bool Equals(object obj)
        {
            if (obj is BlueprintReferenceBase bp)
                return this.deserializedGuid == bp.deserializedGuid;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static implicit operator AnyRef(string guid) => guid == null ? null : new() { deserializedGuid = BlueprintGuid.Parse(guid) };
        public static implicit operator AnyRef(SimpleBlueprint bp) => bp == null ? null : new() { deserializedGuid = bp.AssetGuid };

        public static implicit operator AnyRef(AnyBlueprintReference bp) => Get(bp);
        public static implicit operator AnyBlueprintReference(AnyRef bp) => To<AnyBlueprintReference>(bp);

        public static implicit operator AnyRef(ArmyRootReference bp) => Get(bp);
        public static implicit operator ArmyRootReference(AnyRef bp) => To<ArmyRootReference>(bp);

        public static implicit operator AnyRef(ArtisanItemDeckReference bp) => Get(bp);
        public static implicit operator ArtisanItemDeckReference(AnyRef bp) => To<ArtisanItemDeckReference>(bp);

        public static implicit operator AnyRef(BlueprintAbilityAreaEffectReference bp) => Get(bp);
        public static implicit operator BlueprintAbilityAreaEffectReference(AnyRef bp) => To<BlueprintAbilityAreaEffectReference>(bp);

        public static implicit operator AnyRef(BlueprintAbilityReference bp) => Get(bp);
        public static implicit operator BlueprintAbilityReference(AnyRef bp) => To<BlueprintAbilityReference>(bp);

        public static implicit operator AnyRef(BlueprintAbilityResourceReference bp) => Get(bp);
        public static implicit operator BlueprintAbilityResourceReference(AnyRef bp) => To<BlueprintAbilityResourceReference>(bp);

        public static implicit operator AnyRef(BlueprintActionListReference bp) => Get(bp);
        public static implicit operator BlueprintActionListReference(AnyRef bp) => To<BlueprintActionListReference>(bp);

        public static implicit operator AnyRef(BlueprintActivatableAbilityReference bp) => Get(bp);
        public static implicit operator BlueprintActivatableAbilityReference(AnyRef bp) => To<BlueprintActivatableAbilityReference>(bp);

        public static implicit operator AnyRef(BlueprintAiActionReference bp) => Get(bp);
        public static implicit operator BlueprintAiActionReference(AnyRef bp) => To<BlueprintAiActionReference>(bp);

        public static implicit operator AnyRef(BlueprintAnswerBaseReference bp) => Get(bp);
        public static implicit operator BlueprintAnswerBaseReference(AnyRef bp) => To<BlueprintAnswerBaseReference>(bp);

        public static implicit operator AnyRef(BlueprintAnswerReference bp) => Get(bp);
        public static implicit operator BlueprintAnswerReference(AnyRef bp) => To<BlueprintAnswerReference>(bp);

        public static implicit operator AnyRef(BlueprintAnswersListReference bp) => Get(bp);
        public static implicit operator BlueprintAnswersListReference(AnyRef bp) => To<BlueprintAnswersListReference>(bp);

        public static implicit operator AnyRef(BlueprintArchetypeReference bp) => Get(bp);
        public static implicit operator BlueprintArchetypeReference(AnyRef bp) => To<BlueprintArchetypeReference>(bp);

        public static implicit operator AnyRef(BlueprintAreaEffectPitVisualSettingsReference bp) => Get(bp);
        public static implicit operator BlueprintAreaEffectPitVisualSettingsReference(AnyRef bp) => To<BlueprintAreaEffectPitVisualSettingsReference>(bp);

        public static implicit operator AnyRef(BlueprintAreaEnterPointReference bp) => Get(bp);
        public static implicit operator BlueprintAreaEnterPointReference(AnyRef bp) => To<BlueprintAreaEnterPointReference>(bp);

        public static implicit operator AnyRef(BlueprintAreaPartReference bp) => Get(bp);
        public static implicit operator BlueprintAreaPartReference(AnyRef bp) => To<BlueprintAreaPartReference>(bp);

        public static implicit operator AnyRef(BlueprintAreaPresetReference bp) => Get(bp);
        public static implicit operator BlueprintAreaPresetReference(AnyRef bp) => To<BlueprintAreaPresetReference>(bp);

        public static implicit operator AnyRef(BlueprintAreaReference bp) => Get(bp);
        public static implicit operator BlueprintAreaReference(AnyRef bp) => To<BlueprintAreaReference>(bp);

        public static implicit operator AnyRef(BlueprintAreaTransitionReference bp) => Get(bp);
        public static implicit operator BlueprintAreaTransitionReference(AnyRef bp) => To<BlueprintAreaTransitionReference>(bp);

        public static implicit operator AnyRef(BlueprintArmorEnchantmentReference bp) => Get(bp);
        public static implicit operator BlueprintArmorEnchantmentReference(AnyRef bp) => To<BlueprintArmorEnchantmentReference>(bp);

        public static implicit operator AnyRef(BlueprintArmorTypeReference bp) => Get(bp);
        public static implicit operator BlueprintArmorTypeReference(AnyRef bp) => To<BlueprintArmorTypeReference>(bp);

        public static implicit operator AnyRef(BlueprintArmyLeaderReference bp) => Get(bp);
        public static implicit operator BlueprintArmyLeaderReference(AnyRef bp) => To<BlueprintArmyLeaderReference>(bp);

        public static implicit operator AnyRef(BlueprintArmyPresetReference bp) => Get(bp);
        public static implicit operator BlueprintArmyPresetReference(AnyRef bp) => To<BlueprintArmyPresetReference>(bp);

        public static implicit operator AnyRef(BlueprintBarkBanterReference bp) => Get(bp);
        public static implicit operator BlueprintBarkBanterReference(AnyRef bp) => To<BlueprintBarkBanterReference>(bp);

        public static implicit operator AnyRef(BlueprintBrainReference bp) => Get(bp);
        public static implicit operator BlueprintBrainReference(AnyRef bp) => To<BlueprintBrainReference>(bp);

        public static implicit operator AnyRef(BlueprintBuffReference bp) => Get(bp);
        public static implicit operator BlueprintBuffReference(AnyRef bp) => To<BlueprintBuffReference>(bp);

        public static implicit operator AnyRef(BlueprintCampaignReference bp) => Get(bp);
        public static implicit operator BlueprintCampaignReference(AnyRef bp) => To<BlueprintCampaignReference>(bp);

        public static implicit operator AnyRef(BlueprintCampingEncounterReference bp) => Get(bp);
        public static implicit operator BlueprintCampingEncounterReference(AnyRef bp) => To<BlueprintCampingEncounterReference>(bp);

        public static implicit operator AnyRef(BlueprintCategoryDefaultsReference bp) => Get(bp);
        public static implicit operator BlueprintCategoryDefaultsReference(AnyRef bp) => To<BlueprintCategoryDefaultsReference>(bp);

        public static implicit operator AnyRef(BlueprintCharacterClassGroupReference bp) => Get(bp);
        public static implicit operator BlueprintCharacterClassGroupReference(AnyRef bp) => To<BlueprintCharacterClassGroupReference>(bp);

        public static implicit operator AnyRef(BlueprintCharacterClassReference bp) => Get(bp);
        public static implicit operator BlueprintCharacterClassReference(AnyRef bp) => To<BlueprintCharacterClassReference>(bp);

        public static implicit operator AnyRef(BlueprintCheckReference bp) => Get(bp);
        public static implicit operator BlueprintCheckReference(AnyRef bp) => To<BlueprintCheckReference>(bp);

        public static implicit operator AnyRef(BlueprintClockworkScenarioPartReference bp) => Get(bp);
        public static implicit operator BlueprintClockworkScenarioPartReference(AnyRef bp) => To<BlueprintClockworkScenarioPartReference>(bp);

        public static implicit operator AnyRef(BlueprintCompanionStoryReference bp) => Get(bp);
        public static implicit operator BlueprintCompanionStoryReference(AnyRef bp) => To<BlueprintCompanionStoryReference>(bp);

        public static implicit operator AnyRef(BlueprintComponentListReference bp) => Get(bp);
        public static implicit operator BlueprintComponentListReference(AnyRef bp) => To<BlueprintComponentListReference>(bp);

        public static implicit operator AnyRef(BlueprintControllableProjectileReference bp) => Get(bp);
        public static implicit operator BlueprintControllableProjectileReference(AnyRef bp) => To<BlueprintControllableProjectileReference>(bp);

        public static implicit operator AnyRef(BlueprintCookingRecipeReference bp) => Get(bp);
        public static implicit operator BlueprintCookingRecipeReference(AnyRef bp) => To<BlueprintCookingRecipeReference>(bp);

        public static implicit operator AnyRef(BlueprintCreditsGroupReference bp) => Get(bp);
        public static implicit operator BlueprintCreditsGroupReference(AnyRef bp) => To<BlueprintCreditsGroupReference>(bp);

        public static implicit operator AnyRef(BlueprintCreditsRolesReference bp) => Get(bp);
        public static implicit operator BlueprintCreditsRolesReference(AnyRef bp) => To<BlueprintCreditsRolesReference>(bp);

        public static implicit operator AnyRef(BlueprintCreditsTeamsReference bp) => Get(bp);
        public static implicit operator BlueprintCreditsTeamsReference(AnyRef bp) => To<BlueprintCreditsTeamsReference>(bp);

        public static implicit operator AnyRef(BlueprintCueBaseReference bp) => Get(bp);
        public static implicit operator BlueprintCueBaseReference(AnyRef bp) => To<BlueprintCueBaseReference>(bp);

        public static implicit operator AnyRef(BlueprintDialogExperienceModifierTableReference bp) => Get(bp);
        public static implicit operator BlueprintDialogExperienceModifierTableReference(AnyRef bp) => To<BlueprintDialogExperienceModifierTableReference>(bp);

        public static implicit operator AnyRef(BlueprintDialogReference bp) => Get(bp);
        public static implicit operator BlueprintDialogReference(AnyRef bp) => To<BlueprintDialogReference>(bp);

        public static implicit operator AnyRef(BlueprintDlcReference bp) => Get(bp);
        public static implicit operator BlueprintDlcReference(AnyRef bp) => To<BlueprintDlcReference>(bp);

        public static implicit operator AnyRef(BlueprintDlcRewardCampaignReference bp) => Get(bp);
        public static implicit operator BlueprintDlcRewardCampaignReference(AnyRef bp) => To<BlueprintDlcRewardCampaignReference>(bp);

        public static implicit operator AnyRef(BlueprintDlcRewardReference bp) => Get(bp);
        public static implicit operator BlueprintDlcRewardReference(AnyRef bp) => To<BlueprintDlcRewardReference>(bp);

        public static implicit operator AnyRef(BlueprintDungeonBoonReference bp) => Get(bp);
        public static implicit operator BlueprintDungeonBoonReference(AnyRef bp) => To<BlueprintDungeonBoonReference>(bp);

        public static implicit operator AnyRef(BlueprintDungeonLocalizedStringsReference bp) => Get(bp);
        public static implicit operator BlueprintDungeonLocalizedStringsReference(AnyRef bp) => To<BlueprintDungeonLocalizedStringsReference>(bp);

        public static implicit operator AnyRef(BlueprintDungeonRootReference bp) => Get(bp);
        public static implicit operator BlueprintDungeonRootReference(AnyRef bp) => To<BlueprintDungeonRootReference>(bp);

        public static implicit operator AnyRef(BlueprintDungeonShrineReference bp) => Get(bp);
        public static implicit operator BlueprintDungeonShrineReference(AnyRef bp) => To<BlueprintDungeonShrineReference>(bp);

        public static implicit operator AnyRef(BlueprintDungeonSpawnableReference bp) => Get(bp);
        public static implicit operator BlueprintDungeonSpawnableReference(AnyRef bp) => To<BlueprintDungeonSpawnableReference>(bp);

        public static implicit operator AnyRef(BlueprintDynamicMapObjectReference bp) => Get(bp);
        public static implicit operator BlueprintDynamicMapObjectReference(AnyRef bp) => To<BlueprintDynamicMapObjectReference>(bp);

        public static implicit operator AnyRef(BlueprintEncyclopediaChapterReference bp) => Get(bp);
        public static implicit operator BlueprintEncyclopediaChapterReference(AnyRef bp) => To<BlueprintEncyclopediaChapterReference>(bp);

        public static implicit operator AnyRef(BlueprintEncyclopediaNodeReference bp) => Get(bp);
        public static implicit operator BlueprintEncyclopediaNodeReference(AnyRef bp) => To<BlueprintEncyclopediaNodeReference>(bp);

        public static implicit operator AnyRef(BlueprintEncyclopediaPageReference bp) => Get(bp);
        public static implicit operator BlueprintEncyclopediaPageReference(AnyRef bp) => To<BlueprintEncyclopediaPageReference>(bp);

        public static implicit operator AnyRef(BlueprintEquipmentEnchantmentReference bp) => Get(bp);
        public static implicit operator BlueprintEquipmentEnchantmentReference(AnyRef bp) => To<BlueprintEquipmentEnchantmentReference>(bp);

        public static implicit operator AnyRef(BlueprintEtudeConflictingGroupReference bp) => Get(bp);
        public static implicit operator BlueprintEtudeConflictingGroupReference(AnyRef bp) => To<BlueprintEtudeConflictingGroupReference>(bp);

        public static implicit operator AnyRef(BlueprintEtudeReference bp) => Get(bp);
        public static implicit operator BlueprintEtudeReference(AnyRef bp) => To<BlueprintEtudeReference>(bp);

        public static implicit operator AnyRef(BlueprintFactionReference bp) => Get(bp);
        public static implicit operator BlueprintFactionReference(AnyRef bp) => To<BlueprintFactionReference>(bp);

        public static implicit operator AnyRef(BlueprintFeatureBaseReference bp) => Get(bp);
        public static implicit operator BlueprintFeatureBaseReference(AnyRef bp) => To<BlueprintFeatureBaseReference>(bp);

        public static implicit operator AnyRef(BlueprintFeatureReference bp) => Get(bp);
        public static implicit operator BlueprintFeatureReference(AnyRef bp) => To<BlueprintFeatureReference>(bp);

        public static implicit operator AnyRef(BlueprintFeatureSelectionReference bp) => Get(bp);
        public static implicit operator BlueprintFeatureSelectionReference(AnyRef bp) => To<BlueprintFeatureSelectionReference>(bp);

        public static implicit operator AnyRef(BlueprintFeatureSelectMythicSpellbookReference bp) => Get(bp);
        public static implicit operator BlueprintFeatureSelectMythicSpellbookReference(AnyRef bp) => To<BlueprintFeatureSelectMythicSpellbookReference>(bp);

        public static implicit operator AnyRef(BlueprintFollowersFormationReference bp) => Get(bp);
        public static implicit operator BlueprintFollowersFormationReference(AnyRef bp) => To<BlueprintFollowersFormationReference>(bp);

        public static implicit operator AnyRef(BlueprintFootprintReference bp) => Get(bp);
        public static implicit operator BlueprintFootprintReference(AnyRef bp) => To<BlueprintFootprintReference>(bp);

        public static implicit operator AnyRef(BlueprintFootprintTypeReference bp) => Get(bp);
        public static implicit operator BlueprintFootprintTypeReference(AnyRef bp) => To<BlueprintFootprintTypeReference>(bp);

        public static implicit operator AnyRef(BlueprintGenericPackLootReference bp) => Get(bp);
        public static implicit operator BlueprintGenericPackLootReference(AnyRef bp) => To<BlueprintGenericPackLootReference>(bp);

        public static implicit operator AnyRef(BlueprintGlobalMapPointReference bp) => Get(bp);
        public static implicit operator BlueprintGlobalMapPointReference(AnyRef bp) => To<BlueprintGlobalMapPointReference>(bp);

        public static implicit operator AnyRef(BlueprintGlobalMapReference bp) => Get(bp);
        public static implicit operator BlueprintGlobalMapReference(AnyRef bp) => To<BlueprintGlobalMapReference>(bp);

        public static implicit operator AnyRef(BlueprintItemArmorReference bp) => Get(bp);
        public static implicit operator BlueprintItemArmorReference(AnyRef bp) => To<BlueprintItemArmorReference>(bp);

        public static implicit operator AnyRef(BlueprintItemEnchantmentReference bp) => Get(bp);
        public static implicit operator BlueprintItemEnchantmentReference(AnyRef bp) => To<BlueprintItemEnchantmentReference>(bp);

        public static implicit operator AnyRef(BlueprintItemEquipmentBeltReference bp) => Get(bp);
        public static implicit operator BlueprintItemEquipmentBeltReference(AnyRef bp) => To<BlueprintItemEquipmentBeltReference>(bp);

        public static implicit operator AnyRef(BlueprintItemEquipmentFeetReference bp) => Get(bp);
        public static implicit operator BlueprintItemEquipmentFeetReference(AnyRef bp) => To<BlueprintItemEquipmentFeetReference>(bp);

        public static implicit operator AnyRef(BlueprintItemEquipmentGlassesReference bp) => Get(bp);
        public static implicit operator BlueprintItemEquipmentGlassesReference(AnyRef bp) => To<BlueprintItemEquipmentGlassesReference>(bp);

        public static implicit operator AnyRef(BlueprintItemEquipmentGlovesReference bp) => Get(bp);
        public static implicit operator BlueprintItemEquipmentGlovesReference(AnyRef bp) => To<BlueprintItemEquipmentGlovesReference>(bp);

        public static implicit operator AnyRef(BlueprintItemEquipmentHandReference bp) => Get(bp);
        public static implicit operator BlueprintItemEquipmentHandReference(AnyRef bp) => To<BlueprintItemEquipmentHandReference>(bp);

        public static implicit operator AnyRef(BlueprintItemEquipmentHeadReference bp) => Get(bp);
        public static implicit operator BlueprintItemEquipmentHeadReference(AnyRef bp) => To<BlueprintItemEquipmentHeadReference>(bp);

        public static implicit operator AnyRef(BlueprintItemEquipmentNeckReference bp) => Get(bp);
        public static implicit operator BlueprintItemEquipmentNeckReference(AnyRef bp) => To<BlueprintItemEquipmentNeckReference>(bp);

        public static implicit operator AnyRef(BlueprintItemEquipmentReference bp) => Get(bp);
        public static implicit operator BlueprintItemEquipmentReference(AnyRef bp) => To<BlueprintItemEquipmentReference>(bp);

        public static implicit operator AnyRef(BlueprintItemEquipmentRingReference bp) => Get(bp);
        public static implicit operator BlueprintItemEquipmentRingReference(AnyRef bp) => To<BlueprintItemEquipmentRingReference>(bp);

        public static implicit operator AnyRef(BlueprintItemEquipmentShirtReference bp) => Get(bp);
        public static implicit operator BlueprintItemEquipmentShirtReference(AnyRef bp) => To<BlueprintItemEquipmentShirtReference>(bp);

        public static implicit operator AnyRef(BlueprintItemEquipmentShouldersReference bp) => Get(bp);
        public static implicit operator BlueprintItemEquipmentShouldersReference(AnyRef bp) => To<BlueprintItemEquipmentShouldersReference>(bp);

        public static implicit operator AnyRef(BlueprintItemEquipmentUsableReference bp) => Get(bp);
        public static implicit operator BlueprintItemEquipmentUsableReference(AnyRef bp) => To<BlueprintItemEquipmentUsableReference>(bp);

        public static implicit operator AnyRef(BlueprintItemEquipmentWristReference bp) => Get(bp);
        public static implicit operator BlueprintItemEquipmentWristReference(AnyRef bp) => To<BlueprintItemEquipmentWristReference>(bp);

        public static implicit operator AnyRef(BlueprintItemReference bp) => Get(bp);
        public static implicit operator BlueprintItemReference(AnyRef bp) => To<BlueprintItemReference>(bp);

        public static implicit operator AnyRef(BlueprintItemWeaponReference bp) => Get(bp);
        public static implicit operator BlueprintItemWeaponReference(AnyRef bp) => To<BlueprintItemWeaponReference>(bp);

        public static implicit operator AnyRef(BlueprintKingdomArtisanReference bp) => Get(bp);
        public static implicit operator BlueprintKingdomArtisanReference(AnyRef bp) => To<BlueprintKingdomArtisanReference>(bp);

        public static implicit operator AnyRef(BlueprintKingdomBuffReference bp) => Get(bp);
        public static implicit operator BlueprintKingdomBuffReference(AnyRef bp) => To<BlueprintKingdomBuffReference>(bp);

        public static implicit operator AnyRef(BlueprintKingdomClaimReference bp) => Get(bp);
        public static implicit operator BlueprintKingdomClaimReference(AnyRef bp) => To<BlueprintKingdomClaimReference>(bp);

        public static implicit operator AnyRef(BlueprintKingdomDeckReference bp) => Get(bp);
        public static implicit operator BlueprintKingdomDeckReference(AnyRef bp) => To<BlueprintKingdomDeckReference>(bp);

        public static implicit operator AnyRef(BlueprintKingdomEventBaseReference bp) => Get(bp);
        public static implicit operator BlueprintKingdomEventBaseReference(AnyRef bp) => To<BlueprintKingdomEventBaseReference>(bp);

        public static implicit operator AnyRef(BlueprintKingdomEventReference bp) => Get(bp);
        public static implicit operator BlueprintKingdomEventReference(AnyRef bp) => To<BlueprintKingdomEventReference>(bp);

        public static implicit operator AnyRef(BlueprintKingdomEventTimelineReference bp) => Get(bp);
        public static implicit operator BlueprintKingdomEventTimelineReference(AnyRef bp) => To<BlueprintKingdomEventTimelineReference>(bp);

        public static implicit operator AnyRef(BlueprintKingdomProjectReference bp) => Get(bp);
        public static implicit operator BlueprintKingdomProjectReference(AnyRef bp) => To<BlueprintKingdomProjectReference>(bp);

        public static implicit operator AnyRef(BlueprintKingdomUpgradeReference bp) => Get(bp);
        public static implicit operator BlueprintKingdomUpgradeReference(AnyRef bp) => To<BlueprintKingdomUpgradeReference>(bp);

        public static implicit operator AnyRef(BlueprintLeaderSkillReference bp) => Get(bp);
        public static implicit operator BlueprintLeaderSkillReference(AnyRef bp) => To<BlueprintLeaderSkillReference>(bp);

        public static implicit operator AnyRef(BlueprintLoadingScreenSpriteListReference bp) => Get(bp);
        public static implicit operator BlueprintLoadingScreenSpriteListReference(AnyRef bp) => To<BlueprintLoadingScreenSpriteListReference>(bp);

        public static implicit operator AnyRef(BlueprintLogicConnectorReference bp) => Get(bp);
        public static implicit operator BlueprintLogicConnectorReference(AnyRef bp) => To<BlueprintLogicConnectorReference>(bp);

        public static implicit operator AnyRef(BlueprintLootReference bp) => Get(bp);
        public static implicit operator BlueprintLootReference(AnyRef bp) => To<BlueprintLootReference>(bp);

        public static implicit operator AnyRef(BlueprintMultiEntranceEntryReference bp) => Get(bp);
        public static implicit operator BlueprintMultiEntranceEntryReference(AnyRef bp) => To<BlueprintMultiEntranceEntryReference>(bp);

        public static implicit operator AnyRef(BlueprintMultiEntranceReference bp) => Get(bp);
        public static implicit operator BlueprintMultiEntranceReference(AnyRef bp) => To<BlueprintMultiEntranceReference>(bp);

        public static implicit operator AnyRef(BlueprintMythicInfoReference bp) => Get(bp);
        public static implicit operator BlueprintMythicInfoReference(AnyRef bp) => To<BlueprintMythicInfoReference>(bp);

        public static implicit operator AnyRef(BlueprintMythicsSettingsReference bp) => Get(bp);
        public static implicit operator BlueprintMythicsSettingsReference(AnyRef bp) => To<BlueprintMythicsSettingsReference>(bp);

        public static implicit operator AnyRef(BlueprintParametrizedFeatureReference bp) => Get(bp);
        public static implicit operator BlueprintParametrizedFeatureReference(AnyRef bp) => To<BlueprintParametrizedFeatureReference>(bp);

        public static implicit operator AnyRef(BlueprintPartyFormationReference bp) => Get(bp);
        public static implicit operator BlueprintPartyFormationReference(AnyRef bp) => To<BlueprintPartyFormationReference>(bp);

        public static implicit operator AnyRef(BlueprintPortraitReference bp) => Get(bp);
        public static implicit operator BlueprintPortraitReference(AnyRef bp) => To<BlueprintPortraitReference>(bp);

        public static implicit operator AnyRef(BlueprintProgressionReference bp) => Get(bp);
        public static implicit operator BlueprintProgressionReference(AnyRef bp) => To<BlueprintProgressionReference>(bp);

        public static implicit operator AnyRef(BlueprintProjectileReference bp) => Get(bp);
        public static implicit operator BlueprintProjectileReference(AnyRef bp) => To<BlueprintProjectileReference>(bp);

        public static implicit operator AnyRef(BlueprintProjectileTrajectoryReference bp) => Get(bp);
        public static implicit operator BlueprintProjectileTrajectoryReference(AnyRef bp) => To<BlueprintProjectileTrajectoryReference>(bp);

        public static implicit operator AnyRef(BlueprintQuestGroupsReference bp) => Get(bp);
        public static implicit operator BlueprintQuestGroupsReference(AnyRef bp) => To<BlueprintQuestGroupsReference>(bp);

        public static implicit operator AnyRef(BlueprintQuestObjectiveReference bp) => Get(bp);
        public static implicit operator BlueprintQuestObjectiveReference(AnyRef bp) => To<BlueprintQuestObjectiveReference>(bp);

        public static implicit operator AnyRef(BlueprintQuestReference bp) => Get(bp);
        public static implicit operator BlueprintQuestReference(AnyRef bp) => To<BlueprintQuestReference>(bp);

        public static implicit operator AnyRef(BlueprintRaceReference bp) => Get(bp);
        public static implicit operator BlueprintRaceReference(AnyRef bp) => To<BlueprintRaceReference>(bp);

        public static implicit operator AnyRef(BlueprintRaceVisualPresetReference bp) => Get(bp);
        public static implicit operator BlueprintRaceVisualPresetReference(AnyRef bp) => To<BlueprintRaceVisualPresetReference>(bp);

        public static implicit operator AnyRef(BlueprintRandomEncounterReference bp) => Get(bp);
        public static implicit operator BlueprintRandomEncounterReference(AnyRef bp) => To<BlueprintRandomEncounterReference>(bp);

        public static implicit operator AnyRef(BlueprintRegionReference bp) => Get(bp);
        public static implicit operator BlueprintRegionReference(AnyRef bp) => To<BlueprintRegionReference>(bp);

        public static implicit operator AnyRef(BlueprintRomanceCounterReference bp) => Get(bp);
        public static implicit operator BlueprintRomanceCounterReference(AnyRef bp) => To<BlueprintRomanceCounterReference>(bp);

        public static implicit operator AnyRef(BlueprintScriptableObjectReference bp) => Get(bp);
        public static implicit operator BlueprintScriptableObjectReference(AnyRef bp) => To<BlueprintScriptableObjectReference>(bp);

        public static implicit operator AnyRef(BlueprintScriptZoneReference bp) => Get(bp);
        public static implicit operator BlueprintScriptZoneReference(AnyRef bp) => To<BlueprintScriptZoneReference>(bp);

        public static implicit operator AnyRef(BlueprintSequenceExitReference bp) => Get(bp);
        public static implicit operator BlueprintSequenceExitReference(AnyRef bp) => To<BlueprintSequenceExitReference>(bp);

        public static implicit operator AnyRef(BlueprintSettlementBuildingReference bp) => Get(bp);
        public static implicit operator BlueprintSettlementBuildingReference(AnyRef bp) => To<BlueprintSettlementBuildingReference>(bp);

        public static implicit operator AnyRef(BlueprintSharedVendorTableReference bp) => Get(bp);
        public static implicit operator BlueprintSharedVendorTableReference(AnyRef bp) => To<BlueprintSharedVendorTableReference>(bp);

        public static implicit operator AnyRef(BlueprintSpellbookReference bp) => Get(bp);
        public static implicit operator BlueprintSpellbookReference(AnyRef bp) => To<BlueprintSpellbookReference>(bp);

        public static implicit operator AnyRef(BlueprintSpellListReference bp) => Get(bp);
        public static implicit operator BlueprintSpellListReference(AnyRef bp) => To<BlueprintSpellListReference>(bp);

        public static implicit operator AnyRef(BlueprintSpellsTableReference bp) => Get(bp);
        public static implicit operator BlueprintSpellsTableReference(AnyRef bp) => To<BlueprintSpellsTableReference>(bp);

        public static implicit operator AnyRef(BlueprintStatProgressionReference bp) => Get(bp);
        public static implicit operator BlueprintStatProgressionReference(AnyRef bp) => To<BlueprintStatProgressionReference>(bp);

        public static implicit operator AnyRef(BlueprintSummonPoolReference bp) => Get(bp);
        public static implicit operator BlueprintSummonPoolReference(AnyRef bp) => To<BlueprintSummonPoolReference>(bp);

        public static implicit operator AnyRef(BlueprintTimeOfDaySettingsReference bp) => Get(bp);
        public static implicit operator BlueprintTimeOfDaySettingsReference(AnyRef bp) => To<BlueprintTimeOfDaySettingsReference>(bp);

        public static implicit operator AnyRef(BlueprintTrapReference bp) => Get(bp);
        public static implicit operator BlueprintTrapReference(AnyRef bp) => To<BlueprintTrapReference>(bp);

        public static implicit operator AnyRef(BlueprintTrapSettingsReference bp) => Get(bp);
        public static implicit operator BlueprintTrapSettingsReference(AnyRef bp) => To<BlueprintTrapSettingsReference>(bp);

        public static implicit operator AnyRef(BlueprintTrapSettingsRootReference bp) => Get(bp);
        public static implicit operator BlueprintTrapSettingsRootReference(AnyRef bp) => To<BlueprintTrapSettingsRootReference>(bp);

        public static implicit operator AnyRef(BlueprintUIInteractionTypeSpritesReference bp) => Get(bp);
        public static implicit operator BlueprintUIInteractionTypeSpritesReference(AnyRef bp) => To<BlueprintUIInteractionTypeSpritesReference>(bp);

        public static implicit operator AnyRef(BlueprintUISoundReference bp) => Get(bp);
        public static implicit operator BlueprintUISoundReference(AnyRef bp) => To<BlueprintUISoundReference>(bp);

        public static implicit operator AnyRef(BlueprintUnitAsksListReference bp) => Get(bp);
        public static implicit operator BlueprintUnitAsksListReference(AnyRef bp) => To<BlueprintUnitAsksListReference>(bp);

        public static implicit operator AnyRef(BlueprintUnitFactReference bp) => Get(bp);
        public static implicit operator BlueprintUnitFactReference(AnyRef bp) => To<BlueprintUnitFactReference>(bp);

        public static implicit operator AnyRef(BlueprintUnitLootReference bp) => Get(bp);
        public static implicit operator BlueprintUnitLootReference(AnyRef bp) => To<BlueprintUnitLootReference>(bp);

        public static implicit operator AnyRef(BlueprintUnitPropertyReference bp) => Get(bp);
        public static implicit operator BlueprintUnitPropertyReference(AnyRef bp) => To<BlueprintUnitPropertyReference>(bp);

        public static implicit operator AnyRef(BlueprintUnitReference bp) => Get(bp);
        public static implicit operator BlueprintUnitReference(AnyRef bp) => To<BlueprintUnitReference>(bp);

        public static implicit operator AnyRef(BlueprintUnitTemplateReference bp) => Get(bp);
        public static implicit operator BlueprintUnitTemplateReference(AnyRef bp) => To<BlueprintUnitTemplateReference>(bp);

        public static implicit operator AnyRef(BlueprintUnitTypeReference bp) => Get(bp);
        public static implicit operator BlueprintUnitTypeReference(AnyRef bp) => To<BlueprintUnitTypeReference>(bp);

        public static implicit operator AnyRef(BlueprintUnlockableFlagReference bp) => Get(bp);
        public static implicit operator BlueprintUnlockableFlagReference(AnyRef bp) => To<BlueprintUnlockableFlagReference>(bp);

        public static implicit operator AnyRef(BlueprintWeaponEnchantmentReference bp) => Get(bp);
        public static implicit operator BlueprintWeaponEnchantmentReference(AnyRef bp) => To<BlueprintWeaponEnchantmentReference>(bp);

        public static implicit operator AnyRef(BlueprintWeaponTypeReference bp) => Get(bp);
        public static implicit operator BlueprintWeaponTypeReference(AnyRef bp) => To<BlueprintWeaponTypeReference>(bp);

        public static implicit operator AnyRef(ConsiderationReference bp) => Get(bp);
        public static implicit operator ConsiderationReference(AnyRef bp) => To<ConsiderationReference>(bp);

        public static implicit operator AnyRef(ConsoleRootReference bp) => Get(bp);
        public static implicit operator ConsoleRootReference(AnyRef bp) => To<ConsoleRootReference>(bp);

        public static implicit operator AnyRef(CutsceneReference bp) => Get(bp);
        public static implicit operator CutsceneReference(AnyRef bp) => To<CutsceneReference>(bp);

        public static implicit operator AnyRef(FormationsRootReference bp) => Get(bp);
        public static implicit operator FormationsRootReference(AnyRef bp) => To<FormationsRootReference>(bp);

        public static implicit operator AnyRef(FxRootReference bp) => Get(bp);
        public static implicit operator FxRootReference(AnyRef bp) => To<FxRootReference>(bp);

        public static implicit operator AnyRef(GateReference bp) => Get(bp);
        public static implicit operator GateReference(AnyRef bp) => To<GateReference>(bp);

        public static implicit operator AnyRef(HitSystemRootReference bp) => Get(bp);
        public static implicit operator HitSystemRootReference(AnyRef bp) => To<HitSystemRootReference>(bp);

        public static implicit operator AnyRef(KingdomRootReference bp) => Get(bp);
        public static implicit operator KingdomRootReference(AnyRef bp) => To<KingdomRootReference>(bp);

        public static implicit operator AnyRef(KingdomUIRootReference bp) => Get(bp);
        public static implicit operator KingdomUIRootReference(AnyRef bp) => To<KingdomUIRootReference>(bp);

        public static implicit operator AnyRef(KingmakerEquipmentEntityReference bp) => Get(bp);
        public static implicit operator KingmakerEquipmentEntityReference(AnyRef bp) => To<KingmakerEquipmentEntityReference>(bp);

        public static implicit operator AnyRef(LeadersRootReference bp) => Get(bp);
        public static implicit operator LeadersRootReference(AnyRef bp) => To<LeadersRootReference>(bp);

        public static implicit operator AnyRef(RaceGenderDistributionReference bp) => Get(bp);
        public static implicit operator RaceGenderDistributionReference(AnyRef bp) => To<RaceGenderDistributionReference>(bp);

        public static implicit operator AnyRef(RandomEncountersRootReference bp) => Get(bp);
        public static implicit operator RandomEncountersRootReference(AnyRef bp) => To<RandomEncountersRootReference>(bp);

        public static implicit operator AnyRef(SettlementBuildListReference bp) => Get(bp);
        public static implicit operator SettlementBuildListReference(AnyRef bp) => To<SettlementBuildListReference>(bp);

        public static implicit operator AnyRef(TrashLootSettingsReference bp) => Get(bp);
        public static implicit operator TrashLootSettingsReference(AnyRef bp) => To<TrashLootSettingsReference>(bp);

        public static implicit operator AnyRef(UnitCustomizationPresetReference bp) => Get(bp);
        public static implicit operator UnitCustomizationPresetReference(AnyRef bp) => To<UnitCustomizationPresetReference>(bp);


        /* all reference types
            (.+)
            public static implicit operator AnyRef\(${1} bp\) => Get\(bp\);\npublic static implicit operator ${1}\(AnyRef bp\) => To<${1}>\(bp\);\n
AnyBlueprintReference
AreaBlueprintAreaMechanicsReference
ArmyRootReference
ArtisanItemDeckReference
BlueprintAbilityAreaEffectReference
BlueprintAbilityReference
BlueprintAbilityResourceReference
BlueprintActionListReference
BlueprintActionListReference
BlueprintActivatableAbilityReference
BlueprintAiActionReference
BlueprintAnswerBaseReference
BlueprintAnswerReference
BlueprintAnswersListReference
BlueprintArchetypeReference
BlueprintAreaEffectPitVisualSettingsReference
BlueprintAreaEnterPointReference
BlueprintAreaPartReference
BlueprintAreaPresetReference
BlueprintAreaReference
BlueprintAreaTransitionReference
BlueprintArmorEnchantmentReference
BlueprintArmorTypeReference
BlueprintArmyLeaderReference
BlueprintArmyPresetReference
BlueprintBarkBanterReference
BlueprintBrainReference
BlueprintBuffReference
BlueprintCampaignReference
BlueprintCampingEncounterReference
BlueprintCategoryDefaultsReference
BlueprintCharacterClassGroupReference
BlueprintCharacterClassReference
BlueprintCheckReference
BlueprintClockworkScenarioPartReference
BlueprintCompanionStoryReference
BlueprintComponentListReference
BlueprintControllableProjectileReference
BlueprintCookingRecipeReference
BlueprintCreditsGroupReference
BlueprintCreditsRolesReference
BlueprintCreditsTeamsReference
BlueprintCueBaseReference
BlueprintDialogExperienceModifierTableReference
BlueprintDialogReference
BlueprintDlcReference
BlueprintDlcRewardCampaignReference
BlueprintDlcRewardReference
BlueprintDungeonBoonReference
BlueprintDungeonLocalizedStringsReference
BlueprintDungeonRootReference
BlueprintDungeonShrineReference
BlueprintDungeonSpawnableReference
BlueprintDynamicMapObjectReference
BlueprintEncyclopediaChapterReference
BlueprintEncyclopediaNodeReference
BlueprintEncyclopediaPageReference
BlueprintEquipmentEnchantmentReference
BlueprintEtudeConflictingGroupReference
BlueprintEtudeReference
BlueprintFactionReference
BlueprintFeatureBaseReference
BlueprintFeatureReference
BlueprintFeatureSelectionReference
BlueprintFeatureSelectMythicSpellbookReference
BlueprintFollowersFormationReference
BlueprintFootprintReference
BlueprintFootprintTypeReference
BlueprintGenericPackLootReference
BlueprintGlobalMapPointReference
BlueprintGlobalMapReference
BlueprintItemArmorReference
BlueprintItemEnchantmentReference
BlueprintItemEquipmentBeltReference
BlueprintItemEquipmentFeetReference
BlueprintItemEquipmentGlassesReference
BlueprintItemEquipmentGlovesReference
BlueprintItemEquipmentHandReference
BlueprintItemEquipmentHeadReference
BlueprintItemEquipmentNeckReference
BlueprintItemEquipmentReference
BlueprintItemEquipmentRingReference
BlueprintItemEquipmentShirtReference
BlueprintItemEquipmentShouldersReference
BlueprintItemEquipmentUsableReference
BlueprintItemEquipmentWristReference
BlueprintItemReference
BlueprintItemWeaponReference
BlueprintKingdomArtisanReference
BlueprintKingdomBuffReference
BlueprintKingdomClaimReference
BlueprintKingdomDeckReference
BlueprintKingdomEventBaseReference
BlueprintKingdomEventReference
BlueprintKingdomEventTimelineReference
BlueprintKingdomProjectReference
BlueprintKingdomUpgradeReference
BlueprintLeaderSkillReference
BlueprintLoadingScreenSpriteListReference
BlueprintLogicConnectorReference
BlueprintLootReference
BlueprintMultiEntranceEntryReference
BlueprintMultiEntranceReference
BlueprintMythicInfoReference
BlueprintMythicsSettingsReference
BlueprintParametrizedFeatureReference
BlueprintPartyFormationReference
BlueprintPetReference
BlueprintPortraitReference
BlueprintProgressionReference
BlueprintProjectileReference
BlueprintProjectileTrajectoryReference
BlueprintQuestGroupsReference
BlueprintQuestObjectiveReference
BlueprintQuestReference
BlueprintRaceReference
BlueprintRaceVisualPresetReference
BlueprintRandomEncounterReference
BlueprintRegionReference
BlueprintRomanceCounterReference
BlueprintScriptableObjectReference
BlueprintScriptZoneReference
BlueprintSequenceExitReference
BlueprintSettlementBuildingReference
BlueprintSharedVendorTableReference
BlueprintSpellbookReference
BlueprintSpellListReference
BlueprintSpellsTableReference
BlueprintStatProgressionReference
BlueprintSummonPoolReference
BlueprintTimeOfDaySettingsReference
BlueprintTrapReference
BlueprintTrapSettingsReference
BlueprintTrapSettingsRootReference
BlueprintUIInteractionTypeSpritesReference
BlueprintUISoundReference
BlueprintUnitAsksListReference
BlueprintUnitFactReference
BlueprintUnitLootReference
BlueprintUnitPropertyReference
BlueprintUnitReference
BlueprintUnitTemplateReference
BlueprintUnitTypeReference
BlueprintUnlockableFlagReference
BlueprintWeaponEnchantmentReference
BlueprintWeaponTypeReference
ClassesBlueprintClassAdditionalVisualSettingsReference
ClassesBlueprintClassAdditionalVisualSettingsProgressionReference
ConsiderationReference
ConsoleGamePadTextsReference
ConsoleRootReference
CutsceneReference
FormationsRootReference
FxRootReference
GateReference
HitSystemRootReference
ItemsBlueprintItemsListReference
KingdomRootReference
KingdomUIRootReference
KingmakerEquipmentEntityReference
LeadersRootReference
RaceGenderDistributionReference
RandomEncountersRootReference
ResourcesLibraryBlueprintRootReference
RootCutscenesRootReference
RootFxCastsGroupReference
RootPlayerUpgradeActionsRootReference
RootUIConfigsReference
SettlementBuildListReference
TrashLootSettingsReference
UnitCustomizationPresetReference


KingmakerAIBlueprintsBlueprintAiCastSpellReference
KingmakerAIBlueprintsConsiderationsConsiderationCustomReference
KingmakerAIBlueprintsCustomAiConsiderationsRootReference
KingmakerArmiesArmyLeadereferenceReference
KingmakerArmiesBlueprintLeaderProgressionReference
KingmakerArmiesBlueprintLeaderSkillsListReference
KingmakerArmiesBlueprintsBlueprintArmyPresetReference
KingmakerArmiesBlueprintsMoraleRootReference
KingmakerArmiesTacticalCombatBlueprintsBlueprintTacticalCombatObstaclesMapReference
KingmakerArmiesTacticalCombatBlueprintsBlueprintTacticalCombatRootReference
KingmakerArmiesTacticalCombatBrainBlueprintTacticalCombatAiActionReference
KingmakerArmiesTacticalCombatBrainBlueprintTacticalCombatBrainReference
KingmakerCorruptionBlueprintCorruptionRootReference
KingmakerCraftBlueprintIngredientReference
KingmakerCraftRootReference
KingmakerCrusadeGlobalMagicBlueprintGlobalMagicSpellReference
KingmakerElementsSystemActionsReference
KingmakerElementsSystemCommandReference
KingmakerElementsSystemConditionsReference
KingmakerEntitySystemPersistenceVersioningBlueprintPlayerUpgradereferenceReference
KingmakerEntitySystemPersistenceVersioningBlueprintUnitUpgradereferenceReference
KingmakerGlobalmapBlueprintsBlueprintGlobalMapReference
KingmakerGlobalmapBlueprintsBlueprintGlobalMapEdgeReference
KingmakerGlobalmapBlueprintsBlueprintGlobalMapPointReference
KingmakerGlobalmapBlueprintsBlueprintGlobalMapPointVariationReference
KingmakerGlobalmapBlueprintsBlueprintMultiEntranceReference
KingmakerGlobalmapBlueprintsBlueprintMultiEntranceEntryReference
KingmakerInteractionBlueprintInteractionRootReferense
KingmakerKingdomBlueprintsBlueprintArmyPresetListReference
KingmakerKingdomBlueprintsBlueprintCrusadeEventReference
KingmakerKingdomBlueprintsBlueprintCrusadeEventTimelineReference
KingmakerKingdomBlueprintSettlementReference
KingmakerKingdomFlagsBlueprintKingdomMoraleFlagReference
KingmakerQArbiterBlueprintArbiterRootReferenceBlueprintArbiterRootReference
KingmakerandomEncountersettingsBlueprintSpawnableObjectReference
KingmakerTutorialBlueprintTutorialReference
KingmakerUIMVVM_PCViewLoadingScreenSpritesContainerefRef         
         */
    }
}
