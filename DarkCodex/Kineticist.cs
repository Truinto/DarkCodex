using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Shared;
using System.IO;
using static Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell;

namespace DarkCodex
{
    /* Notes:
     * - when creating new elements, remember to put xxxBlastBladeDamage into the BaseBlast (otherwise substance infusion won't work with it)
     *
     */
    public class Kineticist
    {
        public static KineticistTree Tree = KineticistTree.Instance;

        [PatchInfo(Severity.Create, "Kineticist Background", "regional background: gain +1 Kineticist level for the purpose of feat prerequisites", true)]
        public static void CreateKineticistBackground()
        {
            var feature = Helper.CreateBlueprintFeature(
                "BackgroundElementalist",
                "Elemental Plane Outsider",
                "You were exposed to an elemental saturation as a young child. Attuning you to the elements.\nBenefit: Your Kineticist level count as 1 level higher for determining prerequisites for wild talents.",
                icon: null,
                group: FeatureGroup.BackgroundSelection
                ).SetComponents(
                Helper.CreateClassLevelsForPrerequisites(Tree.Class, 1)
                );

            Helper.AppendAndReplace(ref Helper.Get<BlueprintFeatureSelection>("fa621a249cc836f4382ca413b976e65e").m_AllFeatures, feature.ToRef());
        }

        [PatchInfo(Severity.Create, "Kineticist Extra Wild Talent", "basic feat: Extra Wild Talent; required by other kineticist features!", false, Priority: 300)]
        public static void CreateExtraWildTalentFeat()
        {
            var extra_wild_talent_selection = Helper.CreateBlueprintFeatureSelection(
                "ExtraWildTalentFeat",
                "Extra Wild Talent",
                "You gain a wild talent for which you meet the prerequisites. You can select an infusion or a non-infusion wild talent, but not a blast or defense wild talent.\nSpecial: You can take this feat multiple times. Each time, you must choose a different wild talent.",
                Helper.Get<BlueprintFeature>("42f96fc8d6c80784194262e51b0a1d25").Icon,
                FeatureGroup.Feat
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 1, true)
                );
            extra_wild_talent_selection.Ranks = 10;
            extra_wild_talent_selection.m_AllFeatures = Helper.Append(Tree.SelectionInfusion.Get().m_AllFeatures,
                                                                    Tree.SelectionWildTalent.Get().m_AllFeatures);

            Tree.ExtraWildTalent.SetReference(extra_wild_talent_selection);
            Helper.AddFeats(extra_wild_talent_selection);
        }

        [PatchInfo(Severity.Create, "Whip Infusion", "infusion: Kinetic Whip, expands Kinetic Knight", false, Requirement: typeof(Patch_KineticistAllowOpportunityAttack))]
        public static void CreateWhipInfusion()
        {
            var infusion_selection = Helper.Get<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
            var blade = Helper.ToRef<BlueprintFeatureReference>("9ff81732daddb174aa8138ad1297c787"); //KineticBladeInfusion
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391"); //KineticistClass
            var knight = Helper.Get<BlueprintArchetype>("7d61d9b2250260a45b18c5634524a8fb");

            var applicable = Tree.GetAll(true, true, archetype: true).Select(s => s.Blade.Burn).ToArray();
            //Main.PrintDebug(applicable.Select(s => s.NameSafe()).Join());
            var icon = Helper.StealIcon("0e5ec4d781678234f83118df41fd27c3");

            var ability = Helper.CreateBlueprintActivatableAbility(
                "KineticWhipActivatable",
                out BlueprintBuff buff,
                "Kinetic Whip",
                "Element: universal\nType: form infusion\nLevel: 3\nBurn: 2\nAssociated Blasts: any\nSaving Throw: none\nYou form a long tendril of energy or elemental matter. While active, your kinetic blade increases its reach by 5 feet and you can make attacks of opportunity with your kinetic blade.",
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
                "Whip Maneuver Bonus",
                "Kinetic Knight's kinetic whip has the disarm and trip weapon qualities, granting a +2 bonus to these maneuvers."
                ).SetComponents(
                new ManeuverBonusHasFact(buff, 2, CombatManeuver.Trip),
                new ManeuverBonusHasFact(buff, 2, CombatManeuver.Disarm)
                );

            Helper.AppendAndReplace(ref infusion_selection.m_AllFeatures, whip.ToRef());
            knight.AddFeature(5, whip);
            knight.AddFeature(5, maneuver);

            Resource.Cache.BuffKineticWhip.SetReference(buff);
        }

        [PatchInfo(Severity.Create, "Blade Rush Infusion", "infusion: Blade Rush, expands Kinetic Knight", false)]
        public static void CreateBladeRushInfusion()
        {
            var knight = Helper.Get<BlueprintArchetype>("7d61d9b2250260a45b18c5634524a8fb");
            var infusion_selection = Helper.Get<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
            var blade = Helper.ToRef<BlueprintFeatureReference>("9ff81732daddb174aa8138ad1297c787"); //KineticBladeInfusion
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391"); //KineticistClass
            var flashstep = Helper.Get<BlueprintAbility>("e10424c1afe70cb4384090f4dab8d437"); //StormwalkerFlashStepAbility

            string name = "Blade Rush";
            string description = "Element: universal\nType: form infusion\nLevel: 2\nBurn: 2\nAssociated Blasts: any\nSaving Throw: none\nYou use your element’s power to instantly move 30 feet, manifest a kinetic blade, and attack once. You gain a +2 bonus on the attack roll and take a –2 penalty to your AC until the start of your next turn. The movement doesn’t provoke attacks of opportunity, though activating blade rush does.";
            Sprite icon = Helper.StealIcon("4c349361d720e844e846ad8c19959b1e");

            var ability = Helper.CreateBlueprintAbility(
                "KineticBladeRushAbility",
                name,
                description,
                icon: icon,
                type: AbilityType.SpellLike,
                actionType: UnitCommand.CommandType.Standard,
                range: AbilityRange.Close
                ).SetComponents(
                Helper.CreateAbilityExecuteActionOnCast(
                    Helper.CreateContextActionApplyBuff(BlueprintRoot.Instance.SystemMechanics.ChargeBuff, 1, toCaster: true)
                    ),
                flashstep.GetComponent<AbilityCustomFlashStep>(),   // bug: m_FlashShot would trigger two attacks (impossible rare)
                Step5_burn(null, 1),
                new RestrictionCanGatherPowerAbility()
                ).TargetPoint();
            ability.AvailableMetamagic = Metamagic.Quicken;
            var abref = ability.ToRef();

            var rush = Helper.CreateBlueprintFeature(
                "KineticBladeRush",
                name,
                description,
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
                Helper.CreateAutoMetamagic(Metamagic.Quicken, new List<BlueprintAbilityReference>() { abref }, AutoMetamagic.AllowedType.KineticistBlast)
                );

            Helper.AppendAndReplace(ref infusion_selection.m_AllFeatures, rush.ToRef());
            knight.AddFeature(13, quickblade);

            foreach (var meta in Tree.MetakinesisBuffs)
            {
                var abilities = meta.GetComponent<AutoMetamagic>().Abilities;
                abilities.Add(abref);
                meta.GetComponent<AddKineticistBurnModifier>().m_AppliableTo = abilities.ToArray();
            }
        }

        [PatchInfo(Severity.Create, "Mobile Gathering", "basic feat: Mobile Gathering", false)]
        public static void CreateMobileGatheringFeat()
        {
            // --- base game stuff ---
            var buff1 = Helper.Get<BlueprintBuff>("e6b8b31e1f8c524458dc62e8a763cfb1");   //GatherPowerBuffI
            var buff2 = Helper.Get<BlueprintBuff>("3a2bfdc8bf74c5c4aafb97591f6e4282");   //GatherPowerBuffII
            var buff3 = Helper.Get<BlueprintBuff>("82eb0c274eddd8849bb89a8e6dbc65f8");   //GatherPowerBuffIII
            var gather_original_ab = Helper.Get<BlueprintAbility>("6dcbffb8012ba2a4cb4ac374a33e2d9a");    //GatherPower
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
                Helper.CreateSprite(Path.Combine(Main.ModPath, "icons", "GatherMobileHigh.png")),
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
                icon: Helper.CreateSprite(Path.Combine(Main.ModPath, "icons", "GatherMobileLow.png")),
                type: AbilityType.Special,
                actionType: UnitCommand.CommandType.Move,
                range: AbilityRange.Personal,
                duration: null,
                savingThrow: null
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
                icon: Helper.CreateSprite(Path.Combine(Main.ModPath, "icons", "GatherMobileMedium.png")),
                type: AbilityType.Special,
                actionType: UnitCommand.CommandType.Standard,
                range: AbilityRange.Personal,
                duration: null,
                savingThrow: null
                ).SetComponents(
                can_gather,
                hasMoveAction,
                Helper.CreateAbilityEffectRunAction(0, regain_halfmove, apply_debuff, three2three, two2three, one2three, zero2two),
                new RestrictionCanGatherPowerAbility(),
                Helper.CreateAbilityIsFullRoundInTurnBased());
            mobile_gathering_long_ab.CanTargetSelf = true;
            mobile_gathering_long_ab.Animation = CastAnimationStyle.Self;
            mobile_gathering_long_ab.HasFastAnimation = true;

