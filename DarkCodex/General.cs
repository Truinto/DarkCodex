using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.View.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using System.IO;
using CodexLib;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Parts;

namespace DarkCodex
{
    public class General
    {
        [PatchInfo(Severity.Create | Severity.Faulty, "Ability Focus", "basic feat: Ability Focus, increase DC of one ability by +2", false)]
        public static void CreateAbilityFocus()
        {
            var feat = Helper.CreateBlueprintParametrizedFeature(
                "AbilityFocusCustom",
                "Ability Focus",
                "Choose one special attack. Add +2 to the DC for all saving throws against the special attack on which you focus.",
                requireKnown: true,
                onlyNonSpells: true
                ).SetComponents(
                new AbilityFocusParametrized()
                );
            feat.RequireProficiency = true;

            var list = new List<AnyBlueprintReference>();

            foreach (var ab in BpCache.Get<BlueprintAbility>())
            {
                if (ab.Type == AbilityType.Spell
                    || ab.m_DisplayName.IsEmptyKey()
                    || ab.HasVariants)
                    continue;
                var run = ab.GetComponent<AbilityEffectRunAction>();
                if (run == null || run.SavingThrowType == SavingThrowType.Unknown)
                    continue;

                list.Add(ab.ToReference<AnyBlueprintReference>());
            }

            foreach (var ft in BpCache.Get<BlueprintFeature>())
            {
                if (ft.m_DisplayName.IsEmptyKey()
                    || ft.GetComponent<ContextCalculateAbilityParams>() == null)
                    continue;

                list.Add(ft.ToReference<AnyBlueprintReference>());
            }

            feat.CustomParameterVariants = list.ToArray();
#if DEBUG
            Helper.AddFeats(feat); // TODO: bugfix ability focus
#endif
        }

        [PatchInfo(Severity.Extend, "Empower Angels Light", "'Light of the Angels' give temporary HP equal to character level", true)]
        public static void PatchAngelsLight()
        {
            var angelbuff = Helper.Get<BlueprintBuff>("e173dc1eedf4e344da226ffbd4d76c60"); // AngelMinorAbilityEffectBuff

            var temphp = angelbuff.GetComponent<TemporaryHitPointsFromAbilityValue>();
            temphp.Value = Helper.CreateContextValue(AbilityRankType.Default);
            angelbuff.AddComponents(Helper.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, type: AbilityRankType.Default)); // see FalseLifeBuff
        }

        [PatchInfo(Severity.Extend | Severity.DefaultOff, "Basic Freebie Feats", "reduced feat tax, inspired from https://michaeliantorno.com/feat-taxes-in-pathfinder/", true)]
        public static void PatchBasicFreebieFeats()
        {
            var basics = Helper.Get<BlueprintProgression>("5b72dd2ca2cb73b49903806ee8986325"); //BasicFeatsProgression
            basics.AddComponents(
                new AddFactOnlyParty("9972f33f977fc724c838e59641b2fca5"), //PowerAttackFeature
                new AddFactOnlyParty("0da0c194d6e1d43419eb8d990b28e0ab"), //PointBlankShot
                new AddFactOnlyParty("4c44724ffa8844f4d9bedb5bb27d144a"), //CombatExpertiseFeature
                new AddFactOnlyParty("90e54424d682d104ab36436bd527af09"), //WeaponFinesse
                new AddFactOnlyParty("f47df34d53f8c904f9981a3ee8e84892")  //DeadlyAimFeature
                );

            var powerattack = Helper.Get<BlueprintActivatableAbility>("a7b339e4f6ff93a4697df5d7a87ff619"); //PowerAttackToggleAbility
            powerattack.IsOnByDefault = false;
            powerattack.DoNotTurnOffOnRest = true;
            var combatexpertise = Helper.Get<BlueprintActivatableAbility>("a75f33b4ff41fc846acbac75d1a88442"); //CombatExpertiseToggleAbility
            combatexpertise.IsOnByDefault = false;
            combatexpertise.DoNotTurnOffOnRest = true;
            combatexpertise.DeactivateIfCombatEnded = false;
            combatexpertise.DeactivateAfterFirstRound = false;
            combatexpertise.ActivationType = AbilityActivationType.Immediately;
            combatexpertise.DeactivateIfOwnerDisabled = true;
            var deadlyaim = Helper.Get<BlueprintActivatableAbility>("ccde5ab6edb84f346a74c17ea3e3a70c"); //DeadlyAimToggleAbility
            deadlyaim.IsOnByDefault = false;
            deadlyaim.DoNotTurnOffOnRest = true;

            var mobility = Helper.ToRef<BlueprintUnitFactReference>("2a6091b97ad940943b46262600eaeaeb"); //Mobility
            var dodge = Helper.ToRef<BlueprintUnitFactReference>("97e216dbb46ae3c4faef90cf6bbe6fd5"); //Dodge
            mobility.Get().AddComponents(new AddFactOnlyParty(dodge));
            dodge.Get().AddComponents(new AddFactOnlyParty(mobility));

            var twf = Helper.ToRef<BlueprintUnitFactReference>("ac8aaf29054f5b74eb18f2af950e752d"); //TwoWeaponFighting
            var twfi = Helper.ToRef<BlueprintUnitFactReference>("9af88f3ed8a017b45a6837eab7437629"); //TwoWeaponFightingImproved
            var twfg = Helper.ToRef<BlueprintUnitFactReference>("c126adbdf6ddd8245bda33694cd774e8"); //TwoWeaponFightingGreater
            var multi = Helper.ToRef<BlueprintUnitFactReference>("8ac319e47057e2741b42229210eb43ed"); //Multiattack
            twf.Get().AddComponents(new AddFactOnlyParty(multi));
            twfi.Get().AddComponents(new AddFactOnlyParty(twfg, 8));

            //Deft Maneuvers
            //ImprovedTrip.0f15c6f70d8fb2b49aa6cc24239cc5fa
            //ImprovedDisarm.25bc9c439ac44fd44ac3b1e58890916f
            //ImprovedDirtyTrick.ed699d64870044b43bb5a7fbe3f29494

            //Powerful Maneuvers
            //ImprovedBullRush.b3614622866fe7046b787a548bbd7f59
            //ImprovedSunder.9719015edcbf142409592e2cbaab7fe1

            Helper.Get<BlueprintFeature>("6a556375036ac8b4ebd80e74d308d108").RemoveComponents(r => r.GetType().Name.StartsWith("Recommendation")); //PiranhaStrikeFeature
        }

