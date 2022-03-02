using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using DarkCodex.Components;
using Kingmaker.Utility;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.RuleSystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.ResourceLinks;
using Kingmaker.Blueprints.Facts;
using static Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell;
using Kingmaker.Blueprints.Items.Weapons;
using Newtonsoft.Json;
using System.IO;
using Kingmaker;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using System.Reflection;
using Kingmaker.UnitLogic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.ActivatableAbilities;
using UnityEngine;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Class.Kineticist.ActivatableAbility;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;

namespace DarkCodex
{
    public class Kineticist
    {
        public static AbilityRegister Blasts = new AbilityRegister(
            "c4b74e4448b81d04f9df89ed14c38a95",  //MetakinesisQuickenCheaperBuff
            "f690edc756b748e43bba232e0eabd004",  //MetakinesisQuickenBuff
            "b8f43f0040155c74abd1bc794dbec320",  //MetakinesisMaximizedCheaperBuff
            "870d7e67e97a68f439155bdf465ea191",  //MetakinesisMaximizedBuff
            "f8d0f7099e73c95499830ec0a93e2eeb",  //MetakinesisEmpowerCheaperBuff
            "f5f3aa17dd579ff49879923fb7bc2adb"); //MetakinesisEmpowerBuff

        [PatchInfo(Severity.Create, "Kineticist Background", "regional background: gain +1 Kineticist level for the purpose of feat prerequisites", true)]
        public static void createKineticistBackground()
        {
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391");
            //var dragon_class = Helper.ToRef<BlueprintCharacterClassReference>("01a754e7c1b7c5946ba895a5ff0faffc");

            var feature = Helper.CreateBlueprintFeature(
                "BackgroundElementalist",
                "Elemental Plane Outsider",
                "Elemental Plane Outsider count as 1 Kineticist level higher for determining prerequisites for wild talents.",
                null,
                null,
                0
                ).SetComponents(Helper.CreateClassLevelsForPrerequisites(kineticist_class, 1));

            Helper.AppendAndReplace(ref ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("fa621a249cc836f4382ca413b976e65e").m_AllFeatures, feature.ToRef());
        }

        [PatchInfo(Severity.Create, "Kineticist Extra Wild Talent", "basic feat: Extra Wild Talent", false)]
        public static void createExtraWildTalentFeat()
        {
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391");
            var infusion_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
            var wildtalent_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("5c883ae0cd6d7d5448b7a420f51f8459");

            var extra_wild_talent_selection = Helper.CreateBlueprintFeatureSelection(
                "ExtraWildTalentFeat",
                "Extra Wild Talent",
                "You gain a wild talent for which you meet the prerequisites. You can select an infusion or a non-infusion wild talent, but not a blast or defense wild talent.\nSpecial: You can take this feat multiple times. Each time, you must choose a different wild talent.",
                null,
                ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("42f96fc8d6c80784194262e51b0a1d25").Icon, //ExtraArcanePool.Icon
                FeatureGroup.Feat
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 1, true)
                );
            extra_wild_talent_selection.Ranks = 10;

            extra_wild_talent_selection.m_AllFeatures = Helper.Append(infusion_selection.m_AllFeatures,     //InfusionSelection
                                                                    wildtalent_selection.m_AllFeatures);  //+WildTalentSelection

            Helper.AddFeats(extra_wild_talent_selection);
        }

        [PatchInfo(Severity.Create, "Whip Infusion", "infusion: Kinetic Whip, expands Kinetic Knight", false, Requirement: typeof(Patch_KineticistAllowOpportunityAttack))]
        public static void createWhipInfusion()
        {
            var infusion_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
            var blade = Helper.ToRef<BlueprintFeatureReference>("9ff81732daddb174aa8138ad1297c787"); //KineticBladeInfusion
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391"); //KineticistClass
            var knight = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("7d61d9b2250260a45b18c5634524a8fb");

            var applicable = Blasts.Where(g => g.Get().name.StartsWith("KineticBlade")).ToArray();
            Helper.PrintDebug(applicable.Select(s => s.NameSafe()).Join());
            var icon = Helper.StealIcon("0e5ec4d781678234f83118df41fd27c3");

            var ability = Helper.CreateBlueprintActivatableAbility(
                "KineticWhipActivatable",
                "Kinetic Whip",
                "Element: universal\nType: form infusion\nLevel: 3\nBurn: 2\nAssociated Blasts: any\nSaving Throw: none\nYou form a long tendril of energy or elemental matter. While active, your kinetic blade increases its reach by 5 feet and you can make attacks of opportunity with your kinetic blade.",
                out BlueprintBuff buff,
                icon: icon
                ).SetComponents(
                new RestrictionKineticWhip()
                );

            buff.Flags(hidden: true);
            buff.SetComponents(
                new AddKineticistBurnModifier() { BurnType = KineticistBurnType.Infusion, Value = 1, m_AppliableTo = applicable },
                Helper.CreateAddStatBonus(5, StatType.Reach)
                );

            var whip = Helper.CreateBlueprintFeature(
                "KineticWhipInfusion",
                ability.m_DisplayName,
                ability.m_Description,
                icon: icon,
                group: FeatureGroup.KineticBlastInfusion
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 6),
                Helper.CreatePrerequisiteFeature(blade),
                Helper.CreateAddFacts(ability.ToRef())
                );

            // kinetic knight also gets bonuses with whips
            var maneuver = Helper.CreateBlueprintFeature(
                "KineticKnightManeuverBonus",
                "",
                ""
                ).SetComponents(
                new ManeuverBonus() { Type = CombatManeuver.Trip, Bonus = 4 },
                new ManeuverBonus() { Type = CombatManeuver.Disarm, Bonus = 4 }
                );
            maneuver.HideInUI = true;

            Helper.AppendAndReplace(ref infusion_selection.m_AllFeatures, whip.ToRef());
            knight.AddFeature(5, whip);
            knight.AddFeature(5, maneuver);