            var mobile_gathering_feat = Helper.CreateBlueprintFeature(
                "MobileGatheringFeat",
                "Mobile Gathering",
                "While gathering power, you can move up to half your normal speed. This movement provokes attacks of opportunity as normal.",
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

        [PatchInfo(Severity.Create, "Expanded Element", "basic feat: select extra elements", true, Priority: 300)]
        public static void CreateExpandedElement()
        {
            // note: maybe make compositebuff grant admixture if both wood/void features are picked
            var t = Kineticist.Tree;

            // make sure progression always grants blast feature (basic only)
            t.Earth.Progession.Get().AddComponents(Helper.CreateAddFeatureIfHasFact(t.Earth.BlastFeature));
            t.Fire.Progession.Get().AddComponents(Helper.CreateAddFeatureIfHasFact(t.Fire.BlastFeature));

            // add missing composite cases
            var list = new List<GameAction>();
            foreach (var element in t.GetAll(composite: true))
            {
                if (element.Parent1 != null && element.Parent2 != null)
                {
                    list.Add(Helper.CreateConditional(
                        new Condition[] {
                            new ContextConditionCharacterClass { CheckCaster = true, m_Class = t.Class, MinLevel = 7 },
                            Helper.CreateContextConditionHasFact(element.BlastFeature, true),
                            Helper.CreateContextConditionHasFact(element.Parent1.BlastFeature),
                            Helper.CreateContextConditionHasFact(element.Parent2.BlastFeature)
                        },
                        ifTrue: Helper.CreateContextActionAddFeature(element.BlastFeature).ObjToArray()));
                }
            }

            // add missing boost/admixture cases
            foreach (var boost in t.GetAll(boost: true).Cast<KineticistTree.Boost>())
            {
                list.Add(Helper.CreateConditional(
                    new Condition[] {
                        new ContextConditionCharacterClass { CheckCaster = true, m_Class = t.Class, MinLevel = boost.IsGreaterVersion ? 15 : 7},
                        Helper.CreateContextConditionHasFact(boost.BlastFeature, true),
                        Helper.CreateContextConditionHasFact(boost.Parent1.BlastFeature),
                        Helper.CreateConditionOr(t.GetAll(basic: boost.ModifiesSimple, composite: boost.ModifiesComposite, onlyPhysical: boost.IsOnlyPhysical, onlyEnergy: boost.IsOnlyEnergy)
                                .Select(s => Helper.CreateContextConditionHasFact(s.BlastFeature)).ToArray())
                    },
                    ifTrue: Helper.CreateContextActionAddFeature(boost.BlastFeature).ObjToArray()));
            }
            t.CompositeBuff.Get().GetComponent<AddFactContextActions>().Activated.Actions = list.ToArray();

            // move CompositeBlastBuff to BlastFeature
            Main.RunLast("Expanded Element" + 1, () =>
            {
                foreach (var element in t.GetAll(basic: true))
                    element.BlastFeature.Get()?.AddComponents(Helper.CreateAddFacts(t.CompositeBuff));
            });

            // create new simplified selection
            var expandedElement = Helper.CreateBlueprintFeatureSelection(
                "ExpandedElementSelection",
                "",
                "A kineticist learns to use another element or expands her understanding of her own element. She can choose any element, including her primary element. She gains one of that element's simple blast wild talents that she does not already possess, if any. She also gains all composite blast wild talents whose prerequisites she meets. She doesn't gain the defensive wild talent of the expanded element unless she later selects it with the expanded defense utility wild talent.",
                mode: SelectionMode.OnlyNew
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(t.Class, 7)
                );
            if (t.ElementalScion.Cached != null)
                expandedElement.AddComponents(Helper.CreatePrerequisiteNoArchetype(t.ElementalScion, t.Class)); // disallow expanded element for this archetype
            expandedElement.m_DisplayName = t.FocusSecond.Get().m_DisplayName;

            // add all simple and solo composites to said selection
            Main.RunLast("Expanded Element" + 2, () =>
            {
                var list2 = new List<BlueprintFeatureReference>();
                foreach (var element in t.GetAll(basic: true, composite: true))
                {
                    if (element.Parent1 != null && element.Parent2 == null)
                    {
                        element.BlastFeature.Get()?.AddComponents(Helper.CreatePrerequisiteFeature(element.Parent1.BlastFeature)); // make sure metal and blueflame cannot be taken early
                        list2.Add(element.BlastFeature);
                    }
                    else if (element.Parent1 == null)
                    {
                        list2.Add(element.BlastFeature);
                    }
                }
                expandedElement.m_AllFeatures = list2.ToArray();
            });

            t.ExpandedElement.SetReference(expandedElement);

            // add to feats
            Helper.AddFeats(expandedElement);

            // change prerequisites of wild talents, check for blasts instead of elemental focus since Expanded Element doesn't grant focus
            foreach (var talent in t.GetTalents(utility: true))
            {
                var talent2 = talent.Feature.Get();
                if (talent2 == null)
                    continue;

                var fromlist = talent2.GetComponent<PrerequisiteFeaturesFromList>()?.m_Features;
                if (fromlist != null)
                    for (int i = 0; i < fromlist.Length; i++)
                        swap(ref fromlist[i]);

                foreach (var preq in talent2.GetComponents<PrerequisiteFeature>())
                    swap(ref preq.m_Feature);

                void swap(ref BlueprintFeatureReference original)
                {
                    var guid = original.Guid;
                    var focus = t.GetFocus(f => f.Second.deserializedGuid == guid);
                    if (focus != null)
                    {
                        original = focus.Element1.BlastFeature;
                        return;
                    }

                    focus = t.GetFocus(f => f.Third.deserializedGuid == guid);
                    if (focus != null && focus.Element2 != null)
                    {
                        original = focus.Element2.BlastFeature;
                        return;
                    }
                }
            }
        }

        [PatchInfo(Severity.Create, "Impale Infusion", "infusion: Impale", false)]
        public static void CreateImpaleInfusion()
        {
            var infusion_selection = Helper.Get<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391");
            var weapon = Helper.ToRef<BlueprintItemWeaponReference>("65951e1195848844b8ab8f46d942f6e8");
            var icon = Helper.StealIcon("2aad85320d0751340a0786de073ee3d5"); //TorrentInfusionFeature

            var earth_base = Helper.ToRef<BlueprintAbilityReference>("e53f34fb268a7964caf1566afb82dadd");   //EarthBlastBase
            var earth_blast = Helper.ToRef<BlueprintFeatureReference>("7f5f82c1108b961459c9884a0fa0f5c4");    //EarthBlastFeature

            var metal_base = Helper.ToRef<BlueprintAbilityReference>("6276881783962284ea93298c1fe54c48");   //MetalBlastBase
            var metal_blast = Helper.ToRef<BlueprintFeatureReference>("ad20bc4e586278c4996d4a81b2448998");    //MetalBlastFeature

            var ice_base = Helper.ToRef<BlueprintAbilityReference>("403bcf42f08ca70498432cf62abee434");   //IceBlastBase
            var ice_blast = Helper.ToRef<BlueprintFeatureReference>("a8cc34ca1a5e55a4e8aa5394efe2678e");    //IceBlastFeature


            // impale feat
            var impale_feat = Helper.CreateBlueprintFeature(
                "InfusionImpaleFeature",
                "Impale",
                "Element: earth\nType: form infusion\nLevel: 3\nBurn: 2\nAssociated Blasts: earth, metal, ice\n"
                + "You extend a long, sharp spike of elemental matter along a line, impaling multiple foes. Make a single attack roll against each creature or object in a 30-foot line.",
                icon,
                FeatureGroup.KineticBlastInfusion
                ).SetComponents(
                Helper.CreatePrerequisiteFeaturesFromList(true, earth_blast, metal_blast, ice_blast),
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 6)
                );

            // earth ability
            var earth_impale_ab = Helper.CreateBlueprintAbility(
                "ImpaleEarthBlastAbility",
                impale_feat.m_DisplayName,
                impale_feat.m_Description,
                icon: icon,
                type: AbilityType.SpellLike,
                actionType: UnitCommand.CommandType.Standard,
                range: AbilityRange.Close,
                duration: null,
                savingThrow: null
                ).SetComponents(
                Step1_run_damage(out var actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: true, half: false),
                Step2_rank_dice(twice: false),
                Step3_rank_bonus(half_bonus: false),
                Step4_dc(),
                Step5_burn(actions, infusion: 2, blast: 0),
                Step6_feat(impale_feat),
                Step7_projectile(Resource.Projectile.Kinetic_EarthBlastLine00, true, AbilityProjectileType.Line, 30, 5),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetPoint(CastAnimationStyle.Kineticist);
            actions.InjectCondition(new ContextConditionAttackRoll(weapon));

            // metal ability
            var metal_impale_ab = Helper.CreateBlueprintAbility(
                "ImpaleMetalBlastAbility",
                impale_feat.m_DisplayName,
                impale_feat.m_Description,
                icon: icon,
                type: AbilityType.SpellLike,
                actionType: UnitCommand.CommandType.Standard,
                range: AbilityRange.Close,
                duration: null,
                savingThrow: null
                ).SetComponents(
                Step1_run_damage(out actions, p: PhysicalDamageForm.Bludgeoning | PhysicalDamageForm.Piercing | PhysicalDamageForm.Slashing, isAOE: true, half: false),
                Step2_rank_dice(twice: true),
                Step3_rank_bonus(half_bonus: false),
                Step4_dc(),
                Step5_burn(actions, infusion: 2, blast: 2),
                Step6_feat(impale_feat),
                Step7_projectile(Resource.Projectile.Kinetic_MetalBlastLine00, true, AbilityProjectileType.Line, 30, 5),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetPoint(CastAnimationStyle.Kineticist);
            actions.InjectCondition(new ContextConditionAttackRoll(weapon));

            // ice ability
            var ice_impale_ab = Helper.CreateBlueprintAbility(
                "ImpaleIceBlastAbility",
                impale_feat.m_DisplayName,
                impale_feat.m_Description,
                icon: icon,
                type: AbilityType.SpellLike,
                actionType: UnitCommand.CommandType.Standard,
                range: AbilityRange.Close,
                duration: null,
                savingThrow: null
                ).SetComponents(
                Step1_run_damage(out actions, p: PhysicalDamageForm.Piercing, e: DamageEnergyType.Cold, isAOE: true, half: false),
                Step2_rank_dice(twice: false),
                Step3_rank_bonus(half_bonus: false),
                Step4_dc(),
                Step5_burn(actions, infusion: 2, blast: 2),
                Step6_feat(impale_feat),
                Step7_projectile(Resource.Projectile.Kinetic_IceBlastLine00, true, AbilityProjectileType.Line, 30, 5),
                Step8_spell_description(SpellDescriptor.Cold),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Earth),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Earth)
                ).TargetPoint(CastAnimationStyle.Kineticist);
            actions.InjectCondition(new ContextConditionAttackRoll(weapon));

            // add to feats and append variants
            Helper.AppendAndReplace(ref infusion_selection.m_AllFeatures, impale_feat.ToRef());
            Helper.AddToAbilityVariants(earth_base, earth_impale_ab);
            Helper.AddToAbilityVariants(metal_base, metal_impale_ab);
            Helper.AddToAbilityVariants(ice_base, ice_impale_ab);
        }

