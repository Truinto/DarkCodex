using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Craft;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.GameModes;
using Kingmaker.RuleSystem;
using Kingmaker.UI.MVVM;
using Kingmaker.UI.MVVM._VM.CombatLog;
using Kingmaker.UI.MVVM._VM.InGame;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using CodexLib;
//using FeatureRef = Kingmaker.Blueprints.BlueprintFeatureReference;
//global using FeatureRef = Kingmaker.Blueprints.BlueprintFeatureReference;

namespace DarkCodex
{
    public class Mythic
    {
        [PatchInfo(Severity.Create, "Limitless Bardic Performance / Raging Song", "mythic ability: Bardic Performances cost no resources\nmythic ability: Skald's Raging Song cost no resources", true)]
        public static void CreateLimitlessBardicPerformance()
        {
            var bardic_resource = BlueprintGuid.Parse("e190ba276831b5c4fa28737e5e49e6a6");
            var bardic_prereq = Helper.ToRef<BlueprintFeatureReference>("019ada4530c41274a885dfaa0fbf6218");

            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessBardicPerformance",
                "Limitless Bardic Performance",
                "Your inspiration knows no bounds.\nBenefit: You no longer have a limited amount of Bardic Performance rounds per day.",
                group: FeatureGroup.MythicAbility
                ).SetComponents(
                Helper.CreatePrerequisiteFeature(bardic_prereq)
                );

            SetResourceDecreasing(bardic_resource, limitless);
            Helper.AddMythicTalent(limitless);

            // same again for skald raging song
            var ragesong_resource = BlueprintGuid.Parse("4a2302c4ec2cfb042bba67d825babfec"); //RagingSongResource
            var ragesong_prereq = Helper.ToRef<BlueprintFeatureReference>("334cb75aa673d1d4bb279761c2ef5cf1"); //RagingSong

            var limitless2 = Helper.CreateBlueprintFeature(
                "LimitlessRagingSong",
                "Limitless Raging Song",
                "Your inspire ferocity in your allies.\nBenefit: You no longer have a limited amount of Raging Song rounds per day.",
                group: FeatureGroup.MythicAbility
                ).SetComponents(
                Helper.CreatePrerequisiteFeature(ragesong_prereq)
                );

            SetResourceDecreasing(ragesong_resource, limitless2);
            Helper.AddMythicTalent(limitless2);

            // same again for azata
            var azatasong_resource = BlueprintGuid.Parse("83f8a1c45ed205a4a989b7826f5c0687"); //AzataSongResource
            var azatasong_prereq = Helper.ToRef<BlueprintFeatureReference>("02c96331ed2d87d43a4a3509142678b8"); //AzataPerformanceResourceFeature

            var limitless3 = Helper.CreateBlueprintFeature(
                "LimitlessAzataSong",
                "Limitless Azata Song",
                "Your inspiration knows no bounds.\nBenefit: You no longer have a limited amount of Azata Performance rounds per day.",
                group: FeatureGroup.MythicAbility
                ).SetComponents(
                Helper.CreatePrerequisiteFeature(azatasong_prereq)
                );

            SetResourceDecreasing(azatasong_resource, limitless3);
            Helper.AddMythicTalent(limitless3);
        }

        [PatchInfo(Severity.Create, "Limitless Witch Hexes", "mythic ability: Hexes ignore their cooldown", true)]
        public static void CreateLimitlessWitchHexes()
        {
            Resource.Cache.Ensure();

            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessWitchHexes",
                "Limitless Witch Hexes",
                "Your curse knows no bounds.\nBenefit: You can use your hexes with no time restriction.",
                group: FeatureGroup.MythicAbility
                );

            // cooldown based
            foreach (var ability in Resource.Cache.Ability)
            {
                if (!ability.name.StartsWith("WitchHex") && !ability.name.StartsWith("ShamanHex"))
                    continue;

                var check = ability.GetComponent<AbilityTargetHasFact>();
                if (check == null)
                    continue;

                var checknew = new AbilityTargetHasFactExcept();
                checknew.m_CheckedFacts = check.m_CheckedFacts;
                checknew.Inverted = check.Inverted;
                checknew.PassIfFact = limitless.ToRef2();

                ability.ReplaceComponent(check, checknew);
            }

            // resource based
            Helper.Get<BlueprintActivatableAbility>("298edc3bc21e61044bba25f4e767cb8b").GetComponent<ActivatableAbilityResourceLogic>().m_FreeBlueprint = limitless.ToRef2(); // WitchHexAuraOfPurityActivatableAbility
            Helper.Get<BlueprintAbility>("cedc4959ab311d548881844eecddf57a").GetComponent<AbilityResourceLogic>().ResourceCostDecreasingFacts.Add(limitless.ToRef2()); // WitchHexLifeGiverAbility

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Limitless Smite", "mythic ability: infinite Smites (chaotic and evil), requires Abundant Smite", true)]
        public static void CreateLimitlessSmite()
        {
            var smite_evil = Helper.Get<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec");
            var smite_chaos = Helper.Get<BlueprintAbility>("a4df3ed7ef5aa9148a69e8364ad359c5");
            var mark_of_justice = Helper.Get<BlueprintAbility>("7a4f0c48829952e47bb1fd1e4e9da83a");
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
                ).ToRef<BlueprintUnitFactReference>();

            smite_evil.GetComponent<AbilityResourceLogic>().ResourceCostDecreasingFacts.Add(limitless);
            smite_chaos.GetComponent<AbilityResourceLogic>().ResourceCostDecreasingFacts.Add(limitless);
            mark_of_justice.GetComponent<AbilityResourceLogic>().ResourceCostDecreasingFacts.Add(limitless);
            if (Settings.State.reallyFreeCost)
                mark_of_justice.GetComponent<AbilityResourceLogic>().ResourceCostDecreasingFacts.Add(limitless);

            Helper.AddMythicTalent(limitless.ToRef<BlueprintFeatureReference>());

