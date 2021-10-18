using DarkCodex.Components;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkCodex
{
    public class General
    {
        [HarmonyPatch(typeof(BlueprintParametrizedFeature), nameof(BlueprintParametrizedFeature.CanSelect))]
        public class DEBUGTEST // todo: bugfix ability focus
        {
            public static bool Prefix(BlueprintParametrizedFeature __instance, ref bool __result, UnitDescriptor unit, LevelUpState state, FeatureSelectionState selectionState, IFeatureSelectionItem item)
            {
                if (__instance.ParameterType != FeatureParameterType.Custom)
                    return true;

                if (item.Param == null)
                    __result = false;
                //else if (__instance.Items.FirstOrDefault(i => i.Feature == item.Feature && i.Param == item.Param) == null)
                //    __result = false;
                else if (unit.GetFeature(__instance, item.Param) != null)
                    __result = false;
                else if (!unit.HasFact(item.Param.Blueprint as BlueprintFact))
                    __result = false;
                else
                    __result = true;

                return false;
            }
        }

        public static void createAbilityFocus()
        {
            Resource.Cache.Ensure();

            var abilities = Resource.Cache.Ability.Where(ability =>
            {
                if (ability.Type == AbilityType.Spell)
                    return false;

                if (ability.m_DisplayName.IsEmpty())
                    return false;

                if (ability.HasVariants)
                    return false;

                var run = ability.GetComponent<AbilityEffectRunAction>();
                if (run == null)
                    return false;

                return run.SavingThrowType != SavingThrowType.Unknown;
            }).ToArray();

            var feat = Helper.CreateBlueprintParametrizedFeature(
                "AbilityFocusCustom",
                "Ability Focus",
                "Choose one special attack. Add +2 to the DC for all saving throws against the special attack on which you focus.",
                blueprints: abilities.ToRef3()
                ).SetComponents(
                new AbilityFocusParametrized()
                );
            feat.Ranks = 10;

            return;

            Helper.AddFeats(feat);
        }

        public static void patchAngelsLight()
        {
            var angelbuff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("e173dc1eedf4e344da226ffbd4d76c60"); // AngelMinorAbilityEffectBuff

            var temphp = angelbuff.GetComponent<TemporaryHitPointsFromAbilityValue>();
            temphp.Value = Helper.CreateContextValue(AbilityRankType.Default);
            angelbuff.AddComponents(Helper.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, type: AbilityRankType.Default)); // see FalseLifeBuff
        }

        public static void patchBasicFreebieFeats()
        {
            //https://michaeliantorno.com/feat-taxes-in-pathfinder/

            var basics = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("5b72dd2ca2cb73b49903806ee8986325").LevelEntries[0].m_Features; //BasicFeatsProgression
            basics.Add(Helper.ToRef<BlueprintFeatureBaseReference>("9972f33f977fc724c838e59641b2fca5")); //PowerAttackFeature
            basics.Add(Helper.ToRef<BlueprintFeatureBaseReference>("0da0c194d6e1d43419eb8d990b28e0ab")); //PointBlankShot
            basics.Add(Helper.ToRef<BlueprintFeatureBaseReference>("4c44724ffa8844f4d9bedb5bb27d144a")); //CombatExpertiseFeature
            basics.Add(Helper.ToRef<BlueprintFeatureBaseReference>("90e54424d682d104ab36436bd527af09")); //WeaponFinesse
            basics.Add(Helper.ToRef<BlueprintFeatureBaseReference>("f47df34d53f8c904f9981a3ee8e84892")); //DeadlyAimFeature

            var mobility = Helper.ToRef<BlueprintUnitFactReference>("2a6091b97ad940943b46262600eaeaeb"); //Mobility
            var dodge = Helper.ToRef<BlueprintUnitFactReference>("97e216dbb46ae3c4faef90cf6bbe6fd5"); //Dodge
            mobility.Get().AddComponents(Helper.MakeAddFactSafe(dodge));
            dodge.Get().AddComponents(Helper.MakeAddFactSafe(mobility));

            var twf = Helper.ToRef<BlueprintUnitFactReference>("ac8aaf29054f5b74eb18f2af950e752d"); //TwoWeaponFighting
            var twfi = Helper.ToRef<BlueprintUnitFactReference>("9af88f3ed8a017b45a6837eab7437629"); //TwoWeaponFightingImproved
            var twfg = Helper.ToRef<BlueprintUnitFactReference>("c126adbdf6ddd8245bda33694cd774e8"); //TwoWeaponFightingGreater
            var multi = Helper.ToRef<BlueprintUnitFactReference>("8ac319e47057e2741b42229210eb43ed"); //Multiattack
            twf.Get().AddComponents(Helper.MakeAddFactSafe(multi));
            twfi.Get().AddComponents(Helper.MakeAddFactSafe(twfg));

            //Deft Maneuvers
            //ImprovedTrip.0f15c6f70d8fb2b49aa6cc24239cc5fa
            //ImprovedDisarm.25bc9c439ac44fd44ac3b1e58890916f
            //ImprovedDirtyTrick.ed699d64870044b43bb5a7fbe3f29494

            //Powerful Maneuvers
            //ImprovedBullRush.b3614622866fe7046b787a548bbd7f59
            //ImprovedSunder.9719015edcbf142409592e2cbaab7fe1
        }

        public static void createPreferredSpell()
        {
            var specialization = ResourcesLibrary.TryGetBlueprint<BlueprintParametrizedFeature>("f327a765a4353d04f872482ef3e48c35"); //SpellSpecializationFirst
            var wizard = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("8c3102c2ff3b69444b139a98521a4899"); //WizardFeatSelection
            var heighten = Helper.ToRef<BlueprintFeatureReference>("2f5d1e705c7967546b72ad8218ccf99c"); //HeightenSpellFeat

            var feat = Helper.CreateBlueprintParametrizedFeature(
                "PreferredSpellFeature",
                "Preferred Spell",
                "Choose one spell which you have the ability to cast. You can cast that spell spontaneously by sacrificing a prepared spell or spell slot of equal or higher level. You can apply any metamagic feats you possess to this spell when you cast it. This increases the minimum level of the prepared spell or spell slot you must sacrifice in order to cast it but does not affect the casting time.\nSpecial: You can gain this feat multiple times.Its effects do not stack. Each time you take the feat, it applies to a different spell.",
                icon: null,
                parameterType: FeatureParameterType.SpellSpecialization,
                blueprints: specialization.BlueprintParameterVariants
                ).SetComponents(
                new PreferredSpell(),
                Helper.CreatePrerequisiteFullStatValue(StatType.SkillKnowledgeArcana, 5),
                Helper.CreatePrerequisiteFeature(heighten)
                );
            feat.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.WizardFeat };
            feat.Ranks = 10;

            Helper.AddFeats(feat);
            Helper.AppendAndReplace(ref wizard.m_AllFeatures, feat.ToRef());
        }

        public static void patchExtendSpells()
        {
            string[] guids = new string[] {
                "2cadf6c6350e4684baa109d067277a45", //ProtectionFromAlignmentCommunal only duration string
                "93f391b0c5a99e04e83bbfbe3bb6db64", //ProtectionFromEvilCommunal
                "5bfd4cce1557d5744914f8f6d85959a4", //ProtectionFromGoodCommunal
                "8b8ccc9763e3cc74bbf5acc9c98557b9", //ProtectionFromLawCommunal
                "0ec75ec95d9e39d47a23610123ba1bad", //ProtectionFromChaosCommunal
            };
        }

        public static void patchHideBuffs()
        {
            string[] guids = new string[] {
                "359e8fc68f81b5d4e96fae22be5e439f", //Artifact_RingOfSummonsBuff
                "4677cfde5b184a94e898425d88a4665a", //MetamagicRodLesserKineticBuff
                "7c4ebf464651bbe4798f25e839cead25", //HatOfHearteningSongEffectBuff
            };

            foreach (var guid in guids)
            {
                try
                {
                    var buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(guid);
                    buff.Flags(hidden: true);
                }
                catch (Exception)
                {
                    Helper.Print(" error: couldn't load " + guid);
                }
            }
        }

        public static void patchVarious()
        {
            // remove penalty on Precious Treat item
            var buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("ee8ee3c5c8f055e48a1ec1bfb92778f1"); //PreciousTreatBuff
            buff.RemoveComponents(default(AddStatBonus));

            // extend protection from X to 10 minutes
            var pfa = new string[] {
                "2cadf6c6350e4684baa109d067277a45", //ProtectionFromAlignmentCommunal
                "8b8ccc9763e3cc74bbf5acc9c98557b9", //ProtectionFromLawCommunal
                "0ec75ec95d9e39d47a23610123ba1bad", //ProtectionFromChaosCommunal
                "93f391b0c5a99e04e83bbfbe3bb6db64", //ProtectionFromEvilCommunal
                "5bfd4cce1557d5744914f8f6d85959a4", //ProtectionFromGoodCommunal
                "3026de673d4d8fe45baf40e0b5edd718", //ProtectionFromChaosEvilCommunal
                "b6da529f710491b4fa789a5838c1ae8f", //ProtectionFromChaosCommunalChaosEvil
                "224f03e74d1dd4648a81242c01e65f41", //ProtectionFromEvilCommunalChaosEvil
            };
            foreach (var guid in pfa)
            {
                try
                {
                    var ab = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(guid);
                    ab.LocalizedDuration = Resource.Strings.TenMinutes;
                    if (ab != null)
                        (ab.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionApplyBuff).DurationValue.BonusValue = 10;
                }
                catch (Exception) { }
            }

            // demon graft respects immunities
            var discordRage = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("5d3029eb16956124da2c6b79ed32c675"); //SongOfDiscordEnragedBuff
            discordRage.AddComponents(Helper.CreateSpellDescriptorComponent(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion));

            // fix BloodlineUndeadArcana and DirgeBardSecretsOfTheGraveFeature not allowing shaken/confusion
            var undeadImmunities = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("8a75eb16bfff86949a4ddcb3dd2f83ae"); //UndeadImmunities
            undeadImmunities.GetComponents<BuffDescriptorImmunity>().First(f => f.IgnoreFeature == null)
                .Descriptor &= ~SpellDescriptor.Shaken & ~SpellDescriptor.Confusion;
            undeadImmunities.GetComponents<SpellImmunityToSpellDescriptor>().First(f => f.CasterIgnoreImmunityFact == null)
                .Descriptor &= ~SpellDescriptor.Shaken & ~SpellDescriptor.Confusion;
        }

        public static void createBardStopSong()
        {

        }
    }

    #region Patches

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

    #endregion
}
