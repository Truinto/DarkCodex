using DarkCodex.Components;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
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
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            return;

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

        public static void patchExtendSpells()
        {
            //2cadf6c6350e4684baa109d067277a45:ProtectionFromAlignmentCommunal only duration string
            //93f391b0c5a99e04e83bbfbe3bb6db64:ProtectionFromEvilCommunal
            //5bfd4cce1557d5744914f8f6d85959a4:ProtectionFromGoodCommunal
            //8b8ccc9763e3cc74bbf5acc9c98557b9:ProtectionFromLawCommunal
            //0ec75ec95d9e39d47a23610123ba1bad:ProtectionFromChaosCommunal
        }

        public static void createGangUp()
        {
            /*
             Gang Up (Combat)
             You are adept at using greater numbers against foes.
             Prerequisites: Int 13, Combat Expertise.
             Benefit: You are considered to be flanking an opponent if at least two of your allies are threatening that opponent, regardless of your actual positioning.
             Normal: You must be positioned opposite an ally to flank an opponent.
            */
        }

        public static void patchHideBuffs()
        {
            //MetamagicRodLesserKineticBuff.4677cfde5b184a94e898425d88a4665a.json
            //ca55b55c4cbac7947b513ea0e76b01d2
        }

        public static void createBardStopSong()
        {

        }
    }
}