            Resource.Cache.BuffKineticWhip.SetReference(buff);
        }

        [PatchInfo(Severity.Create, "Blade Rush Infusion", "infusion: Blade Rush, expands Kinetic Knight", false)]
        public static void createBladeRushInfusion()
        {
            var knight = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("7d61d9b2250260a45b18c5634524a8fb");
            var infusion_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
            var demoncharge = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("1b677ed598d47a048a0f6b4b671b8f84"); //DemonChargeMainAbility
            var blade = Helper.ToRef<BlueprintFeatureReference>("9ff81732daddb174aa8138ad1297c787"); //KineticBladeInfusion
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391"); //KineticistClass
            var dimensiondoor = Helper.ToRef<BlueprintAbilityReference>("a9b8be9b87865744382f7c64e599aeb2"); //DimensionDoorCasterOnly
            var flashstep = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("e10424c1afe70cb4384090f4dab8d437"); //StormwalkerFlashStepAbility

            string name = "Blade Rush";
            string description = "Element: universal\nType: form infusion\nLevel: 2\nBurn: 2\nAssociated Blasts: any\nSaving Throw: none\nYou use your element’s power to instantly move 30 feet, manifest a kinetic blade, and attack once. You gain a +2 bonus on the attack roll and take a –2 penalty to your AC until the start of your next turn. The movement doesn’t provoke attacks of opportunity, though activating blade rush does.";
            Sprite icon = Helper.StealIcon("4c349361d720e844e846ad8c19959b1e");

            var buff = Helper.CreateBlueprintBuff(
                "KineticBladeRushBuff",
                "KineticBladeRushBuff",
                ""
                ).SetComponents(
                Helper.CreateAddFactContextActions(on: Helper.CreateContextActionMeleeAttack(true).ObjToArray())
                ).Flags(hidden: true);

            var ability = Helper.CreateBlueprintAbility(
                "KineticBladeRushAbility",
                name,
                description,
                null,
                icon,
                AbilityType.SpellLike,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close
                ).SetComponents(
                Helper.CreateAbilityExecuteActionOnCast(
                    Helper.CreateContextActionApplyBuff(BlueprintRoot.Instance.SystemMechanics.ChargeBuff, 1, toCaster: true)
                    //Helper.CreateContextActionApplyBuff(buff, 3f, toCaster: true)
                    //Helper.CreateContextActionCastSpell(dimensiondoor)
                    //new ContextActionMeleeAttackPoint()
                    //Helper.CreateContextActionMeleeAttack(true)
                    ),
                //demoncharge.GetComponent<AbilityCustomTeleportation>(),
                flashstep.GetComponent<AbilityCustomFlashStep>(),
                Step5_burn(null, 1),
                new RestrictionCanGatherPowerAbility()
                ).TargetPoint();
            ability.AvailableMetamagic = Metamagic.Quicken;

            var rush = Helper.CreateBlueprintFeature(
                "KineticBladeRush",
                name,
                description,
                null,
                icon,
                FeatureGroup.KineticBlastInfusion
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 4),
                Helper.CreatePrerequisiteFeature(blade),
                Helper.CreateAddFacts(ability.ToRef2())
                );

            var quickblade = Helper.CreateBlueprintFeature(
                "KineticKnightQuickenBladeRush",
                "Blade Rush — Quicken",
                "At 13th level as a swift action, a Kinetic Knight can accept 2 points of burn to unleash a kinetic blast with the blade rush infusion."
                ).SetComponents(
                Helper.CreateAutoMetamagic(Metamagic.Quicken, new List<BlueprintAbilityReference>() { ability.ToRef() }, AutoMetamagic.AllowedType.KineticistBlast)
                );

            Helper.AppendAndReplace(ref infusion_selection.m_AllFeatures, rush.ToRef());
            knight.AddFeature(13, quickblade);
            Blasts.Add(ability);
        }

        [PatchInfo(Severity.Create, "Mobile Gathering", "basic feat: Mobile Gathering", false)]
        public static void createMobileGatheringFeat()
        {
            // --- base game stuff ---
            var buff1 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("e6b8b31e1f8c524458dc62e8a763cfb1");   //GatherPowerBuffI
            var buff2 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("3a2bfdc8bf74c5c4aafb97591f6e4282");   //GatherPowerBuffII
            var buff3 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("82eb0c274eddd8849bb89a8e6dbc65f8");   //GatherPowerBuffIII
            var gather_original_ab = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("6dcbffb8012ba2a4cb4ac374a33e2d9a");    //GatherPower
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391");

            // rename buffs, so it's easier to tell them apart
            buff1.m_Icon = gather_original_ab.Icon;
            buff1.m_DisplayName = Helper.CreateString(buff1.m_DisplayName + " Lv1");
            buff2.m_Icon = gather_original_ab.Icon;
            buff2.m_DisplayName = Helper.CreateString(buff2.m_DisplayName + " Lv2");
            buff3.m_Icon = gather_original_ab.Icon;
            buff3.m_DisplayName = Helper.CreateString(buff3.m_DisplayName + " Lv3");

            // new buff that halves movement speed, disallows normal gathering
            var mobile_debuff = Helper.CreateBlueprintBuff(
                "MobileGatheringDebuff",
                "Mobile Gathering Debuff",
                "Your movement speed is halved after gathering power.",
                null,
                Helper.CreateSprite("GatherMobileHigh.png"),
                null
                ).SetComponents(
                new TurnBasedBuffMovementSpeed(multiplier: 0.5f));

            var apply_debuff = Helper.CreateContextActionApplyBuff(mobile_debuff, 1);
            var can_gather = Helper.CreateAbilityRequirementHasBuffTimed(CompareType.LessOrEqual, 1.Rounds().Seconds, buff1, buff2, buff3);

            // cannot use usual gathering after used mobile gathering
            gather_original_ab.AddComponents(Helper.CreateAbilityRequirementHasBuffs(true, mobile_debuff));

            // ability as free action that applies buff and 1 level of gatherpower
            // - increases gather power by 1 level, similiar to GatherPower:6dcbffb8012ba2a4cb4ac374a33e2d9a
            // - applies debuff
            // - get same restriction as usual gathering
            var three2three = Helper.CreateConditional(Helper.CreateContextConditionHasBuff(buff3), Helper.CreateContextActionApplyBuff(buff3, 2));
            var two2three = Helper.CreateConditional(Helper.CreateContextConditionHasBuff(buff2).ObjToArray(), new GameAction[] { Helper.CreateContextActionRemoveBuff(buff2), Helper.CreateContextActionApplyBuff(buff3, 2) });
            var one2two = Helper.CreateConditional(Helper.CreateContextConditionHasBuff(buff1).ObjToArray(), new GameAction[] { Helper.CreateContextActionRemoveBuff(buff1), Helper.CreateContextActionApplyBuff(buff2, 2) });
            var zero2one = Helper.CreateConditional(Helper.MakeConditionHasNoBuff(buff1, buff2, buff3), new GameAction[] { Helper.CreateContextActionApplyBuff(buff1, 2) });
            var regain_halfmove = new ContextActionUndoAction(command: UnitCommand.CommandType.Move, amount: 1.5f);
            var mobile_gathering_short_ab = Helper.CreateBlueprintAbility(
                "MobileGatheringShort",
                "Mobile Gathering (Move Action)",
                "You may move up to half your normal speed while gathering power.",
                null,
                Helper.CreateSprite("GatherMobileLow.png"),
                AbilityType.Special,
                UnitCommand.CommandType.Move,
                AbilityRange.Personal,
                null,
                null
                ).SetComponents(
                can_gather,
                Helper.CreateAbilityEffectRunAction(0, regain_halfmove, apply_debuff, three2three, two2three, one2two, zero2one),
                new RestrictionCanGatherPowerAbility());
            mobile_gathering_short_ab.CanTargetSelf = true;
            mobile_gathering_short_ab.Animation = CastAnimationStyle.Self;//UnitAnimationActionCastSpell.CastAnimationStyle.Kineticist;
            mobile_gathering_short_ab.HasFastAnimation = true;

            // same as above but standard action and 2 levels of gatherpower
            var one2three = Helper.CreateConditional(Helper.CreateContextConditionHasBuff(buff1).ObjToArray(), new GameAction[] { Helper.CreateContextActionRemoveBuff(buff1), Helper.CreateContextActionApplyBuff(buff3, 2) });
            var zero2two = Helper.CreateConditional(Helper.MakeConditionHasNoBuff(buff1, buff2, buff3), new GameAction[] { Helper.CreateContextActionApplyBuff(buff2, 2) });
            var hasMoveAction = Helper.CreateAbilityRequirementActionAvailable(false, ActionType.Move, 6f);
            var lose_halfmove = new ContextActionUndoAction(command: UnitCommand.CommandType.Move, amount: -1.5f);
            var mobile_gathering_long_ab = Helper.CreateBlueprintAbility(
                "MobileGatheringLong",
                "Mobile Gathering (Full Round)",
                "You may move up to half your normal speed while gathering power.",
                null,
                Helper.CreateSprite("GatherMobileMedium.png"),
                AbilityType.Special,
                UnitCommand.CommandType.Standard,
                AbilityRange.Personal,
                null,
                null
                ).SetComponents(
                can_gather,
                hasMoveAction,
                Helper.CreateAbilityEffectRunAction(0, lose_halfmove, apply_debuff, three2three, two2three, one2three, zero2two),
                new RestrictionCanGatherPowerAbility());
            mobile_gathering_long_ab.CanTargetSelf = true;
            mobile_gathering_long_ab.Animation = CastAnimationStyle.Self;
            mobile_gathering_long_ab.HasFastAnimation = true;

            var mobile_gathering_feat = Helper.CreateBlueprintFeature(
                "MobileGatheringFeat",
                "Mobile Gathering",
                "While gathering power, you can move up to half your normal speed. This movement provokes attacks of opportunity as normal.",
                null,
                mobile_debuff.Icon,
                FeatureGroup.Feat
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 7, true),
                Helper.CreateAddFacts(mobile_gathering_short_ab.ToRef2(), mobile_gathering_long_ab.ToRef2()));
            mobile_gathering_feat.Ranks = 1;

            Helper.AddFeats(mobile_gathering_feat);

            // make original gather ability visible for manual gathering and allow to extend buff3
            Helper.AppendAndReplace(ref gather_original_ab.GetComponent<AbilityEffectRunAction>().Actions.Actions, three2three);

        }

        [PatchInfo(Severity.Create, "Impale Infusion", "infusion: Impale", false)]
        public static void createImpaleInfusion()
        {
            var infusion_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391");
            var weapon = Helper.ToRef<BlueprintItemWeaponReference>("65951e1195848844b8ab8f46d942f6e8");
            var icon = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("2aad85320d0751340a0786de073ee3d5").Icon; //TorrentInfusionFeature

            var earth_base = Helper.ToRef<BlueprintAbilityReference>("e53f34fb268a7964caf1566afb82dadd");   //EarthBlastBase
            var earth_blast = Helper.ToRef<BlueprintFeatureReference>("7f5f82c1108b961459c9884a0fa0f5c4");    //EarthBlastFeature

            var metal_base = Helper.ToRef<BlueprintAbilityReference>("6276881783962284ea93298c1fe54c48");   //MetalBlastBase
            var metal_blast = Helper.ToRef<BlueprintFeatureReference>("ad20bc4e586278c4996d4a81b2448998");    //MetalBlastFeature

            var ice_base = Helper.ToRef<BlueprintAbilityReference>("403bcf42f08ca70498432cf62abee434");   //IceBlastBase
            var ice_blast = Helper.ToRef<BlueprintFeatureReference>("a8cc34ca1a5e55a4e8aa5394efe2678e");    //IceBlastFeature


            // impale feat
            BlueprintFeature impale_feat = Helper.CreateBlueprintFeature(
                "InfusionImpaleFeature",
                "Impale",
                "Element: earth\nType: form infusion\nLevel: 3\nBurn: 2\nAssociated Blasts: earth, metal, ice\n"
                + "You extend a long, sharp spike of elemental matter along a line, impaling multiple foes. Make a single attack roll against each creature or object in a 30-foot line.",
                null,
                icon,
                FeatureGroup.KineticBlastInfusion
                ).SetComponents(
                Helper.CreatePrerequisiteFeaturesFromList(true, earth_blast, metal_blast, ice_blast),
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 6)
                );

            // earth ability
            var step1 = Step1_run_damage(p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: true, half: false);
            var earth_impale_ab = Helper.CreateBlueprintAbility(
                "ImpaleEarthBlastAbility",
                impale_feat.m_DisplayName,
                impale_feat.m_Description,
                null,
                icon,
                AbilityType.SpellLike,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close,
                duration: null,
                savingThrow: null
                ).SetComponents(
                step1,
                Step2_rank_dice(twice: false),
                Step3_rank_bonus(half_bonus: false),
                step4_dc(),
                Step5_burn(step1.Actions.Actions, infusion: 2, blast: 0),
                Step6_feat(impale_feat),
                Step7_projectile(Resource.Projectile.Kinetic_EarthBlastLine00, true, AbilityProjectileType.Line, 30, 5),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetPoint(CastAnimationStyle.Kineticist);
            var attack = Helper.CreateConditional(new ContextConditionAttackRoll(weapon));
            attack.IfTrue = step1.Actions;
            step1.Actions = Helper.CreateActionList(attack);

            // metal ability
            step1 = Step1_run_damage(p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: true, half: false);
            var metal_impale_ab = Helper.CreateBlueprintAbility(
                "ImpaleMetalBlastAbility",
                impale_feat.m_DisplayName,
                impale_feat.m_Description,
                null,
                icon,
                AbilityType.SpellLike,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close,
                duration: null,
                savingThrow: null
                ).SetComponents(
                step1,
                Step2_rank_dice(twice: true),
                Step3_rank_bonus(half_bonus: false),
                step4_dc(),
                Step5_burn(step1.Actions.Actions, infusion: 2, blast: 2),
                Step6_feat(impale_feat),
                Step7_projectile(Resource.Projectile.Kinetic_MetalBlastLine00, true, AbilityProjectileType.Line, 30, 5),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetPoint(CastAnimationStyle.Kineticist);
            attack = Helper.CreateConditional(new ContextConditionAttackRoll(weapon));
            attack.IfTrue = step1.Actions;
            step1.Actions = Helper.CreateActionList(attack);

            // ice ability
            step1 = Step1_run_damage(p: PhysicalDamageForm.Piercing, e: DamageEnergyType.Cold, isAOE: true, half: false);
            var ice_impale_ab = Helper.CreateBlueprintAbility(
                "ImpaleIceBlastAbility",
                impale_feat.m_DisplayName,
                impale_feat.m_Description,
                null,
                icon,
                AbilityType.SpellLike,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close,
                duration: null,
                savingThrow: null
                ).SetComponents(
                step1,
                Step2_rank_dice(twice: true),
                Step3_rank_bonus(half_bonus: false),
                step4_dc(),
                Step5_burn(step1.Actions.Actions, infusion: 2, blast: 2),
                Step6_feat(impale_feat),
                Step7_projectile(Resource.Projectile.Kinetic_IceBlastLine00, true, AbilityProjectileType.Line, 30, 5),
                Step8_spell_description(SpellDescriptor.Cold),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetPoint(CastAnimationStyle.Kineticist);
            attack = Helper.CreateConditional(new ContextConditionAttackRoll(weapon));
            attack.IfTrue = step1.Actions;
            step1.Actions = Helper.CreateActionList(attack);

            // add to feats and append variants
            Helper.AppendAndReplace(ref infusion_selection.m_AllFeatures, impale_feat.ToRef());
            Helper.AddToAbilityVariants(earth_base, earth_impale_ab);
            Helper.AddToAbilityVariants(metal_base, metal_impale_ab);
            Helper.AddToAbilityVariants(ice_base, ice_impale_ab);
        }

        [PatchInfo(Severity.Extend, "Gather Power", "Kineticist Gather Power can be used manually", false, Requirement: typeof(Patch_TrueGatherPowerLevel))]
        public static void patchGatherPower()
        {
            var gather_original_ab = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("6dcbffb8012ba2a4cb4ac374a33e2d9a"); //GatherPower
            gather_original_ab.Hidden = false;
            gather_original_ab.Animation = CastAnimationStyle.SelfTouch;
            gather_original_ab.AddComponents(new RestrictionCanGatherPowerAbility());
        }

        [PatchInfo(Severity.Extend, "Demon Charge", "Demon Charge also gathers power", true)]
        public static void patchDemonCharge()
        {
            var charge = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("1b677ed598d47a048a0f6b4b671b8f84"); //DemonChargeMainAbility
            var gather = Helper.ToRef<BlueprintAbilityReference>("6dcbffb8012ba2a4cb4ac374a33e2d9a"); //GatherPower

            Helper.AppendAndReplace(ref charge.GetComponent<AbilityExecuteActionOnCast>().Actions.Actions, new ContextActionCastSpellOnCaster() { m_Spell = gather });
        }

        [PatchInfo(Severity.Extend, "Dark Elementalist QoL", "faster animation and use anywhere, but only out of combat", true)]
        public static void patchDarkElementalist()
        {
            var soulability = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("31a1e5b27cdb78f4094630610519981c"); //DarkElementalistSoulPowerAbility
            soulability.ActionType = UnitCommand.CommandType.Free;
            soulability.m_IsFullRoundAction = false;
            soulability.HasFastAnimation = true;
            var targets = soulability.GetComponent<AbilityTargetsAround>();
            targets.m_Condition.Conditions = Array.Empty<Condition>();
            soulability.AddComponents(new AbilityRequirementOnlyCombat { Not = true });
        }

        [PatchInfo(Severity.Extend, "Various Tweaks", "bowling works with sandstorm blast, apply PsychokineticistStat setting", true)]
        public static void patchVarious()
        {
            // allow bowling infusion on sandblasts
            var bowling = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("918b2524af5c3f647b5daa4f4e985411"); //BowlingInfusionBuff
            var sandstorm = Helper.ToRef<BlueprintAbilityReference>("b93e1f0540a4fa3478a6b47ae3816f32"); //SandstormBlastBase
            ExpandSubstance(bowling, sandstorm);

            // apply PsychokineticistStat setting
            var pstat = Settings.StateManager.State.PsychokineticistStat;
            if (pstat != StatType.Wisdom)
            {
                var pfeat = Helper.Get<BlueprintFeature>("2fa48527ba627254ba9bf4556330a4d4"); //PsychokineticistBurnFeature
                pfeat.GetComponent<AddKineticistPart>().MainStat = pstat;

                var presource = Helper.Get<BlueprintAbilityResource>("4b8b95612529a8640bb6e07c580b947b"); //PsychokineticistBurnResource
                presource.m_MaxAmount.ResourceBonusStat = pstat;
            }
        }

        [PatchInfo(Severity.Fix, "Spell-like Blasts", "makes blasts register as spell like, instead of supernatural", false)]
        public static void fixBlastsAreSpellLike()
        {
            foreach (var blast in Blasts.GetBaseAndVariants())
                blast.Get().Type = AbilityType.SpellLike;
        }

        [PatchInfo(Severity.Fix, "Fix Wall Infusion", "fix Wall Infusion not dealing damage while standing inside", false)]
        public static void fixWallInfusion()
        {
            int counter = 0;
            foreach (var bp in ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints.Values)
            {
                if (bp.Blueprint is BlueprintAbilityAreaEffect)
                {
                    var run = (bp.Blueprint as BlueprintAbilityAreaEffect).GetComponent<AbilityAreaEffectRunAction>();
                    if (run == null)
                        continue;

                    if (bp.Blueprint.name.StartsWith("Wall"))
                    {
                        run.Round = run.UnitEnter;
                        //run.UnitEnter = Helper.CreateActionList();
                        counter++;
                    }
                }
            }
            Helper.Print("Patched Wall Infusions: " + counter);
        }

        [PatchInfo(Severity.Create, "Selective Metakinesis", "gain selective metakinesis at level 7", true)]
        public static void createSelectiveMetakinesis()
        {
            //var empower1 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("f5f3aa17dd579ff49879923fb7bc2adb"); //MetakinesisEmpowerBuff
            //var empower2 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("f8d0f7099e73c95499830ec0a93e2eeb"); //MetakinesisEmpowerCheaperBuff
            var kineticist = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>("b79e92dd495edd64e90fb483c504b8df"); //KineticistProgression
            var knight = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("7d61d9b2250260a45b18c5634524a8fb");

            Sprite icon = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("85f3340093d144dd944fff9a9adfd2f2").Icon;
            string displayname = "Metakinesis — Selective";
            string description = "At 7th level, by accepting 1 point of burn, a kineticist can adjust her kinetic blast as if using Selective Spell.";

            var applicable = Blasts.GetVariants(
                g => g.CanTargetPoint
                  || g.GetComponent<AbilityTargetsAround>() && g.CanTargetFriends
                  || g.GetComponent<AbilityDeliverChain>());

            Helper.PrintDebug(applicable.Select(s => s.NameSafe()).Join());

            BlueprintActivatableAbility ab1 = Helper.CreateBlueprintActivatableAbility(
                "MetakinesisSelectiveAbility",
                displayname,
                description,
                out BlueprintBuff buff1,
                icon: icon
                );
            buff1.SetComponents(
                new AddKineticistBurnModifier() { BurnType = KineticistBurnType.Metakinesis, Value = 1, m_AppliableTo = applicable.ToArray() },
                Helper.CreateAutoMetamagic(Metamagic.Selective, applicable, AutoMetamagic.AllowedType.KineticistBlast)
                ).Flags(hidden: true, stayOnDeath: true);

            var feature1 = Helper.CreateBlueprintFeature(
                "MetakinesisSelectiveFeature",
                displayname,
                description,
                icon: icon
                ).SetComponents(
                Helper.CreateAddFacts(ab1.ToRef())
                );

            kineticist.AddFeature(7, feature1, "70322f5a2a294e54a9552f77ee85b0a7");
            knight.RemoveFeature(7, feature1);
        }

        [PatchInfo(Severity.Create, "Auto Metakinesis", "activatable to automatically empower and maximize blasts, if you have unused burn", false)]
        public static void createAutoMetakinesis()
        {
            var empower = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("70322f5a2a294e54a9552f77ee85b0a7"); //MetakinesisEmpowerFeature
            var quickenbuff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("f690edc756b748e43bba232e0eabd004"); //MetakinesisQuickenBuff
            var quickenbuff2 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("c4b74e4448b81d04f9df89ed14c38a95"); //MetakinesisQuickenCheaperBuff

            var auto = Helper.CreateBlueprintActivatableAbility(
                "MetakinesisAutoAbility",
                "Metakinesis — Empower/Maximize (Automatic)",
                "Apply Empower and Maxmize automatically depending on leftover gather power burn.",
                out BlueprintBuff autobuff,
                icon: Helper.StealIcon("45d94c6db453cfc4a9b99b72d6afe6f6"),
                onByDefault: true
                );

            autobuff.SetComponents(new AutoMetakinesis());
            autobuff.Flags(hidden: true, stayOnDeath: true);

            Helper.AppendAndReplace(ref empower.GetComponent<AddFacts>().m_Facts, auto.ToRef());

            // quicken removes itself after use
            quickenbuff.GetComponent<AutoMetamagic>().Once = true;
            quickenbuff2.GetComponent<AutoMetamagic>().Once = true;
        }

        [PatchInfo(Severity.Create | Severity.WIP, "Hurricane Queen", "Wild Talent: Hurricane Queen", false, Requirement: typeof(Patch_EnvelopingWindsCap))]
        public static void createHurricaneQueen()
        {
            var windsBuff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("b803fcd9da7b1564fb52978f08372767"); //EnvelopingWindsBuff
            var windsFeat = Helper.ToRef<BlueprintFeatureReference>("bb0de2047c448bd46aff120be3b39b7a");  //EnvelopingWinds
            var windsEffect = Helper.ToRef<BlueprintUnitFactReference>("bbba1600582cf8446bb515a33bd89af8"); //EnvelopingWindsEffectFeature
            var wildtalent_selection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("5c883ae0cd6d7d5448b7a420f51f8459");
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391");

            var feat = Helper.CreateBlueprintFeature(
                "HurricaneQueen",
                "Hurricane Queen",
                "You are one with the hurricane. Your enveloping winds defense wild talent has an additional 25% chance of deflecting ranged attacks, and your total deflection chance can exceed the usual cap of 75%. All wind and weather (including creatures using the whirlwind monster ability) affect you and your attacks only if you wish them to do so; for example, you could shoot arrows directly through a tornado without penalty."
                ).SetComponents(
                Helper.CreateAddFacts(windsEffect, windsEffect, windsEffect, windsEffect, windsEffect),
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 18),
                Helper.CreatePrerequisiteFeature(windsFeat)
                );

            var ray = new SetAttackerMissChance()
            {
                m_Type = SetAttackerMissChance.Type.RangedTouch,
                Value = Helper.CreateContextValue(AbilitySharedValue.Damage),
                Conditions = Helper.CreateConditionsChecker(0, Helper.CreateContextConditionCasterHasFact(feat.ToRef2()))
            };
            windsBuff.AddComponents(ray);

            Helper.AppendAndReplace(ref wildtalent_selection.m_AllFeatures, feat.ToRef());

            // immune to air-elemental whirlwind
            // bb57c37bfb5982d4bbed8d0fea75e404:WildShapeElementalAirWhirlwindDebuff
            // SpecificBuffImmunity(sleet_storm)
            // ignore miss chances
            // ignore weather in UnitPartConcealment.Calculate
        }

        [PatchInfo(Severity.Create, "Mind Shield", "Wild Talent: half Psychokineticist's penalties", true)]
        public static void createMindShield()
        {
            var buff = Helper.Get<BlueprintBuff>("a9e3e785ea41449499b6b5d3d22a0856");  //PsychokineticistBurnBuff
            var wildtalent_selection = Helper.Get<BlueprintFeatureSelection>("5c883ae0cd6d7d5448b7a420f51f8459");
            var psychokineticist = Helper.ToRef<BlueprintArchetypeReference>("f2847dd4b12fffd41beaa3d7120d27ad");

            var feature = Helper.CreateBlueprintFeature(
                "MindShieldFeature",
                "Mind Shield",
                "Reduce the penalties of Mind Burn by 1."
                ).SetComponents(
                Helper.CreatePrerequisiteArchetypeLevel(psychokineticist));
            Resource.Cache.FeatureMindShield.SetReference(feature);

            var property = Helper.CreateBlueprintUnitProperty("PsychokineticistMindPropertyGetter")
                .SetComponents(new PropertyMindShield { Feature = feature });

            var rank = buff.GetComponent<ContextRankConfig>();
            rank.m_StepLevel = 1;
            rank.m_CustomProperty = property.ToRef();

            Helper.AppendAndReplace(ref wildtalent_selection.m_AllFeatures, feature.ToRef());
        }

        [HarmonyPatch]
        public class Patch_ActivatableGroups
        {
            // improvements:
            // - combine demon summons? (is non activatable)
            // - fix sometimes always shows auto-border (maybe add right click disable all)
            // - fix doesn't show resource count
            // - fix cannot remove from toolbar

            [HarmonyPatch(typeof(ActionBarSlotVM), nameof(ActionBarSlotVM.SetMechanicSlot))]
            [HarmonyPostfix]
            public static void ShowButton(ActionBarSlotVM __instance)
            {
                // makes the unfold button visible
                if (__instance?.MechanicActionBarSlot is MechanicActionBarSlotGroup)
                    __instance.HasConvert.Value = true;
            }

            [HarmonyPatch(typeof(ActionBarVM), nameof(ActionBarVM.CollectAbilities))]
            [HarmonyPostfix]
            public static void GroupAbilities(ActionBarVM __instance)
            {
                var all = new Dictionary<int, List<MechanicActionBarSlot>>();

                for (int i = __instance.GroupAbilities.Count - 1; i >= 0; i--)
                {
                    BlueprintGuid guid;
                    var slot = __instance.GroupAbilities[i].MechanicActionBarSlot;
                    if (slot is MechanicActionBarSlotActivableAbility act)
                        guid = act.ActivatableAbility.Blueprint.AssetGuid;
                    else if (slot is MechanicActionBarSlotAbility ab)
                        guid = ab.Ability.Blueprint.AssetGuid;
                    else
                        continue;

                    int index = MechanicActionBarSlotGroup.Groups.FindIndex(f => f.Guids.Contains(guid));
                    if (index < 0)
                        continue;

                    if (!all.TryGetValue(index, out var list))
                        all.Add(index, list = new());

                    list.Add(slot);
                    __instance.GroupAbilities.RemoveAt(i);
                }

                foreach (var list in all)
                    __instance.GroupAbilities.Add(new ActionBarSlotVM(new MechanicActionBarSlotGroup(list.Key, list.Value)));
            }

            [HarmonyPatch(typeof(ActionBarSlotVM), nameof(ActionBarSlotVM.OnShowConvertRequest))]
            [HarmonyPriority(Priority.VeryHigh)]
            [HarmonyPrefix]
            public static bool OnShowConvert(ActionBarSlotVM __instance)
            {
                if (__instance.MechanicActionBarSlot is not MechanicActionBarSlotGroup mechanic)
                    return true;

                if (__instance.ConvertedVm.Value != null && !__instance.ConvertedVm.Value.IsDisposed)
                {
                    __instance.CloseConvert();
                    return false;
                }

                __instance.ConvertedVm.Value = new ActionBarConvertedActivableVM(mechanic.Slots, __instance.CloseConvert); // if null is used, it won't close; possible useful for nesting
                return false;
            }

            [HarmonyPatch(typeof(ActionBarSlotVM), nameof(ActionBarSlotVM.OnMainClick))]
            [HarmonyPostfix]
            public static void OnClick(ActionBarSlotVM __instance)
            {
                if (__instance.MechanicActionBarSlot is MechanicActionBarSlotGroup)
                    __instance.OnShowConvertRequest();
            }

            [HarmonyPatch(typeof(ActionBarVM), nameof(ActionBarVM.OnUnitChanged))]
            [HarmonyPrefix]
            public static void KeepOpen1(ActionBarVM __instance, out int __state)
            {
                __state = -1;

                foreach (var slot in __instance.Slots)
                {
                    if (slot.ConvertedVm?.Value != null && slot.MechanicActionBarSlot is MechanicActionBarSlotGroup mechanic)
                    {
                        __state = mechanic.Index;
                        break;
                    }
                }
            }

            [HarmonyPatch(typeof(ActionBarVM), nameof(ActionBarVM.OnUnitChanged))]
            [HarmonyPostfix]
            public static void KeepOpen2(ActionBarVM __instance, int __state)
            {
                if (__state < 0)
                    return;

                foreach (var slot in __instance.Slots)
                {
                    if (slot.MechanicActionBarSlot is MechanicActionBarSlotGroup mechanic && mechanic.Index == __state)
                    {
                        Helper.PrintDebug("reopend OnShowConvertRequest after refresh");
                        slot.OnShowConvertRequest();
                        break;
                    }
                }
            }

            /*[HarmonyPatch(typeof(ActionBarVM), nameof(ActionBarVM.ClearSlot))]
            [HarmonyPrefix]
            public static void Debug1(ActionBarSlotVM viewModel)
            {
                Helper.Print("clearing slot " + viewModel.GetType().Name);
            }*/

            public class MechanicActionBarSlotGroup : MechanicActionBarSlot
            {
                #region DefGroup
                public static List<DefGroup> Groups = new()
                {
                    new DefGroup(
                        "Kinetic Blades",
                        "DESC",
                        Helper.StealIcon("41e9a0626aa54824db9293f5de71f23f"),
                        "89acea313b9a9cb4d86bbbca01b90346",	//KineticBladeAirBlastAbility
                        "55790f1d270297f4a998292e1573a09e",	//KineticBladeBlizzardBlastAbility
                        "98f0da4bf25a34a4caffa6b8a2d33ef6",	//KineticBladeBloodBlastAbility
                        "4005fc2cd91860142ba55a369fbbec23",	//KineticBladeBlueFlameBlastAbility
                        "371b160cbb2ce9c4a8d6c28e61393f6d",	//KineticBladeChargeWaterBlastAbility
                        "37c87f140af6166419fe4c1f1305b2b8",	//KineticBladeColdBlastAbility
                        "77d9c04214a9bd84bbc1eefabcd98220",	//KineticBladeEarthBlastAbility
                        "b9e9011e24abcab4996e6bd3228bd60b",	//KineticBladeElectricBlastAbility
                        "41e9a0626aa54824db9293f5de71f23f",	//KineticBladeFireBlastAbility
                        "3f68b8bdd90ccb0428acd38b84934d30",	//KineticBladeIceBlastAbility
                        "cf1085900220be5459273282389aa9c2",	//KineticBladeMagmaBlastAbility
                        "ea2b3e7e3b8726d4c94ba58118749742",	//KineticBladeMetalBlastAbility
                        "5639fadad8b45e2418b356327d072789",	//KineticBladeMudBlastAbility
                        "acc31b4666e923b49b3ab85b2304f26c",	//KineticBladePlasmaBlastAbility
                        "dc6f0b906566aca4d8b86729855959cb",	//KineticBladeSandstormBlastAbility
                        "66028030b96875b4c97066525ff75a27",	//KineticBladeSteamBlastAbility
                        "287e0c88af08f3e4ba4aca52566f33a7",	//KineticBladeThunderstormBlastAbility
                        "70524e9d61b22e948aee1dfe11dc67c8"),//KineticBladeWaterBlastAbility)
                    new DefGroup(
                        "Demon Aspects",
                        "DESC",
                        Helper.StealIcon("e24fbd97558f06b45a09c7fbe7170a55"),
                        "0b57876f5dbbc784186b8b1f7d678602", //DemonFirstAscensionAbility, this is the gore attack
                        "7b63a532fe1ad654fa1aa8f5ebf3cefb", //DemonClawsAbility
                        "b17352531cb25d64fbf4078b856383c5", //DemonSummonAbilityTier2
                        "9365979e813d90f4db1579dd36f0a3c9", //DemonSummonAbilityTier3
                        "375089aeb3bcfa4479de8476b1589996", //DemonSummonAbilityTier4
                        "fd1669c290212484894bc276d79bc63f", //DemonSummonAbilityTier5
                        "745734402784ef34894aac64e35d46f0", //DemonSummonAbilityTier6
                        "a693ad7d3783f8a4680ab410d9858525", //DemonSummonAbilityTier7
                        "7a6f84f3df641d64e8f59e8fccf00568", //DemonSummonAbilityTier8
                        "cf6355be6d63541418279a560039a866", //DemonSummonAbilityTier9
                        "54d981871c4241844b7dcfc5d4893025", //DemonSummonAbilityTier10
                        "b968988d6c0e830458fd49efbfb86202", //NocticulaAspectAbility
                        "e24fbd97558f06b45a09c7fbe7170a55",	//BabauAspectAbility
                        "3070984d4c8bd4f48877337da6c7535d",	//BalorAspectAbility
                        "e642444d21a4dab4ea07cd042e6f9dc0",	//BrimorakAspectAbility
                        "49e1df551bc9cdc499930be39a3fc8cf",	//ColoxusAspectAbility
                        "55c6e91192b92b8479fa66d6aee33074",	//IncubusAspectAbility
                        "37bfe9e5535e54c49b248bd84305ebd5",	//KalavakusAspectAbility
                        "868c4957c5671114eaaf8e0b6b55ad3f",	//NabasuAspectAbility
                        "e305991cb9461a04a97e4f5b02b8b767",	//OmoxAspectAbility
                        "fae00e8f4de9cd54da800d383ede7812",	//SchirAspectAbility
                        "0d817aa4f8bc00541a43ea2f822d124b",	//ShadowDemonAspectAbility
                        "8a474cae6e2788a498f616d36b56b5d2",	//SuccubusAspectAbility
                        "b6dc815e86a12654eb7f78c5f14008df",	//VavakiaAspectAbility
                        "600cf1ff1d381d8488faed4e7fbda865",	//VrockAspectAbility
                        "df9e7bbc606b0cd4087ee2d08cc2c09b"),//VrolikaiAspectAbility
                    new DefGroup(
                        "Metakinesis",
                        "DESC",
                        Helper.StealIcon("bb4369a9be4406147ad4f1a1f05adccf"),
                        "2dc9630110d0434ba7df785777b67be7",	//MetakinesisAutoAbility
                        "c93c0217b3e0b4441a4f789dfb95fc8b",	//MetakinesisEmpowerAbility
                        "b65ad9782f697f245baeb90cd5670546",	//MetakinesisEmpowerCheaperAbility
                        "bb4369a9be4406147ad4f1a1f05adccf",	//MetakinesisMaximizedAbility
                        "5c9f2b38404f118479a44234777e1ea8",	//MetakinesisMaximizedCheaperAbility
                        "990dd3388df6a8d4cad1429380e71853",	//MetakinesisQuickenAbility
                        "53c7b6accfa1e8e4eba7004b17f61ac1",	//MetakinesisQuickenCheaperAbility
                        "031d823e0b804b3c868bd031e539cac3"),//MetakinesisSelectiveAbility
                    new DefGroup(
                        "Infusions",
                        "DESC",
                        Helper.StealIcon("88c37d8a7d808a844ba0116dd37e4059"),
                        "6d35b4f39de9eb446b2d0a65b931246b",	//BleedingInfusionAbility
                        "88c37d8a7d808a844ba0116dd37e4059",	//BowlingInfusionAbility
                        "96b3fc11991f2664080c7c5e41417f48",	//BurningInfusionAbility
                        "fb426ea002abbbc4198b1cd6b99f1be8",	//ChillingInfusionAbility
                        "abf5c26910fda5949abbc285c60416f9",	//DazzlingInfusionAbility
                        "091b297f43ac5be43af31979c00ade57",	//EntanglingInfusionAbility
                        "323be9d573657374da4e3f1456a2366c",	//FlashInfusionAbility
                        "d0007fed20710ae4a96cebd2ba99f08b",	//FoxfireInfusionAbility
                        "2816fad233e15a54c86729cee6e8969d",	//GrapplingInfusionAbility
                        "e2e3ce12bdfc9d14a9ca4d51696dc8db",	//GutWrenchingInfusionAbility
                        "b2d91bac690b74140b4fa3eec443edee",	//MagneticInfusionAbility
                        "06e3ac0ec6341744eb87f1f70a11576b",	//PureFlameInfusionAbility
                        "bc5665a318bc4eb46a0537455509851a",	//PushingInfusionAbility
                        "097c209e378144045ab97f4d54876959",	//RareMetalInfusionAbility
                        "db3ccc72faeac0343891ba71bb692a42",	//SynapticInfusionAbility
                        "59303d0eb693cd2438fc89f91e29ab19",	//UnravelingInfusionAbility
                        "30c81aff8e5293d418759d10f193f347"),//VampiricInfusionAbility
                    new DefGroup(
                        "Metamagic Rods",
                        "DESC",
                        Helper.StealIcon("485ffd3bd7877fb4d81409b120a41076"),
                        "ccffef1193d04ad1a9430a8009365e81",	//MetamagicRodGreaterBolsterToggleAbility
                        "cc266cfb106a5a3449b383a25ab364f0",	//MetamagicRodGreaterEmpowerToggleAbility
                        "c137a17a798334c4280e1eb811a14a70",	//MetamagicRodGreaterExtendToggleAbility
                        "78b5971c7a0b7f94db5b4d22c2224189",	//MetamagicRodGreaterMaximizeToggleAbility
                        "5016f110e5c742768afa08224d6cde56",	//MetamagicRodGreaterPersistentToggleAbility
                        "fca35196b3b23c346a7d1b1ce20c6f1c",	//MetamagicRodGreaterQuickenToggleAbility
                        "cc116b4dbb96375429107ed2d88943a1",	//MetamagicRodGreaterReachToggleAbility
                        "f0d798f5139440a8b2e72fe445678d29",	//MetamagicRodGreaterSelectiveToggleAbility
                        "056b9f1aa5c54a7996ca8c4a00a88f88",	//MetamagicRodLesserBolsterToggleAbility
                        "ed10ddd385a528944bccbdc4254f8392",	//MetamagicRodLesserEmpowerToggleAbility
                        "605e64c0b4586a34494fc3471525a2e5",	//MetamagicRodLesserExtendToggleAbility
                        "868673cd023f96945a2ee61355740a96",	//MetamagicRodLesserKineticToggleAbility
                        "485ffd3bd7877fb4d81409b120a41076",	//MetamagicRodLesserMaximizeToggleAbility
                        "5a87350fcc6b46328a2b345f23bbda44",	//MetamagicRodLesserPersistentToggleAbility
                        "b8b79d4c37981194fa91771fc5376c5e",	//MetamagicRodLesserQuickenToggleAbility
                        "7dc276169f3edd54093bf63cec5701ff",	//MetamagicRodLesserReachToggleAbility
                        "66e68fd0b661413790e3000ede141f16",	//MetamagicRodLesserSelectiveToggleAbility
                        "afb2e1f96933c22469168222f7dab8fb",	//MetamagicRodMasterpieceToggleAbility
                        "6cc31148ae2d48359c02712308cb4167",	//MetamagicRodNormalBolsterToggleAbility
                        "077ec9f9394b8b347ba2b9ec45c74739",	//MetamagicRodNormalEmpowerToggleAbility
                        "69de70b88ca056440b44acb029a76cd7",	//MetamagicRodNormalExtendToggleAbility
                        "3b5184a55f98f264f8b39bddd3fe0e88",	//MetamagicRodNormalMaximizeToggleAbility
                        "9ae2e56b24404144bd911378fe541597",	//MetamagicRodNormalPersistentToggleAbility
                        "1f390e6f38d3d5247aacb25ab3a2a6d2",	//MetamagicRodNormalQuickenToggleAbility
                        "f0b05e39b82c3be408009e26be40bc91",	//MetamagicRodNormalReachToggleAbility
                        "04f768c59bb947e3948ce2e7e72feecb"),//MetamagicRodNormalSelectiveToggleAbility
                };
                #endregion

                [JsonProperty]
                public int Index;
                [JsonProperty]
                public List<MechanicActionBarSlot> Slots;

                public MechanicActionBarSlotGroup(int index, List<MechanicActionBarSlot> slots)
                {
                    this.Index = index;
                    this.Slots = slots;

                    // this is just for sanity check
                    var guid = BlueprintGuid.Empty;
                    var slot = this.Slots.FirstOrDefault();
                    if (slot is MechanicActionBarSlotActivableAbility act)
                        guid = act.ActivatableAbility.Blueprint.AssetGuid;
                    else if (slot is MechanicActionBarSlotAbility ab)
                        guid = ab.Ability.Blueprint.AssetGuid;
                    if (guid != BlueprintGuid.Empty)
                    {
                        this.Index = Groups.FindIndex(f => f.Guids.Contains(guid));
                    }
                }

                public override bool CanUseIfTurnBasedInternal() => true;
                public override object GetContentData() => this;
                public override Color GetDecorationColor() => default; //new Color(0f, 0f, 0f, 0f);
                public override Sprite GetDecorationSprite() => null;
                public override string GetTitle() => Groups.ElementAtOrDefault(this.Index)?.Title;
                public override string GetDescription() => Groups.ElementAtOrDefault(this.Index)?.Description;
                public override Sprite GetIcon() => Groups.ElementAtOrDefault(this.Index)?.Icon;
                public override int GetResource()
                {
                    int count = 0;
                    foreach (var slot in Slots)
                        if (slot.IsActive())
                            count++;
                    if (count == 0)
                        return -1;
                    return count;
                }
                public override bool IsCasting() => false;

                public override string KeyName => GetTitle();
                public override bool IsActive() => Slots.Any(a => a.IsActive()); // active border
                public override bool IsDisabled(int resourceCount) => false;
                public override bool IsPossibleActive(int? resource = null) => true;
                public override void UpdateSlotInternal(ActionBarSlot slot)
                {
                    if (slot.ActiveMark != null && IsActive())
                    {
                        slot.ActiveMark.color = slot.RunningColor;
                        slot.ActiveMark.gameObject.SetActive(true);
                    }
                }
                public override bool IsBad() => Index < 0; // use this to remove invalid entries

                public override TooltipBaseTemplate GetTooltipTemplate()
                {
                    string title = "";
                    string description = "";
                    Sprite icon = null;

                    var info = Groups.ElementAtOrDefault(this.Index);
                    if (info != null)
                    {
                        title = info.Title;
                        description = info.Description;
                        icon = info.Icon;
                    }

                    return new TooltipTemplateDataProvider(new UIData(title, description, icon));
                }

            }

            public class DefGroup
            {
                public List<BlueprintGuid> Guids;
                public string Title;
                public string Description;
                public Sprite Icon;

                public DefGroup(string title, string description, Sprite icon, params string[] guids)
                {
                    this.Title = title;
                    this.Description = description;
                    this.Icon = icon;
                    this.Guids = guids.Select(s => BlueprintGuid.Parse(s)).ToList();
                }
            }

            public class ActionBarConvertedActivableVM : ActionBarConvertedVM
            {
                public ActionBarConvertedActivableVM(List<MechanicActionBarSlot> list, Action onClose) : base(new(), onClose)
                {
                    foreach (var item in list)
                        this.Slots.Add(new ActionBarSlotVM(item, -1, -1));
                }
            }
        }

        #region Helper

        /// <summary>
        /// 1) make BlueprintAbility
        /// 2) set m_Parent to XBlastBase
        /// 3) set SpellResistance
        /// 4) make components with helpers (step1 to 9)
        /// Logic for dealing damage. Will make a composite blast, if both p and e are set. How much damage is dealt is defined in step 2.
        /// </summary>
        public static AbilityEffectRunAction Step1_run_damage(PhysicalDamageForm p = 0, DamageEnergyType e = (DamageEnergyType)255, SavingThrowType save = SavingThrowType.Unknown, bool isAOE = false, bool half = false)
        {
            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, AbilityRankType.DamageDice, AbilityRankType.DamageBonus);

            List<ContextAction> list = new List<ContextAction>(2);

            bool isComposite = e != 0 && e != (DamageEnergyType)255;

            if (p != 0)
                list.Add(Helper.CreateContextActionDealDamage(p, dice, isAOE, isAOE, false, half, isComposite, AbilitySharedValue.DurationSecond, writeShare: isComposite));
            if (e != (DamageEnergyType)255)
                list.Add(Helper.CreateContextActionDealDamage(e, dice, isAOE, isAOE, false, half, isComposite, AbilitySharedValue.DurationSecond, readShare: isComposite));

            var runaction = Helper.CreateAbilityEffectRunAction(save, list.ToArray());

            return runaction;
        }

        /// <summary>
        /// Defines damage dice. Set twice for composite blasts. You shouldn't need half at all.
        /// </summary>
        public static ContextRankConfig Step2_rank_dice(bool twice = false, bool half = false)
        {
            var progression = ContextRankProgression.AsIs;
            if (half) progression = ContextRankProgression.Div2;
            if (twice) progression = ContextRankProgression.MultiplyByModifier;

            var rankdice = Helper.CreateContextRankConfig(
                baseValueType: ContextRankBaseValueType.FeatureRank,
                type: AbilityRankType.DamageDice,
                progression: progression,
                stepLevel: twice ? 2 : 0,
                feature: "93efbde2764b5504e98e6824cab3d27c".ToRef<BlueprintFeatureReference>()); //KineticBlastFeature
            return rankdice;
        }

        /// <summary>
        /// Defines bonus damage. Set half_bonus for energy blasts.
        /// </summary>
        public static ContextRankConfig Step3_rank_bonus(bool half_bonus = false)
        {
            var rankdice = Helper.CreateContextRankConfig(
                baseValueType: ContextRankBaseValueType.CustomProperty,
                progression: half_bonus ? ContextRankProgression.Div2 : ContextRankProgression.AsIs,
                type: AbilityRankType.DamageBonus,
                stat: StatType.Constitution,
                customProperty: "f897845bbbc008d4f9c1c4a03e22357a".ToRef<BlueprintUnitPropertyReference>()); //KineticistMainStatProperty
            return rankdice;
        }

        /// <summary>
        /// Simply makes the DC dex based.
        /// </summary>
        public static ContextCalculateAbilityParamsBasedOnClass step4_dc()
        {
            var dc = new ContextCalculateAbilityParamsBasedOnClass();
            dc.StatType = StatType.Dexterity;
            dc.m_CharacterClass = "42a455d9ec1ad924d889272429eb8391".ToRef<BlueprintCharacterClassReference>(); //KineticistClass
            return dc;
        }

        /// <summary>
        /// Creates damage tooltip from the run-action. Defines burn cost. Blast cost is 0, except for composite blasts which is 2. Talent is not used.
        /// </summary>
        public static AbilityKineticist Step5_burn(GameAction[] actions, int infusion = 0, int blast = 0, int talent = 0)
        {
            var comp = new AbilityKineticist();
            comp.InfusionBurnCost = infusion;
            comp.BlastBurnCost = blast;
            comp.WildTalentBurnCost = talent;

            if (actions == null)
                return comp;

            for (int i = 0; i < actions.Length; i++)
            {
                var action = actions[i] as ContextActionDealDamage;
                if (action == null) continue;

                comp.CachedDamageInfo.Add(new AbilityKineticist.DamageInfo() { Value = action.Value, Type = action.DamageType, Half = action.Half });
            }
            return comp;
        }

        /// <summary>
        /// Required feat for this ability to show up.
        /// </summary>
        public static AbilityShowIfCasterHasFact Step6_feat(BlueprintFeature fact)
        {
            return Helper.CreateAbilityShowIfCasterHasFact(fact.ToRef2());
        }

        /// <summary>
        /// Defines projectile.
        /// </summary>
        public static AbilityDeliverProjectile Step7_projectile(string projectile_guid, bool isPhysical, AbilityProjectileType type, float length, float width)
        {
            string weapon = isPhysical ? "65951e1195848844b8ab8f46d942f6e8" : "4d3265a5b9302ee4cab9c07adddb253f"; //KineticBlastPhysicalWeapon //KineticBlastEnergyWeapon
            //KineticBlastPhysicalBlade b05a206f6c1133a469b2f7e30dc970ef
            //KineticBlastEnergyBlade a15b2fb1d5dc4f247882a7148d50afb0

            var projectile = Helper.CreateAbilityDeliverProjectile(
                projectile_guid.ToRef<BlueprintProjectileReference>(),
                type,
                weapon.ToRef<BlueprintItemWeaponReference>(),
                length.Feet(),
                width.Feet());
            return projectile;
        }

        /// <summary>
        /// Element descriptor for energy blasts.
        /// </summary>
        public static SpellDescriptorComponent Step8_spell_description(SpellDescriptor descriptor)
        {
            return new SpellDescriptorComponent
            {
                Descriptor = descriptor
            };
        }

        // <summary>
        // This is identical for all blasts or is missing completely. It seems to me as if it not used and a leftover.
        // </summary>
        //public static ContextCalculateSharedValue step9_shared_value()
        //{
        //    return Helper.CreateContextCalculateSharedValue();
        //}

        /// <summary>
        /// Defines sfx for casting.
        /// Use either use either OnPrecastStart or OnStart for time.
        /// </summary>
        public static AbilitySpawnFx Step_sfx(AbilitySpawnFxTime time, string sfx_guid)
        {
            var sfx = new AbilitySpawnFx();
            sfx.Time = time;
            sfx.PrefabLink = new PrefabLink() { AssetId = sfx_guid };
            return sfx;
        }

        public static BlueprintBuff ExpandSubstance(BlueprintBuff buff, BlueprintAbilityReference baseBlast)
        {
            Helper.AppendAndReplace(ref buff.GetComponent<AddKineticistInfusionDamageTrigger>().m_AbilityList, baseBlast);
            Helper.AppendAndReplace(ref buff.GetComponent<AddKineticistBurnModifier>().m_AppliableTo, baseBlast);
            return buff;
        }

        #endregion
    }

    #region Patches

    [PatchInfo(Severity.Harmony, "Patch: Enveloping Winds Cap", "removes 50% evasion cap for Hurricane Queen", false)]
    [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.SetMissChance))]
    public class Patch_EnvelopingWindsCap
    {
        //this.MissChance = Math.Min(50, Math.Max(this.MissChance, value));
        public static bool Prefix(RuleAttackRoll __instance, ref int value)
        {
            if (value > 0 && value > __instance.MissChance)
            {
                __instance.MissChance = Math.Min(value, 100);
            }
            return false;
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Fix Polymorph Gather", "makes it so polymorphed creatures can use Gather Power and creatures with hands Kinetic Blade", false)]
    [HarmonyPatch]
    public class Patch_FixPolymorphGather
    {
        [HarmonyPatch(typeof(RestrictionCanGatherPower), nameof(RestrictionCanGatherPower.IsAvailable))]
        [HarmonyPrefix]
        public static bool Prefix1(RestrictionCanGatherPower __instance, ref bool __result)
        {
            UnitPartKineticist unitPartKineticist = __instance.Owner.Get<UnitPartKineticist>();
            if (!unitPartKineticist)
            {
                __result = false;
                return false;
            }
            UnitBody body = __instance.Owner.Body;
            ItemEntityWeapon weapon = body.PrimaryHand.MaybeItem as ItemEntityWeapon;
            if (weapon != null && !weapon.IsMonkUnarmedStrike && !(weapon != null && weapon.Blueprint.Category == WeaponCategory.KineticBlast))
            {
                __result = false;
                return false;
            }
            ItemEntity weapon2 = body.SecondaryHand.MaybeItem;
            if (weapon2 != null)
            {
                ArmorProficiencyGroup? armorProficiencyGroup = body.SecondaryHand.MaybeShield?.Blueprint.Type.ProficiencyGroup;
                if (armorProficiencyGroup != null)
                {
                    if (!(armorProficiencyGroup.GetValueOrDefault() == ArmorProficiencyGroup.TowerShield & armorProficiencyGroup != null))
                    {
                        __result = unitPartKineticist.CanGatherPowerWithShield;
                        return false;
                    }
                }
                __result = false;
                return false;
            }
            __result = true;
            return false;
        }

        [HarmonyPatch(typeof(RestrictionCanUseKineticBlade), nameof(RestrictionCanUseKineticBlade.IsAvailable), new Type[0])]
        [HarmonyPrefix]
        public static bool Prefix2(RestrictionCanUseKineticBlade __instance, ref bool __result)
        {
            var unit = __instance.Owner;
            var body = unit.Body;
            if (body.IsPolymorphed && !body.IsPolymorphKeepSlots || !body.HandsAreEnabled)
            {
                __result = false;
                return false;
            }
            UnitPartKineticist unitPartKineticist = unit.Get<UnitPartKineticist>();
            if (!unitPartKineticist)
            {
                __result = false;
                return false;
            }
            ItemEntityWeapon maybeWeapon = body.PrimaryHand.MaybeWeapon;
            BlueprintItemWeapon blueprintItemWeapon = maybeWeapon?.Blueprint;
            bool flag = blueprintItemWeapon.GetComponent<WeaponKineticBlade>() != null;
            if (body.PrimaryHand.MaybeItem != null && !flag)
            {
                __result = false;
                return false;
            }
            AddKineticistBlade addKineticistBlade = __instance.Fact.Blueprint.Buff.GetComponent<AddKineticistBlade>().Or(null);
            BlueprintItemWeapon blueprintItemWeapon2 = addKineticistBlade?.Blade;
            if (blueprintItemWeapon2 == null)
            {
                __result = false;
                return false;
            }
            if (blueprintItemWeapon != blueprintItemWeapon2 || !unitPartKineticist.IsBladeActivated)
            {
                WeaponKineticBlade weaponKineticBlade = blueprintItemWeapon2.GetComponent<WeaponKineticBlade>().Or(null);
                KineticistAbilityBurnCost? kineticistAbilityBurnCost = null;
                if (((AbilityKineticist.CalculateAbilityBurnCost(weaponKineticBlade?.GetActivationAbility(unit)) != null) ? kineticistAbilityBurnCost.GetValueOrDefault().Total : 0) > unitPartKineticist.LeftBurnThisRound)
                {
                    __result = false;
                    return false;
                }
            }
            __result = true;
            return false;
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: True Gather Power Level", "Normal: The level of gathering power is determined by the mode (none, low, medium, high) selected. If the mode is lower than the already accumulated gather level, then levels are lost.\nPatched: The level of gathering is true to the accumulated level or the selected mode, whatever is higher.", false)]
    [HarmonyPatch(typeof(KineticistController), nameof(KineticistController.TryApplyGatherPower))]
    public class Patch_TrueGatherPowerLevel
    {
        public static BlueprintBuff buff1 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("e6b8b31e1f8c524458dc62e8a763cfb1");
        public static BlueprintBuff buff2 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("3a2bfdc8bf74c5c4aafb97591f6e4282");
        public static BlueprintBuff buff3 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("82eb0c274eddd8849bb89a8e6dbc65f8");

        public static bool Prefix(UnitPartKineticist kineticist, BlueprintAbility abilityBlueprint, ref KineticistAbilityBurnCost cost)
        {
            if (kineticist == null || abilityBlueprint.GetComponent<AbilityKineticist>() == null || kineticist.GatherPowerAbility == null)
                return false;

            int buffRank = kineticist.TargetGatherPowerRank; // get the target power rank

            // check if stronger buff exists and if so apply it instead
            if (buffRank < 1 && kineticist.Owner.Buffs.GetBuff(buff1) != null)
                buffRank = 1;
            else if (buffRank < 2 && kineticist.Owner.Buffs.GetBuff(buff2) != null)
                buffRank = 2;
            else if (buffRank < 3 && kineticist.Owner.Buffs.GetBuff(buff3) != null)
                buffRank = 3;

            int value = KineticistUtils.CalculateGatherPowerBonus(kineticist.GatherPowerBaseValue, buffRank); // add increase from Supercharge

            cost.IncreaseGatherPower(value); // apply value

            return false;
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Kineticist Allow Opportunity Attack", "allows Attack of Opportunities with anything but standard Kinetic Blade; so that Kinetic Whip works; also allows natural attacks to be used, if Whip isn't available", false)]
    [HarmonyPatch]
    public class Patch_KineticistAllowOpportunityAttack
    {
        private static BlueprintGuid blade_p = BlueprintGuid.Parse("b05a206f6c1133a469b2f7e30dc970ef"); //KineticBlastPhysicalBlade
        private static BlueprintGuid blade_e = BlueprintGuid.Parse("a15b2fb1d5dc4f247882a7148d50afb0"); //KineticBlastEnergyBlade

        [HarmonyPatch(typeof(AddKineticistBlade), nameof(AddKineticistBlade.OnActivate))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1(IEnumerable<CodeInstruction> instr)
        {
            List<CodeInstruction> list = instr.ToList();
            MethodInfo original = AccessTools.Method(typeof(UnitState), nameof(UnitState.AddCondition));
            MethodInfo replacement = AccessTools.Method(typeof(Patch_KineticistAllowOpportunityAttack), nameof(NullReplacement));

            for (int i = 0; i < list.Count; i++)
            {
                var mi = list[i].operand as MethodInfo;
                if (mi != null && mi == original)
                {
                    Helper.PrintDebug("KineticistAoO at " + i);
                    list[i].operand = replacement;
                }
            }

            return list;
        }
        public static void NullReplacement(UnitState state, UnitCondition condition, Buff sourceBuff)
        {
        }

        [HarmonyPatch(typeof(UnitHelper), nameof(UnitHelper.IsThreatHand))]
        [HarmonyPrefix]
        public static bool Prefix2(UnitEntityData unit, WeaponSlot hand, ref bool __result)
        {
            if (!hand.HasWeapon)
                __result = false;

            else if (!hand.Weapon.Blueprint.IsMelee && !unit.State.Features.SnapShot)
                __result = false;

            else if (hand.Weapon.Blueprint.IsUnarmed && !unit.Descriptor.State.Features.ImprovedUnarmedStrike)
                __result = false;

            else if ((hand.Weapon.Blueprint.Type.AssetGuid == blade_p || hand.Weapon.Blueprint.Type.AssetGuid == blade_e)
                     && unit.Buffs.GetBuff(Resource.Cache.BuffKineticWhip) == null)
                __result = false;

            else
                __result = true;

            return false;
        }

    }

    //[HarmonyPatch(typeof(KineticistController), nameof(KineticistController.TryRunKineticBladeActivationAction))]
    //public class Patch_KineticistWhipReach
    //{
    //    public static void Postfix(UnitPartKineticist kineticist, UnitCommand cmd, bool __result)
    //    {
    //        if (!__result)
    //            return;
    //        if (kineticist.Owner.Buffs.GetBuff(Patch_KineticistAllowOpportunityAttack2.whip_buff) == null) 
    //            return;
    //        cmd.ApproachRadius += 5f * 0.3048f;
    //    }
    //}

    #endregion

}