            // MicroscopicContentExpansion
            if (Helper.Get("09a8551df5e041b4a872bd33f67d70a3") is BlueprintAbility smite_good //AntipaladinSmiteGoodAbility
                && Helper.Get("8939eff25a0a4b77ad1ab6be4c760a6c") is BlueprintCharacterClass antipaladin) //AntipaladinClass
            {
                smite_good.GetComponent<AbilityResourceLogic>()?.ResourceCostDecreasingFacts.Add(limitless);
                limitless.Get().AddComponents(Helper.CreatePrerequisiteClassLevel(antipaladin, 1, true));
            }
            if (Helper.Get("0ee0605a1c9342a0b635956a3755fa35") is BlueprintAbility mark_of_injustice) //AntipaladinAuraofVengeanceAbility
            {
                mark_of_injustice.GetComponent<AbilityResourceLogic>()?.ResourceCostDecreasingFacts.Add(limitless);
                if (Settings.State.reallyFreeCost)
                    mark_of_injustice.GetComponent<AbilityResourceLogic>()?.ResourceCostDecreasingFacts.Add(limitless);
            }
        }

        [PatchInfo(Severity.Create, "Limitless Bombs", "mythic ability: infinite alchemist bombs and incenses", true)]
        public static void CreateLimitlessBombs()
        {
            var bomb_resource = BlueprintGuid.Parse("1633025edc9d53f4691481b48248edd7");
            var incense_resource = BlueprintGuid.Parse("d03d97aac38e798479b81dfa9eda55c6");
            var bomb_prereq = Helper.ToRef<BlueprintFeatureReference>("54c57ce67fa1d9044b1b3edc459e05e2"); //AlchemistBombsFeature
            var incense_prereq = Helper.ToRef<BlueprintFeatureReference>("7614401346b64a8409f7b8c367db488f"); //IncenseFogFeature

            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessBombs",
                "Limitless Alchemist's Creations",
                "You learn how to create a philosopher’s stone that turns everything into chemicals.\nBenefit: You no longer have a limited amount of Bombs or Incenses per day.",
                group: FeatureGroup.MythicAbility,
                icon: Helper.StealIcon("5fa0111ac60ed194db82d3110a9d0352")
                ).SetComponents(
                Helper.CreatePrerequisiteFeature(bomb_prereq, true),
                Helper.CreatePrerequisiteFeature(incense_prereq, true)
                );

            SetResourceDecreasing(bomb_resource, limitless);
            SetResourceDecreasing(incense_resource, limitless);

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Limitless Arcane Pool", "mythic ability: infinite arcane pool, expect spell recall", true)]
        public static void CreateLimitlessArcanePool()
        {
            var arcane_resource = BlueprintGuid.Parse("effc3e386331f864e9e06d19dc218b37");
            var arcane_resourceEldritch = BlueprintGuid.Parse("17b6158d363e4844fa073483eb2655f8");
            var abundant_arcane = Helper.ToRef<BlueprintFeatureReference>("8acebba92ada26043873cae5b92cef7b"); //AbundantArcanePool

            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessArcanePool",
                "Limitless Arcane Pool",
                "Benefit: You can use your arcana without using your arcane pool. You still need to spend arcane points for spell recall.",
                group: FeatureGroup.MythicAbility,
                icon: abundant_arcane.Get().Icon
                ).SetComponents(
                Helper.CreatePrerequisiteFeature(abundant_arcane)
                );

            SetResourceDecreasing(arcane_resource, limitless, true);
            SetResourceDecreasing(arcane_resourceEldritch, limitless, true);

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Limitless Arcane Reservoir", "mythic ability: infinite arcane reservoir", true)]
        public static void CreateLimitlessArcaneReservoir()
        {
            var arcane_resource = BlueprintGuid.Parse("cac948cbbe79b55459459dd6a8fe44ce");
            var arcane_prereq = Helper.ToRef<BlueprintFeatureReference>("55db1859bd72fd04f9bd3fe1f10e4cbb"); //ArcanistArcaneReservoirFeature
            var enforcerer_prereq = Helper.ToRef<BlueprintFeatureReference>("9d1e2212594cf47438fff2fa3477b954"); //ArcaneEnforcerArcaneReservoirFeature

            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessArcaneReservoir",
                "Limitless Arcane Reservoir",
                "Benefit: You can use your arcane exploits without using your arcane reservoir. This does not apply to Magical Supremacy.",
                group: FeatureGroup.MythicAbility,
                icon: Helper.StealIcon("42f96fc8d6c80784194262e51b0a1d25")
                ).SetComponents(
                Helper.CreatePrerequisiteFeature(arcane_prereq, true),
                Helper.CreatePrerequisiteFeature(enforcerer_prereq, true)
                );

            SetResourceDecreasing(arcane_resource, limitless, true);

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Limitless Ki", "mythic ability: reduce ki costs by 1", true)]
        public static void CreateLimitlessKi()
        {
            var ki_resource = BlueprintGuid.Parse("9d9c90a9a1f52d04799294bf91c80a82");
            var scaled_resource = BlueprintGuid.Parse("7d002c1025fbfe2458f1509bf7a89ce1");
            var abundant_ki = Helper.ToRef<BlueprintFeatureReference>("e8752f9126d986748b10d0bdac693264"); //AbundantKiPool

            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessKi",
                "Limitless Ki",
                "Your body and mind are in perfect equilibrium.\nBenefit: Your abilities cost one less ki.",
                group: FeatureGroup.MythicAbility,
                icon: abundant_ki.Get().Icon
                ).SetComponents(
                Helper.CreatePrerequisiteFeature(abundant_ki)
                );

            SetResourceDecreasing(ki_resource, limitless);
            SetResourceDecreasing(scaled_resource, limitless);

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Limitless Domain", "mythic ability: use domain powers at will", true)]
        public static void CreateLimitlessDomain()
        {
            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessDomainPowers",
                "Limitless Domain Powers",
                "You are chosen by your deity.\nBenefit: You can use the abilities of your domains at will.",
                group: FeatureGroup.MythicAbility
                );

            foreach (var ability in Resource.Cache.Ability)
            {
                var logic = ability.GetComponent<AbilityResourceLogic>();
                if (logic == null)
                    continue;
                if (logic.CostIsCustom)
                    continue;
                if (!ability.name.Contains("Domain"))
                    continue;

                logic.ResourceCostDecreasingFacts.Add(limitless.ToRef2());
            }

            foreach (var ability in Resource.Cache.Activatable)
            {
                var logic = ability.GetComponent<ActivatableAbilityResourceLogic>();
                if (logic == null)
                    continue;
                if (!ability.name.Contains("Domain"))
                    continue;

                if (logic.m_FreeBlueprint != null && !logic.m_FreeBlueprint.IsEmpty())
                    Main.Print($"ERROR: {ability.name} has already a FreeBlueprint");
                logic.m_FreeBlueprint = limitless.ToRef2();
            }

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Limitless Shaman", "mythic ability: infinite spirit weapon uses (shaman, spirit hunter)", true, Priority: 200)]
        public static void CreateLimitlessShaman()
        {
            var shaman_resource = BlueprintGuid.Parse("ecf700928d1e3a647a92c095f5de1999"); //ShamanWeaponPoolResourse
            var shaman_prereq = Helper.ToRef<BlueprintFeatureReference>("ef7e19661304e124f95c49637f931429"); //ShamanWeaponPoolFeature

            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessShamanWeapon",
                "Limitless Spirit Weapon",
                "Your soul fused with a spirit.\nBenefit: You no longer have a limited amount of Spirit Weapon uses per day.",
                group: FeatureGroup.MythicAbility,
                icon: Helper.StealIcon("0e5ec4d781678234f83118df41fd27c3")
                ).SetComponents(
                Helper.CreatePrerequisiteFeature(shaman_prereq, true)
                );

            SetResourceDecreasing(shaman_resource, limitless);

            Helper.AddMythicTalent(limitless);

            // TableTopTweaks
            if (Helper.Get("22731cf358814278b18e6ab4e741e988") is BlueprintFeature ttt_feature //WarriorSpiritFeature
                && Helper.Get("55929e3504f84f5095dd60ad0fbb1c29") is BlueprintAbility ttt_ability) //WarriorSpiritToggleAbility
            {
                limitless.AddComponents(Helper.CreatePrerequisiteFeature(ttt_feature.ToRef(), true));
                ttt_ability.GetComponent<AbilityResourceLogic>()?.ResourceCostDecreasingFacts?.Add(limitless.ToRef2());
            }
        }

        [PatchInfo(Severity.Create, "Limitless Warpriest", "mythic ability: infinite scared weapon uses", true)]
        public static void CreateLimitlessWarpriest()
        {
            var weapon_resource = BlueprintGuid.Parse("cc700ef06c6fec449ab085cbcd74709c"); //SacredWeaponEnchantResource
            var armor_resource = BlueprintGuid.Parse("04af200173fafb94381b33e8fe3146e7"); //SacredArmorEnchantResource
            var warpriest_prereq = Helper.ToRef<BlueprintCharacterClassReference>("30b5e47d47a0e37438cc5a80c96cfb99"); //WarpriestClass

            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessWarpriest",
                "Limitless Sacred Warpriest",
                "You are chosen by your deity.\nBenefit: You no longer have a limited amount of Sacred Weapon and Sacred Armor rounds per day.",
                group: FeatureGroup.MythicAbility,
                icon: Helper.StealIcon("0e5ec4d781678234f83118df41fd27c3")
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(warpriest_prereq, 1)
                );

            SetResourceDecreasing(weapon_resource, limitless);
            SetResourceDecreasing(armor_resource, limitless);

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Limitless Warpriest Blessing", "mythic ability: use blessing powers at will", true)]
        public static void CreateLimitlessWarpriestBlessing()
        {
            var blessing_resource = BlueprintGuid.Parse("d128a6332e4ea7c4a9862b9fdb358cca"); //BlessingResource
            var warpriest_prereq = Helper.ToRef<BlueprintCharacterClassReference>("30b5e47d47a0e37438cc5a80c96cfb99"); //WarpriestClass

            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessWarpriestBlessing",
                "Limitless Warpriest Blessings",
                "You are chosen by your deity.\nBenefit: You can use the abilities of your blessings at will.",
                group: FeatureGroup.MythicAbility
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(warpriest_prereq, 1)
                );

            SetResourceDecreasing(blessing_resource, limitless);

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Kinetic Mastery", "mythic feat: physical Kinetic Blasts gain attack bonus equal to mythic level, or half with energy Blasts", true)]
        public static void CreateKineticMastery()
        {
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391");

            var kinetic_mastery = Helper.CreateBlueprintFeature(
                "KineticMastery",
                "Kinetic Mastery",
                "You mastered the elements. Benefit: You add your mythic rank to attack rolls of physical kinetic blasts and half your mythic rank to attack rolls of energy kinetic blasts.",
                group: FeatureGroup.MythicFeat
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 1),
                new KineticMastery()
                );

            Helper.AddMythicFeat(kinetic_mastery);
        }

        [PatchInfo(Severity.Create, "Magic Item Adept", "mythic feat: trinket items use character level as caster level", true, Requirement: typeof(Patch_MagicItemAdept))]
        public static void CreateMagicItemAdept()
        {
            var itemAdept = Helper.CreateBlueprintFeature(
                "MagicItemAdept",
                "Magic Item Adept",
                "You learned how to pour your own magic into magic items. Benefit: When you are using magic trinkets you use your character level inplace of the items caster level. This does not work with potions, scrolls, or wands.",
                group: FeatureGroup.MythicFeat
                );

            Resource.Cache.FeatureMagicItemAdept.SetReference(itemAdept);

            Helper.AddMythicFeat(itemAdept);
        }

        [PatchInfo(Severity.Create, "Extra Mythic Feats", "mythic feat: can pick mythic abilities as feats and vice versa", true)]
        public static void CreateExtraMythicFeats()
        {
            var base_selection1 = Helper.Get<BlueprintFeatureSelection>("9ee0f6745f555484299b0a1563b99d81"); //MythicFeatSelection
            var base_selection2 = Helper.Get<BlueprintFeatureSelection>("ba0e5a900b775be4a99702f1ed08914d"); //MythicAbilitySelection
            var extra_selection1 = Helper.Get<BlueprintFeatureSelection>("8a6a511c55e67d04db328cc49aaad2b8"); //ExtraMythicAbilityMythicFeat

            extra_selection1.Ranks = 10;

            var extra_selection2 = Helper.CreateBlueprintFeatureSelection(
                "ExtraMythicFeatMythicAbility",
                "Extra Mythic Feat",
                "",
                icon: extra_selection1.m_Icon,
                group: FeatureGroup.MythicAbility
                );
            extra_selection2.Ranks = 10;
            extra_selection2.m_AllFeatures = base_selection1.m_AllFeatures;

            Helper.AppendAndReplace(ref base_selection2.m_AllFeatures, extra_selection2.ToRef());
        }

        [PatchInfo(Severity.Create, "Swift Hunters Bond", "mythic ability: ranger's Hunter's Bond can be used as a swift action", true)]
        public static void CreateSwiftHuntersBond()
        {
            var hunterfeat = Helper.ToRef<BlueprintFeatureReference>("6dddf5ba2291f41498df2df7f8fa2b35"); //HuntersBondFeature
            var huntersab = Helper.ToRef<BlueprintAbilityReference>("cd80ea8a7a07a9d4cb1a54e67a9390a5"); //HuntersBondAbility

            var freebooterfeat = Helper.ToRef<BlueprintFeatureReference>("13954cd0f95cb4f4eaa9e9cdabbc2bbb"); //FreebootersBondFeature
            var freebooterab = Helper.ToRef<BlueprintAbilityReference>("f9f94fdcb4a3e314ea19e5398ab81251"); //FreebootersBondAbility

            //huntersab.Get().AvailableMetamagic |= Metamagic.Quicken;

            var feat = Helper.CreateBlueprintFeature(
                "SwiftHuntersBond",
                "Swift Hunters Bond",
                "Benefit: You can use Hunter's Bond as a swift action."
                ).SetComponents(
                Helper.CreateAutoMetamagic(Metamagic.Quicken, new List<BlueprintAbilityReference>() { huntersab, freebooterab }),
                Helper.CreatePrerequisiteFeature(hunterfeat, true),
                Helper.CreatePrerequisiteFeature(freebooterfeat, true)
                );

            Helper.AddMythicTalent(feat);
        }

        [PatchInfo(Severity.Create, "Cursing Gaze", "mythic ability: hexes other than grant can be used as a swift action", true, 295)]
        public static void CreateSwiftHex()
        {
            var witch_selection = Helper.Get<BlueprintFeatureSelection>("9846043cf51251a4897728ed6e24e76f"); //WitchHexSelection
            var shaman_selection = Helper.Get<BlueprintFeatureSelection>("4223fe18c75d4d14787af196a04e14e7"); //ShamanHexSelection
            var trickster_selection = Helper.Get<BlueprintFeatureSelection>("290bbcc3c3bb92144b853fd8fb8ff452"); //SylvanTricksterTalentSelection
            var hexcrafterMagus_selection = Helper.Get<BlueprintFeatureSelection>("ad6b9cecb5286d841a66e23cea3ef7bf"); //HexcrafterHexSelection
            var grant_hex = BlueprintGuid.Parse("d24c2467804ce0e4497d9978bafec1f9"); //WitchGrandHex

            var abilities = new List<BlueprintAbilityReference>();

            foreach (var sel in witch_selection.m_AllFeatures.Concat(shaman_selection.m_AllFeatures))
            {
                var hex = sel.Get();
                if (hex.GetComponent<PrerequisiteFeature>()?.m_Feature?.Guid == grant_hex)
                    continue;

                foreach (var fact in hex.GetComponent<AddFacts>()?.m_Facts ?? Array.Empty<BlueprintUnitFactReference>())
                    if (fact.Get() is BlueprintAbility ab)
                        abilities.Add(ab.ToRef());
            }

            var feat = Helper.CreateBlueprintFeature(
                "MythicSwiftHex",
                "Cursing Gaze",
                "A mere glimpse is enough to curse your enemies. You may use hexes and major hexes, but not grant hexes, as a swift action."
                ).SetComponents(
                Helper.CreateAutoMetamagic(Metamagic.Quicken, abilities),
                Helper.CreatePrerequisiteFeature(witch_selection.ToRef(), true),
                Helper.CreatePrerequisiteFeature(shaman_selection.ToRef(), true),
                Helper.CreatePrerequisiteFeature(trickster_selection.ToRef(), true),
                Helper.CreatePrerequisiteFeature(hexcrafterMagus_selection.ToRef(), true)
                );

            Helper.AddMythicTalent(feat);
        }

        [PatchInfo(Severity.Create | Severity.WIP, "Demon Lord", "adds features of Demon Lords to the mythic Demon progression: teleport at will, ...", true)]
        public static void CreateDemonLord()
        {
            // DC = 10 + ~~HD + Mythic Rank~~ + Mental Stat
            // works only on demons with HD < player HD
            var dp = Helper.Get<BlueprintAbility>("d7cbd2004ce66a042aeab2e95a3c5c61"); //DominatePerson
            var dpbuff = Helper.Get<BlueprintBuff>("c0f4e1c24c9cd334ca988ed1bd9d201f"); //DominatePersonBuff
            var typedemon = Helper.ToRef<BlueprintUnitFactReference>("dc960a234d365cb4f905bdc5937e623a"); //SubtypeDemon
            var ddbuff = dpbuff.Clone("DominateDemonBuff")
                .RemoveComponents<AddFactContextActions>();
            var dd = dp.Clone("DominateDemon")
                .RemoveComponents<SpellListComponent>()
                .RemoveComponents<CraftInfoComponent>()
                .RemoveComponents<AbilityTargetHasNoFactUnless>()
                .ReplaceComponent(default(AbilityTargetHasFact),
                    new AbilityTargetHasFact()
                    {
                        m_CheckedFacts = new BlueprintUnitFactReference[] { typedemon } //SubtypeDemon, DemonOfMagicFeature, DemonOfSlaughterFeature, DemonOfStrengthFeature
                    })
                .ReplaceComponent(default(AbilityEffectRunAction),
                    Helper.CreateAbilityEffectRunAction(SavingThrowType.Will,
                        Helper.CreateConditional(new ContextConditionMoreHitDice(),
                            ifTrue: Helper.CreateContextActionApplyBuff(ddbuff, permanent: true))))
                .AddComponents(
                    new ContextCalculateAbilityParams()
                    {
                        StatTypeFromCustomProperty = true,
                        m_CustomProperty = Resource.Cache.PropertyMaxMentalAttribute,
                        CasterLevel = Helper.CreateContextValue()
                    });

            // todo add dominate demon to progression

            // todo auto revive once per year
            // AngelPhoenixGiftBuff_9988e25ec217c0249a28213e7dc0017c.json
            // AddImmortality

            // teleport at will
            var teleport = Helper.Get<BlueprintAbility>("b3e8e307811b2a24387c2c9226fb4c10");
            teleport.AddComponents(new AbilityAtWill());
            teleport.ActionType = UnitCommand.CommandType.Swift;
            teleport.Range = AbilityRange.DoubleMove;

            // fix wings
            var wingsDemon = Helper.Get<BlueprintBuff>("3c958be25ab34dc448569331488bee27"); //BuffWingsDemon
            wingsDemon.AddComponents(
                Helper.CreateSpellImmunityToSpellDescriptor(SpellDescriptor.Ground),
                Helper.CreateAddFacts(Helper.ToRef<BlueprintUnitFactReference>("c1b26f97b974aec469613f968439e7bb")),   //TripImmune
                Helper.CreateAddMechanicsFeature(AddMechanicsFeature.MechanicsFeatureType.Flying));
        }

        [PatchInfo(Severity.Create, "Demon Mastery", "mythic feat: requires demon lv6; change the rage effect of an demon aspect into a passive effect")]
        public static void CreateDemonMastery()
        {
            var list = new List<(string guid, string id, string displayName, string description)>()
            {
                ("756b77ed5c070b8489922ad66da84df4", "MythicDemonMasteryBabau", "Mastery: Babau", "An aspect of Babau increases the user's sneak {g|Encyclopedia:Attack}attack{/g} {g|Encyclopedia:Damage}damage{/g} by {g|Encyclopedia:Dice}2d6{/g}. Any creature that strikes the Demon with a Babau aspect with a non-{g|Encyclopedia:Reach}reach{/g} melee weapon suffers 2d6 {g|Encyclopedia:Energy_Damage}acid damage{/g}. Both these damages increase by 1d6 at 6th and 9th mythic ranks."),
                ("f154542e0b97908479a578dd7bf6d3f7", "MythicDemonMasteryBrimorak", "Mastery: Brimorak", "Any creature that strikes the Demon with a Brimorak aspect with a non-{g|Encyclopedia:Reach}reach{/g} melee weapon suffers {g|Encyclopedia:Dice}2d6{/g} {g|Encyclopedia:Energy_Damage}fire damage{/g}. This {g|Encyclopedia:Damage}damage{/g} increases by 1d6 at 6th and 9th Demon's rank. Whenever the Demon with an aspect of Brimorak casts a {g|Encyclopedia:Spell}spell{/g} that deals damage, that spell deals +2 point of damage per die rolled. The bonus to damage increases by one at 6th and 9th mythic ranks."),
                ("491e5c7e0acfa1149ad901aecf1fb657", "MythicDemonMasteryIncubus", "Mastery: Incubus", "Whenever the Demon with an aspect of Incubus casts a {g|Encyclopedia:Spell}spell{/g} or uses an ability, increase it's {g|Encyclopedia:DC}DC{/g} by 1. This bonus increases by one at 6th and 9th Demon's rank. The Demon with an aspect of Incubus gains a +4 bonus to weapon {g|Encyclopedia:Damage}damage rolls{/g}. The bonus to damage increases by two at 6th and 9th mythic ranks."),
                ("c9cdd3af3c5f93c4fb8e9119adaa582e", "MythicDemonMasteryKalavakus", "Mastery: Kalavakus", "The Demon with an aspect of Kalavakus gains a +2 bonus to {g|Encyclopedia:CMD}CMD{/g}, {g|Encyclopedia:CMB}CMB{/g}, and natural armor to {g|Encyclopedia:Armor_Class}AC{/g}. The bonus increases by one at 6th and 9th mythic ranks. Whenever the Demon with an aspect of Kalavakus succeeds at a {g|Encyclopedia:Combat_Maneuvers}combat maneuver{/g} {g|Encyclopedia:Check}check{/g}, the Demon can make an {g|Encyclopedia:Attack}attack{/g} against that target as a {g|Encyclopedia:Free_Action}free action{/g}. When the Demon with an aspect of Kalavakus hits with a charge attack, the Demon can attempt to disarm the target as a free {g|Encyclopedia:CA_Types}action{/g}."),
                ("de74410d47d41de4cbae7ea8e341e9af", "MythicDemonMasteryNabasu", "Mastery: Nabasu", "The Demon with an aspect of Nabasu gains a +2 bonus on all {g|Encyclopedia:Saving_Throw}saving throws{/g}. The bonus increases by one at 6th and 9th Demon's rank. All enemies within 30 feet of a Demon with an aspect of Nabasu gain a -1 {g|Encyclopedia:Penalty}penalty{/g} to Fortitude saves and a -1 penalty to {g|Encyclopedia:Attack}attack rolls{/g}. These penalties increase by one at 6th and 9th mythic ranks."),
                ("b0b1dfa396e137749bd628b1f82d1160", "MythicDemonMasterySchir", "Mastery: Schir", "The Demon with an aspect of Schir gains a +2 bonus on {g|Encyclopedia:MeleeAttack}melee attack{/g} {g|Encyclopedia:Dice}rolls{/g}. The bonus increases by one at 6th and 9th mythic ranks. The Demon with an aspect of Schir gains a Powerful Charge ability, increasing the number of weapon {g|Encyclopedia:Damage}damage{/g} dice by two and applying an additional one and half of the Demons {g|Encyclopedia:Strength}strength{/g} bonus to damage from charge."),
                ("51fa8cdc3754c0946b80e81f7952b77f", "MythicDemonMasterySuccubus", "Mastery: Succubus", "All enemies within 30 feet of a Demon with an aspect of Succubus gain a -1 {g|Encyclopedia:Penalty}penalty{/g} to Will {g|Encyclopedia:Saving_Throw}saves{/g} and a -1 penalty to {g|Encyclopedia:Attack}attack rolls{/g}, while all allies within 30 feet gain a +1 bonus to Will saves and attack {g|Encyclopedia:Dice}rolls{/g}, and an immunity to compulsion and fear effects. These penalties and bonuses increase by one at 6th and 9th mythic ranks."),
                ("76eb2cd9b1eec0b4681c648d33c5ae3b", "MythicDemonMasteryVrock", "Mastery: Vrock", "The Demon with an aspect of Vrock gains a +2 bonus on {g|Encyclopedia:RangedAttack}ranged attack{/g} {g|Encyclopedia:Dice}rolls{/g}. The bonus increases by one at 6th and 9th Demon's rank. All {g|Encyclopedia:Spell}spells{/g} and non-physical abilities that the Demon with an aspect of Vrock uses count as {g|Encyclopedia:Reach}Reach{/g} spells. All enemies within 30 feet of a Demon with an aspect of Vrock gain a -1 {g|Encyclopedia:Penalty}penalty{/g} to Reflex {g|Encyclopedia:Saving_Throw}saves{/g} and move as if on difficult terrain. These penalty increases by one at 6th and 9th mythic ranks."),
                
                //("516462cc6f2e4774292fc7922393e297", "MythicDemonMasteryBalor", "Mastery: Balor", "An aspect of Balor makes all allies in a 50-foot radius assume Demonic rage, but without the advantages of the demonic aspects."),
                ("303e34666de545d4d8b604d720da41b4", "MythicDemonMasteryColoxus", "Mastery: Coloxus", "The aspect of Coloxus allows the Demon to use abilities and cast {g|Encyclopedia:Spell}spells{/g} as {g|Encyclopedia:Move_Action}move actions{/g}."),
                ("85b5c055c144ae34c98d3414d51e314e", "MythicDemonMasteryOmox", "Mastery: Omox", "An aspect of Omox causes all {g|Encyclopedia:Attack}attacks{/g} and abilities make their target grappled unless they pass a {g|Encyclopedia:Saving_Throw}Reflex saving throw{/g}."),
                ("8b1e3c9b630d61244a3da3fe5e15cd47", "MythicDemonMasteryShadowDemon", "Mastery: Shadow Demon", "An aspect of Shadow Demon makes the user {g|Encyclopedia:Incorporeal_Touch_Attack}incorporeal{/g}."),
                ("4a032a066bbb4414284a6b49a5653f9d", "MythicDemonMasteryVavakia", "Mastery: Vavakia", "Every round the aspect of Vavakia deals all enemies within 50 feet (1 + 1/3 mythic rank, rounded up) {g|Encyclopedia:Dice}d6{/g} unholy {g|Encyclopedia:Damage}damage{/g}."),
                ("b8e52fe4b85bcb540b273fa8aea26339", "MythicDemonMasteryVrolikai", "Mastery: Vrolikai", "The aspect of Vrolikai makes all demon's {g|Encyclopedia:MeleeAttack}melee attacks{/g} and {g|Encyclopedia:Spell}spells{/g} apply one negative level on enemies who fail a {g|Encyclopedia:Saving_Throw}Will saving throw{/g} ({g|Encyclopedia:DC}DC{/g} = 15 + twice the mythic rank)."),
            };

            var feats = new List<BlueprintFeatureReference>(14);
            for (int i = 0; i < list.Count; i++)
            {
                var (guid, id, displayName, description) = list[i];
                var buff = Helper.Get<BlueprintBuff>(guid);
                feats.Add(Helper.CreateBlueprintFeature(
                    id,
                    displayName,
                    description,
                    icon: buff.m_Icon
                    ).SetComponents(
                    Helper.CreateAddFacts(buff.ToRef2())
                    ).ToRef());
            }

            var selection = Helper.CreateBlueprintFeatureSelection(
                "MythicDemonMastery",
                "Demon Mastery",
                "You perfect a demonic aspect. Select a demon aspect. You can apply its active effect even outside a demon rage.",
                Helper.StealIcon("9ca1d01966a44b7e88ef91cdae230817"),
                FeatureGroup.MythicFeat
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(Helper.ToRef<BlueprintCharacterClassReference>("8e19495ea576a8641964102d177e34b7"), 4));
            selection.m_AllFeatures = feats.ToArray();

            Helper.AddMythicFeat(selection);
        }

        [PatchInfo(Severity.Extend, "Kinetic Overcharge", "Kinetic Overcharge works always, not only while gathering power", true)]
        public static void PatchKineticOvercharge()
        {
            var overcharge = Helper.Get<BlueprintFeature>("4236ca275c6fa9b4a9945b2645262616");
            overcharge.m_Description = Helper.CreateString("You use your mythic powers to fuel kineticist abilities.\nBenefit: Reduce the burn cost of kineticist blasts by one burn point.");
            overcharge.Ranks = 6;
            overcharge.RemoveComponents<AddFacts>();
            overcharge.AddComponents(
                new KineticistReduceBurnPooled() { ReduceBurn = Helper.CreateContextValue(AbilityRankType.Default) },
                Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, feature: overcharge.ToRef())
                );
        }

        [PatchInfo(Severity.Extend, "Limitless Demon Rage", "Limitless Rage also applies to Demon Rage", true)]
        public static void PatchLimitlessDemonRage()
        {
            var rage = Helper.Get<BlueprintActivatableAbility>("0999f99d6157e5c4888f4cfe2d1ce9d6"); //DemonRageAbility
            var rage2 = Helper.Get<BlueprintAbility>("260daa5144194a8ab5117ff568b680f5"); //DemonRageActivateAbility
            var limitless = Helper.ToRef<BlueprintUnitFactReference>("5cb58e6e406525342842a073fb70d068"); //LimitlessRage

            rage.GetComponent<ActivatableAbilityResourceLogic>().m_FreeBlueprint = limitless;
            rage2.GetComponent<AbilityResourceLogic>().ResourceCostDecreasingFacts.Add(limitless);
        }

        [PatchInfo(Severity.Extend, "Unstoppable", "Unstoppable works against more conditions like stun, daze, and confusion", true)]
        public static void PatchUnstoppable()
        {
            var unstoppable = Helper.Get<BlueprintFeature>("74afc3465db56924c9618a42d84efab8"); //Unstoppable
            var dominate = Helper.ToRef<BlueprintBuffReference>("c0f4e1c24c9cd334ca988ed1bd9d201f"); //DominatePersonBuff
            var entangle = Helper.ToRef<BlueprintBuffReference>("f7f6330726121cf4b90a6086b05d2e38"); //EntangleBuff

            unstoppable.GetComponents<BuffSubstitutionOnApply>().FirstOrDefault(f => f.SpellDescriptor.HasAnyFlag(SpellDescriptor.Paralysis))
                .SpellDescriptor |= SpellDescriptor.Stun | SpellDescriptor.Daze | SpellDescriptor.Confusion | SpellDescriptor.Petrified;

            //MindAffecting or Compulsion or DominatePersonBuff
            unstoppable.AddComponents(new BuffSubstitutionOnApply() { m_GainedFact = dominate, m_SubstituteBuff = entangle });
        }

        [PatchInfo(Severity.Extend, "Boundless Healing", "Boundless Healing also grants healing spells to spellbooks", true)]
        public static void PatchBoundlessHealing()
        {
            var feat = Helper.Get<BlueprintFeature>("c8bbb330aaecaf54dbc7570200653f8c"); //BoundlessHealing
            feat.AddComponents(new AddKnownSpellsAnyClass()
            {
                Spells = new BlueprintAbilityReference[] {
                    Helper.ToRef<BlueprintAbilityReference>("5590652e1c2225c4ca30c4a699ab3649"), //1: CureLightWoundsCast
                    Helper.ToRef<BlueprintAbilityReference>("6b90c773a6543dc49b2505858ce33db5"), //2: CureModerateWoundsCast
                    Helper.ToRef<BlueprintAbilityReference>("3361c5df793b4c8448756146a88026ad"), //3: CureSeriousWoundsCast
                    Helper.ToRef<BlueprintAbilityReference>("41c9016596fe1de4faf67425ed691203"), //4: CureCriticalWoundsCast
                    Helper.ToRef<BlueprintAbilityReference>("5d3d689392e4ff740a761ef346815074"), //5: CureLightWoundsMass
                    Helper.ToRef<BlueprintAbilityReference>("571221cc141bc21449ae96b3944652aa"), //6: CureModerateWoundsMass
                    Helper.ToRef<BlueprintAbilityReference>("0cea35de4d553cc439ae80b3a8724397"), //7: CureSeriousWoundsMass
                    Helper.ToRef<BlueprintAbilityReference>("1f173a16120359e41a20fc75bb53d449"), //8: CureCriticalWoundsMass
                    Helper.ToRef<BlueprintAbilityReference>("5da172c4c89f9eb4cbb614f3a67357d3"), //6: HealCast
                    Helper.ToRef<BlueprintAbilityReference>("867524328b54f25488d371214eea0d90"), //9: HealMass
                    Helper.ToRef<BlueprintAbilityReference>("d5847cad0b0e54c4d82d6c59a3cda6b0"), //5: BreathOfLifeCast
                    Helper.ToRef<BlueprintAbilityReference>("e84fc922ccf952943b5240293669b171"), //2: RestorationLesser
                    Helper.ToRef<BlueprintAbilityReference>("f2115ac1148256b4ba20788f7e966830"), //4: Restoration
                    Helper.ToRef<BlueprintAbilityReference>("fafd77c6bfa85c04ba31fdc1c962c914"), //7: RestorationGreater
                },
                Levels = new int[] {
                    1, //1: CureLightWoundsCast
                    2,
                    3,
                    4,
                    5,
                    6,
                    7,
                    8,
                    6, //6: HealCast
                    9, //9: HealMass
                    5, //5: BreathOfLifeCast
                    2, //2: RestorationLesser
                    4,
                    7,
                }
            });

            feat.m_Description.CreateString(feat.m_Description + "\nGain cure wound bonus spells to your spellbooks.");
        }

        [PatchInfo(Severity.Extend, "Boundless Injury", "mythic ability: like Boundless Injury but for harm spells", true)]
        public static void PatchBoundlessInjury()
        {
            var addKnownSpells = new AddKnownSpellsAnyClass()
            {
                Spells = new BlueprintAbilityReference[] {
                    Helper.ToRef<BlueprintAbilityReference>("e5af3674bb241f14b9a9f6b0c7dc3d27"), //1: InflictLightWoundsCast
                    Helper.ToRef<BlueprintAbilityReference>("65f0b63c45ea82a4f8b8325768a3832d"), //2: InflictModerateWoundsCast
                    Helper.ToRef<BlueprintAbilityReference>("bd5da98859cf2b3418f6d68ea66cabbe"), //3: InflictSeriousWoundsCast
                    Helper.ToRef<BlueprintAbilityReference>("651110ed4f117a948b41c05c5c7624c0"), //4: InflictCriticalWoundsCast
                    Helper.ToRef<BlueprintAbilityReference>("9da37873d79ef0a468f969e4e5116ad2"), //5: InflictLightWoundsMass
                    Helper.ToRef<BlueprintAbilityReference>("03944622fbe04824684ec29ff2cec6a7"), //6: InflictModerateWoundsMass
                    Helper.ToRef<BlueprintAbilityReference>("820170444d4d2a14abc480fcbdb49535"), //7: InflictSeriousWoundsMass
                    Helper.ToRef<BlueprintAbilityReference>("5ee395a2423808c4baf342a4f8395b19"), //8: InflictCriticalWoundsMass
                    Helper.ToRef<BlueprintAbilityReference>("cc09224ecc9af79449816c45bc5be218"), //6: HarmCast
                    //Helper.ToRef<BlueprintAbilityReference>(""), //9: HarmMass
                },
                Levels = new int[] {
                    1, //1: InflictLightWoundsCast
                    2,
                    3,
                    4,
                    5,
                    6,
                    7,
                    8,
                    6, //6: HarmCast
                    //9, //9: HarmMass
                }
            };

            //var feat = Helper.Get<BlueprintFeature>("c8bbb330aaecaf54dbc7570200653f8c"); //BoundlessHealing
            //feat.AddComponents(addKnownSpells);
            //feat.GetComponent<AutoMetamagic>().Abilities.AddRange(addKnownSpells.Spells);
            //Helper.AppendAndReplace(ref feat.GetComponent<AddUnlimitedSpell>().m_Abilities, addKnownSpells.Spells.StickyResolve());
            //return;

            var feat2 = Helper.CreateBlueprintFeature(
                "BoundlessInjury",
                "Boundless Injury",
                "Your negative energy is no longer affected by common limitations.\nBenefit: Whenever you cast an inflict wounds {g|Encyclopedia:Spell}spell{/g} or another harming spell, it becomes {g|Encyclopedia:Reach}reach{/g} as though using the Reach Spell {g|Encyclopedia:Feat}feat{/g}. Also, the amount of {g|Encyclopedia:HP}hit points{/g} it now restores depends entirely on your {g|Encyclopedia:Caster_Level}caster level{/g} and disregards any limits in the original spell.\nReach Spell: You can alter a spell with a range of {g|Encyclopedia:TouchAttack}touch{/g}, close, or medium to increase its range by one range category, using the following order: touch, close, medium, and long.\nGain inflict wound bonus spells to your spellbooks."
                ).SetComponents(
                addKnownSpells,
                Helper.CreateAutoMetamagic(Metamagic.Reach, addKnownSpells.Spells.ToList(), AutoMetamagic.AllowedType.SpellOnly),
                new AddUnlimitedSpell { m_Abilities = addKnownSpells.Spells.StickyResolve() }
                );

            Helper.AddMythicTalent(feat2);
        }

        [PatchInfo(Severity.Create, "Resourceful Caster", "mythic ability: regain spells that fail because of spell failure, concentration, SR, saving throws", true, Requirement: typeof(Patch_ResourcefulCaster))]
        public static void CreateResourcefulCaster()
        {
            var feature = Helper.CreateBlueprintFeature(
                "ResourcefulCasterFeature",
                "Resourceful Caster",
                "You can repurpose magic energy of failed spells. You don't expend a spell slot, if you fail to cast a spell due to arcane spell failure or concentration checks. Furthermore you regain your spell slot, whenever all targets of your spells resist due to spell resistance or succeed on their saving throws."
                );

            Resource.Cache.FeatureResourcefulCaster.SetReference(feature);

            Helper.AddMythicTalent(Resource.Cache.FeatureResourcefulCaster);
        }

        [PatchInfo(Severity.Create, "Mythic Metamagic Adept", "mythic feat: allow spontaneous spellcasters to apply metamagic without casting time penalty", true)]
        public static void CreateMetamagicAdept()
        {
            var feat = Helper.CreateBlueprintFeature(
                "MythicMetamagicAdept",
                "Mythic Metamagic Adept",
                "Whenever you apply metamagic feats to spells of spontaneous classes, you can do so without increasing the casting time."
                ).SetComponents(
                new PrerequisiteSpontaneousCaster(),
                new AddMechanicFeatureCustom(MechanicFeature.SpontaneousMetamagicNoFullRound)
                );

            Helper.AddMythicFeat(feat);
        }

        [PatchInfo(Severity.Create, "Mythic Eschew Materials", "mythic ability: you cast spells without expending material components", true)]
        public static void CreateMythicEschewMaterials()
        {
            var feat = Helper.CreateBlueprintFeature(
                "MythicEschewMaterials",
                "Mythic Eschew Materials",
                "You can cast many spells without needing to utilize material components.\nBenefit: You can cast any spell without needing material components. The casting of the spell still provokes attacks of opportunity as normal."
                ).SetComponents(
                new AddMechanicFeatureCustom(MechanicFeature.NoMaterialComponent)
                );

            Helper.AddMythicTalent(feat);
        }

        [PatchInfo(Severity.Create, "Mythic Animal Companion", "mythic feat: unlocks companion equipment slots", true)]
        public static void CreateMythicCompanion()
        {
            var feat = Helper.CreateBlueprintFeature(
                "MythicAnimalCompanionInventory",
                "Mythic Animal Companion",
                "Your animal companion can don any type of equipment."
                ).SetComponents(
                new RemoveFeatureOnApplyToPet("75bb2b3c41c99e041b4743fdb16a4289") //AnimalCompanionSlotFeature
                );

            Helper.AddMythicFeat(feat);
        }

        [PatchInfo(Severity.Create, "Not A Chance", "mythic ability: immunity to crits", true)]
        public static void CreateNotAChance()
        {
            Main.Patch(typeof(Patch_NotAChance));

            var feat = Helper.CreateBlueprintFeature(
                "NotAChanceFeat",
                "Not A Chance",
                "You bring misfortune to those that oppose you. Benefit: Attack rolls against you don't automatically succeed on a natural 20. You become immune to critical hits."
                ).SetComponents(
                new AddMechanicFeatureCustom(MechanicFeature.NotAChance),
                Helper.CreateAddFacts("ced0f4e5d02d5914a9f9ff74acacf26d") //ImmunityToCritical
                );

            Helper.AddMythicTalent(feat);
        }

        [PatchInfo(Severity.Extend, "Ranging Shots", "doesn't get weaker when hitting", true)]
        public static void PatchRangingShots()
        {
            var feat = Helper.Get<BlueprintFeature>("2b558167862b05d47b8472d176257f82"); //RangingShots
            var buff = Helper.Get<BlueprintBuff>("e9da1dc30535fc94ab458ee20f8d3c4e"); //RangingShotsBuff

            // keep attack bonus while hitting
            feat.RemoveComponents(r => r is AddInitiatorAttackWithWeaponTrigger trigger && trigger.OnlyHit);
            // feat.AddComponents(Helper.CreateCombatStateTrigger(Helper.CreateContextActionRemoveBuff(buff))); already removes after combat
            feat.m_Description = Helper.CreateString("Every time you miss an enemy with a ranged weapon attack, your aim improves, giving you a stacking +1 bonus on attacks up to the maximum of your mythic rank for the remainder of the combat.");
        }

        [PatchInfo(Severity.Extend, "Wandering Hex", "can swap hex at will", true)]
        public static void PatchWanderingHex()
        {
            var feat = Helper.Get<BlueprintAbility>("b209beab784d93546b40a2fa2a09ffa8"); //WitchWanderingHexAbility
            feat.RemoveComponents<AbilityResourceLogic>();
        }

        [PatchInfo(Severity.Extend, "Judgement Aura", "Everlasting Judgement also applies to Judgement Aura", true)]
        public static void PatchJudgementAura()
        {
            var ability = Helper.Get<BlueprintAbility>("faa50b5d67eb4745b7b1157629d5c304"); //JudgementAuraAbility
            var fact = Helper.ToRef<BlueprintUnitFactReference>("4a6dc772c9a7fe742a65820007107f03"); //EverlastingJudgement

            ability.GetComponent<AbilityResourceLogic>().ResourceCostDecreasingFacts.Add(fact);
        }

        [PatchInfo(Severity.Extend, "Ascendant Summons", "buffed Ascendant Summons by +4 stats and DR 10", true)]
        public static void PatchAscendantSummons()
        {
            var feat = Helper.Get<BlueprintFeature>("fcdd46e2ead3d904cb9311c56112ce5f");
            var buff = Helper.Get<BlueprintBuff>("3db4a1f9ffa46e7469f817bced1a0df2");

            string text = "The creatures you summon gain a piece of your mythic power.\nBenefit: Creatures summoned by you gain a {g|Encyclopedia:Bonus}bonus{/g} to all its ability scores equal to half your mythic rank plus 5. Their {g|Encyclopedia:Attack}attacks{/g} now ignore {g|Encyclopedia:Damage_Reduction}damage reduction{/g} except N/-. Their {g|Encyclopedia:Damage_Reduction}damage reduction{/g} becomes N/- and it increases to at least 10/-.";
            feat.m_Description = text.CreateString();

            var rank = buff.GetComponent<ContextRankConfig>();
            rank.m_Progression = ContextRankProgression.Div2PlusStep;
            rank.m_StepLevel = 5;

            buff.AddComponents(
                Helper.CreateAddContextStatBonus(StatType.Intelligence),
                new AddDamageResistancePhysicalImproved() { Value = 10 }
                );
        }

        [PatchInfo(Severity.Harmony, "Always A Chance", "'Always A Chance' succeeds on a natural one and applies to most d20 rolls", true)]
        public static void PatchAlwaysAChance()
        {
            var bp = Helper.Get<BlueprintFeature>("d57301613ad6a5140b2fdac40fa368e3"); //AlwaysAChance
            if (bp != null)
                bp.m_Description = Helper.CreateString("You automatically succeed when you roll a natural 1.");

            Main.Patch(typeof(Patch_AlwaysAChance));
        }

        [PatchInfo(Severity.Extend | Severity.DefaultOff, "Elemental Barrage", "reverse patch, trigger of weapon attacks again", true)]
        public static void PatchElementalBarrage()
        {
            // allow Elemental Barrage on any damage trigger
            foreach (var comp in Helper.Get<BlueprintFeature>("da56a1b21032a374783fdf46e1a92adb").ComponentsArray) //ElementalBarrage
            {
                if (comp is AddOutgoingDamageTrigger trigger)
                    trigger.CheckAbilityType = false;
            }
        }

        [PatchInfo(Severity.Extend, "Various Tweaks", "allow quicken on Demon Teleport, allow Elemental Barrage on any damage, Elemental Rampage works with Limitless Rage", true)]
        public static void PatchVarious()
        {
            // allow quicken metamagic on demon teleport
            Helper.Get<BlueprintAbility>("b3e8e307811b2a24387c2c9226fb4c10") //DemonTeleport
                .AvailableMetamagic |= Metamagic.Quicken;

            // elemental rampage works with limitless rage
            Helper.AppendAndReplace(
                ref Helper.Get<BlueprintFeature>("5cb58e6e406525342842a073fb70d068").GetComponent<PrerequisiteFeaturesFromList>().m_Features, //LimitlessRage
                Helper.ToRef<BlueprintFeatureReference>("64c5dfe0ba664dd38b7e914ef0912a1c")); //ElementalRampagerRampageFeature
            Helper.Get<BlueprintActivatableAbility>("5814af7966274c2f9f0456c5f4ab271c").GetComponent<ActivatableAbilityResourceLogic>().m_FreeBlueprint
                = Helper.ToRef<BlueprintUnitFactReference>("5cb58e6e406525342842a073fb70d068"); //LimitlessRage
        }

        #region Helper

        public static void SetResourceDecreasing(BlueprintGuid resource, AnyRef limitless, bool repeat = false)
        {
            Resource.Cache.Ensure();
            repeat |= Settings.State.reallyFreeCost;

            foreach (var ability in Resource.Cache.Ability)
            {
                var logic = ability.GetComponent<AbilityResourceLogic>();
                if (logic == null || logic.m_RequiredResource?.deserializedGuid != resource)
                    continue;
                if (logic.CostIsCustom)
                    continue;

                int total = !repeat ? 1 : logic.Amount + logic.ResourceCostIncreasingFacts.Count;
                for (int i = 0; i < total; i++)
                    logic.ResourceCostDecreasingFacts.Add(limitless);
            }

            foreach (var ability in Resource.Cache.Activatable)
            {
                var logic = ability.GetComponent<ActivatableAbilityResourceLogic>();
                if (logic == null || logic.m_RequiredResource?.deserializedGuid != resource)
                    continue;

                if (logic.m_FreeBlueprint != null && !logic.m_FreeBlueprint.IsEmpty())
                    Main.Print($"ERROR: {ability.name} has already a FreeBlueprint");
                logic.m_FreeBlueprint = limitless;
            }
        }

        #endregion
    }
}
