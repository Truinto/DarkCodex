using CodexLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class MartialArt
    {
        [PatchInfo(Severity.Create | Severity.WIP, "Paladin Virtuous Bravo", "archetype", false)]
        public static void CreatePaladinVirtuousBravo()
        {
            var paladin = Helper.ToRef<BlueprintCharacterClassReference>("bfa11238e7ae3544bbeb4d0b92e897ec"); //PaladinClass

            var archetype = Helper.CreateBlueprintArchetype(
                "VirtuousBravoArchetype",
                "Virtuous Bravo",
                "Although no less a beacon of hope and justice than other paladins, virtuous bravos rely on their wit and grace rather than might and strong armor.",
                removeSpellbook: true
                );

            var f1_prof = Helper.CreateBlueprintFeature(
                "VirtuousBravoProficiencies",
                "Virtuous Bravo Proficiencies",
                "Virtuous bravos are proficient with all simple and martial weapons, with light and medium armor, and with bucklers."
                ).SetComponents(
                Helper.CreateAddFacts("6d3728d4e9c9898458fe5e9532951132", "46f4fb320f35704488ba3d513397789d", "e70ecf1ed95ca2f40b754f1adb22bbdd", "203992ef5b35c864390b4e4a1e200629", "7c28228ce4eed1543a6b670fd2a88e72")
                );

            var f1_finesse = Helper.CreateBlueprintFeature(
                "VirtuousBravoFinesse",
                "Bravo’s Finesse",
                "A virtuous bravo gains Weapon Finesse as a bonus feat. She can use her Charisma score in place of her Intelligence score to meet prerequisites of combat feats."
                ).SetComponents(
                Helper.CreateAddFeatureOnApply("90e54424d682d104ab36436bd527af09"), //WeaponFinesse
                new ReplaceStatForPrerequisites() { Policy = ReplaceStatForPrerequisites.StatReplacementPolicy.NewStat, OldStat = StatType.Intelligence, NewStat = StatType.Charisma }
                );

            var f3_nimble = Helper.CreateBlueprintFeature(
                "VirtuousBravoNimble",
                "Nimble",
                "At 3rd level, a virtuous bravo gains a +1 dodge bonus to AC while wearing light armor or no armor.\nAnything that causes the virtuous bravo to lose her Dexterity bonus to AC also causes her to lose this dodge bonus. This bonus increases by 1 for every 4 paladin levels beyond 3rd (to a maximum of +5 at 19th level).\nThis ability replaces mercy."
                ).SetComponents(
                new CreateAddStatBonusInArmor(Resource.ValueRank, StatType.AC, ModifierDescriptor.Dodge, ArmorProficiencyGroup.None, ArmorProficiencyGroup.Light),
                Helper.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.StartPlusDivStep, AbilityRankType.Default,
                    startLevel: 3, stepLevel: 4, classes: paladin.ObjToArray())
                );

            var resourcePanache = Helper.CreateBlueprintAbilityResource(
                "PanacheResource",
                "Panache",
                "A swashbuckler fights with panache: a fluctuating measure of a swashbuckler’s ability to perform amazing actions in combat. At the start of each day, a swashbuckler gains a number of panache points equal to her Charisma modifier (minimum 1). Her panache goes up or down throughout the day, but cannot go higher than her Charisma modifier (minimum 1). A swashbuckler spends panache to accomplish deeds, and regains panache in the following ways.\nCritical Hit with a Light or One-Handed Piercing Melee Weapon: Each time the swashbuckler confirms a critical hit with a light or one-handed piercing melee weapon, she regains 1 panache point. Confirming a critical hit on a helpless or unaware creature or a creature that has fewer Hit Dice than half the swashbuckler’s character level doesn’t restore panache.\nKilling Blow with a Light or One-Handed Piercing Melee Weapon: When the swashbuckler reduces a creature to 0 or fewer hit points with a light or one-handed piercing melee weapon attack while in combat, she regains 1 panache point. Destroying an unattended object, reducing a helpless or unaware creature to 0 or fewer hit points, or reducing a creature that has fewer Hit Dice than half the swashbuckler’s character level to 0 or fewer hit points doesn’t restore any panache.",
                stat: StatType.Charisma
                ).ToReference<BlueprintAbilityResourceReference>();

            var panache1 = Helper.CreateBlueprintFeature(
                "Panache_PreciseStrike",
                "Precise Strike",
                "At 3rd level, while she has at least 1 panache point, a swashbuckler gains the ability to strike precisely with a light or one-handed piercing melee weapon (though not natural weapon attacks), adding her swashbuckler level to the damage dealt. To use this deed, a swashbuckler cannot attack with a weapon in her other hand or use a shield other than a buckler. She can even use this ability with thrown light or one-handed piercing melee weapons, so long as the target is within 30 feet of her. Any creature that is immune to sneak attacks is immune to the additional damage granted by precise strike, and any item or ability that protects a creature from critical hits also protects a creature from the additional damage of a precise strike. This additional damage is precision damage, and isn’t multiplied on a critical hit."
                ).SetComponents(
                new DuelistPreciseStrike() { m_Duelist = paladin }
                );

            var panache2 = Helper.CreateBlueprintFeature(
                "Panache_SwashbucklerInitiative",
                "Swashbuckler Initiative",
                "At 3rd level, while the swashbuckler has at least 1 panache point, she gains a +2 bonus on initiative checks. In addition, if she has the Quick Draw feat, her hands are free and unrestrained, and she has any single light or one-handed piercing melee weapon that isn’t hidden, she can draw that weapon as part of the initiative check."
                ).SetComponents(
                Helper.CreateAddStatBonus(2, StatType.Initiative)
                );

            var panache3 = Helper.CreateBlueprintFeature(
                "Panache_DodgingPanache",
                "Dodging Panache",
                "At 1st level, when an opponent attempts a melee attack against the swashbuckler, the swashbuckler can as an immediate action spend 1 panache point to gain a dodge bonus to AC equal to her Charisma modifier (minimum 0) against the triggering attack. The swashbuckler can only perform this deed while wearing light or no armor, and while carrying no heavier than a light load."
                ).SetComponents(
                new PanacheDodge(resourcePanache)
                );

            var panache4 = Helper.CreateBlueprintFeature(
                "Panache_OpportuneParryRiposte",
                "Opportune Parry and Riposte",
                "At 1st level, the swashbuckler gains the duelist’s parry class feature, but may only parry attacks targeted at herself. Upon performing a successful parry and if she has at least 1 panache point, the swashbuckler can as an immediate action make an attack against the creature whose attack she parried, provided that creature is within her reach."
                ).SetComponents(
                Helper.CreateAddFacts("c0248f304998aa64da458e097c022d73"), //DuelistParrySelfAbility
                Helper.CreateAddMechanicsFeature(AddMechanicsFeature.MechanicsFeatureType.DuelistRiposte)
                );

            var swordplay_ab = Helper.CreateBlueprintActivatableAbility(
                "MenacingSwordplayActivatable",
                out var buff,
                "Menacing Swordplay",
                "At 3rd level, while she has at least 1 panache point, when a swashbuckler hits an opponent with a light or one-handed piercing melee weapon, she can choose to use Intimidate to demoralize that opponent as a swift action instead of a standard action.",
                icon: null,
                onByDefault: true,
                deactivateWhenStunned: true,
                activationType: AbilityActivationType.WithUnitCommand,
                commandType: UnitCommand.CommandType.Swift
                ).SetComponents(
                new DeactivateImmediatelyIfNoAttacksThisRound(),
                new ActivatableAbilityUnitCommand() { Type = UnitCommand.CommandType.Swift }
                );
            buff.SetComponents(
                Helper.CreateAddInitiatorAttackWithWeaponTrigger(
                    Helper.Get<BlueprintAbility>("7d2233c3b7a0b984ba058a83b736e6ac").GetComponent<AbilityEffectRunAction>().Actions, //PersuasionUseAbility
                    DuelistWeapon: true));
            var panache5 = Helper.CreateBlueprintFeature(
                "Panache_MenacingSwordplay"
                ).SetComponents(
                Helper.CreateAddFacts(swordplay_ab)
                ).SetUIData(swordplay_ab);

            var f4_panache = Helper.CreateBlueprintFeature(
                "VirtuousBravoPanacheDeeds",
                "Panache and Deeds",
                "At 4th level, a virtuous bravo gains the swashbuckler’s panache class feature along with the following swashbuckler deeds: dodging panache, menacing swordplay, opportune parry and riposte, precise strike, and swashbuckler initiative. The virtuous bravo’s paladin levels stack with any swashbuckler levels when using these deeds.\nThis ability replaces the paladin’s spellcasting."
                ).SetComponents(
                Helper.CreateAddAbilityResources(resourcePanache),
                Helper.CreateAddFacts(panache1, panache2, panache3, panache4, panache5),
                Helper.CreateAddInitiatorAttackWithWeaponTrigger(
                    Helper.CreateActionList(new ContextRestoreResource() { m_Resource = resourcePanache }),
                    ActionsOnInitiator: true,
                    DuelistWeapon: true));

            var bleedbuff1 = Helper.CreateBlueprintBuff(
                "Panache_BleedingWound_SelfBuff",
                "Bleeding Wound",
                "As a free action you can spend 1 panache point to have your next light or one-handed piercing melee weapon attack deal additional bleed damage. The amount of bleed damage dealt is equal to the swashbuckler’s Dexterity modifier.",
                icon: Helper.StealIcon("75039846c3d85d940aa96c249b97e562")
                ).SetComponents(
                Helper.CreateAddInitiatorAttackWithWeaponTrigger(
                    Helper.CreateActionList(new ContextActionRemoveSelf(), new ContextActionIncreaseBleed(false)),
                    DuelistWeapon: true),
                Helper.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Dexterity)
                );
            var bleed1 = Helper.CreateBlueprintAbility(
                "Panache_BleedingWound_Ability",
                type: AbilityType.Extraordinary,
                actionType: UnitCommand.CommandType.Free,
                range: AbilityRange.Personal
                ).TargetSelf(
                ).SetComponents(
                new AbilityRestrictionDuelist(),
                Helper.MakeRunActionApplyBuff(bleedbuff1),
                Helper.CreateAbilityResourceLogic(resourcePanache, 1)
                ).SetUIData(bleedbuff1);

            var bleedbuff2 = Helper.CreateBlueprintBuff(
                "Panache_GreaterBleedingWound_SelfBuff",
                "Greater Bleeding Wound",
                "As a free action you can spend 2 panache point to have your next light or one-handed piercing melee weapon attack deal additional 1d4 points of Constitution bleed damage.",
                icon: Helper.StealIcon("75039846c3d85d940aa96c249b97e562")
                ).SetComponents(
                Helper.CreateAddInitiatorAttackWithWeaponTrigger(
                    Helper.CreateActionList(new ContextActionRemoveSelf(), Helper.CreateContextActionApplyBuff("f80de2a32fc2a7141b23ec29bc36f395")), //BleedConst1d4Buff
                    DuelistWeapon: true)
                );
            var bleed2 = Helper.CreateBlueprintAbility(
                "Panache_GreaterBleedingWound_Ability",
                type: AbilityType.Extraordinary,
                actionType: UnitCommand.CommandType.Free,
                range: AbilityRange.Personal
                ).TargetSelf(
                ).SetComponents(
                new AbilityRestrictionDuelist(),
                Helper.MakeRunActionApplyBuff(bleedbuff2),
                Helper.CreateAbilityResourceLogic(resourcePanache, 2)
                ).SetUIData(bleedbuff2);

            var panache6 = Helper.CreateBlueprintFeature(
                "Panache_BleedingWound",
                "Bleeding Wound",
                "At 11th level, when the swashbuckler hits a living creature with a light or one-handed piercing melee weapon attack, as a free action she can spend 1 panache point to have that attack deal additional bleed damage. The amount of bleed damage dealt is equal to the swashbuckler’s Dexterity modifier. Alternatively, the swashbuckler can spend 2 panache points to deal 1d4 points of Constitution bleed damage instead. Creatures that are immune to sneak attacks are also immune to these types of bleed damage."
                ).SetComponents(
                Helper.CreateAddFacts(bleed1, bleed2)
                );

            var panache7 = Helper.CreateBlueprintFeature(
                "Panache_SubtleBlade",
                "Subtle Blade",
                "At 11th level, while a swashbuckler has at least 1 panache point, she is immune to disarm, steal, and sunder combat maneuvers made against a light or one-handed piercing melee weapon she is wielding."
                ).SetComponents(
                new AddCombatManeuverImmunity(CombatManeuver.Disarm)
                );

            var feint_buff = Helper.CreateBlueprintBuff(
                "SuperiorFeintBuff",
                "Feinted",
                "You got tricked by a feint and are denied your Dexterity bonus to AC until next round."
                ).SetComponents(
                new FlatFootedForced()
                );
            var feint_ab = Helper.CreateBlueprintAbility(
                "SuperiorFeintAbility",
                "Superior Feint",
                "At 7th level, a swashbuckler with at least 1 panache point can, as a standard action, purposefully miss a creature she could make a melee attack against with a wielded light or one-handed piercing weapon. When she does, the creature is denied its Dexterity bonus to AC until the start of the swashbuckler’s next turn.",
                icon: Helper.StealIcon("dda92ebaf6a03f84387f7104fd597c2e"),
                type: AbilityType.Extraordinary,
                range: AbilityRange.Weapon
                ).TargetEnemy(
                ).SetComponents(
                new AbilityRestrictionDuelist(),
                Helper.MakeRunActionApplyBuff(feint_buff, Helper.CreateContextDurationValue(bonus: 1))
                );
            var panache8 = Helper.CreateBlueprintFeature(
                "Panache_SuperiorFeint"
                ).SetComponents(
                Helper.CreateAddFacts(feint_ab)
                ).SetUIData(feint_ab);

            var panache9 = Helper.CreateBlueprintFeature(
                "Panache_SwashbucklersGrace",
                "Swashbuckler’s Grace",
                "At 7th level, while the swashbuckler has at least 1 panache point, she takes no penalty for moving at full speed when she uses Acrobatics to attempt to move through a threatened area or an enemy’s space."
                ).SetComponents(
                new AddMechanicFeatureCustom(MechanicFeature.MobilityAtFullSpeed)
                );

            var disarm_ab = Helper.CreateBlueprintAbility(
                "TargetedStrikeDisarm",
                "Targeted Strike: Disarm",
                "Arms: The target takes no damage from the attack, but you disarm it. If successful, the target cannot use his weapons for 1 {g|Encyclopedia:Combat_Round}round{/g}. For every 5 by which your {g|Encyclopedia:Attack}attack{/g} exceeds your opponent's {g|Encyclopedia:AC}AC{/g}, the disarmed condition lasts 1 additional round.",
                icon: Helper.StealIcon("45d94c6db453cfc4a9b99b72d6afe6f6"),
                type: AbilityType.Extraordinary,
                range: AbilityRange.Weapon
                ).TargetEnemy(
                ).SetComponents(
                new AbilityRestrictionDuelist(),
                Helper.CreateAbilityResourceLogic(resourcePanache, 1),
                Helper.CreateAbilityEffectRunAction(
                    SavingThrowType.Unknown,
                    new ContextActionCombatManeuverWithWeapon(CombatManeuver.Disarm, false, true)
                    )
                );
            var confuse_ab = Helper.CreateBlueprintAbility(
                "TargetedStrikeConfuse",
                "Targeted Strike: Confuse",
                "Head: The target is confused for 1 round. This is a mind-affecting effect.",
                icon: Helper.StealIcon("cf6c901fb7acc904e85c63b342e9c949"),
                type: AbilityType.Extraordinary,
                range: AbilityRange.Weapon
                ).TargetEnemy(
                ).SetComponents(
                new AbilityRestrictionDuelist(),
                Helper.CreateAbilityResourceLogic(resourcePanache, 1),
                Helper.CreateAbilityEffectRunAction(
                    SavingThrowType.Unknown,
                    new ContextActionAttack(
                        Helper.CreateContextActionApplyBuff("886c7407dc629dc499b9f1465ff382df", Helper.DurationOneRound, dispellable: false)) //Confusion
                    )
                );
            var trip_ab = Helper.CreateBlueprintAbility(
                "TargetedStrikeTrip",
                "Targeted Strike: Trip",
                "Legs: The target is knocked prone. Creatures with four or more legs or that are immune to trip attacks are immune to this effect.",
                icon: Helper.StealIcon("6fd05c4ecfebd6f4d873325de442fc17"),
                type: AbilityType.Extraordinary,
                range: AbilityRange.Weapon
                ).SetComponents(
                new AbilityRestrictionDuelist(),
                Helper.CreateAbilityResourceLogic(resourcePanache, 1),
                Helper.CreateAbilityEffectRunAction(
                    SavingThrowType.Unknown,
                    new ContextActionAttack(
                        Helper.CreateContextActionApplyBuff("24cf3deb078d3df4d92ba24b176bda97", Helper.DurationOneRound, dispellable: false)) //Prone
                    )
                );
            var stagger_ab = Helper.CreateBlueprintAbility(
                "TargetedStrikeStagger",
                "Targeted Strike: Stagger",
                "Torso or Wings: The target is staggered for 1 round.",
                icon: Helper.StealIcon("df3950af5a783bd4d91ab73eb8fa0fd3"),
                type: AbilityType.Extraordinary,
                range: AbilityRange.Weapon
                ).SetComponents(
                new AbilityRestrictionDuelist(),
                Helper.CreateAbilityResourceLogic(resourcePanache, 1),
                Helper.CreateAbilityEffectRunAction(
                    SavingThrowType.Unknown,
                    new ContextActionAttack(
                        Helper.CreateContextActionApplyBuff("df3950af5a783bd4d91ab73eb8fa0fd3", Helper.DurationOneRound, dispellable: false)) //Staggered
                    )
                );
            var panache10 = Helper.CreateBlueprintFeature(
                "Panache_TargetedStrike",
                "Targeted Strike",
                "At 7th level, as a full-round action the swashbuckler can spend 1 panache point to make an attack with a single light or one-handed piercing melee weapon that cripples part of a foe’s body. The swashbuckler chooses a part of the body to target. If the attack succeeds, in addition to the attack’s normal damage, the target suffers one of the following effects based on the part of the body targeted. If a creature doesn’t have one of the listed body locations, that body part cannot be targeted. Creatures that are immune to sneak attacks are also immune to targeted strikes. Items or abilities that protect a creature from critical hits also protect a creature from targeted strikes."
                ).SetComponents(
                Helper.CreateAddFacts(disarm_ab, confuse_ab, trip_ab, stagger_ab)
                );

            var f11_panache = Helper.CreateBlueprintFeature(
                "VirtuousBravoAdvancedDeeds",
                "Advanced Deeds",
                "At 11th level, a virtuous bravo gains the following swashbuckler deeds: bleeding wound, evasive, subtle blade, superior feint, swashbuckler’s grace, and targeted strike.\nThis ability replaces aura of justice."
                ).SetComponents(
                Helper.CreateAddFacts(panache6, panache7, panache8, panache9, panache10,
                "576933720c440aa4d8d42b0c54b77e80", //Evasion
                "3c08d842e802c3e4eb19d15496145709", //UncannyDodge
                "485a18c05792521459c7d06c63128c79") //ImprovedUncannyDodge
                );

            var holy_strike_buff = Helper.CreateBlueprintBuff(
                "VirtuousBravoHolyStrikeBuff"
                ).Flags(hidden: true);
            var f20_holy_strike = Helper.CreateBlueprintFeature(
                "VirtuousBravoHolyStrike",
                "Bravo’s Holy Strike",
                "At 20th level, a virtuous bravo becomes a master at dispensing holy justice with her blade.\nWhen the virtuous bravo confirms a critical hit with a light or one-handed piercing melee weapon, the target is slain. The target can attempt a Fortitude save. On a success, the target is instead stunned for 1 round (it still takes damage). The DC of this save is equal to 10 + 1/2 the virtuous bravo’s paladin level + her Charisma modifier. Once a creature has been the target of a bravo’s holy strike, regardless of whether or not it succeeds at the save, that creature is immune to that bravo’s holy strike for 24 hours. Creatures that are immune to critical hits are also immune to this ability."
                ).SetComponents(
                Helper.CreateAddInitiatorAttackWithWeaponTrigger(
                    Helper.CreateActionList(Helper.CreateConditional(
                        condition: new Condition[] {
                            Helper.CreateContextConditionHasBuff(holy_strike_buff) },
                        ifFalse: new GameAction[] {
                            Helper.CreateContextActionApplyBuff(holy_strike_buff, Helper.DurationOneDay, dispellable: false),
                            Helper.MakeContextActionSavingThrow(
                                SavingThrowType.Fortitude,
                                succeed: Helper.CreateContextActionApplyBuff("09d39b38bb7c6014394b6daced9bacd3", Helper.DurationOneRound, dispellable: false), //Stunned
                                failed: new ContextActionKill()) }
                        )),
                    CriticalHit: true,
                    DuelistWeapon: true)
                );

            archetype.SetAddFeatures(
                Helper.CreateLevelEntry(1, f1_prof),
                Helper.CreateLevelEntry(1, f1_finesse),
                Helper.CreateLevelEntry(3, f3_nimble),
                Helper.CreateLevelEntry(4, f4_panache),
                Helper.CreateLevelEntry(11, f11_panache),
                Helper.CreateLevelEntry(20, f20_holy_strike)
                );

            archetype.SetRemoveFeatures(
                Helper.CreateLevelEntry(1, "b10ff88c03308b649b50c31611c2fefb"), //PaladinProficiencies
                Helper.CreateLevelEntry(3, "02b187038a8dce545bb34bbfb346428d"), //SelectionMercy
                Helper.CreateLevelEntry(11, "9f13fdd044ccb8a439f27417481cb00e"), //AuraOfJusticeFeature
                Helper.CreateLevelEntry(20, "eff3b63f744868845a2f511e9929f0de") //HolyChampion
                );

            Main.Patch(typeof(Patch_VirtuousBravo));

            Helper.AppendAndReplace(ref paladin.Get().m_Archetypes, archetype.ToRef());
        }

        [PatchInfo(Severity.Create, "Bladed Brush", "combat feat: use glaive with Weapon Finesse and Precise Strike", false)]
        public static void CreateBladedBrush()
        {
            var feat = Helper.CreateBlueprintFeature(
                "BladedBrushFeat",
                "Bladed Brush",
                "You can use the Weapon Finesse feat to apply your Dexterity modifier instead of your Strength modifier to attack rolls with a glaive sized for you, even though it isn’t a light weapon. When wielding a glaive, you can treat it as a one-handed piercing or slashing melee weapon and as if you were not making attacks with your off-hand for all feats and class abilities that require such a weapon (such as a duelist’s or swashbuckler’s precise strike)."
                ).SetComponents(
                new AddDuelistWeapon(WeaponCategory.Glaive),
                new AttackStatReplacement() { ReplacementStat = StatType.Dexterity, CheckWeaponTypes = true, m_WeaponTypes = Helper.ToRef<BlueprintWeaponTypeReference>("7a14a1b224cd173449cb7ffc77d5f65c").ObjToArray() }, //Glaive
                Helper.CreatePrerequisiteParametrizedFeature("1e1f627d26ad36f43bbd26cc2bf8ac7e", WeaponCategory.Glaive), //WeaponFocus
                Helper.CreatePrerequisiteFeature("b382afa31e4287644b77a8b30ed4aa0b") //Deity ShelynFeature
                );

            Helper.AddCombatFeat(feat);

            Main.Patch(typeof(Patch_BladedBrush));
        }

        [PatchInfo(Severity.Create, "Prodigious Two-Weapon Fighting", "combat feat: use STR for TWF and always treat offhand as light", false)]
        public static void CreateProdigiousTwoWeaponFighting()
        {
            var feat = Helper.CreateBlueprintFeature(
                "ProdigiousTwoWeaponFighting",
                "Prodigious Two-Weapon Fighting",
                "You may fight with a one-handed weapon in your offhand as if it were a light weapon. In addition, you may use your Strength score instead of your Dexterity score for the purpose of qualifying for Two-Weapon Fighting and any feats with Two-Weapon Fighting as a prerequisite."
                ).SetComponents(
                Helper.CreatePrerequisiteStatValue(StatType.Strength, 13),
                Helper.CreateAddMechanicsFeature(MechanicFeature.ProdigiousTWF),
                new ReplaceStatForPrerequisites() { Policy = ReplaceStatForPrerequisites.StatReplacementPolicy.NewStat, OldStat = StatType.Dexterity, NewStat = StatType.Strength }
                );

            Main.Patch(typeof(Patch_ProdigiousTWF));

            Helper.AddCombatFeat(feat);
        }
    }
}
