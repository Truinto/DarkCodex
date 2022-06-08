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
                blueprints: null,
                requireKnown: true
                ).SetComponents(
                new AbilityFocusParametrized()
                );
            feat.RequireProficiency = true;

            Resource.Cache.Ensure();
            var list = new List<AnyBlueprintReference>();

            foreach (var ab in Resource.Cache.Ability)
            {
                if (ab.Type == AbilityType.Spell
                    || ab.m_DisplayName == null
                    || ab.m_DisplayName.IsEmpty()
                    || ab.HasVariants)
                    continue;
                var run = ab.GetComponent<AbilityEffectRunAction>();
                if (run == null || run.SavingThrowType == SavingThrowType.Unknown)
                    continue;

                list.Add(ab.ToReference<AnyBlueprintReference>());
            }

            foreach (var ft in Resource.Cache.Feature)
            {
                if (ft.m_DisplayName == null
                    || ft.m_DisplayName.IsEmpty()
                    || ft.GetComponent<ContextCalculateAbilityParams>() == null)
                    continue;

                list.Add(ft.ToReference<AnyBlueprintReference>());
            }

            feat.BlueprintParameterVariants = list.ToArray();
#if DEBUG
            Helper.AddFeats(feat); // TODO: bugfix ability focus
#endif
        }

        [PatchInfo(Severity.Extend, "Empower Angels Light", "'Light of the Angels' give temporary HP equal to character level", true)]
        public static void PatchAngelsLight()
        {
            var angelbuff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("e173dc1eedf4e344da226ffbd4d76c60"); // AngelMinorAbilityEffectBuff

            var temphp = angelbuff.GetComponent<TemporaryHitPointsFromAbilityValue>();
            temphp.Value = Helper.CreateContextValue(AbilityRankType.Default);
            angelbuff.AddComponents(Helper.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, type: AbilityRankType.Default)); // see FalseLifeBuff
        }

        [PatchInfo(Severity.Extend | Severity.DefaultOff, "Basic Freebie Feats", "reduced feat tax, inspired from https://michaeliantorno.com/feat-taxes-in-pathfinder/", true)]
        public static void PatchBasicFreebieFeats()
        {
            var basics = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("5b72dd2ca2cb73b49903806ee8986325"); //BasicFeatsProgression
            basics.AddComponents(
                Helper.CreateAddFactOnlyParty(Helper.ToRef<BlueprintUnitFactReference>("9972f33f977fc724c838e59641b2fca5")), //PowerAttackFeature
                Helper.CreateAddFactOnlyParty(Helper.ToRef<BlueprintUnitFactReference>("0da0c194d6e1d43419eb8d990b28e0ab")), //PointBlankShot
                Helper.CreateAddFactOnlyParty(Helper.ToRef<BlueprintUnitFactReference>("4c44724ffa8844f4d9bedb5bb27d144a")), //CombatExpertiseFeature
                Helper.CreateAddFactOnlyParty(Helper.ToRef<BlueprintUnitFactReference>("90e54424d682d104ab36436bd527af09")), //WeaponFinesse
                Helper.CreateAddFactOnlyParty(Helper.ToRef<BlueprintUnitFactReference>("f47df34d53f8c904f9981a3ee8e84892")) //DeadlyAimFeature
                );

            var powerattack = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>("a7b339e4f6ff93a4697df5d7a87ff619"); //PowerAttackToggleAbility
            powerattack.IsOnByDefault = false;
            powerattack.DoNotTurnOffOnRest = true;
            var combatexpertise = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>("a75f33b4ff41fc846acbac75d1a88442"); //CombatExpertiseToggleAbility
            combatexpertise.IsOnByDefault = false;
            combatexpertise.DoNotTurnOffOnRest = true;
            combatexpertise.DeactivateIfCombatEnded = false;
            combatexpertise.DeactivateAfterFirstRound = false;
            combatexpertise.ActivationType = AbilityActivationType.Immediately;
            combatexpertise.DeactivateIfOwnerDisabled = true;
            var deadlyaim = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>("ccde5ab6edb84f346a74c17ea3e3a70c"); //DeadlyAimToggleAbility
            deadlyaim.IsOnByDefault = false;
            deadlyaim.DoNotTurnOffOnRest = true;

            var mobility = Helper.ToRef<BlueprintUnitFactReference>("2a6091b97ad940943b46262600eaeaeb"); //Mobility
            var dodge = Helper.ToRef<BlueprintUnitFactReference>("97e216dbb46ae3c4faef90cf6bbe6fd5"); //Dodge
            mobility.Get().AddComponents(Helper.CreateAddFactOnlyParty(dodge));
            dodge.Get().AddComponents(Helper.CreateAddFactOnlyParty(mobility));

            var twf = Helper.ToRef<BlueprintUnitFactReference>("ac8aaf29054f5b74eb18f2af950e752d"); //TwoWeaponFighting
            var twfi = Helper.ToRef<BlueprintUnitFactReference>("9af88f3ed8a017b45a6837eab7437629"); //TwoWeaponFightingImproved
            var twfg = Helper.ToRef<BlueprintUnitFactReference>("c126adbdf6ddd8245bda33694cd774e8"); //TwoWeaponFightingGreater
            var multi = Helper.ToRef<BlueprintUnitFactReference>("8ac319e47057e2741b42229210eb43ed"); //Multiattack
            twf.Get().AddComponents(Helper.CreateAddFactOnlyParty(multi));
            twfi.Get().AddComponents(Helper.CreateAddFactOnlyParty(twfg));

            //Deft Maneuvers
            //ImprovedTrip.0f15c6f70d8fb2b49aa6cc24239cc5fa
            //ImprovedDisarm.25bc9c439ac44fd44ac3b1e58890916f
            //ImprovedDirtyTrick.ed699d64870044b43bb5a7fbe3f29494

            //Powerful Maneuvers
            //ImprovedBullRush.b3614622866fe7046b787a548bbd7f59
            //ImprovedSunder.9719015edcbf142409592e2cbaab7fe1
        }

        [PatchInfo(Severity.Create, "Preferred Spell", "basic feat: Preferred Spell, spontaneously cast a specific spell", false, Requirement: typeof(Patch_PreferredSpellMetamagic))]
        public static void CreatePreferredSpell()
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
                blueprints: specialization.BlueprintParameterVariants,
                requireKnown: true
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
                    var buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(guid);
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
            var buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("ee8ee3c5c8f055e48a1ec1bfb92778f1"); //PreciousTreatBuff
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
                    var ab = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(guid);
                    ab.LocalizedDuration = Resource.Strings.TenMinutes;
                    ab.Get<ContextActionApplyBuff>(f => f.DurationValue.BonusValue = 10);
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

            // add icon to CompletelyNormal metamagic
            //if (UIRoot.Instance.SpellBookColors.MetamagicCompletelyNormal == null)
            //{
            //    var cnfeat = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("094b6278f7b570f42aeaa98379f07cf2"); //CompletelyNormalSpellFeat
            //    cnfeat.m_Icon = Helper.CreateSprite("CompletelyNormal.png");
            //    UIRoot.Instance.SpellBookColors.MetamagicCompletelyNormal = cnfeat.m_Icon;
            //}

            // add confusion descriptor to song of discord
            var songDiscord = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("2e1646c2449c88a4188e58043455a43a"); //SongOfDiscordBuff
            songDiscord.GetComponent<SpellDescriptorComponent>().Descriptor |= SpellDescriptor.Confusion;

            // fix destrutive dispel scaling
            Helper.Get<BlueprintUnitProperty>("13e4f1dd08954723b173335a54b48746") //DestructiveDispelProperty
                .SetComponents(
                new PropertyAttributeMax { MentalStat = true },
                new SimplePropertyGetter { Property = UnitProperty.Level, Settings = new() { m_Progression = PropertySettings.Progression.Div2 } });
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

            foreach (var buff in Resource.Cache.Buff)
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
        }

        [PatchInfo(Severity.Create, "Backgrounds", "basic feat: Additional Traits\ntraits: Magical Lineage, Metamagic Master", false)]
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

            */
            var specialization = Helper.Get<BlueprintParametrizedFeature>("f327a765a4353d04f872482ef3e48c35"); //SpellSpecializationFirst

            var scholarMetamagic = Helper.CreateBlueprintParametrizedFeature(
                "BackgroundScholarMagicalLineage",
                "Magical Lineage",
                "One of your parents was a gifted spellcaster who not only used metamagic often, but also developed many magical items and perhaps even a new spell or two — and you have inherited a fragment of this greatness.\nBenefit: Pick one spell when you choose this trait. When you apply metamagic feats to this spell that add at least 1 level to the spell, treat its actual level as 1 lower for determining the spell's final adjusted level. Additionally your caster level gains a +2 trait bonus as long as this bonus doesn’t raise your caster level above your current Hit Dice.",
                icon: null,
                group: FeatureGroup.Trait,
                parameterType: FeatureParameterType.SpellSpecialization,
                blueprints: specialization.BlueprintParameterVariants
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
                parameterType: FeatureParameterType.SpellSpecialization,
                blueprints: specialization.BlueprintParameterVariants
                ).SetComponents(
                new MetamagicReduceCostParametrized() { Reduction = 2 }
                );

            var scholar = Helper.Get<BlueprintFeatureSelection>("273fab44409035f42a7e2af0858a463d"); //BackgroundsScholarSelection
            scholar.Add(scholarMetamagic, scholarMetamagic2);

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

            var atavism = Helper.CreateBlueprintFeatureSelection(
                "AtavismOrc",
                "Orc Atavism",
                "Some half-orcs have much stronger orc blood than human blood. Such half-orcs count as only half-orcs and orcs (not also humans) for any effect related to race. They gain a +2 bonus to Strength, a +2 bonus to one ability score of their choice, and a –2 penalty to one mental ability score of their choice. Finally, they gain the ferocity universal monster ability. This racial trait replaces the half-orc’s usual racial ability score modifiers, as well as intimidating, orc blood, and orc ferocity.",
                group: FeatureGroup.Racial
                ).SetComponents(
                Helper.CreateAddStatBonus(2, StatType.Strength, ModifierDescriptor.Racial),
                new AddMechanicsFeature { m_Feature = AddMechanicsFeature.MechanicsFeatureType.Ferocity },
                Helper.CreateRemoveFeatureOnApply("885f478dff2e39442a0f64ceea6339c9"), //Intimidating
                Helper.CreateRemoveFeatureOnApply("c99f3405d1ef79049bd90678a666e1d7") //HalfOrcFerocity
                ).Add(minusInt, minusWis, minusCha);
            orc.Add(atavism);
            Helper.AppendAndReplace(ref mythicRaces.m_Features, atavism.ToRef());
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

        public static void CreateBleedBuff()
        {
            var buff = Helper.CreateBlueprintBuff(
                "BleedVariableBuff",
                "Bleed",
                "This creature takes hit point damage each turn. Bleeding can be stopped through the application of any spell that cures hit point damage.",
                Helper.StealIcon("75039846c3d85d940aa96c249b97e562")
                ).SetComponents(
                new BleedBuff(),
                Helper.CreateSpellDescriptorComponent(SpellDescriptor.Bleed)
                );

            Resource.Cache.BuffBleed.SetReference(buff);
            ContextActionIncreaseBleed.BuffBleed.SetReference(buff);
        }

        #endregion

    }
}
