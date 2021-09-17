using DarkCodex.Components;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class Mythic
    {
        public static void createLimitlessBardicPerformance()
        {
            Resource.Cache.Ensure();
            var bardic = Helper.ToRef<BlueprintAbilityResourceReference>("e190ba276831b5c4fa28737e5e49e6a6");

            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessBardicPerformance",
                "Limitless Bardic Performance",
                "Your inspiration knows no bounds.\nBenefit: You no longer have a limited amount of Bardic Performance rounds per day.",
                group: FeatureGroup.MythicAbility
                ); // todo: icon?

            foreach (var ability in Resource.Cache.Ability)
            {
                var logic = ability.GetComponent<AbilityResourceLogic>();

                if (logic == null || logic.m_RequiredResource?.deserializedGuid != bardic.deserializedGuid)
                    continue;

                Helper.PrintDebug("Patching Mythic Barde " + ability.name);
                logic.ResourceCostDecreasingFacts.Add(limitless.ToRef2());
            }

            foreach (var ability in Resource.Cache.Activatable)
            {
                var logic = ability.GetComponent<ActivatableAbilityResourceLogic>();
                if (logic == null || logic.m_RequiredResource?.deserializedGuid != bardic.deserializedGuid)
                    continue;

                Helper.PrintDebug("Patching Mythic Barde " + ability.name);
                if (logic.m_FreeBlueprint != null && !logic.m_FreeBlueprint.IsEmpty())
                    Helper.Print($"ERROR: {ability.name} has already a FreeBlueprint");
                logic.m_FreeBlueprint = limitless.ToRef2();
            }

            Helper.AddMythicTalent(limitless);
        }

        public static void createLimitlessWitchHexes()
        {
            Resource.Cache.Ensure();

            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessWitchHexes",
                "Limitless Witch Hexes",
                "Your curse knows no bounds.\nBenefit: You can use your hexes with no time restriction.",
                group: FeatureGroup.MythicAbility
                ); // todo: icon?

            // cooldown based
            foreach (var ability in Resource.Cache.Ability)
            {
                if (!ability.name.StartsWith("WitchHex") && !ability.name.StartsWith("ShamanHex"))
                    continue;

                var check = ability.GetComponent<AbilityTargetHasFact>();
                if (check == null)
                    continue;

                Helper.PrintDebug("Patching Mythic Witch " + ability.name);
                var checknew = new AbilityTargetHasFactExcept();
                checknew.m_CheckedFacts = check.m_CheckedFacts;
                checknew.Inverted = check.Inverted;
                checknew.PassIfFact = limitless.ToRef2();

                ability.ReplaceComponent(check, new AbilityTargetHasFactExcept());
            }

            // resource based
            ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>("298edc3bc21e61044bba25f4e767cb8b").GetComponent<ActivatableAbilityResourceLogic>().m_FreeBlueprint = limitless.ToRef2(); // WitchHexAuraOfPurityActivatableAbility
            ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("cedc4959ab311d548881844eecddf57a").GetComponent<AbilityResourceLogic>().ResourceCostDecreasingFacts.Add(limitless.ToRef2()); // WitchHexLifeGiverAbility

            Helper.AddMythicTalent(limitless);
        }
    
        public static void createLimitlessSmite()
        {
            var smite_evil = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec");
            var smite_chaos = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("a4df3ed7ef5aa9148a69e8364ad359c5");
            var abundant_smite = Helper.ToRef<BlueprintFeatureReference>("7e5b63faeca24474db0bfd019167dda4");
            var abundant_smitechaos = Helper.ToRef<BlueprintFeatureReference>("4cdc155e26204491ba4d193646cb4443");

            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessSmite",
                "Limitless Smite",
                "Benefit: You no longer have a limited amount of Smite per day.",
                icon: smite_evil.Icon,
                group: FeatureGroup.MythicAbility
                ).SetComponents(
                Helper.CreatePrerequisiteFeature(abundant_smite, true),
                Helper.CreatePrerequisiteFeature(abundant_smitechaos, true)
                );

            smite_evil.GetComponent<AbilityResourceLogic>().ResourceCostDecreasingFacts.Add(limitless.ToRef2());
            smite_chaos.GetComponent<AbilityResourceLogic>().ResourceCostDecreasingFacts.Add(limitless.ToRef2());

            Helper.AddMythicTalent(limitless);
        }

        public static void createKineticMastery()
        {
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391");

            var kinetic_mastery = Helper.CreateBlueprintFeature(
                "KineticMastery",
                "Kinetic Mastery",
                "You mastered the elements. Benefit: You add your mythic rank to physical kinetic blasts and half your mythic rank to energy kinetic blasts.",
                group: FeatureGroup.MythicFeat
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 1),
                new KineticMastery()
                );

            Helper.AddMythicFeat(kinetic_mastery);
        }
    }
}