        [PatchInfo(Severity.Create, "Preferred Spell", "basic feat: Preferred Spell, spontaneously cast a specific spell", false, Requirement: typeof(Patch_PreferredSpellMetamagic))]
        public static void CreatePreferredSpell()
        {
            var specialization = Helper.Get<BlueprintParametrizedFeature>("f327a765a4353d04f872482ef3e48c35"); //SpellSpecializationFirst
            var wizard = Helper.Get<BlueprintFeatureSelection>("8c3102c2ff3b69444b139a98521a4899"); //WizardFeatSelection
            var heighten = Helper.ToRef<BlueprintFeatureReference>("2f5d1e705c7967546b72ad8218ccf99c"); //HeightenSpellFeat

            var feat = Helper.CreateBlueprintParametrizedFeature(
                "PreferredSpellFeature",
                "Preferred Spell",
                "Choose one spell which you have the ability to cast. You can cast that spell spontaneously by sacrificing a prepared spell or spell slot of equal or higher level. You can apply any metamagic feats you possess to this spell when you cast it. This increases the minimum level of the prepared spell or spell slot you must sacrifice in order to cast it but does not affect the casting time.\nSpecial: You can gain this feat multiple times.Its effects do not stack. Each time you take the feat, it applies to a different spell.",
                icon: null,
                onlyKnownSpells: true,
                requireKnown: true
                ).SetComponents(
                new PreferredSpell(),
                Helper.CreatePrerequisiteStatValue(StatType.SkillKnowledgeArcana, 5),
                Helper.CreatePrerequisiteFeature(heighten)
                );
            feat.Groups = new FeatureGroup[] { FeatureGroup.Feat, FeatureGroup.WizardFeat };
            feat.Ranks = 10;

            Helper.AddFeats(feat);
            Helper.AppendAndReplace(ref wizard.m_AllFeatures, feat.ToRef());

            Main.RunLast("Preferred Spell", () => feat.BlueprintParameterVariants = specialization.BlueprintParameterVariants);
        }

