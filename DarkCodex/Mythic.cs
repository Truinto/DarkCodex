using DarkCodex.Components;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers;
using Kingmaker.Craft;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.MVVM;
using Kingmaker.UI.MVVM._VM.CombatLog;
using Kingmaker.UI.MVVM._VM.InGame;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class Mythic
    {
        [PatchInfo(Severity.Create, "Limitless Bardic Performance / Raging Song", "mythic ability: Bardic Performances cost no resources\nmythic ability: Skald's Raging Song cost no resources", true)]
        public static void createLimitlessBardicPerformance()
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
                ); // todo: icon?

            setResourceDecreasing(bardic_resource, limitless.ToRef2());
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

            setResourceDecreasing(ragesong_resource, limitless2.ToRef2());
            Helper.AddMythicTalent(limitless2);
        }

        [PatchInfo(Severity.Create, "Limitless Witch Hexes", "mythic ability: Hexes ignore their cooldown", true)]
        public static void createLimitlessWitchHexes()
        {
            Resource.Cache.Ensure();

            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessWitchHexes",
                "Limitless Witch Hexes",
                "Your curse knows no bounds.\nBenefit: You can use your hexes with no time restriction.",
                group: FeatureGroup.MythicAbility
                ); // todo: icon?

            Helper.PrintJoinDebug(" -remove cooldown ", null);

            // cooldown based
            foreach (var ability in Resource.Cache.Ability)
            {
                if (!ability.name.StartsWith("WitchHex") && !ability.name.StartsWith("ShamanHex"))
                    continue;

                var check = ability.GetComponent<AbilityTargetHasFact>();
                if (check == null)
                    continue;

                Helper.PrintJoinDebug(ability.name);
                var checknew = new AbilityTargetHasFactExcept();
                checknew.m_CheckedFacts = check.m_CheckedFacts;
                checknew.Inverted = check.Inverted;
                checknew.PassIfFact = limitless.ToRef2();

                ability.ReplaceComponent(check, checknew);
            }

            Helper.PrintJoinDebug(flush: true);

            // resource based
            ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>("298edc3bc21e61044bba25f4e767cb8b").GetComponent<ActivatableAbilityResourceLogic>().m_FreeBlueprint = limitless.ToRef2(); // WitchHexAuraOfPurityActivatableAbility
            ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("cedc4959ab311d548881844eecddf57a").GetComponent<AbilityResourceLogic>().ResourceCostDecreasingFacts.Add(limitless.ToRef2()); // WitchHexLifeGiverAbility

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Limitless Smite", "mythic ability: infinite Smites (chaotic and evil), requires Abundant Smite", true)]
        public static void createLimitlessSmite()
        {
            var smite_evil = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec");
            var smite_chaos = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("a4df3ed7ef5aa9148a69e8364ad359c5");
            var mark_of_justice = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("7a4f0c48829952e47bb1fd1e4e9da83a");
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
            mark_of_justice.GetComponent<AbilityResourceLogic>().ResourceCostDecreasingFacts.Add(limitless.ToRef2());

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Limitless Bombs", "mythic ability: infinite alchemist bombs and incenses", true)]
        public static void createLimitlessBombs()
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

            setResourceDecreasing(bomb_resource, limitless.ToRef2());
            setResourceDecreasing(incense_resource, limitless.ToRef2());

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Limitless Arcane Pool", "mythic ability: infinite arcane pool, expect spell recall", true)]
        public static void createLimitlessArcanePool()
        {
            var arcane_resource = BlueprintGuid.Parse("effc3e386331f864e9e06d19dc218b37");
            var eldritch_resource = BlueprintGuid.Parse("effc3e386331f864e9e06d19dc218b37");
            var arcane_prereq = Helper.ToRef<BlueprintFeatureReference>("3ce9bb90749c21249adc639031d5eed1"); //ArcanePoolFeature
            var eldritch_prereq = Helper.ToRef<BlueprintFeatureReference>("95e04a9e86aa9e64dad7122625b79c62"); //EldritchPoolFeature
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

            setResourceDecreasing(arcane_resource, limitless.ToRef2(), true);

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Limitless Arcane Reservoir", "mythic ability: infinite arcane reservoir", true)]
        public static void createLimitlessArcaneReservoir()
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

            setResourceDecreasing(arcane_resource, limitless.ToRef2(), true);

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Limitless Ki", "mythic ability: reduce ki costs by 1", true)]
        public static void createLimitlessKi()
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

            setResourceDecreasing(ki_resource, limitless.ToRef2());
            setResourceDecreasing(scaled_resource, limitless.ToRef2());

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Limitless Domain", "mythic ability: use domain powers at will", true)]
        public static void createLimitlessDomain()
        {
            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessDomainPowers",
                "Limitless Domain Powers",
                "You are chosen by your deity.\nBenefit: You can use the abilities of your domains at will.",
                group: FeatureGroup.MythicAbility
                );

            Helper.PrintJoinDebug(" -free ability ", null);

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
                Helper.PrintJoinDebug(ability.name);
            }

            Helper.PrintJoinDebug(" -free activatable ", null, true);

            foreach (var ability in Resource.Cache.Activatable)
            {
                var logic = ability.GetComponent<ActivatableAbilityResourceLogic>();
                if (logic == null)
                    continue;
                if (!ability.name.Contains("Domain"))
                    continue;

                if (logic.m_FreeBlueprint != null && !logic.m_FreeBlueprint.IsEmpty())
                    Helper.Print($"ERROR: {ability.name} has already a FreeBlueprint");
                logic.m_FreeBlueprint = limitless.ToRef2();
                Helper.PrintJoinDebug(ability.name);
            }

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Limitless Shaman", "mythic ability: infinite spirit weapon uses (shaman, spirit hunter)", true, Priority: 200)]
        public static void createLimitlessShaman()
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

            setResourceDecreasing(shaman_resource, limitless.ToRef2());

            Helper.AddMythicTalent(limitless);

            // TableTopTweaks
            var ttt_feature = Helper.Get<BlueprintFeature>("22731cf358814278b18e6ab4e741e988"); //WarriorSpiritFeature
            var ttt_ability = Helper.Get<BlueprintAbility>("55929e3504f84f5095dd60ad0fbb1c29"); //WarriorSpiritToggleAbility
            if (ttt_feature != null && ttt_ability != null)
            {
                limitless.AddComponents(Helper.CreatePrerequisiteFeature(ttt_feature.ToRef(), true));
                ttt_ability.GetComponent<AbilityResourceLogic>()?.ResourceCostDecreasingFacts?.Add(limitless.ToRef2());
            }
        }

        [PatchInfo(Severity.Create, "Limitless Warpriest", "mythic ability: infinite scared weapon uses", true)]
        public static void createLimitlessWarpriest()
        {
            var weapon_resource = BlueprintGuid.Parse("cc700ef06c6fec449ab085cbcd74709c"); //SacredWeaponEnchantResource
            var armor_resource = BlueprintGuid.Parse("04af200173fafb94381b33e8fe3146e7"); //SacredArmorEnchantResource
            var warpriest_prereq = Helper.ToRef<BlueprintCharacterClassReference>("30b5e47d47a0e37438cc5a80c96cfb99"); //WarpriestClass.

            var limitless = Helper.CreateBlueprintFeature(
                "LimitlessWarpriest",
                "Limitless Sacred Warpriest",
                "You are chosen by your deity.\nBenefit: You no longer have a limited amount of Sacred Weapon and Sacred Armor rounds per day.",
                group: FeatureGroup.MythicAbility,
                icon: Helper.StealIcon("0e5ec4d781678234f83118df41fd27c3")
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(warpriest_prereq, 1)
                );

            setResourceDecreasing(weapon_resource, limitless.ToRef2());
            setResourceDecreasing(armor_resource, limitless.ToRef2());

            Helper.AddMythicTalent(limitless);
        }

        [PatchInfo(Severity.Create, "Kinetic Mastery", "mythic feat: physical Kinetic Blasts gain attack bonus equal to mythic level, or half with energy Blasts", true)]
        public static void createKineticMastery()
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
        public static void createMagicItemAdept()
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
        public static void createExtraMythicFeats()
        {
            var base_selection1 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("9ee0f6745f555484299b0a1563b99d81"); //MythicFeatSelection
            var base_selection2 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("ba0e5a900b775be4a99702f1ed08914d"); //MythicAbilitySelection
            var extra_selection1 = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("8a6a511c55e67d04db328cc49aaad2b8"); //ExtraMythicAbilityMythicFeat

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
        public static void createSwiftHuntersBond()
        {
            var hunterfeat = Helper.ToRef<BlueprintFeatureReference>("6dddf5ba2291f41498df2df7f8fa2b35"); //HuntersBondFeature
            var huntersab = Helper.ToRef<BlueprintAbilityReference>("cd80ea8a7a07a9d4cb1a54e67a9390a5"); //HuntersBondAbility

            //huntersab.Get().AvailableMetamagic |= Metamagic.Quicken;

            var feat = Helper.CreateBlueprintFeature(
                "SwiftHuntersBond",
                "Swift Hunters Bond",
                "Benefit: You can use Hunter's Bond as a swift action."
                ).SetComponents(
                Helper.CreateAutoMetamagic(Metamagic.Quicken, new List<BlueprintAbilityReference>() { huntersab }),
                Helper.CreatePrerequisiteFeature(hunterfeat)
                );

            Helper.AddMythicTalent(feat);
        }

        [PatchInfo(Severity.Create, "Cursing Gaze", "mythic ability: hexes other than grant can be used as a swift action", true, 295)]
        public static void createSwiftHex()
        {
            var witch_selection = Helper.Get<BlueprintFeatureSelection>("9846043cf51251a4897728ed6e24e76f"); //WitchHexSelection
            var shaman_selection = Helper.Get<BlueprintFeatureSelection>("4223fe18c75d4d14787af196a04e14e7"); //ShamanHexSelection
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
                Helper.CreatePrerequisiteFeature(shaman_selection.ToRef(), true)
                );

            Helper.AddMythicTalent(feat);
        }

        public static void createDemonLord()
        {
            var dp = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("d7cbd2004ce66a042aeab2e95a3c5c61"); //DominatePerson
            var dpbuff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("c0f4e1c24c9cd334ca988ed1bd9d201f"); //DominatePersonBuff
            var typedemon = Helper.ToRef<BlueprintUnitFactReference>("dc960a234d365cb4f905bdc5937e623a"); //SubtypeDemon

            var ddbuff = dpbuff.Clone("DominateDemonBuff")
                .RemoveComponents(default(AddFactContextActions));

            var dd = dp.Clone("DominateDemon")
                .RemoveComponents(default(SpellListComponent))
                .RemoveComponents(default(CraftInfoComponent))
                .RemoveComponents(default(AbilityTargetHasNoFactUnless))
                .ReplaceComponent(default(AbilityTargetHasFact), new AbilityTargetHasFact()
                {
                    m_CheckedFacts = new BlueprintUnitFactReference[] { typedemon } //SubtypeDemon, DemonOfMagicFeature, DemonOfSlaughterFeature, DemonOfStrengthFeature
                })
                .ReplaceComponent(default(AbilityEffectRunAction),
                    Helper.CreateAbilityEffectRunAction(SavingThrowType.Will, Helper.CreateContextActionApplyBuff(ddbuff, permanent: true)));

            var x = new ContextCalculateAbilityParams()
            {
                StatTypeFromCustomProperty = true,
                m_CustomProperty = Resource.Cache.PropertyMaxMentalAttribute
            };

            // DC = 10 + HD + Mythic Rank + Mental Stat
            // works only on demons with HD < player HD

            // auto revive once per year :D

            // teleport at will
        }
       
        [PatchInfo(Severity.Extend, "Kinetic Overcharge", "Kinetic Overcharge works always, not only while gathering power", true)]
        public static void patchKineticOvercharge()
        {
            var overcharge = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("4236ca275c6fa9b4a9945b2645262616");
            overcharge.m_Description = Helper.CreateString("You use your mythic powers to fuel kineticist abilities.\nBenefit: Reduce the burn cost of kineticist blasts by one burn point.");
            overcharge.Ranks = 6;
            overcharge.RemoveComponents(default(AddFacts));
            overcharge.AddComponents(
                new KineticistReduceBurnPooled() { ReduceBurn = Helper.CreateContextValue(AbilityRankType.Default) },
                Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, feature: overcharge.ToRef())
                );
        }

        [PatchInfo(Severity.Extend, "Limitless Demon Rage", "Limitless Rage also applies to Demon Rage", true)]
        public static void patchLimitlessDemonRage()
        {
            var rage = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>("0999f99d6157e5c4888f4cfe2d1ce9d6"); //DemonRageAbility
            var rage2 = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("260daa5144194a8ab5117ff568b680f5"); //DemonRageActivateAbility
            var limitless = Helper.ToRef<BlueprintUnitFactReference>("5cb58e6e406525342842a073fb70d068"); //LimitlessRage

            rage.GetComponent<ActivatableAbilityResourceLogic>().m_FreeBlueprint = limitless;
            rage2.GetComponent<AbilityResourceLogic>().ResourceCostDecreasingFacts.Add(limitless);
        }

        [PatchInfo(Severity.Extend, "Unstoppable", "Unstoppable works against more conditions like stun, daze, and confusion", true)]
        public static void patchUnstoppable()
        {
            var unstoppable = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("74afc3465db56924c9618a42d84efab8"); //Unstoppable
            var dominate = Helper.ToRef<BlueprintBuffReference>("c0f4e1c24c9cd334ca988ed1bd9d201f"); //DominatePersonBuff
            var entangle = Helper.ToRef<BlueprintBuffReference>("f7f6330726121cf4b90a6086b05d2e38"); //EntangleBuff

            unstoppable.GetComponents<BuffSubstitutionOnApply>().FirstOrDefault(f => f.SpellDescriptor.HasAnyFlag(SpellDescriptor.Paralysis))
                .SpellDescriptor |= SpellDescriptor.Stun | SpellDescriptor.Daze | SpellDescriptor.Confusion | SpellDescriptor.Petrified;

            //MindAffecting or Compulsion or DominatePersonBuff
            unstoppable.AddComponents(new BuffSubstitutionOnApply() { m_GainedFact = dominate, m_SubstituteBuff = entangle });
        }

        [PatchInfo(Severity.Extend, "Boundless Healing", "Boundless Healing also grants healing spells to spellbooks", true)]
        public static void patchBoundlessHealing()
        {
            var feat = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("c8bbb330aaecaf54dbc7570200653f8c"); //BoundlessHealing
            feat.AddComponents(new AddKnownSpellsAnyClass()
            {
                Spells = new BlueprintAbilityReference[] {
                    Helper.ToRef<BlueprintAbilityReference>("5590652e1c2225c4ca30c4a699ab3649"), //1: CureLightWoundsCast
                    Helper.ToRef<BlueprintAbilityReference>("6b90c773a6543dc49b2505858ce33db5"), //2: CureModerateWoundsCast
                    Helper.ToRef<BlueprintAbilityReference>("41c9016596fe1de4faf67425ed691203"), //3: CureCriticalWoundsCast
                    Helper.ToRef<BlueprintAbilityReference>("3361c5df793b4c8448756146a88026ad"), //4: CureSeriousWoundsCast
                    Helper.ToRef<BlueprintAbilityReference>("5d3d689392e4ff740a761ef346815074"), //5: CureLightWoundsMass
                    Helper.ToRef<BlueprintAbilityReference>("571221cc141bc21449ae96b3944652aa"), //6: CureModerateWoundsMass
                    Helper.ToRef<BlueprintAbilityReference>("0cea35de4d553cc439ae80b3a8724397"), //7: CureSeriousWoundsMass
                    Helper.ToRef<BlueprintAbilityReference>("1f173a16120359e41a20fc75bb53d449"), //8: CureCriticalWoundsMass
                    Helper.ToRef<BlueprintAbilityReference>("5da172c4c89f9eb4cbb614f3a67357d3"), //6: HealCast
                    Helper.ToRef<BlueprintAbilityReference>("867524328b54f25488d371214eea0d90"), //9: HealMass
                    Helper.ToRef<BlueprintAbilityReference>("d5847cad0b0e54c4d82d6c59a3cda6b0"), //5: BreathOfLifeCast
                    Helper.ToRef<BlueprintAbilityReference>("e84fc922ccf952943b5240293669b171"), //2: RestorationLesser
                    Helper.ToRef<BlueprintAbilityReference>("f2115ac1148256b4ba20788f7e966830"), //3: Restoration
                    Helper.ToRef<BlueprintAbilityReference>("fafd77c6bfa85c04ba31fdc1c962c914"), //4: RestorationGreater
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
                    3,
                    4,
                }
            });
        }

        [PatchInfo(Severity.Create, "Resourceful Caster", "mythic ability: regain spells that fail because of spell failure, concentration, SR, saving throws", true, Requirement: typeof(Patch_ResourcefulCaster))]
        public static void createResourcefulCaster()
        {
            var feature = Helper.CreateBlueprintFeature(
                "ResourcefulCasterFeature",
                "Resourceful Caster",
                "You can repurpose magic energy of failed spell. You don't expend a spell slot, if you fail to cast a spell due to arcane spell failure or concentration checks. Furthermore you regain your spell slot, whenever all targets of your spells resist due to spell resistance or succeed on their saving throws."
                );

            Resource.Cache.FeatureResourcefulCaster.SetReference(feature);

            Helper.AddMythicTalent(Resource.Cache.FeatureResourcefulCaster);
        }

        [PatchInfo(Severity.Extend, "Ranging Shots", "doesn't get weaker when hitting", true)]
        public static void patchRangingShots()
        {
            var feat = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("2b558167862b05d47b8472d176257f82"); //RangingShots
            var buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("e9da1dc30535fc94ab458ee20f8d3c4e"); //RangingShotsBuff

            // keep attack bonus while hitting
            feat.RemoveComponents(r => r is AddInitiatorAttackWithWeaponTrigger trigger && trigger.OnlyHit);
            // feat.AddComponents(Helper.CreateCombatStateTrigger(Helper.CreateContextActionRemoveBuff(buff))); already removes after combat
            feat.m_Description = Helper.CreateString("Every time you miss an enemy with a ranged weapon attack, your aim improves, giving you a stacking +1 bonus on attacks up to the maximum of your mythic rank for the remainder of the combat.");
        }

        [PatchInfo(Severity.Extend, "Wandering Hex", "can swap hex at will", true)]
        public static void patchWanderingHex()
        {
            var feat = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("b209beab784d93546b40a2fa2a09ffa8"); //WitchWanderingHexAbility
            feat.RemoveComponents(default(AbilityResourceLogic));
        }

        [PatchInfo(Severity.Extend, "Various Tweaks", "allow quicken on Demon Teleport, fix description", true)]
        public static void patchVarious()
        {
            // allow quicken metamagic on demon teleport
            ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("b3e8e307811b2a24387c2c9226fb4c10") //DemonTeleport
                .AvailableMetamagic |= Metamagic.Quicken;

            // update Always A Chance description
            ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("d57301613ad6a5140b2fdac40fa368e3") //AlwaysAChance
                .m_Description = Helper.CreateString("You automatically succeed when you roll a natural 1."); // todo: make own patch!
        }

        #region Helper

        public static void setResourceDecreasing(BlueprintGuid resource, BlueprintUnitFactReference limitless, bool repeat = false)
        {
            Resource.Cache.Ensure();

            Helper.PrintJoinDebug(" -free abilities: ", null);

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
                Helper.PrintJoinDebug(ability.name);
            }

            Helper.PrintJoinDebug(" -free activatable: ", null, true);

            foreach (var ability in Resource.Cache.Activatable)
            {
                var logic = ability.GetComponent<ActivatableAbilityResourceLogic>();
                if (logic == null || logic.m_RequiredResource?.deserializedGuid != resource)
                    continue;

                Helper.PrintJoinDebug(ability.name);
                if (logic.m_FreeBlueprint != null && !logic.m_FreeBlueprint.IsEmpty())
                    Helper.Print($"ERROR: {ability.name} has already a FreeBlueprint");
                logic.m_FreeBlueprint = limitless;
            }

            Helper.PrintJoinDebug(flush: true);
        }

        #endregion

    }

    #region Patches

    [PatchInfo(Severity.Harmony, "Patch: Always A Chance", "Always A Chance succeeds on a natural one and applies to most d20 rolls", true)]
    [HarmonyPatch]
    public class Patch_AlwaysAChance
    {
        [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.IsSuccessRoll))]
        [HarmonyPrefix]
        public static bool Prefix1(int d20, RuleAttackRoll __instance, ref bool __result)
        {
            __result = d20 == 20 || (d20 == 1 && __instance.Initiator.State.Features.AlwaysChance) || (d20 > 1 && d20 + __instance.AttackBonus >= __instance.TargetAC);
            return false;
        }

        [HarmonyPatch(typeof(RuleCombatManeuver), nameof(RuleCombatManeuver.IsSuccessRoll))]
        [HarmonyPrefix]
        public static bool Prefix2(int d20, RuleCombatManeuver __instance, ref bool __result)
        {
            __result = d20 == 20 || (d20 == 1 && __instance.Initiator.State.Features.AlwaysChance) || (d20 > 1 && d20 + __instance.InitiatorCMB >= __instance.TargetCMD);
            return false;
        }

        [HarmonyPatch(typeof(RuleDispelMagic), nameof(RuleDispelMagic.IsSuccessRoll))]
        [HarmonyPrefix]
        public static bool Prefix3(int d20, RuleDispelMagic __instance, ref bool __result)
        {
            __result = ((d20 == 20 || d20 == 1) && __instance.Initiator.State.Features.AlwaysChance) || __instance.DC == 0 || __instance.CasterLevelCheckValue(d20) >= __instance.DC;
            return false;
        }

        [HarmonyPatch(typeof(RuleSkillCheck), nameof(RuleSkillCheck.IsSuccessRoll))]
        [HarmonyPrefix]
        public static bool Prefix4(int d20, int successBonus, RuleSkillCheck __instance, ref bool __result)
        {
            __result = ((d20 == 20 || d20 == 1) && __instance.Initiator.State.Features.AlwaysChance) || d20 + __instance.TotalBonus + successBonus >= __instance.DC;
            return false;
        }

        [HarmonyPatch(typeof(RuleSavingThrow), nameof(RuleSavingThrow.IsSuccessRoll))]
        [HarmonyPrefix]
        public static bool Prefix5(int d20, int successBonus, RuleSavingThrow __instance, ref bool __result)
        {
            __result = d20 == 20 || (d20 == 1 && __instance.Initiator.State.Features.AlwaysChance) || (d20 > 1 && d20 + __instance.StatValue + successBonus >= __instance.DifficultyClass);
            return false;
        }

        private static bool Check(int d20, int bonus, int DC, bool AlwaysChance, bool CanCrit)
        {
            if (CanCrit)
                return d20 == 20 || AlwaysChance && d20 == 1 || d20 + bonus >= DC;
            else
                return (AlwaysChance && (d20 == 20 || d20 == 1)) || d20 + bonus >= DC;
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Resourceful Caster", "patches for Resourceful Caster", true)]
    [HarmonyPatch]
    public class Patch_ResourcefulCaster
    {
        [HarmonyPatch(typeof(RuleCastSpell), nameof(RuleCastSpell.ShouldSpendResource), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Postfix1(RuleCastSpell __instance, ref bool __result) // arcane spell failure
        {
            if (__result == false)
                return;

            if (!__instance.IsSpellFailed && !__instance.IsArcaneSpellFailed)
                return;

            if (__instance.Initiator.Descriptor.HasFact(Resource.Cache.FeatureResourcefulCaster))
                __result = false;
        }

        [HarmonyPatch(typeof(UnitUseAbility), nameof(UnitUseAbility.FailIfConcentrationCheckFailed))]
        [HarmonyPrefix]
        public static bool Prefix2(UnitUseAbility __instance, ref bool __result) // concentration failed
        {
            if (__instance.ConcentrationCheckFailed)
            {
                __instance.SpawnInterruptFx();
                __instance.ForceFinish(UnitCommand.ResultType.Fail);
                if (!__instance.Executor.Descriptor.HasFact(Resource.Cache.FeatureResourcefulCaster))
                    __instance.Ability.Spend();
                if (__instance.AiAction != null)
                    __instance.Executor.CombatState.AIData.UseAction(__instance.AiAction, __instance);
            }
            __result = __instance.ConcentrationCheckFailed;
            return false;
        }

        public static List<UnitEntityData> unitsSpellNotResisted = new List<UnitEntityData>();
        [HarmonyPatch(typeof(AbilityExecutionProcess), nameof(AbilityExecutionProcess.Tick))]
        [HarmonyPostfix]
        public static void Postfix3(AbilityExecutionProcess __instance) // all targets resisted
        {
            if (!__instance.IsEnded)
                return;
            Helper.PrintDebug($"Cast complete {__instance.Context.AbilityBlueprint.name} from {__instance.Context.MaybeCaster?.CharacterName}");
            if (!__instance.Context.MaybeCaster.Descriptor.HasFact(Resource.Cache.FeatureResourcefulCaster))
                return;
            if (__instance.Context.IsDuplicateSpellApplied)
                return;
            var spell = __instance.Context.Ability;
            if (spell == null)
                return;
            var spellbook = __instance.Context.Ability.Spellbook;
            if (spellbook == null)
                return;

            bool hasSaves = false;
            bool allSavesPassed = true;
            unitsSpellNotResisted.Clear();
            foreach (var rule in __instance.Context.RulebookContext.AllEvents)
            {
                if (rule is RuleSpellResistanceCheck resistance)
                {
                    Helper.PrintDebug($" -SR {resistance.Target}");
                    hasSaves = true;
                    if (!resistance.IsSpellResisted)
                    {
                        unitsSpellNotResisted.Add(resistance.Target);
                    }
                }

                else if (rule is RuleSavingThrow save)
                {
                    Helper.PrintDebug($" -{save.Type} Save {save.Initiator}");
                    hasSaves = true;
                    if (save.IsPassed)
                    {
                        unitsSpellNotResisted.Remove(save.Initiator);
                    }
                    else
                    {
                        allSavesPassed = false;
#if !DEBUG
                        break;
#endif
                    }
                }

                //else Helper.PrintDebug(" -" + rule.GetType().FullName);
            }

            if (hasSaves && allSavesPassed && unitsSpellNotResisted.Count == 0)
            {
                // refund spell if all targets resisted

                spell = spell.ConvertedFrom ?? spell;

                Helper.PrintDebug("Refunding spell");
                Resource.Log.Print($"{__instance.Context.Caster.CharacterName} regained {spell.Name}");
                //LogThreadController.Instance.m_Logs[LogChannelType.AnyCombat].;
                //var items = RootUIContext.Instance.InGameVM.StaticPartVM.CombatLogVM.Items;
                //for (int i = items.Count - 1; i >= 0 && i >= items.Count - 11; i--)
                //{
                //    var text = items[i].Message.Message;
                //    var tooltip = items[i].Message.Tooltip as TooltipTemplateSimple;
                //    Helper.Print($"text={text} header={tooltip?.Header} tooltip={tooltip?.Description}");
                //}

                int level = spellbook.GetSpellLevel(spell);
                if (spellbook.Blueprint.Spontaneous)
                {
                    if (level > 0)
                        spellbook.m_SpontaneousSlots[level]++;
                }
                else
                {
                    foreach (var slot in spellbook.m_MemorizedSpells[level])
                    {
                        if (!slot.Available && slot.Spell == spell)
                        {
                            slot.Available = true;
                            break;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RuleSpellResistanceCheck), nameof(RuleSpellResistanceCheck.OnTrigger))]
        [HarmonyPostfix]
        public static void Postfix4(RuleSpellResistanceCheck __instance) // push SR checks into history
        {
            try
            {
                __instance.Context?.SourceAbilityContext?.RulebookContext?.m_AllEvents.Add(__instance);
                Helper.PrintDebug("added SR check to stack");
            }
            catch (Exception e)
            {
                Helper.Print("Patch_ResourcefulCaster4 " + e);
            }
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Magic Item Adept", "patches for Magic Item Adept", true)]
    [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.GetParamsFromItem))]
    public class Patch_MagicItemAdept
    {
        public static void Postfix(AbilityData __instance, AbilityParams __result)
        {
            if (__instance.Caster == null)
                return;

            if (!__instance.Caster.Unit.Descriptor.HasFact(Resource.Cache.FeatureMagicItemAdept))
                return;

            __result.CasterLevel = __instance.Caster.Progression.CharacterLevel;
        }
    }

    #endregion
}
