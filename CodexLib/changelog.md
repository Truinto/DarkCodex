# Changelog

## [1.2.0]
- compiled for 1.4
- fixed serialization issue
- added new helpers
- expanded VariantSelectionData; added UINumber

## [1.1.1]
- added harmony and allowGuidGeneration to Scope settings; old constructors will be removed in future releases
- changed AddFactOnlyParty to include minimum character level

## [1.1.0]
- !signature change: CreateAddFeatureIfHasFact, CreateContextConditionHasFact, CreatePrerequisiteFeature, CreateAddInitiatorAttackWithWeaponTrigger
- added limit to exception count
- added GetConflictPatches
- changed Clone<T> will now clear BlueprintComponent.OwnerBlueprint
- updates some helpers to AnyRef
- added patch Patch_ContextRankBonus
- fixed AnyRef naming convention is now consistent
- AnyRef implicitly casts into blueprints

## [1.0.5]
- added PrerequisiteSpontaneousCaster

## [1.0.4]
- small bugfixes

## [1.0.3]
- !signature change: AddFeature
- added CreateRemoveFeatureOnApply
- added Helper AddCondition and RemoveMethods
- changed PartCustomData.Flags to type CountableFlagArray
- added Patch_RuleSpendCharge, ConvertSpellSlots
- renamed Patch_MetamagicFullRound to Patch_AbilityIsFullRound

## [1.0.2]
- !signature change: CreateBlueprintParametrizedFeature
- added EnumCreateWeaponCategory and Patch_WeaponCategory
- added FlagArray
- added PartCustomData
- added MetamagicReduceCostParametrized
- added MasterShapeshifterFix
- added Patch_AbilityAtWill
- added Patch_ActivatableActionBar
- added Patch_AOEAttackRolls
- added Patch_ConditionExemption
- added Patch_DebugReport
- added Patch_FixAbilityTargets
- added Patch_SpellSelectionParametrized
- added Patch_MetamagicFullRound and MechanicFeature.SpontaneousMetamagicNoFullRound

## [1.0.1]
- !signature change: TargetEnemy, TargetAlly, TargetAny
- added AnyRef
- added ContextStatValue
- added AbilityDeliverTeleportTrample
- added AddAttackBonus
- added AddDamageResistancePhysicalImproved
- added Patch_GetTargetProjectileFix
- added Const: common constants
- added Patch_RulebookEventBusPriority and IBeforeRule