        [PatchInfo(Severity.Create, "Chain Infusion", "infusion: Chain", false)]
        public static void CreateChainInfusion()
        {
            // idea: make thunderstorm full electric and add this infusion
            var infusion_selection = Helper.Get<BlueprintFeatureSelection>("58d6f8e9eea63f6418b107ce64f315ea");
            var kineticist_class = Helper.ToRef<BlueprintCharacterClassReference>("42a455d9ec1ad924d889272429eb8391");
            var icon = Helper.StealIcon("645558d63604747428d55f0dd3a4cb58"); //ChainLightning

            var electric_base = Helper.ToRef<BlueprintAbilityReference>("45eb571be891c4c4581b6fcddda72bcd");   //ElectricBlastBase
            var electric_blast = Helper.ToRef<BlueprintFeatureReference>("c2c28b6f6f000314eb35fff49bb99920");    //ElectricBlastFeature

            // chain feat
            var chain_feat = Helper.CreateBlueprintFeature(
                "InfusionChainFeature",
                "Chain",
                "Element: air\nType: form infusion\nLevel: 4\nBurn: 3\nAssociated Blasts: electric\n"
                + "Your electric blast leaps from target to target. When you hit a target with your infused blast, you can attempt a ranged touch attack against an additional target that is within 30 feet of the first. Each additional attack originates from the previous target, which could alter cover and other conditions. Each additional target takes 1d6 fewer points of damage than the last, and you can’t chain the blast back to a previous target. You can continue chaining your blasts until it misses or it's reduced to a single damage die.",
                icon,
                FeatureGroup.KineticBlastInfusion
                ).SetComponents(
                Helper.CreatePrerequisiteFeaturesFromList(true, electric_blast),
                Helper.CreatePrerequisiteClassLevel(kineticist_class, 8),
                Helper.CreatePrerequisiteNoArchetype(Tree.KineticKnight, Tree.Class)
                );

            // electric ability
            var electric_chain_ab = Helper.CreateBlueprintAbility(
                "ChainElectricBlastAbility",
                chain_feat.m_DisplayName,
                chain_feat.m_Description,
                icon: icon,
                type: AbilityType.SpellLike,
                actionType: UnitCommand.CommandType.Standard,
                range: AbilityRange.Close,
                duration: null,
                savingThrow: null
                ).SetComponents(
                Step1_run_damage(out var actions, e: DamageEnergyType.Electricity, isAOE: false, half: false), //p: PhysicalDamageForm.Bludgeoning
                Step2_rank_dice(twice: false),
                Step3_rank_bonus(half_bonus: true),
                Step4_dc(),
                Step5_burn(actions, infusion: 3, blast: 0),
                Step6_feat(chain_feat),
                //Step7_projectile(Resource.Projectile.LightningBolt00, false, AbilityProjectileType.Simple, 0, 0),
                Step7b_chain_projectile(Resource.Projectile.LightningBolt00, Resource.Cache.WeaponBlastEnergy, 0.5f),
                Step8_spell_description(SpellDescriptor.Electricity),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Electric),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Electric)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            electric_chain_ab.SpellResistance = true;
            Helper.AppendAndReplace(ref actions.Actions, new ContextActionChangeRankValue(AbilityRankChangeType.Add, AbilityRankType.DamageDice, -1));

            // thunderstorm ability
            var thunderstorm_chain_ab = Helper.CreateBlueprintAbility(
                "ChainThunderstormBlastAbility",
                chain_feat.m_DisplayName,
                chain_feat.m_Description,
                icon: icon,
                type: AbilityType.SpellLike,
                actionType: UnitCommand.CommandType.Standard,
                range: AbilityRange.Close,
                duration: null,
                savingThrow: null
                ).SetComponents(
                Step1_run_damage(out actions, p: PhysicalDamageForm.Bludgeoning, e: DamageEnergyType.Electricity, isAOE: false, half: false), //p: PhysicalDamageForm.Bludgeoning
                Step2_rank_dice(twice: false),
                Step3_rank_bonus(half_bonus: false),
                Step4_dc(),
                Step5_burn(actions, infusion: 3, blast: 2),
                Step6_feat(chain_feat),
                //Step7_projectile(Resource.Projectile.LightningBolt00, false, AbilityProjectileType.Simple, 0, 0),
                Step7b_chain_projectile(Resource.Projectile.Kinetic_Thunderstorm00_Projectile, Resource.Cache.WeaponBlastPhysical, 0.5f),
                Step8_spell_description(SpellDescriptor.Electricity),
                Step_sfx(AbilitySpawnFxTime.OnPrecastStart, Resource.Sfx.PreStart_Electric),
                Step_sfx(AbilitySpawnFxTime.OnStart, Resource.Sfx.Start_Electric)
                ).TargetEnemy(CastAnimationStyle.Kineticist);
            thunderstorm_chain_ab.SpellResistance = true;
            Helper.AppendAndReplace(ref actions.Actions, new ContextActionChangeRankValue(AbilityRankChangeType.Add, AbilityRankType.DamageDice, -1));

