using DarkCodex.Components;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
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
        public static void createAbilityFocus()
        {
            Resource.Cache.Ensure();

            var abilities = Resource.Cache.Ability.Where(ability =>
            {
                if (ability.Type == AbilityType.Spell)
                    return false;

                if (ability.m_Parent != null || !ability.m_Parent.IsEmpty())
                    return false;

                if (ability.m_DisplayName.IsEmpty())
                    return false;

                var run = ability.GetComponent<AbilityEffectRunAction>();
                if (run == null)
                    return false;

                return run.SavingThrowType != SavingThrowType.Unknown;
            }).ToArray();

            var feat = Helper.CreateBlueprintParametrizedFeature(
                "AbilityFocus",
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

            var twfi = Helper.ToRef<BlueprintUnitFactReference>("9af88f3ed8a017b45a6837eab7437629"); //TwoWeaponFightingImproved
            var twfg = Helper.ToRef<BlueprintUnitFactReference>("c126adbdf6ddd8245bda33694cd774e8"); //TwoWeaponFightingGreater
            twfi.Get().AddComponents(Helper.MakeAddFactSafe(twfg));

            //Deft Maneuvers
            //ImprovedTrip.0f15c6f70d8fb2b49aa6cc24239cc5fa
            //ImprovedDisarm.25bc9c439ac44fd44ac3b1e58890916f
            //ImprovedDirtyTrick.ed699d64870044b43bb5a7fbe3f29494

            //Powerful Maneuvers
            //ImprovedBullRush.b3614622866fe7046b787a548bbd7f59
            //ImprovedSunder.9719015edcbf142409592e2cbaab7fe1
        }

        public static void createQuickDraw()
        {
            var quickdraw = Helper.CreateBlueprintFeature(
                "QuickDraw",
                "Quick Draw",
                "You can draw a weapon as a free action instead of as a move action. A character who has selected this feat may throw weapons at his full normal rate of attacks (much like a character with a bow)."
                );

            // reduce weapon change time

            // apply two weapon fighting to throwing weapons like javelin, throwing axe
            // see TwoWeaponFightingImproved and TwoWeaponFightingBasicMechanics
        }
    }
}
