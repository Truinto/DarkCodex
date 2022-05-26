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

        public T To<T>() where T : BlueprintReferenceBase
        {
            var reference = this as T;

            if (reference is null)
                Helper.PrintError($"AnyRef could not be cast to {typeof(T)}");

            return reference;
        }

        public static implicit operator AnyRef(string guid) => new() { deserializedGuid = BlueprintGuid.Parse(guid) };
        public static implicit operator AnyRef(SimpleBlueprint bp) => new() { deserializedGuid = bp.AssetGuid };

        public static implicit operator AnyRef(AnyBlueprintReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator AnyBlueprintReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintAbilityAreaEffectReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintAbilityAreaEffectReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintAbilityReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintAbilityReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintAbilityResourceReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintAbilityResourceReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintActivatableAbilityReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintActivatableAbilityReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintAiActionReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintAiActionReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintArchetypeReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintArchetypeReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintAreaReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintAreaReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintArmorEnchantmentReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintArmorEnchantmentReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintArmorTypeReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintArmorTypeReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintBrainReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintBrainReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintBuffReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintBuffReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintCharacterClassGroupReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintCharacterClassGroupReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintCharacterClassReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintCharacterClassReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintEquipmentEnchantmentReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintEquipmentEnchantmentReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintEtudeReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintEtudeReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintFactionReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintFactionReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintFeatureBaseReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintFeatureBaseReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintFeatureReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintFeatureReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintFeatureSelectionReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintFeatureSelectionReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintFeatureSelectMythicSpellbookReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintFeatureSelectMythicSpellbookReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemArmorReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemArmorReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemEnchantmentReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemEnchantmentReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemEquipmentBeltReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemEquipmentBeltReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemEquipmentFeetReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemEquipmentFeetReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemEquipmentGlassesReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemEquipmentGlassesReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemEquipmentGlovesReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemEquipmentGlovesReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemEquipmentHandReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemEquipmentHandReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemEquipmentHeadReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemEquipmentHeadReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemEquipmentNeckReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemEquipmentNeckReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemEquipmentReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemEquipmentReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemEquipmentRingReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemEquipmentRingReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemEquipmentShirtReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemEquipmentShirtReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemEquipmentShouldersReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemEquipmentShouldersReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemEquipmentUsableReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemEquipmentUsableReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemEquipmentWristReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemEquipmentWristReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintItemWeaponReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintItemWeaponReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintKingdomBuffReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintKingdomBuffReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintLootReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintLootReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintParametrizedFeatureReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintParametrizedFeatureReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintProgressionReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintProgressionReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintProjectileReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintProjectileReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintProjectileTrajectoryReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintProjectileTrajectoryReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintRaceReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintRaceReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintRaceVisualPresetReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintRaceVisualPresetReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintScriptableObjectReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintScriptableObjectReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintSharedVendorTableReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintSharedVendorTableReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintSpellbookReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintSpellbookReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintSpellListReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintSpellListReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintSpellsTableReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintSpellsTableReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintStatProgressionReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintStatProgressionReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintSummonPoolReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintSummonPoolReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintUnitFactReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintUnitFactReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintUnitLootReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintUnitLootReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintUnitPropertyReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintUnitPropertyReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintUnitReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintUnitReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintUnitTemplateReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintUnitTemplateReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintUnitTypeReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintUnitTypeReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintUnlockableFlagReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintUnlockableFlagReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintWeaponEnchantmentReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintWeaponEnchantmentReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(BlueprintWeaponTypeReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator BlueprintWeaponTypeReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(ConsiderationReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator ConsiderationReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };

        public static implicit operator AnyRef(TrashLootSettingsReference bp) => new() { deserializedGuid = bp.deserializedGuid };
        public static implicit operator TrashLootSettingsReference(AnyRef bpref) => new() { deserializedGuid = bpref.deserializedGuid };


        /* all reference types
            (.+)
            public static implicit operator AnyRef\(${1} bp\) => new\(\) { deserializedGuid = bp.deserializedGuid };\npublic static implicit operator ${1}\(AnyRef bpref\) => new\(\) { deserializedGuid = bpref.deserializedGuid };\n
AchievementDataReference
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