            Helper.AppendAndReplace(ref infusion_selection.m_AllFeatures, chain_feat.ToRef());
            Helper.AddToAbilityVariants(electric_base, electric_chain_ab);
            Helper.AddToAbilityVariants(Tree.Composite_Thunder.BaseAbility, thunderstorm_chain_ab);
        }

        [PatchInfo(Severity.Extend, "Gather Power", "Kineticist Gather Power can be used manually", false, Requirement: typeof(Patch_TrueGatherPowerLevel))]
        public static void PatchGatherPower()
        {
            var gather_original_ab = Helper.Get<BlueprintAbility>("6dcbffb8012ba2a4cb4ac374a33e2d9a"); //GatherPower
            gather_original_ab.Hidden = false;
            gather_original_ab.Animation = CastAnimationStyle.SelfTouch;
            gather_original_ab.AddComponents(new RestrictionCanGatherPowerAbility());
        }

        [PatchInfo(Severity.Extend, "Demon Charge", "Demon Charge also gathers power", true)]
        public static void PatchDemonCharge()
        {
            var charge = Helper.Get<BlueprintAbility>("1b677ed598d47a048a0f6b4b671b8f84"); //DemonChargeMainAbility
            var gather = Helper.ToRef<BlueprintAbilityReference>("6dcbffb8012ba2a4cb4ac374a33e2d9a"); //GatherPower

            Helper.AppendAndReplace(ref charge.GetComponent<AbilityExecuteActionOnCast>().Actions.Actions, new ContextActionCastSpellOnCaster() { m_Spell = gather });
        }

        [PatchInfo(Severity.Extend, "Dark Elementalist QoL", "faster animation and use anywhere, but only out of combat", true)]
        public static void PatchDarkElementalist()
        {
            var soulability = Helper.Get<BlueprintAbility>("31a1e5b27cdb78f4094630610519981c"); //DarkElementalistSoulPowerAbility
            soulability.ActionType = UnitCommand.CommandType.Free;
            soulability.m_IsFullRoundAction = false;
            soulability.HasFastAnimation = true;
            var targets = soulability.GetComponent<AbilityTargetsAround>();
            targets.m_Condition.Conditions = Array.Empty<Condition>();
            soulability.AddComponents(new AbilityRequirementOnlyCombat { Not = true });
        }

        [PatchInfo(Severity.Extend, "Various Tweaks", "bowling works with sandstorm blast, apply PsychokineticistStat setting, fixed Negative Energy Mastery", true)]
        public static void PatchVarious()
        {
            // allow bowling infusion on sandblasts
            var bowling = Helper.Get<BlueprintBuff>("918b2524af5c3f647b5daa4f4e985411"); //BowlingInfusionBuff
            var sandstorm = Helper.ToRef<BlueprintAbilityReference>("b93e1f0540a4fa3478a6b47ae3816f32"); //SandstormBlastBase
            ExpandSubstance(bowling, sandstorm);

            // apply PsychokineticistStat setting
            var pstat = Settings.State.PsychokineticistStat;
            if (pstat != StatType.Wisdom)
            {
                var pfeat = Helper.Get<BlueprintFeature>("2fa48527ba627254ba9bf4556330a4d4"); //PsychokineticistBurnFeature
                pfeat.GetComponent<AddKineticistPart>().MainStat = pstat;

                var presource = Helper.Get<BlueprintAbilityResource>("4b8b95612529a8640bb6e07c580b947b"); //PsychokineticistBurnResource
                presource.m_MaxAmount.ResourceBonusStat = pstat;
            }

            // fixed Negative Energy Mastery
            var negativeEnergy = Helper.Get<BlueprintBuff>("c02841956a5744741a68af974789d06d").GetComponent<AdditionalDiceOnAttack>();
            negativeEnergy.DamageType.Type = DamageType.Energy;
            negativeEnergy.DamageType.Energy = DamageEnergyType.NegativeEnergy;
        }

        [PatchInfo(Severity.Fix, "Spell-like Blasts", "makes blasts register as spell like, instead of supernatural", false, Priority: 300)]
        public static void FixBlastsAreSpellLike()
        {
            Main.RunLast("Spell-like Blasts", () =>
            {
                foreach (var blast in Tree.GetBlasts(bases: true, variants: true, bladeburn: true))
                    blast.Type = AbilityType.SpellLike;
            });
        }

        [PatchInfo(Severity.Fix, "Fix Wall Infusion", "fix Wall Infusion not dealing damage while standing inside", false)]
        public static void FixWallInfusion()
        {
            Main.RunLast("Fix Wall Infusion", () =>
            {
                int counter = 0;
                foreach (var ab in Tree.GetBlasts(variants: true).Where(w => w.name.StartsWith("Wall"))) //Resource.Cache.Ability
                {
                    var abRun = ab.GetComponent<AbilityEffectRunAction>();
                    if (abRun == null || abRun.Actions.Actions[0] is not ContextActionSpawnAreaEffect area)
                        continue;

                    var areaRun = area.AreaEffect.GetComponent<AbilityAreaEffectRunAction>();
                    if (areaRun == null)
                        continue;

                    areaRun.Round = areaRun.UnitEnter;
                    counter++;
                }
                Main.Print("Patched Wall Infusions: " + counter);
            });
        }

        [PatchInfo(Severity.Create, "Selective Metakinesis", "gain selective metakinesis at level 7", true)]
        public static void CreateSelectiveMetakinesis()
        {
            //var empower1 = Helper.Get<BlueprintBuff>("f5f3aa17dd579ff49879923fb7bc2adb"); //MetakinesisEmpowerBuff
            //var empower2 = Helper.Get<BlueprintBuff>("f8d0f7099e73c95499830ec0a93e2eeb"); //MetakinesisEmpowerCheaperBuff
            var kineticist = Helper.Get<BlueprintProgression>("b79e92dd495edd64e90fb483c504b8df"); //KineticistProgression
            var knight = Helper.Get<BlueprintArchetype>("7d61d9b2250260a45b18c5634524a8fb");

            Sprite icon = Helper.Get<BlueprintFeature>("85f3340093d144dd944fff9a9adfd2f2").Icon;
            string displayname = "Metakinesis — Selective";
            string description = "At 7th level, by accepting 1 point of burn, a kineticist can adjust her kinetic blast as if using Selective Spell.";

            BlueprintActivatableAbility ab1 = Helper.CreateBlueprintActivatableAbility(
                "MetakinesisSelectiveAbility",
                out BlueprintBuff buff1,
                displayname,
                description,
                icon: icon
                );
            buff1.SetComponents(
                new AddKineticistBurnModifier() { BurnType = KineticistBurnType.Metakinesis, Value = 1 },
                Helper.CreateAutoMetamagic(Metamagic.Selective, null, AutoMetamagic.AllowedType.KineticistBlast)
                ).Flags(hidden: true, stayOnDeath: true);

            //Tree.MetakinesisBuffs.Register(buff1, g => g.Get() is BlueprintAbility ab &&
            //    (ab.CanTargetPoint
            //    || ab.GetComponent<AbilityTargetsAround>() && ab.CanTargetFriends
            //    || ab.GetComponent<AbilityDeliverChain>()));

            Main.RunLast("Selective Metakinesis", () =>
            {
                var list = new List<BlueprintAbilityReference>();
                foreach (var blast in Tree.GetBlasts(variants: true))
                {
                    if (blast.CanTargetPoint
                        || blast.GetArea()?.Get()?.m_TargetType == BlueprintAbilityAreaEffect.TargetType.Any
                        || blast.GetComponent<AbilityTargetsAround>() && blast.CanTargetFriends
                        || blast.GetComponent<AbilityDeliverChain>()?.m_TargetType == TargetType.Any)
                    {
                        list.Add(blast.ToReference<BlueprintAbilityReference>());
                        blast.AvailableMetamagic |= Metamagic.Selective;
                    }
                }

                buff1.GetComponent<AddKineticistBurnModifier>().m_AppliableTo = list.ToArray();
                buff1.GetComponent<AutoMetamagic>().Abilities = list;
            });

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
        public static void CreateAutoMetakinesis()
        {
            var empower = Helper.Get<BlueprintFeature>("70322f5a2a294e54a9552f77ee85b0a7"); //MetakinesisEmpowerFeature
            var quickenbuff = Helper.Get<BlueprintBuff>("f690edc756b748e43bba232e0eabd004"); //MetakinesisQuickenBuff
            var quickenbuff2 = Helper.Get<BlueprintBuff>("c4b74e4448b81d04f9df89ed14c38a95"); //MetakinesisQuickenCheaperBuff

            var auto = Helper.CreateBlueprintActivatableAbility(
                "MetakinesisAutoAbility",
                out BlueprintBuff autobuff,
                "Metakinesis — Empower/Maximize (Automatic)",
                "Apply Empower and Maxmize automatically depending on leftover gather power burn.",
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

        [PatchInfo(Severity.Create, "Hurricane Queen", "Wild Talent: Hurricane Queen", false, Requirement: typeof(Patch_EnvelopingWindsCap))]
        public static void CreateHurricaneQueen()
        {
            var windsBuff = Helper.Get<BlueprintBuff>("b803fcd9da7b1564fb52978f08372767"); //EnvelopingWindsBuff
            var windsFeat = Helper.ToRef<BlueprintFeatureReference>("bb0de2047c448bd46aff120be3b39b7a");  //EnvelopingWinds
            var windsEffect = Helper.ToRef<BlueprintUnitFactReference>("bbba1600582cf8446bb515a33bd89af8"); //EnvelopingWindsEffectFeature

            var feat = Helper.CreateBlueprintFeature(
                "HurricaneQueen",
                "Hurricane Queen",
                "You are one with the hurricane. Your enveloping winds defense wild talent has an additional 25% chance of deflecting ranged attacks, and your total deflection chance can exceed the usual cap of 75%. All wind and weather (including creatures using the whirlwind monster ability) affect you and your attacks only if you wish them to do so; for example, you could shoot arrows directly through a tornado without penalty."
                ).SetComponents(
                Helper.CreateAddFacts(windsEffect, windsEffect, windsEffect, windsEffect, windsEffect),
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 18),
                Helper.CreatePrerequisiteFeature(windsFeat)
                );

            var ray = new SetAttackerMissChance()
            {
                m_Type = SetAttackerMissChance.Type.RangedTouch,
                Value = Helper.CreateContextValue(AbilitySharedValue.Damage),
                Conditions = Helper.CreateConditionsChecker(0, Helper.CreateContextConditionCasterHasFact(feat.ToRef2()))
            };
            windsBuff.AddComponents(ray);

            Tree.HurricaneQueen.Feature.SetReference(feat);
            Helper.AppendAndReplace(ref Tree.SelectionWildTalent.Get().m_AllFeatures, Tree.HurricaneQueen.Feature);

            // immune to air-elemental whirlwind
            // bb57c37bfb5982d4bbed8d0fea75e404:WildShapeElementalAirWhirlwindDebuff
            // SpecificBuffImmunity(sleet_storm)
            // ignore miss chances
            // ignore weather in UnitPartConcealment.Calculate
        }

        [PatchInfo(Severity.Create, "Mind Shield", "Wild Talent: half Psychokineticist's penalties", true)]
        public static void CreateMindShield()
        {
            var buff = Helper.Get<BlueprintBuff>("a9e3e785ea41449499b6b5d3d22a0856");  //PsychokineticistBurnBuff
            var psychokineticist = Helper.ToRef<BlueprintArchetypeReference>("f2847dd4b12fffd41beaa3d7120d27ad");

            var feature = Helper.CreateBlueprintFeature(
                "MindShieldFeature",
                "Mind Shield",
                "Reduce the penalties of Mind Burn by 1."
                ).SetComponents(
                Helper.CreatePrerequisiteArchetypeLevel(psychokineticist, characterClass: Tree.Class));
            Resource.Cache.FeatureMindShield.SetReference(feature);

            var property = Helper.CreateBlueprintUnitProperty("PsychokineticistMindPropertyGetter")
                .SetComponents(new PropertyMindShield { Feature = feature });

            var rank = buff.GetComponent<ContextRankConfig>();
            rank.m_StepLevel = 1;
            rank.m_CustomProperty = property.ToRef();

            Tree.MindShield.Feature.SetReference(feature);
            Helper.AppendAndReplace(ref Tree.SelectionWildTalent.Get().m_AllFeatures, Tree.MindShield.Feature);
        }

        [PatchInfo(Severity.Create, "Venom Infusion", "infusion: applies sickened or poisons the target", false)]
        public static void CreateVenomInfusion()
        {
            var poison = Helper.CreateBlueprintBuff(
                "VenomInfusionPoisonBuff",
                "Venom Blast Poison",
                "Blast—injury; save Fort; frequency 1/round for 6 rounds; effect 1d2 constitution damage or dexterity if undead; cure 2 consecutive saves; sickened while poisoned.",
                icon: Helper.StealIcon("ba1ae42c58e228c4da28328ea6b4ae34")
                ).SetComponents(
                new BuffPoisonStatDamageFix() { Stat = StatType.Constitution, AltStat = StatType.Dexterity, Value = new DiceFormula(1, DiceType.D2), Ticks = 6, SuccesfullSaves = 2, ConsecutiveSaves = true },
                Helper.CreateAddCondition(Kingmaker.UnitLogic.UnitCondition.Sickened),
                Helper.CreateSpellDescriptorComponent(SpellDescriptor.Poison | SpellDescriptor.Sickened)
                );
            poison.Stacking = StackingType.Poison;

            // venom infusion
            var ab = Helper.CreateBlueprintActivatableAbility(
                "VenomInfusionActivatable",
                out var buff,
                "Venom Infusion",
                "Element: any\nType: substance infusion\nLevel: 3\nBurn: 2\nAssociated Blasts: all\n{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: Fortitude negates\nAll of your blasts are mildly toxic. Creatures that take damage from your blast are sickened for 1 round.",
                group: ActivatableAbilityGroup.SubstanceInfusion,
                icon: Helper.StealIcon("1f788b54e93751d43923596b8e09035d")
                );
            var save = Helper.MakeContextActionSavingThrow(SavingThrowType.Fortitude,
                    succeed: null,
                    failed: Helper.CreateContextActionApplyBuff(Helper.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323"), 1));
            buff.SetComponents(
                Step4_dc(true),
                new RecalculateOnStatChange() { UseKineticistMainStat = true },
                new AddKineticistBurnModifier() { BurnType = KineticistBurnType.Infusion, Value = 2, m_AppliableTo = Tree.BaseAll },
                new AddKineticistInfusionDamageTrigger() { TriggerOnDirectDamage = true, Actions = Helper.CreateActionList(save), CheckSpellParent = true, m_AbilityList = Tree.BaseAll }
                );

            // venom infusion, greater
            var ab_greater = Helper.CreateBlueprintActivatableAbility(
                "VenomInfusionGreaterActivatable",
                out var buff_greater,
                "Venom Infusion, Greater",
                "Element: any\nType: substance infusion\nLevel: 6\nBurn: 3\nAssociated Blasts: all\n{g|Encyclopedia:Saving_Throw}Saving Throw{/g}: Fortitude negates\nYour plant toxin is more virulent. Each time you use this infusion, choose a physical ability score (only constitution). Creatures that take damage from your blast are exposed to your poison and are sickended for its duration.\n\nBlast—injury; save Fort; frequency 1/round for 6 rounds; effect 1d2 constitution damage or dexterity if undead; cure 2 consecutive saves; sickened while poisoned.",
                group: ActivatableAbilityGroup.SubstanceInfusion,
                icon: Helper.StealIcon("46660d0da7797124aa221818778edc9d")
                );
            var save_greater = Helper.MakeContextActionSavingThrow(SavingThrowType.Fortitude,
                    succeed: Helper.CreateContextActionApplyBuff(Helper.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323"), 1),
                    failed: Helper.CreateContextActionApplyBuff(poison, permanent: true));
            buff_greater.SetComponents(
                Step4_dc(true),
                new RecalculateOnStatChange() { UseKineticistMainStat = true },
                new AddKineticistBurnModifier() { BurnType = KineticistBurnType.Infusion, Value = 3, m_AppliableTo = Tree.BaseAll },
                new AddKineticistInfusionDamageTrigger() { TriggerOnDirectDamage = true, Actions = Helper.CreateActionList(save_greater), CheckSpellParent = true, m_AbilityList = Tree.BaseAll }
                );

            // features and adding to lists
            var feat = Helper.CreateBlueprintFeature(
                "VenomInfusion",
                ab.m_DisplayName,
                ab.m_Description,
                icon: Helper.StealIcon("1f788b54e93751d43923596b8e09035d")
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 6),
                Helper.CreateAddFacts(ab)
                );

            var greater = Helper.CreateBlueprintFeature(
                "VenomInfusionGreater",
                ab_greater.m_DisplayName,
                ab_greater.m_Description,
                icon: Helper.StealIcon("46660d0da7797124aa221818778edc9d")
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel(Tree.Class, 12),
                Helper.CreatePrerequisiteFeature(feat),
                Helper.CreateAddFacts(ab_greater)
                );

            Helper.AddInfusion(feat);
            Helper.AddInfusion(greater);
        }

        [PatchInfo(Severity.Fix | Severity.WIP, "Fix Blood Kineticist", "seeks to fix bugs in Blood Kineticist", false)]
        public static void FixBloodKineticist()
        {
            var blood = Tree.Composite_Blood;
            var displayName = blood.BlastFeature.Get().m_DisplayName;
            var description = blood.BlastFeature.Get().m_Description;
            var bladeName = "Blood Blast — Kinetic Blade".CreateString();

            // fix strings
            blood.BaseAbility.Get().m_DisplayName = displayName;
            blood.BaseAbility.Get().m_Description = description;
            blood.Blade.Damage.Get().m_DisplayName = displayName;
            blood.Blade.Damage.Get().m_Description = description;

            blood.Blade.Activatable.Get().m_DisplayName = bladeName;
            blood.Blade.Damage.Get().m_DisplayName = bladeName;

            // update to new bleed mechanic
            var bleed = Helper.Get<BlueprintBuff>("492a8156ecede6345a8e82475eed85ac"); //BleedingInfusionBuff
            bleed.AddComponents(
                Helper.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, feature: Tree.KineticBlast));
            bleed.GetComponent<AddKineticistInfusionDamageTrigger>().Actions.Actions[0]
                = Helper.MakeContextActionSavingThrow(SavingThrowType.Fortitude, null, new ContextActionIncreaseBleed(false));

            // make vampiric infusion activate immediately
            var healer = Helper.Get<BlueprintAbility>("eff667a3a43a77d45a193bb7c94b3a6c"); //KineticHealerAbility
            var vampiric = Helper.Get<BlueprintBuff>("e50e653cff511cd49a55b979346699f1"); //VampiricInfusionBuff
            var onCaster = vampiric.GetComponent<AddKineticistInfusionDamageTrigger>().Actions.Actions[0] as ContextActionOnContextCaster;
            onCaster.Actions.Actions = healer.GetComponent<AbilityEffectRunAction>().Actions.Actions;
            vampiric.AddComponents(
                healer.GetComponents<ContextRankConfig>(),
                healer.GetComponents<ContextCalculateSharedValue>()
                );

            // fix spell descriptors and cached damage info
            foreach (var ab in blood.BaseAbility.Get().AbilityVariants.Variants)
            {
                var spelldescriptor = ab.GetComponent<SpellDescriptorComponent>();
                if (spelldescriptor != null)
                    spelldescriptor.Descriptor &= ~SpellDescriptor.Electricity;

                var damageInfo = ab.GetComponent<AbilityKineticist>()?.CachedDamageInfo;
                for (int i = (damageInfo?.Count ?? 0) - 1; i >= 0; i--)
                {
                    var info = damageInfo[i];
                    if (info.Type.IsEnergy)
                    {
                        damageInfo.RemoveAt(i);
                        continue;
                    }
                    info.Half = false;
                    damageInfo[i] = info;
                }
            }
        }

        [PatchInfo(Severity.Create, "Elemental Scion (3PP)", "new Kineticist archetype", false)]
        public static void CreateElementalScion()
        {
            var comps = new List<BlueprintComponent>();

            /* https://libraryofmetzofitz.fandom.com/wiki/Elemental_Scion
            
            Elemental Heart (Su)
            The devotion of an elemental scion supersedes all others. When an elemental scion selects an element for their elemental focus class feature, they can choose to either gain both associated simple blasts for their element (if it has two different simple blasts) or permanently increase the damage die of their chosen simple blast by one step (1 > d2 > d3 > d4 > d6 > d8 > d10 > d12). They cannot increase the damage die beyond d12 in this way.
            This alters elemental focus and replaces the 1st-level infusion.

            Focused Element (Su)
            At 7th level, an elemental scion gains a composite blast that requires the expanded element for their primary element (such as metal blast for earth). An elemental scion is treated as 2 levels higher for the purpose of which infusions and utility wild talents they can select, as well as increasing the DCs of their infusions and wild talents by +1. In addition, they also gain an additional utility wild talent or infusion. If an elemental scion did not increase the damage die of their simple blast at 1st level, they can choose to do so for one of their simple blasts in place of the infusion or utility wild talent gained with this ability.
            This replaces the 7th-level expanded element.

            Elemental Master (Su)
            At 15th level, an elemental scion increases the DCs of their infusions and wild talents by an additional +1, as well as increasing the damage of all simple and composite blasts they possess by 1 additional step (to a maximum size of d12). In addition, they also gain an additional utility wild talent or infusion.
            This replaces the 15th-level expanded element.

            Elemental Embodiment (Su)
            At 20th level, an elemental scion has reached their peak of power. An elemental scion treats all infusions and wild talents as though they had accepted 1 point of burn or increased the burn cost by 1 for the purpose of their effects. They also gain an additional infusion and an additional utility wild talent.
            This replaces omnikinesis. 
            */

            var scion = Helper.CreateBlueprintArchetype(
                "ElementalScionArchetype",
                "Elemental Scion",
                "There are some who, instead of focusing on the powers beyond their own, decide to concentrate on their innate talents to acquire powers far beyond a minor dabbler. All elements are equally likely to embark on such singular focus, confident in their innate gifts."
                );

            // Elemental Heart
            var f1_increaseDamage = Helper.CreateBlueprintFeature(
                "KineticBlastDiceIncrease",
                "Increase Kinetic Blast Dice",
                "Permanently increase the damage die of the chosen simple blast by one step (1 > d2 > d3 > d4 > d6 > d8 > d10 > d12)."
                ).SetComponents(
                new KineticBlastDiceIncrease(true)
                );

            foreach (var focus in Tree.GetFocus().Where(w => w.Element2 != null && w.Element1 != null))
            {
                comps.Add(Helper.CreatePrerequisiteFeature(focus.First, true));
                comps.Add(Helper.CreateAddFeatureIfHasFact(focus.First, focus.Element1.BlastFeature));
                comps.Add(Helper.CreateAddFeatureIfHasFact(focus.First, focus.Element2.BlastFeature));
            }
            var f1_getSecondBlast = Helper.CreateBlueprintFeature(
                "ElementalScionSecondBlast",
                "Other Simple Blast",
                "Gain both associated simple blasts for your element."
                ).SetComponents(comps.ToArray());

            var f1_selection = Helper.CreateBlueprintFeatureSelection(
                "ElementalScionElementalHeart",
                "Elemental Heart",
                "The devotion of an elemental scion supersedes all others. When an elemental scion selects an element for their elemental focus class feature, they can choose to either gain both associated simple blasts for their element (if it has two different simple blasts) or permanently increase the damage die of their chosen simple blast by one step (1 > d2 > d3 > d4 > d6 > d8 > d10 > d12).\nThis alters elemental focus and replaces the 1st-level infusion."
                ).SetSelection(f1_increaseDamage, f1_getSecondBlast);

            // Focused Element
            comps.Clear();
            foreach (var focus in Tree.GetFocus())
            {
                comps.Add(Helper.CreateAddFeatureIfHasFact(focus.First, focus.Composite.BlastFeature));
            }
            comps.Add(Helper.CreateAddFacts(Tree.CompositeBuff));
            var f7_focusedElement = Helper.CreateBlueprintFeatureSelection(
                "ElementalScionFocusedElement",
                "Focused Element",
                "At 7th level, an elemental scion gains a composite blast that requires the expanded element for their primary element (such as metal blast for earth). An elemental scion is treated as 2 levels higher for the purpose of which infusions and utility wild talents they can select, as well as increasing the DCs of their infusions and wild talents by +1. In addition, they also gain an additional utility wild talent or infusion. If an elemental scion did not increase the damage die of their simple blast at 1st level, they can choose to do so for one of their simple blasts in place of the infusion or utility wild talent gained with this ability.\nThis replaces the 7th-level expanded element."
                ).SetComponents(
                Helper.CreateClassLevelsForPrerequisites(Tree.Class, 2),
                new KineticistIncreaseDC(1)
                ).SetSelection(f1_increaseDamage, f1_getSecondBlast, Tree.ExtraWildTalent);
            f7_focusedElement.AddComponents(comps.ToArray());

            // Elemental Master
            var f15_elementalMaster = Helper.CreateBlueprintFeature(
                "ElementalScionElementalMaster",
                "Elemental Master",
                "At 15th level, an elemental scion increases the DCs of their infusions and wild talents by an additional +1, as well as increasing the damage of all simple and composite blasts they possess by 1 additional step (to a maximum size of d12). In addition, they also gain an additional utility wild talent or infusion.\nThis replaces the 15th-level expanded element."
                ).SetComponents(
                new KineticistIncreaseDC(1),
                new KineticBlastDiceIncrease(false)
                );

            // Elemental Embodiment
            var f20_elementalEmbodiment = Helper.CreateBlueprintFeature(
                "ElementalScionElementalEmbodiment",
                "Elemental Embodiment",
                "At 20th level, an elemental scion has reached their peak of power. An elemental scion treats all infusions and wild talents as though they had accepted 1 point of burn or increased the burn cost by 1 for the purpose of their effects. They also gain an additional infusion and an additional utility wild talent.\nThis replaces omnikinesis."
                ).SetComponents(
                Helper.CreateAddFacts("bbba1600582cf8446bb515a33bd89af8", "fc083e19a8c961c4890de1a36e2b5c20"), // EnvelopingWindsEffectFeature ShroudOfWaterUpgradeFeature
                new AddKineticistBurnModifier
                {
                    BurnType = KineticistBurnType.WildTalent,
                    BurnValue = 1,
                    m_AppliableTo = new BlueprintAbilityReference[]
                    {
                        //Helper.ToRef<BlueprintAbilityReference>("3c030a62a0efa1c419ecf315a9d694ef"), //SlickAbility
                        //Helper.ToRef<BlueprintAbilityReference>("80e7e30cdf96be0418a615ebb38ea4b9"), //Celerity
                        //Helper.ToRef<BlueprintAbilityReference>("c3a13237b17de5742a2dbf2da46f23d5"), //FlameShieldAbility
                    }
                });
            PropertyKineticistBurn.ElementalEmbodiment = f20_elementalEmbodiment; // this fakes 1 burn more than you actually have; for Adaptation talents
            Helper.Get<BlueprintUnitProperty>("02c5943c77717974cb7fa1b7c0dc51f8").SetComponents(new PropertyKineticistBurn()); //BurnNumberProperty

            Helper.Get<BlueprintFeature>("a942347023fedb2419f8bdbb4450e528").AddComponents(new ContextSharedBonus(1, AbilitySharedValue.Damage)); //FleshOfStoneEffectFeature
            Helper.Get<BlueprintFeature>("642bb6097c37b3b4b8be1f46d2d9296e").AddComponents(new ContextSharedBonus(1, AbilitySharedValue.Damage)); //SearingFleshEffectFeature
            Helper.Get<BlueprintAbility>("41281aa38b6b27f4fa3a05c97cc01783").ReplaceComponent(default(AbilityAcceptBurnOnCast), new AbilityAcceptBurnOnCast2()); //AerialEvasionAbility

            scion.SetAddFeatures(
                Helper.CreateLevelEntry(1, f1_selection),
                Helper.CreateLevelEntry(7, f7_focusedElement),
                Helper.CreateLevelEntry(15, f15_elementalMaster),
                Helper.CreateLevelEntry(15, Tree.ExtraWildTalent),
                Helper.CreateLevelEntry(20, f20_elementalEmbodiment),
                Helper.CreateLevelEntry(20, Tree.SelectionInfusion),
                Helper.CreateLevelEntry(20, Tree.SelectionWildTalent)
                );
            scion.SetRemoveFeatures(
                Helper.CreateLevelEntry(1, "58d6f8e9eea63f6418b107ce64f315ea"), //InfusionSelection
                Helper.CreateLevelEntry(7, "4204bc10b3d5db440b1f52f0c375848b"),  //SecondatyElementalFocusSelection
                Helper.CreateLevelEntry(15, "e2c1718828fc843479f18ab4d75ded86")  //ThirdElementalFocusSelection
                );

            Tree.ElementalScion.SetReference(scion);
            Helper.AppendAndReplace(ref Tree.Class.Get().m_Archetypes, Tree.ElementalScion); // add to character class selection
        }

        [PatchInfo(Severity.Create, "Kinetic Fist", "infusion", false)]
        public static void CreateKineticFist()
        {
            var act = Helper.CreateBlueprintActivatableAbility(
                "KineticFistActivatable",
                out var _,
                "Kinetic Fist",
                "Element: universal\nType: form infusion\nLevel: 1\nBurn: 1\nAssociated Blasts: any\nYou surround your body with energy or elemental matter from your kinetic abilities. You can use this form infusion as part of an attack action, a charge action, or a full-attack action to add damage to each of your natural attacks and unarmed strikes until the beginning of your next turn. Since kinetic fist is part of another action, using this wild talent doesn’t provoke any additional attack of opportunity. You deal an additional 1d6 points of damage per 3 dice of your kinetic blast’s damage (minimum 1d6), and this damage is of the same type as your kinetic blast’s damage. This extra damage ignores spell resistance and doesn’t apply any modifiers to your kinetic blast’s damage, such as your Constitution modifier.",
                icon: Helper.StealIcon("a29a582c3daa4c24bb0e991c596ccb28")
                ).SetComponents(
                new KineticEnergizeFist(true)
                );

            var feat = Helper.CreateBlueprintFeature(
                "KineticFistInfusion"
                ).SetUIData(act
                ).SetComponents(
                Helper.CreateAddFacts(act)
                );

            Helper.AddInfusion(feat);
            Tree.KineticFist.Feature.SetReference(feat);
        }

        [PatchInfo(Severity.Create, "Energize Weapon", "infusion", false)]
        public static void CreateKineticEnergizeWeapon()
        {
            var act = Helper.CreateBlueprintActivatableAbility(
                "EnergizeWeaponActivatable",
                out var _,
                "Energize Weapon",
                "Element: universal\nType: form infusion\nLevel: 1\nBurn: 1\nAssociated Blasts: any\nYou imbue your manufactured weapons (not an unarmed strike or natural attack) with your elemental energy as part of an attack action, charge action, or full-attack action to deal an additional 1d6 points of damage to each of your attacks with these weapons until the beginning of your next turn. At 7th level and every 6 levels thereafter, this bonus damage increases by 1d6 points. Composite blasts deal double this additional damage.\n\nThe additional damage is of the same type as the infused blast’s damage. This additional damage ignores spell resistance and doesn’t apply any modifiers to your kinetic blast’s damage (such as your Constitution modifier).",
                icon: Helper.StealIcon("d7fdd79f0f6b6a2418298e936bb68e40")
                ).SetComponents(
                new KineticEnergizeFist(false)
                );

            var feat = Helper.CreateBlueprintFeature(
                "EnergizeWeaponInfusion"
                ).SetUIData(act
                ).SetComponents(
                Helper.CreateAddFacts(act)
                );

            Helper.AddInfusion(feat);
            Tree.EnergizeWeapon.Feature.SetReference(feat);
        }

        [PatchInfo(Severity.Create, "Fix Expanded Element", "fix missing talents and mastery when picking the same element focus multiple times", false, Priority: 300)]
        public static void FixExpandedElementFocus()
        {
            var mastery = Helper.CreateBlueprintFeatureSelection(
                "KineticistMastery123",
                "Greater Element Mastery",
                "A Kineticist focusing on her primary element gains a bonus wild talent. For blasts of her element, the kineticist gains a +1 bonus on attack rolls and damage rolls, as well as to caster level and DCs."
                ).SetComponents(
                new KineticExpandedMastery()
                );
            mastery.m_AllFeatures = Tree.GetTalents(form: true, substance: true, utility: true).Select(s => s.Feature).ToArray();

            foreach (var focus in Tree.GetFocus())
            {
                focus.Second.Get()?.AddComponents(
                    new AddFeatureOnApplyPrerequisite(1, Tree.ExtraWildTalent, focus.First, focus.Knight)
                    );

                focus.Third.Get()?.AddComponents(
                    new AddFeatureOnApplyPrerequisite(2, mastery, focus.First, focus.Second, focus.Knight)
                    ).RemoveComponents<PrerequisiteNoFeature>();
            }
        }

        [PatchInfo(Severity.Create | Severity.WIP | Severity.Hidden, "Elemental Ascetic", "new Kineticist archetype", false)]
        public static void CreateElementalAscetic()
        {
            var burnFeature = Helper.Get<BlueprintFeature>("57e3577a0eb53294e9d7cc649d5239a3");

            /*
            Combining the elemental powers of a kineticist with the rigid physical discipline of a monk, an elemental ascetic channels his powers through his body to enhance himself in combat.
            
            Elemental Flurry (Su)
            At 1st level, an elemental ascetic gains Improved Unarmed Strike as a bonus feat. He gains the kinetic fist form infusion and it costs 0 points of burn instead of 1 point of burn. When using the kinetic fist form infusion with a full attack, he can make a flurry of blows as the monk class feature. He must use only his fists to make this flurry, no matter what other abilities he possesses.
            Like a monk, he can use this ability only when unarmored, not using a shield, and unencumbered. He can’t use his kinetic blast without a form infusion, nor can he ever use his kinetic blast with the chain, extended range, extreme range, foe throw, flurry of blasts, many throw, or snake form infusions, or with any other form infusion that requires a ranged attack roll or ranged touch attack roll.
            This ability alters kinetic blast and replaces elemental overflow.

            Elemental Wisdom (Su)
            An elemental ascetic can use his Wisdom modifier instead of his Constitution modifier to determine the DCs of Constitution-based wild talents, the duration of wild talents with a Constitution-based duration, and his bonus on concentration checks for wild talents.
            This ability alters the key ability scores of wild talents.

            AC Bonus (Ex)
            At 2nd level, when unarmored, not using a shield, and unencumbered, an elemental ascetic adds his Wisdom bonus (if any) to his AC and his CMD.
            These bonuses to AC apply even against touch attacks or when the elemental ascetic is flat-footed. He loses these bonuses when he is immobilized or helpless. This bonus increases by 1 for every 4 kineticist levels the elemental ascetic possesses beyond 2nd. The elemental ascetic’s kineticist levels stack with monk levels for the purpose of determining when the bonus increases. An elemental ascetic can never take the expanded defense utility wild talent.
            This ability replaces elemental defense.

            Powerful Fist (Su)
            At 5th level, an elemental ascetic can accept 2 additional points of burn when using kinetic fist to increase that infusion’s damage dice from d6s to d8s.
            At 9th level, he can instead accept 3 additional points of burn to increase the damage dice from d6s to d10s. At 13th level, he can instead accept 4 additional points of burn to increase the damage dice from d6s to d12s. All of these options count as burn from a form infusion and can thus be reduced by infusion specialization.
            This ability replaces the 5th-, 9th-, and 13th-level infusions.
             */

            var ascetic = Helper.CreateBlueprintArchetype(
                "ElementalAsceticArchetype",
                "Elemental Ascetic",
                "Combining the elemental powers of a kineticist with the rigid physical discipline of a monk, an elemental ascetic channels his powers through his body to enhance himself in combat."
                );

            var flurry_feat = Helper.CreateBlueprintFeature(
                "AsceticElementalFlurry",
                "Elemental Flurry",
                "At 1st level, an elemental ascetic gains Improved Unarmed Strike as a bonus feat. He gains the kinetic fist form infusion and it costs 0 points of burn instead of 1 point of burn. When using the kinetic fist form infusion with a full attack, he can make a flurry of blows as the monk class feature. He must use only his fists to make this flurry, no matter what other abilities he possesses.\nLike a monk, he can use this ability only when unarmored, not using a shield, and unencumbered. He can’t use his kinetic blast without a form infusion, nor can he ever use his kinetic blast with the chain, extended range, extreme range, foe throw, flurry of blasts, many throw, or snake form infusions, or with any other form infusion that requires a ranged attack roll or ranged touch attack roll."
                ).SetComponents(
                new AddKineticistBurnModifier { BurnType = KineticistBurnType.Infusion, Value = -1, m_AppliableTo = Tree.DefaultAbility.Variants.ToArray() }
                );

            var wisdom_feat = burnFeature.Clone(
                "AsceticElementalWisdom"
                ).SetUIData(
                "Elemental Wisdom",
                "An elemental ascetic can use his Wisdom modifier instead of his Constitution modifier to determine the DCs of Constitution-based wild talents, the duration of wild talents with a Constitution-based duration, and his bonus on concentration checks for wild talents.\n"
                );
            wisdom_feat.GetComponent<AddKineticistPart>().MainStat = StatType.Wisdom;

            var monk_ac = Helper.CreateBlueprintFeature(
                "AsceticMonkACBonus",
                "AC Bonus",
                "At 2nd level, when unarmored, not using a shield, and unencumbered, an elemental ascetic adds his Wisdom bonus (if any) to his AC and his CMD.\nThese bonuses to AC apply even against touch attacks or when the elemental ascetic is flat-footed. He loses these bonuses when he is immobilized or helpless. This bonus increases by 1 for every 4 kineticist levels the elemental ascetic possesses beyond 2nd. The elemental ascetic’s kineticist levels stack with monk levels for the purpose of determining when the bonus increases. An elemental ascetic can never take the expanded defense utility wild talent.\nThis ability replaces elemental defense."
                ).SetComponents(
                Helper.CreateAddFeatureIfHasFact("2615c5f87b3d72b42ac0e73b56d895e0"), //MonkACBonusUnlock
                Tree.GetFocus().Select(s => Helper.CreateRemoveFeatureOnApply(s.Defense)) // remove any defense talent the progression usually grants
                );

            var pfist = Helper.CreateBlueprintActivatableAbility(
                "AsceticPowerfulFistActivatable",
                out var _,
                "Powerful Fist",
                "At 5th level, an elemental ascetic can accept 2 additional points of burn when using kinetic fist to increase that infusion’s damage dice from d6s to d8s.\nAt 9th level, he can instead accept 3 additional points of burn to increase the damage dice from d6s to d10s. At 13th level, he can instead accept 4 additional points of burn to increase the damage dice from d6s to d12s. All of these options count as burn from a form infusion and can thus be reduced by infusion specialization.",
                icon: Helper.StealIcon("5d7c3a3eed0546a598e3d2a1c7e0026a")
                ).SetComponents(
                new KineticistPowerfulFist()
                );
            var power_feat = Helper.CreateBlueprintFeature(
                "AsceticPowerfulFist"
                ).SetUIData(
                pfist
                ).SetComponents(
                Helper.CreateAddFacts(pfist)
                );

            ascetic.SetAddFeatures(
                Helper.CreateLevelEntry(1, wisdom_feat, flurry_feat, Tree.KineticFist.Feature, "7812ad3672a4b9a4fb894ea402095167", "fd99770e6bd240a4aab70f7af103e56a"), //ImprovedUnarmedStrike, MonkFlurryOfBlowstUnlock
                Helper.CreateLevelEntry(2, monk_ac),
                Helper.CreateLevelEntry(5, power_feat)
                );

            ascetic.SetRemoveFeatures(
                Helper.CreateLevelEntry(1, burnFeature, "86beb0391653faf43aec60d5ec05b538"), //ElementalOverflowProgression
                Helper.CreateLevelEntry(3, "2496916d8465dbb4b9ddeafdf28c67d8"), //ElementalOverflowBonusFeature
                Helper.CreateLevelEntry(5, "58d6f8e9eea63f6418b107ce64f315ea"), //InfusionSelection
                Helper.CreateLevelEntry(9, "58d6f8e9eea63f6418b107ce64f315ea"), //InfusionSelection
                Helper.CreateLevelEntry(13, "58d6f8e9eea63f6418b107ce64f315ea") //InfusionSelection
                );

            Tree.ElementalAscetic.SetReference(ascetic);
            Helper.AppendAndReplace(ref Tree.Class.Get().m_Archetypes, Tree.ElementalAscetic); // add to character class selection

            Tree.ExpandedDefense.Feature.Get().AddComponents(
                Helper.CreatePrerequisiteNoArchetype(Tree.ElementalAscetic, Tree.Class)
                );

            // add to monk ac progression
            var ac = Helper.Get<BlueprintBuff>("f132c4c4279e4646a05de26635941bfe").GetComponents<ContextRankConfig>().First(f => f.m_BaseValueType == ContextRankBaseValueType.SummClassLevelWithArchetype);
            ac.m_AdditionalArchetypes = Helper.Append(ac.m_AdditionalArchetypes, Tree.ElementalAscetic);

            Main.RunLast("Elemental Flurry", () =>
            {
                // disallow all form infusions with ranged attack rolls
                foreach (var infusion in Tree.GetTalents(form: true))
                {
                    if (!infusion.RequiresRangedAttackRoll)
                        continue;

                    var feat = infusion.Feature.Get();
                    if (feat == null)
                        continue;

                    feat.AddComponents(Helper.CreatePrerequisiteNoArchetype(Tree.ElementalAscetic, Tree.Class));
                }

                // hide base form
                foreach (var variant in Tree.DefaultAbility.Variants)
                {
                    var ab = variant.Get();
                    if (ab == null)
                        continue;

                    ab.AddComponents(Helper.CreateAbilityShowIfCasterHasFact(flurry_feat, true));
                }
            });
        }

        [PatchInfo(Severity.Fix | Severity.WIP | Severity.Hidden, "Blade Whirlwind", "fix Blade Whirlwind reach", false)]
        public static void FixBladeWhirlwind()
        {
            var whirlwind = Tree.BladeWhirlwind.Activator.GetBlueprint() as BlueprintAbility;
            whirlwind.ReplaceComponent(default(AbilityTargetsAround), new AbilityTargetsWeaponReach(10.Feet()));
        }

        #region Helper

        /// <summary>
        /// 1) make BlueprintAbility
        /// 2) set SpellResistance
        /// 3) make components with helpers (step1 to 9)
        /// 4) set m_Parent to XBlastBase with Helper.AddToAbilityVariants
        /// Logic for dealing damage. Will make a composite blast, if both p and e are set. How much damage is dealt is defined in step 2.
        /// </summary>
        public static AbilityEffectRunAction Step1_run_damage(out ActionList actions, PhysicalDamageForm p = 0, DamageEnergyType e = (DamageEnergyType)255, SavingThrowType save = SavingThrowType.Unknown, bool isAOE = false, bool half = false)
        {
            ContextDiceValue dice = Helper.CreateContextDiceValue(DiceType.D6, AbilityRankType.DamageDice, AbilityRankType.DamageBonus);

            List<ContextAction> list = new(2);

            bool isComposite = p != 0 && e != (DamageEnergyType)255;

            if (p != 0)
                list.Add(Helper.CreateContextActionDealDamage(p, dice, isAOE, isAOE, false, half, isComposite, AbilitySharedValue.DurationSecond, writeShare: isComposite));
            if (e != (DamageEnergyType)255)
                list.Add(Helper.CreateContextActionDealDamage(e, dice, isAOE, isAOE, false, half, isComposite, AbilitySharedValue.DurationSecond, readShare: isComposite));

            var runaction = Helper.CreateAbilityEffectRunAction(save, ifFalse: list.ToArray());
            actions = runaction.Actions;
            return runaction;
        }

        /// <summary>
        /// Defines damage dice. Set twice for composite blasts that are pure energy or pure physical. You shouldn't need half at all.
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
                feature: Tree.KineticBlast); //KineticBlastFeature
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
                customProperty: Tree.KineticistMainStatProperty); //KineticistMainStatProperty
            return rankdice;
        }

        /// <summary>
        /// If useMainStat is false; make DC dex based (form infusion).
        /// IF useMainStat is true; make DC con based (substance infusion).
        /// </summary>
        public static ContextCalculateAbilityParamsBasedOnClass Step4_dc(bool useMainStat = false)
        {
            var dc = new ContextCalculateAbilityParamsBasedOnClass();
            dc.StatType = StatType.Dexterity;
            dc.UseKineticistMainStat = useMainStat;
            dc.m_CharacterClass = Tree.Class; //KineticistClass
            return dc;
        }

        /// <summary>
        /// Creates damage tooltip from the run-action. Defines burn cost. Blast cost is 0, except for composite blasts which is 2. Talent is not used.
        /// </summary>
        public static AbilityKineticist Step5_burn(ActionList actions, int infusion = 0, int blast = 0, int talent = 0)
        {
            var comp = new AbilityKineticist();
            comp.InfusionBurnCost = infusion;
            comp.BlastBurnCost = blast;
            comp.WildTalentBurnCost = talent;

            if (actions?.Actions == null)
                return comp;

            for (int i = 0; i < actions.Actions.Length; i++)
            {
                if (actions.Actions[i] is not ContextActionDealDamage action)
                    continue;
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
                projectile_guid,
                type,
                weapon,
                length.Feet(),
                width.Feet());
            return projectile;
        }

        /// <summary>
        /// Alternative projectile. Requires attack roll, if weapon is not null.
        /// </summary>
        public static AbilityDeliverChainAttack Step7b_chain_projectile(string projectile_guid, [CanBeNull] BlueprintItemWeaponReference weapon, float delay = 0f)
        {
            var result = new AbilityDeliverChainAttack();
            result.TargetsCount = Helper.CreateContextValue(AbilityRankType.DamageDice);
            result.TargetType = TargetType.Enemy;
            result.Weapon = weapon;
            result.Projectile = projectile_guid.ToRef<BlueprintProjectileReference>();
            result.DelayBetweenChain = delay;
            return result;
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
}