        [PatchInfo(Severity.Extend | Severity.WIP, "Hide Buffs", "unclogs UI by hiding a few buffs", false)]
        public static void PatchHideBuffs()
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
                    var buff = Helper.Get<BlueprintBuff>(guid);
                    buff.Flags(hidden: true);
                }
                catch (Exception)
                {
                    Main.Print(" error: couldn't load " + guid);
                }
            }
        }

        [PatchInfo(Severity.Extend, "Various Tweaks", "removed PreciousTreat penalty, extend protection from X to 10 minutes", true)]
        public static void PatchVarious()
        {
            // remove penalty on Precious Treat item
            var buff = Helper.Get<BlueprintBuff>("ee8ee3c5c8f055e48a1ec1bfb92778f1"); //PreciousTreatBuff
            buff.RemoveComponents<AddStatBonus>();

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
                    var ab = Helper.Get<BlueprintAbility>(guid);
                    ab.LocalizedDuration = Resource.Strings.TenMinutes;
                    ab.Get<ContextActionApplyBuff>(f => f.DurationValue.BonusValue = 10);
                }
                catch (Exception) { }
            }

            // demon graft respects immunities
            var discordRage = Helper.Get<BlueprintBuff>("5d3029eb16956124da2c6b79ed32c675"); //SongOfDiscordEnragedBuff
            discordRage.AddComponents(Helper.CreateSpellDescriptorComponent(SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion));

            // fix BloodlineUndeadArcana and DirgeBardSecretsOfTheGraveFeature not allowing shaken/confusion
            var undeadImmunities = Helper.Get<BlueprintFeature>("8a75eb16bfff86949a4ddcb3dd2f83ae"); //UndeadImmunities
            undeadImmunities.GetComponents<BuffDescriptorImmunity>().First(f => f.IgnoreFeature == null)
                .Descriptor &= ~SpellDescriptor.Shaken & ~SpellDescriptor.Confusion;
            undeadImmunities.GetComponents<SpellImmunityToSpellDescriptor>().First(f => f.CasterIgnoreImmunityFact == null)
                .Descriptor &= ~SpellDescriptor.Shaken & ~SpellDescriptor.Confusion;

            // add icon to CompletelyNormal metamagic
            //if (UIRoot.Instance.SpellBookColors.MetamagicCompletelyNormal == null)
            //{
            //    var cnfeat = Helper.Get<BlueprintFeature>("094b6278f7b570f42aeaa98379f07cf2"); //CompletelyNormalSpellFeat
            //    cnfeat.m_Icon = Helper.CreateSprite("CompletelyNormal.png");
            //    UIRoot.Instance.SpellBookColors.MetamagicCompletelyNormal = cnfeat.m_Icon;
            //}

            // add confusion descriptor to song of discord
            var songDiscord = Helper.Get<BlueprintBuff>("2e1646c2449c88a4188e58043455a43a"); //SongOfDiscordBuff
            songDiscord.GetComponent<SpellDescriptorComponent>().Descriptor |= SpellDescriptor.Confusion;

            // fix destrutive dispel scaling
            Helper.Get<BlueprintUnitProperty>("13e4f1dd08954723b173335a54b48746") //DestructiveDispelProperty
                .SetComponents(
                new PropertyAttributeMax() { MentalStat = true },
                new SimplePropertyGetter() { Property = UnitProperty.Level, Settings = new() { m_Progression = PropertySettings.Progression.Div2 } });

            // body forms become immune to horrid wilting
            var resistWilt = new AddSpellImmunity() { Type = SpellImmunityType.Specific, m_Exceptions = Helper.ToRef<BlueprintAbilityReference>("08323922485f7e246acb3d2276515526").ObjToArray() }; //HorridWilting
            Helper.Get<BlueprintBuff>("b574e1583768798468335d8cdb77e94c").AddComponents(resistWilt); //FieryBodyBuff
            Helper.Get<BlueprintBuff>("a6da7d6a5c9377047a7bd2680912860f").AddComponents(resistWilt); //IceBodyBuff
            Helper.Get<BlueprintBuff>("2eabea6a1f9a58246a822f207e8ca79e").AddComponents(resistWilt); //IronBodyBuff
        }

        [PatchInfo(Severity.Create, "Stop Activatable", "adds ability to stop any activatable immediately", false)] // TODO: playtest
        public static void CreateBardStopSong()
        {
            //ContextActionStopActivatables
            Helper.CreateBlueprintAbility(
                "StopActivatables",
                "Stop Activatables",
                "Immediately stop all disabled activatables. Useful for bardic performance.",
                icon: Helper.CreateSprite(Path.Combine(Main.ModPath, "icons", "StopSong.png")),
                type: AbilityType.Special,
                actionType: UnitCommand.CommandType.Free,
                range: AbilityRange.Personal
                ).TargetSelf(
                ).SetComponents(
                Helper.CreateAbilityExecuteActionOnCast(new ContextActionStopActivatables())
                );
        }

        [PatchInfo(Severity.Create, "Mad Magic", "combat feat: allows spell casting during a rage", false)]
        public static void CreateMadMagic()
        {
            var feat = Helper.CreateBlueprintFeature(
                "MadMagicFeat",
                "Mad Magic",
                "You can cast spells from any class while in a rage.",
                group: FeatureGroup.CombatFeat
                ).SetComponents(
                //Helper.CreatePrerequisiteFeature(Helper.ToRef<BlueprintFeatureReference>("6991ee8175d87c04790067515f6fb322"), // BloodragerRageFeature
                Helper.CreateAddConditionExceptions(UnitCondition.SpellcastingForbidden,
                    Helper.ToRef<BlueprintBuffReference>("da8ce41ac3cd74742b80984ccc3c9613"), //StandartRageBuff
                    Helper.ToRef<BlueprintBuffReference>("3513326cd64f475781799685c57fa452"), //StandartFocusedRageBuff
                    Helper.ToRef<BlueprintBuffReference>("345d36cd45f5614409824209f26d0130"), //InspiredRageEffectBuffBeforeMasterSkald
                    Helper.ToRef<BlueprintBuffReference>("6928adfa56f0dcc468162efde545786b"), //RageSpellBuff
                    Helper.ToRef<BlueprintBuffReference>("2ff155ab5a6316e4e809f42148ef4d09")  //InfectiousRageBuff
                ));

            Helper.AddCombatFeat(feat);
        }

        [PatchInfo(Severity.Fix, "Fix Spell Element Change", "fixes Elemental Bloodline and Spell Focus interaction", false)]
        public static void FixSpellElementChange()
        {
            Main.Patch(typeof(Patch_ChangeSpellElement));

            foreach (var buff in BpCache.Get<BlueprintBuff>())
            {
                var changeElement = buff.GetComponent<ChangeSpellElementalDamage>();
                if (changeElement != null)
                {
                    var fix = new ChangeSpellElementalDamageFix(changeElement.Element);
                    buff.AddComponents(fix);
                    Main.PrintDebug($"FixSpellElementChange {buff.name} {fix.Descriptor}");
                }
            }
        }

        [PatchInfo(Severity.Fix, "Fix Master Shapeshifter", "ensures spells with the Polymorph descriptor get the benefit of Master Shapeshifter", false)]
        public static void FixMasterShapeshifter()
        {
            var feat = Helper.Get<BlueprintFeature>("934670ef88b281b4da5596db8b00df2f"); //MasterShapeshifter
            feat.AddComponents(new MasterShapeshifterFix());

            var fright = Helper.Get<BlueprintAbility>("e788b02f8d21014488067bdd3ba7b325"); //FrightfulAspect
            fright.AddComponents(Helper.CreateSpellDescriptorComponent(SpellDescriptor.Polymorph));
        }

        [PatchInfo(Severity.Create, "Backgrounds", "basic feat: Additional Traits\ntraits: Magical Lineage, Metamagic Master, Fate’s Favored", false)]
        public static void CreateBackgrounds()
        {
            /*

            Magical Knack: Magic Traits
            You were raised, either wholly or in part, by a magical creature, either after it found you abandoned in the woods or because your parents often left you in the care of a magical minion. This constant exposure to magic has made its mysteries easy for you to understand, even when you turn your mind to other devotions and tasks.
            Benefit: Pick a class when you gain this trait—your caster level in that class gains a +2 trait bonus as long as this bonus doesn’t raise your caster level above your current Hit Dice.

            Magical Lineage: Magic Traits
            One of your parents was a gifted spellcaster who not only used metamagic often, but also developed many magical items and perhaps even a new spell or two—and you have inherited a fragment of this greatness.
            Benefit: Pick one spell when you choose this trait. When you apply metamagic feats to this spell that add at least 1 level to the spell, treat its actual level as 1 lower for determining the spell’s final adjusted level.

            Metamagic Master: Regional Traits
            Your ability to alter your spell of choice is greater than expected.
            Choose: A spell of 3rd level or below.
            Benefit: When you use the chosen spell with a metamagic feat, it uses up a spell slot one level lower than it normally would.

            Reluctant Apprentice
            Your early training grants you knowledge of the arcane.
            Benefits: You gain a +1 trait bonus on Knowledge (arcana) checks, and are considered trained in that skill even if you have no ranks in it.

            Inspired by Greatness: Campaign Trait
            Choose one spell you can cast. From now on, you always cast this spell at +1 caster level.

            Fate’s Favored: Faith Trait
            The fates watch over you.
            Benefit: Whenever you are under the effect of a luck bonus of any kind, that bonus increases by 1.

            */

            var scholarMetamagic = Helper.CreateBlueprintParametrizedFeature(
                "BackgroundScholarMagicalLineage",
                "Magical Lineage",
                "One of your parents was a gifted spellcaster who not only used metamagic often, but also developed many magical items and perhaps even a new spell or two — and you have inherited a fragment of this greatness.\nBenefit: Pick one spell when you choose this trait. When you apply metamagic feats to this spell that add at least 1 level to the spell, treat its actual level as 1 lower for determining the spell's final adjusted level. Additionally your caster level gains a +2 trait bonus as long as this bonus doesn’t raise your caster level above your current Hit Dice.",
                icon: null,
                group: FeatureGroup.Trait,
                maxlevel: 10
                ).SetComponents(
                new MetamagicReduceCostParametrized(),
                new AddCasterLevelLimit() { Bonus = 2 }
                );

            var scholarMetamagic2 = Helper.CreateBlueprintParametrizedFeature(
                "BackgroundScholarMetamagicMaster",
                "Metamagic Master",
                "You have mastered a particular spell. Whenever you apply metamagic feats to it, you can reduce its final adjusted level by up to 2, but not below the spell's original cost.",
                icon: null,
                group: FeatureGroup.Trait,
                maxlevel: 10
                ).SetComponents(
                new MetamagicReduceCostParametrized(2)
                );

            var faithTrait1 = Helper.CreateBlueprintFeature(
                "BackgroundFatesFavored",
                "Fate's Favored",
                "The fates watch over you.\nBenefit: Whenever you are under the effect of a luck bonus of any kind, that bonus increases by 1."
                ).SetComponents(
                new IncreaseModifierBonus(1, ModifierDescriptor.Luck)
                );

            // Magic Traits
            var scholar = Helper.Get<BlueprintFeatureSelection>("273fab44409035f42a7e2af0858a463d"); //BackgroundsScholarSelection
            scholar.Add(scholarMetamagic, scholarMetamagic2);

            // Faith Traits
            var faith = Helper.Get<BlueprintFeatureSelection>("c25021c31f302c6449ecdbc978822507"); //BackgroundsOblateSelection
            faith.Add(faithTrait1);

            // disallow picking the same group twice
            var backgroundSelection = Helper.Get<BlueprintFeatureSelection>("f926dabeee7f8a54db8f2010b323383c");    //BackgroundsBaseSelection
            foreach (var feat in backgroundSelection.m_AllFeatures)
            {
                if (feat.Get() is BlueprintFeatureSelection selection)
                    selection.AddComponents(Helper.CreatePrerequisiteNoFeature(feat));
            }

            var extra = Helper.CreateBlueprintFeatureSelection(
                "AdditionalTraits",
                "Additional Traits",
                "You have more traits than normal.\nBenefit: You gain another character background of your choice. This background must be chosen from a different list, and cannot be chosen from lists from which you have already selected a character background.",
                icon: null,
                group: FeatureGroup.Feat
                ).Add(backgroundSelection.m_AllFeatures);

            Helper.AddFeats(extra);
        }

        [PatchInfo(Severity.Create, "Heritages", "adds Orc-Atavism; Kindred-Raised Half-Elf regain Elven Immunity", true)]
        public static void CreateHeritage()
        {
            var orc = Helper.Get<BlueprintFeatureSelection>("8c3244440e0b4d1d9d9b182685cbacbd"); //HalfOrcHeritageSelection
            var mythic = Helper.ToRef<BlueprintUnitFactReference>("325f078c584318849bfe3da9ea245b9d").ObjToArray(); //DestinyBeyondBirthMythicFeat
            var mythicRaces = mythic[0].Get().GetComponent<PrerequisiteFeaturesFromList>();

            var minusInt = Helper.CreateBlueprintFeature(
                "MinusIntelligence",
                "Intelligence Malus",
                "You take a -2 penalty to Intelligence.",
                group: FeatureGroup.Racial
                ).SetComponents(
                new AddStatBonusIfHasFact
                {
                    Descriptor = ModifierDescriptor.Racial,
                    Stat = StatType.Intelligence,
                    Value = -2,
                    InvertCondition = true,
                    m_CheckedFacts = mythic
                });

            var minusWis = Helper.CreateBlueprintFeature(
                "MinusWisdom",
                "Wisdom Malus",
                "You take a -2 penalty to Wisdom.",
                group: FeatureGroup.Racial
                ).SetComponents(
                new AddStatBonusIfHasFact
                {
                    Descriptor = ModifierDescriptor.Racial,
                    Stat = StatType.Wisdom,
                    Value = -2,
                    InvertCondition = true,
                    m_CheckedFacts = mythic
                });

            var minusCha = Helper.CreateBlueprintFeature(
                "MinusCharisma",
                "Charisma Malus",
                "You take a -2 penalty to Charisma.",
                group: FeatureGroup.Racial
                ).SetComponents(
                new AddStatBonusIfHasFact
                {
                    Descriptor = ModifierDescriptor.Racial,
                    Stat = StatType.Charisma,
                    Value = -2,
                    InvertCondition = true,
                    m_CheckedFacts = mythic
                });


            var kindred = Helper.Get<BlueprintFeature>("5609d6e6cbd5422c8d6a1e7ee0b31a87"); //KindredRaisedHalfElf
            kindred.RemoveComponents(r => r is RemoveFeatureOnApply remove && remove.m_Feature.deserializedGuid == "2483a523984f44944a7cf157b21bf79c");
            kindred.m_Description.CreateString("Kindred-Raised loses keen senses and adaptability, but gains a +2 racial {g|Encyclopedia:Bonus}bonus{/g} to {g|Encyclopedia:Charisma}Charisma{/g}.");

            var ferocity = Helper.CreateBlueprintFeature(
                "FerocityFeature"
                ).SetUIData(
                Helper.GetString("9d01b26c-00ce-403e-a8b8-9b675af90bfb"),
                Helper.GetString("ad5e639d-6731-47c7-8cd6-db98c22b568c")
                ).SetComponents(
                Helper.CreateAddMechanicsFeature(AddMechanicsFeature.MechanicsFeatureType.Ferocity)
                );

            var atavism = Helper.CreateBlueprintFeatureSelection(
                "AtavismOrc",
                "Orc Atavism",
                "Some half-orcs have much stronger orc blood than human blood. Such half-orcs count as only half-orcs and orcs (not also humans) for any effect related to race. They gain a +2 bonus to Strength, a +2 bonus to one ability score of their choice, and a –2 penalty to one mental ability score of their choice. They also gain razortusk. Finally, they gain the ferocity universal monster ability. This racial trait replaces the half-orc’s usual racial ability score modifiers, as well as intimidating, orc blood, and orc ferocity.",
                group: FeatureGroup.Racial
                ).SetComponents(
                Helper.CreateAddStatBonus(2, StatType.Strength, ModifierDescriptor.Racial),
                Helper.CreateAddFacts(ferocity, "86af486a0d92427280c46127a216c85a"), //FerocityFeature, Razortusk
                Helper.CreateRemoveFeatureOnApply("885f478dff2e39442a0f64ceea6339c9"), //Intimidating
                Helper.CreateRemoveFeatureOnApply("c99f3405d1ef79049bd90678a666e1d7") //HalfOrcFerocity
                ).Add(minusInt, minusWis, minusCha);
            orc.Add(atavism);
            Helper.AppendAndReplace(ref mythicRaces.m_Features, atavism.ToRef());

            // remove resource limit on KitsuneMageLight
            Helper.Get<BlueprintAbility>("1fa738778f4811247befeaa9b19da91f").RemoveComponents<AbilityResourceLogic>();
        }

        [PatchInfo(Severity.Create, "Sacred Summons", "basic feat: requires Channel Energy, summons act immediately", false)]
        public static void CreateSacredSummons()
        {
            var selective = Helper.Get<BlueprintFeature>("fd30c69417b434d47b6b03b9c1f568ff"); //SelectiveChannel
            var preq = selective.GetComponents<PrerequisiteFeature>(f => f.Group == Prerequisite.GroupType.Any).Clone();

            var feat = Helper.CreateBlueprintFeature(
                "SacredSummons",
                "Sacred Summons",
                "When using summon monster to summon creatures whose alignment subtype or subtypes exactly match your aura, you may cast the spell as a standard action instead of with a casting time of 1 round."
                ).SetComponents(preq.ToArray<BlueprintComponent>());
            feat.AddComponents(
                new AddMechanicFeatureCustom(MechanicFeature.SummoningNoFullRound),
                new SacredSummons());

            Helper.AddFeats(feat);
        }

        [PatchInfo(Severity.Create, "Dirty Fighting", "basic feat: Dirty Fighting; you don't suffer an attack of opportunity but incure a -4 penalty if you are not flanking and don't have the right maneuver feat", false)]
        public static void CreateDirtyFighting()
        {
            var feat = Helper.CreateBlueprintFeature(
                "DirtyFighting",
                "Dirty Fighting",
                "You can take advantage of a distracted foe.\nBenefit(s): When you attempt a combat maneuver check against a foe you are flanking, you can forgo the +2 bonus on your attack roll for flanking to instead have the combat maneuver not provoke an attack of opportunity. If you have a feat or ability that allows you to attempt the combat maneuver without provoking an attack of opportunity, you can instead increase the bonus on your attack roll for flanking to +4 for the combat maneuver check.\nSpecial: This feat counts as having Dex 13, Int 13, Combat Expertise, and Improved Unarmed Strike for the purposes of meeting the prerequisites of the various improved combat maneuver feats, as well as feats that require those improved combat maneuver feats as prerequisites.",
                group: FeatureGroup.CombatFeat
                ).SetComponents(
                new FeatureForPrerequisite() { FakeFact = Helper.ToRef<BlueprintUnitFactReference>("4c44724ffa8844f4d9bedb5bb27d144a") }, //CombatExpertiseFeature
                new FeatureForPrerequisite() { FakeFact = Helper.ToRef<BlueprintUnitFactReference>("7812ad3672a4b9a4fb894ea402095167") }, //ImprovedUnarmedStrike
                new ReplaceStatForPrerequisites() { OldStat = StatType.Dexterity, SpecificNumber = 13, Policy = ReplaceStatForPrerequisites.StatReplacementPolicy.SpecificNumber },
                new ReplaceStatForPrerequisites() { OldStat = StatType.Intelligence, SpecificNumber = 13, Policy = ReplaceStatForPrerequisites.StatReplacementPolicy.SpecificNumber },
                new DirtyFightingBonus(),
                Helper.CreateAddFacts(DirtyFightingBonus.List.Select(s => s.Key.ToRef<BlueprintUnitFactReference>()))
                );

            Helper.AddCombatFeat(feat);
        }

        [PatchInfo(Severity.Create | Severity.WIP | Severity.Hidden, "Poisons", "WIP", true)]
        public static void CreatePoison()
        {
            // Ability "Coat Weapon" -> applies Enchantment with fixed DC, stickiness -> on RuleDealDamage apply poison buff (bonus DC if already poisoned)

            /*
            Venom Speaker
            Source Heroes of Golarion pg. 26
            Talent Link Link
            Element universal; Type utility (Su); Level 1; Burn 0
            You gain the investigator’s poison lore class feature, using your kineticist level as your investigator level, and can use your gather power ability even while holding a dose of poison in one of your hands or appendages as long as you could otherwise use that ability. If you are at least 6th level, you can learn the alchemist’s swift poisoning class feature or one of the following alchemist discoveries in place of a utility wild talent, using your kineticist level as your alchemist level: concentrate poison, poison conversion, or sticky poison.

            Poison Lore (Ex): An investigator has a deep understanding and appreciation for poisons. At 2nd level, he cannot accidentally poison himself when applying poison to a weapon. If the investigator spends 1 minute physically examining the poison, he can attempt a Knowledge (nature) check to identify any natural poison or Knowledge (arcana) check to identify any magical poison (DC = the poison’s saving throw DC). Lastly, once a poison is identified, he can spend 1 minute and attempt a Craft (alchemy) check (DC = the poison’s saving throw DC) to neutralize 1 dose of the poison. Success renders the dose harmless. The investigator has no chance of accidentally poisoning himself when examining or attempting to neutralize a poison.

            Concentrate poison (Advanced Player's Guide pg. 29): The alchemist can combine two doses of the same poison to increase their effects. This requires two doses of the poison and 1 minute of concentration. When completed, the alchemist has one dose of poison. The poison's frequency is extended by 50% and the save DC increases by +2. This poison must be used within 1 hour of its creation or it is ruined.
            Poison Conversion (Ultimate Combat pg. 24): By spending 1 minute, the alchemist can convert 1 dose of poison from its current type (contact, ingested, inhaled, or injury) to another type. For example, the alchemist can convert a dose of Small centipede poison (an injury poison) to an inhaled poison. This process requires an alchemy lab. An alchemist must be at least 6th level before selecting this discovery.
            Sticky poison (Advanced Player's Guide pg. 31): Any poison the alchemist creates is sticky—when the alchemist applies it to a weapon, the weapon remains poisoned for a number of strikes equal to the alchemist's Intelligence modifier. An alchemist must be at least 6th level before selecting this discovery.
            Swift Poison (Ex) Benefit: A rogue with this talent can apply poison to a weapon as a move action, instead of a standard action.
            Quick Poison (Ex) Prerequisites: Advanced Rogue Talents, Swift Poison; Benefit: A rogue with this talent may apply poison to a weapon as a swift action.

             */

            (string displayName, int dc, StatType statType, DiceFormula damage, int ticks, int successfullSaves)[] poisons = {
                ("Deathblade", 20, StatType.Constitution, new DiceFormula(1, DiceType.D3), 6, 2),
                ("Wyvern Poison", 17, StatType.Constitution, new DiceFormula(1, DiceType.D4), 6, 1),
                ("Bluetip Eurypterid Poison", 16, StatType.Constitution, new DiceFormula(1, DiceType.D4), 6, 2),
                ("Common Eurypterid Poison", 12, StatType.Constitution, new DiceFormula(1, DiceType.D2), 4, 1),

                ("Giant Wasp Poison", 18, StatType.Dexterity, new DiceFormula(1, DiceType.D2), 6, 1),
                ("Blood Marsh Spider Venom", 14, StatType.Dexterity, new DiceFormula(1, DiceType.D4), 6 ,2), // confused
                ("Cockatrice Spit", 12, StatType.Dexterity, new DiceFormula(1, DiceType.D2), 6, 1), // petrified at 0 dex
                
                ("Dragon Bile", 26, StatType.Strength, new DiceFormula(1, DiceType.D3), 6, 6),
                ("Purple Worm Poison", 24, StatType.Strength, new DiceFormula(1, DiceType.D3), 6, 2),
                ("Large Scorpion Venom", 17, StatType.Strength, new DiceFormula(1, DiceType.D2), 6, 1),

                ("Glass Urchin Venom", 16, StatType.Wisdom, new DiceFormula(1, DiceType.D4), 6, 2),
                ("Hag Spit", 16, StatType.Wisdom, new DiceFormula(1, DiceType.D4), 6, 2), // blindness

                ("Tongue Twist", 16, StatType.Intelligence, new DiceFormula(1, DiceType.D2), 6, 2),
            };

            var sfx = Helper.GetPrefabLink("fbf39991ad3f5ef4cb81868bb9419bff");
            var list = new List<BlueprintAbility>();
            foreach (var poison in poisons)
            {
                string name = poison.displayName.Replace(" ", "");

                var buff = Helper.CreateBlueprintBuff(
                    name,
                    poison.displayName,
                    $"This creature got poisoned with {poison.displayName}."
                    ).SetComponents(
                    new BuffPoisonStatDamage() { Descriptor = ModifierDescriptor.UntypedStackable, Stat = poison.statType, Value = poison.damage, Ticks = poison.ticks, SuccesfullSaves = poison.successfullSaves, SaveType = SavingThrowType.Fortitude },
                    Helper.CreateContextSetAbilityParams(dc: poison.dc, add10toDC: false)
                    );
                buff.Stacking = StackingType.Poison;
                buff.FxOnStart = sfx;

                var ab = Helper.CreateBlueprintAbility(
                    "PoisonUse_" + name,
                    "Coat Weapon: " + poison.displayName,
                    "Apply poison an ally's currently equipped weapons or up to 30 projectiles.",
                    null,
                    AbilityType.Extraordinary,
                    UnitCommand.CommandType.Move,
                    AbilityRange.Touch
                    ).SetComponents(
                    Helper.CreateAbilityEffectRunAction(actions: new ContextActionCoatWeapon(buff))
                    ).TargetAlly();
                list.Add(ab);
            }

            var parent = Helper.CreateBlueprintAbility(
                   "PoisonUse_Parent",
                   "Coat weapons with poison",
                   "Apply poison an ally's currently equipped weapons or up to 30 projectiles.",
                   null,
                   AbilityType.Extraordinary,
                   UnitCommand.CommandType.Move,
                   AbilityRange.Touch
                   ).AddToAbilityVariants(list);

            CodexLib.PoisonEnchantment.Create();

        }

        [PatchInfo(Severity.Create, "Spell Perfection", "basic feat: Spell Perfection", false)]
        public static void CreateSpellPerfection()
        {
            var specialization = Helper.Get<BlueprintParametrizedFeature>("f327a765a4353d04f872482ef3e48c35"); //SpellSpecializationFirst

            Main.Patch(typeof(Patch_SpellPerfection));

            setValue(ref Helper.Get<BlueprintFeature>("16fa59cc9a72a6043b566b49184f53fe").GetComponent<SpellFocusParametrized>().Descriptor); //SpellFocus
            setValue(ref Helper.Get<BlueprintFeature>("5b04b45b228461c43bad768eb0f7c7bf").GetComponent<SpellFocusParametrized>().Descriptor); //SpellFocusGreater

            setValue(ref Helper.Get<BlueprintFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e").GetComponent<WeaponFocusParametrized>().Descriptor); //WeaponFocus
            setValue(ref Helper.Get<BlueprintFeature>("09c9e82965fb4334b984a1e9df3bd088").GetComponent<WeaponFocusParametrized>().Descriptor); //WeaponFocusGreater

            setValue(ref Helper.Get<BlueprintFeature>("ee7dc126939e4d9438357fbd5980d459").GetComponent<SpellPenetrationBonus>().Descriptor); //SpellPenetration
            setValue(ref Helper.Get<BlueprintFeature>("1978c3f91cfbbc24b9c9b0d017f4beec").GetComponent<SpellPenetrationBonus>().Descriptor); //GreaterSpellPenetration

            setValue(ref Helper.Get<BlueprintFeature>("52135eada006e9045a848cd659749608").GetComponent<IncreaseSpellDescriptorDC>().ModifierDescriptor); //ElementalFocusAcid
            setValue(ref Helper.Get<BlueprintFeature>("49926dc94aca16145b6a608277b6f31c").GetComponent<IncreaseSpellDescriptorDC>().ModifierDescriptor); //GreaterElementalFocusAcid
            setValue(ref Helper.Get<BlueprintFeature>("2ed9d8bf76412ba4a8afe38fa9925fca").GetComponent<IncreaseSpellDescriptorDC>().ModifierDescriptor); //ElementalFocusCold
            setValue(ref Helper.Get<BlueprintFeature>("f37a210a77d769c4ea2b23c22c07b83a").GetComponent<IncreaseSpellDescriptorDC>().ModifierDescriptor); //GreaterElementalFocusCold
            setValue(ref Helper.Get<BlueprintFeature>("13bdf8d542811ac4ca228a53aa108145").GetComponent<IncreaseSpellDescriptorDC>().ModifierDescriptor); //ElementalFocusFire
            setValue(ref Helper.Get<BlueprintFeature>("7a722c3e782aa5349a867c3516a2a4cf").GetComponent<IncreaseSpellDescriptorDC>().ModifierDescriptor); //GreaterElementalFocusFire
            setValue(ref Helper.Get<BlueprintFeature>("d439691f37d17804890bd9c263ae1e80").GetComponent<IncreaseSpellDescriptorDC>().ModifierDescriptor); //ElementalFocusElectricity
            setValue(ref Helper.Get<BlueprintFeature>("6a3be3df06f555d44a2b9dbfbcc2df23").GetComponent<IncreaseSpellDescriptorDC>().ModifierDescriptor); //GreaterElementalFocusElectricity

            var feat = Helper.CreateBlueprintParametrizedFeature(
                "SpellPerfection",
                "Spell Perfection",
                "You are unequaled at the casting of one particular spell.\nBenefit: Pick one spell which you have the ability to cast. Whenever you cast that spell you may apply any one metamagic feat you have to that spell without affecting its level or casting time, as long as the total modified level of the spell does not use a spell slot above 9th level. In addition, if you have other feats which allow you to apply a set numerical bonus to any aspect of this spell (such as Spell Focus, Spell Penetration, Weapon Focus [ray], and so on), double the bonus granted by that feat when applied to this spell.",
                onlyKnownSpells: true
                ).SetComponents(
                new MetamagicReduceCostParametrized(reduceByMostExpensive: true),
                new SpellPerfection(),
                Helper.CreatePrerequisiteStatValue(StatType.SkillKnowledgeArcana, 15)
                );

            Main.RunLast("Spell Perfection", () => 
            {
                feat.AddComponents(
                    Helper.CreatePrerequisiteFeaturesFromList(BpCache.Get<BlueprintFeature>().Where(w => w.GetComponent<AddMetamagicFeat>() && w.IsClassFeature).ToAny(), 3)
                    );
            });

            Helper.AddFeats(feat);
            return;

            void setValue(ref ModifierDescriptor descriptor)
            {
                if (descriptor == ModifierDescriptor.UntypedStackable)
                    descriptor = ModifierDescriptor.Feat;
            }
        }

        [PatchInfo(Severity.Create, "Opportune Parry", "combat feat: duelist parry by expending Attack of Opportunities", true)]
        public static void CreateOpportuneParry()
        {
            var feat = Helper.CreateBlueprintFeature(
                "OpportuneParry",
                "Opportune Parry",
                "You learn how to forgo the opportunity to strike to instead block attacks against you.\nBenefit: You parry as a free action, but need to expend one of your own potential attacks of opportunity. You cannot parry, if you are unable to do attacks of opportunity."
                ).SetComponents(
                new AddMechanicFeatureCustom(MechanicFeature.ParryUseAttackOfOpportunity),
                Helper.CreatePrerequisiteFeature("47e9ac1bf9c376e44b64cb5844a5e6a6") //DuelistParryFeature
                );

            Helper.AddCombatFeat(feat);

            Main.Patch(typeof(Patch_DuelistParry));
        }

        [PatchInfo(Severity.Create, "Kitsune Foxfire", "magical tail also grants Foxfire Bolt", true)]
        public static void CreateKitsuneFoxfire()
        {
            var magictail = Helper.Get<BlueprintFeature>("5114829572da5a04f896a8c5b67be413"); //MagicalTail1
            var firebolt = Helper.Get<BlueprintAbility>("4ecdf240d81533f47a5279f5075296b9"); //FireDomainBaseAbility

            AnyRef resource = Helper.CreateBlueprintAbilityResource(
                "KitsuneFoxfireResource",
                baseValue: 2,
                stat: StatType.Charisma
                );

            AnyRef foxfire_ab = firebolt.Clone(
                "KitsuneFoxfireAbility"
                ).SetUIData(
                "Foxfire Bolt",
                "As a standard action, you can unleash a scorching bolt of foxfire from your outstretched hand, and target any single foe within 30 feet as a ranged touch attack. If you hit the foe, the foxfire bolt deals 1d6 points of fire damage + 1 point for every two character levels you possess. You can use this ability a number of times er day equal to 2 + your Charisma modifier + once for each magical tail feat you possess."
                );

            foxfire_ab.GetComponent<AbilityResourceLogic>().m_RequiredResource = resource;
            foxfire_ab.GetComponent<ContextRankConfig>().m_BaseValueType = ContextRankBaseValueType.CharacterLevel;

            magictail.AddComponents(
                Helper.CreateAddAbilityResources(resource),
                new IncreaseResourceAmountPlus(resource, 1, magictail, "c032f65c0bd9f6048a927fb07fc0195d", "d5050e13742d9b64da20921aaf7c2b2a", "342b6aed6b2eaab4786de243f0bcbcb8", "044cd84818c36854abf61064ade542a1", "053e37697a0d20547b06c3dbd8b71702", "041f91c25586d48469dce6b4575053f6", "df186ef345849d149bdbf4ddb45aee35")
                );

            magictail.m_Description.CreateString(magictail.m_Description + "\nAdditionally, as a standard action, you can unleash a scorching bolt of foxfire from your outstretched hand, and target any single foe within 30 feet as a ranged touch attack. If you hit the foe, the foxfire bolt deals 1d6 points of fire damage + 1 point for every two character levels you possess. You can use this ability a number of times er day equal to 2 + your Charisma modifier + once for each magical tail feat you possess.");
            Helper.AppendAndReplace(ref magictail.GetComponent<AddFacts>().m_Facts, foxfire_ab);
        }

        #region General Resources and Stuff

        public static void CreatePropertyMaxMentalAttribute()
        {
            var prop = Helper.CreateBlueprintUnitProperty(
                "MaxMentalAttributePropertyGetter"
                ).SetComponents(new PropertyAttributeMax() { PhysicalStat = false, MentalStat = true });

            Resource.Cache.PropertyMaxMentalAttribute.SetReference(prop);
        }

        public static void CreatePropertyGetterSneakAttack()
        {
            var prop = Helper.CreateBlueprintUnitProperty(
                "SneakAttackPropertyGetter"
                ).SetComponents(new PropertyGetterSneakAttack());

            Resource.Cache.PropertySneakAttackDice.SetReference(prop);
        }

        public static void CreateMythicDispelProperty()
        {
            var prop = Helper.CreateBlueprintUnitProperty(
                "MythicDispelPropertyGetter"
                ).SetComponents(new PropertyMythicLevel()
                {
                    Fact = Helper.ToRef<BlueprintUnitFactReference>("51b6b22ff184eef46a675449e837365d"),   //SpellPenetrationMythicFeat
                    Greater = Helper.ToRef<BlueprintUnitFactReference>("1978c3f91cfbbc24b9c9b0d017f4beec") //GreaterSpellPenetration
                });

            Resource.Cache.PropertyMythicDispel.SetReference(prop);
        }

        #endregion

    }
}
