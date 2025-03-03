# Changelog

## [1.7.0]
- fixes for SpellSelectionParametrized
- maybe fixed issue with json settings

## [1.6.0]
- fixes for update

## [1.4.2]
- renamed FlameBladeLogic to SummonWeaponLogic

## [1.4.1]
- removed unnecessary generics from CountableFlagArray and PartExtensions

## [1.4.0]
- added new UnityMod project, which can be imported into any Unity project (not just Pathfinder)
- UnityMod is merged with CodexLib via ILRepack; can no longer use ReferenceProject and must use Reference to compiled library
- UnityMod's namespace is 'Shared'
- removed CodexLib.TranspilerData; now available as Shared.TranspilerTool
- enabled XML documentation
- removed CodexLib.CacheData; now available as Shared.CacheData

## [1.3.3]
- added BpCache

## [1.3.2]
- compiled for 2.0.4j
- removed Patch_DuelistParry
- changed Helper.TargetAny
- changed Helper.CreateBlueprintParametrizedFeature

## [1.3.1]
- !signature change Helper.Calls(CodeInstruction, FieldInfo) to Helper.Calls(CodeInstruction, MemberInfo)
- changed logic in CreateAbilityEffectRunAction
- removed redudant Helper.IsStloc
- fixed morphing bug in Helper.Clone
- changed CreateBlueprintItemWeapon

## [1.3.0]
- fixes for 2.0 release

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
