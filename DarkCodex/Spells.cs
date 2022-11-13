using CodexLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class Spells
    {
        [PatchInfo(Severity.Create, "Bladed Dash", "spell: Bladed Dash", false)]
        public static void CreateBladedDash()
        {
            // sfx from CallLightningAbility AbilitySpawnFx
            // StormwalkerFlashStepAbility AbilityCustomFlashStep
            // DragonsBreathBlue AbilityDeliverProjectile
            var flashstep = Helper.Get<BlueprintAbility>("e10424c1afe70cb4384090f4dab8d437"); //StormwalkerFlashStepAbility

            var checkWeapon = Helper.CreateAbilityRequirementHasItemInHands();
            var school = Helper.CreateSpellComponent(SpellSchool.Transmutation);
            var craft = Helper.CreateCraftInfoComponent();
            var icon = Helper.StealIcon("df4537fb64116694d82a6736940542b7");

            var greater = Helper.CreateBlueprintAbility(
                "BladedDashGreater",
                "Bladed Dash, Greater",
                "This spell functions like bladed dash, save that you can make a single melee attack against every creature you pass during the 30 feet of your dash. You cannot attack an individual creature more than once with this spell.",
                icon: icon,
                AbilityType.Spell,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close
                ).SetComponents(
                new AbilityDeliverTeleportTrample()
                {
                    Projectile = "4990cdb96ea77b5439afbc804f12d922".ToRef<BlueprintProjectileReference>(),
                    Type = AbilityProjectileType.Line,
                    m_Length = 30.Feet(),
                    m_LineWidth = 10.Feet(),
                    UseReach = true
                },
                Helper.CreateAbilitySpawnFx("503b78b507366cc4da0f462cb40131f6"),
                Helper.CreateAbilityEffectRunAction(
                    condition: new Condition[] {
                        Helper.CreateContextConditionIsEnemy(),
                        new ContextConditionAttackRoll { IgnoreAoO = true, ApplyBladedBonus = true, CanBeRanged = false }
                    },
                    ifTrue: new GameAction[] {
                        new ContextActionDealWeaponDamage()
                    }),
                checkWeapon,
                school,
                craft
                ).TargetEnemy(point: true);

            var normal = Helper.CreateBlueprintAbility(
                "BladedDash",
                "Bladed Dash",
                "When you cast this spell, you immediately move up to 30 feet in a straight line any direction, momentarily leaving a multi-hued cascade of images behind you. This movement does not provoke attacks of opportunity. You may make a single melee attack at your highest base attack bonus against any one creature you are adjacent to at any point along this 30 feet. You gain a circumstance bonus on your attack roll equal to your Intelligence or Charisma modifier, whichever is higher. You must end the bonus movement granted by this spell in an unoccupied space. Despite the name, the spell works with any melee weapon.",
                icon: icon,
                AbilityType.Spell,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close
                ).SetComponents(
                new AbilityDeliverTeleportTrample()
                {
                    Projectile = "4990cdb96ea77b5439afbc804f12d922".ToRef<BlueprintProjectileReference>(),
                    Type = AbilityProjectileType.Simple,
                    m_Length = 30.Feet(),
                    m_LineWidth = 1.Feet(),
                    TargetLimit = 1,
                    UseReach = true
                },
                Helper.CreateAbilitySpawnFx("503b78b507366cc4da0f462cb40131f6"),
                Helper.CreateAbilityEffectRunAction(
                    condition: new Condition[] {
                        Helper.CreateContextConditionIsEnemy(),
                        new ContextConditionAttackRoll { IgnoreAoO = true, ApplyBladedBonus = true, CanBeRanged = false }
                    },
                    ifTrue: new GameAction[] {
                        new ContextActionDealWeaponDamage()
                    }),
                school,
                craft
                ).TargetEnemy(point: true);

            normal.Add(2, "4d72e1e7bd6bc4f4caaea7aa43a14639", "25a5013493bdcf74bb2424532214d0c8"); // Magus, Bard
            greater.Add(5, "4d72e1e7bd6bc4f4caaea7aa43a14639", "25a5013493bdcf74bb2424532214d0c8"); // Magus, Bard
        }

        [PatchInfo(Severity.Create, "Healing Flames", "spell: Healing Flames", false)]
        public static void CreateHealingFlames()
        {
            /*
            Healing Flames
            School conjuration (healing) [fire, good]; Level cleric 4, inquisitor 4, oracle 4, paladin 4, warpriest 4
            Casting Time 1 standard action
            Area 10-ft.-radius burst, centered on you
            Duration instantaneous
            Saving Throw Reflex half; see text; Spell Resistance yes

            You unleash a blast of holy flames that washes over all creatures in the area in a glorious display of divine power. This deals damage to evil creatures and heals good creatures in the area. The amount of damage dealt and the number of hit points restored in each case is 1d8 points per 2 caster levels (maximum 5d8).
            Half of the damage this spell deals to evil creatures is fire damage, and half of the damage is pure divine power that is therefore not subject to reduction by energy resistance to fire-based attacks.
            Neutral enemies within the spell’s area of effect also take the fire damage, but do not take the divine damage. Neutral allies within the area are healed by half as much as good creatures. A successful Reflex saving throw halves the damage taken in all cases.
            */

            var ab = Helper.CreateBlueprintAbility(
                "HealingFlames",
                "Healing Flames",
                "You unleash a blast of holy flames that washes over all creatures in the area in a glorious display of divine power. This deals damage to evil creatures and heals good creatures in the area. The amount of damage dealt and the number of hit points restored in each case is 1d8 points per 2 caster levels (maximum 5d8).\nHalf of the damage this spell deals to evil creatures is fire damage, and half of the damage is pure divine power that is therefore not subject to reduction by energy resistance to fire-based attacks.\nNeutral enemies within the spell’s area of effect also take the fire damage, but do not take the divine damage. Neutral allies within the area are healed like good creatures. A successful Reflex saving throw halves the damage taken in all cases.",
                icon: Helper.StealIcon("f5fc9a1a2a3c1a946a31b320d1dd31b2"),
                type: AbilityType.Spell,
                range: AbilityRange.Personal
                ).TargetSelf(
                ).SetComponents(
                Helper.CreateAbilityEffectRunAction(
                    SavingThrowType.Unknown,
                    Helper.CreateConditional(
                        Helper.CreateContextConditionAlignment(AlignmentComponent.Good),
                        ifTrue: Helper.CreateContextActionHealTarget(bonus: Helper.SharedHeal),
                        ifFalse: Helper.CreateConditional(
                            Helper.CreateContextConditionAlignment(AlignmentComponent.Evil),
                            ifTrue: Helper.CreateContextActionSavingThrow(SavingThrowType.Reflex, 
                                Helper.CreateContextActionDealDamage(DamageEnergyType.Fire, Helper.SharedDiceHeal, halfIfSaved: true, half: true), 
                                Helper.CreateContextActionDealDamage(DamageEnergyType.Holy, Helper.SharedDiceHeal, halfIfSaved: true, half: true)),
                            ifFalse: Helper.CreateConditional(
                                Helper.CreateContextConditionIsAlly(),
                                ifTrue: Helper.CreateContextActionHealTarget(bonus: Helper.SharedHeal),
                                ifFalse: Helper.CreateContextActionDealDamage(DamageEnergyType.Fire, Helper.SharedDiceHeal, halfIfSaved: true, half: true)
                                )))),
                Helper.CreateContextRankConfig(ContextRankBaseValueType.CasterLevel, ContextRankProgression.Div2, max: 5),
                Helper.CreateContextCalculateSharedValue(AbilitySharedValue.Heal, DiceType.D8, Helper.ContextDefault),
                Helper.CreateAbilityTargetsAround(13.Feet()),
                Helper.CreateAbilitySpawnFx(Resource.Sfx.Burst10_Fire, anchor: AbilitySpawnFxAnchor.Caster),
                Helper.CreateSpellComponent(SpellSchool.Conjuration),
                Helper.CreateSpellDescriptorComponent(SpellDescriptor.Good),
                Helper.CreateCraftInfoComponent()
                );

            ab.Add(4,
                "8443ce803d2d31347897a3d85cc32f53", //ClericSpellList +Oracle
                "57c894665b7895c499b3dce058c284b3", //InquisitorSpellList
                "9f5be2f7ea64fe04eb40878347b147bc", //PaladinSpellList
                "c5a1b8df32914d74c9b44052ba3e686a"  //WarpriestSpelllist
                );
        }

        [PatchInfo(Severity.Create, "Flame Blade", "spell: Flame Blade, feat: Flame Blade Dervish Combat", false)]
        public static void CreateFlameBlade()
        {
            /*
            Flame Blade Dervish Combat
            You move effortlessly when wielding a flame blade.
            Prerequisite(s): Ability to cast flame blade as a spell or spell-like ability.
            Benefit(s): When you cast flame blade, you gain a +10 enhancement bonus to your base speed as long as the spell persists, along with a +4 competence bonus on all Acrobatics checks. You add your Charisma modifier to damage rolls with your flame blade, and ignore the first 10 points of fire resistance possessed by a creature you hit with the flame blade for the purposes of determining the damage dealt by the flame blade. Against undead foes, you ignore the first 30 points of fire resistance. Immunity to fire still completely protects against damage from your flame blade.
            */

            AnyRef feat = Helper.CreateBlueprintFeature(
                "FlameBladeDervishCombat",
                "Flame Blade Dervish Combat",
                "Benefit(s): When you cast flame blade, you gain a +10 enhancement bonus to your base speed as long as the spell persists, along with a +4 competence bonus on all Acrobatics checks. You add your Charisma modifier to damage rolls with your flame blade, and ignore the first 10 points of fire resistance possessed by a creature you hit with the flame blade for the purposes of determining the damage dealt by the flame blade. Against undead foes, you ignore the first 30 points of fire resistance. Immunity to fire still completely protects against damage from your flame blade."
                ).SetComponents(
                Helper.CreatePrerequisiteClassLevel("610d836f3a3a9ed42a4349b62f002e96", 3, true), //DruidClass
                Helper.CreatePrerequisiteClassLevel("145f1d3d360a7ad48bd95d392c81b38e", 3, true)  //ShamanClass // TODO: make spell prerequisite
                );

            Helper.AddFeats(feat);

            /*
            Flame Blade
            School evocation [fire]; Level druid 2, shaman 2
            Casting Time 1 standard action
            Range personal
            Duration 1 min./level (D)
            Saving Throw none; Spell Resistance yes

            A 3-foot-long, blazing beam of red-hot fire springs forth from your hand. You wield this blade-like beam as if it were a scimitar. Attacks with the flame blade are melee touch attacks. The blade deals 1d8 points of fire damage + 1 point per two caster levels (maximum +10). Since the blade is immaterial, your Strength modifier does not apply to the damage. A flame blade can ignite combustible materials such as parchment, straw, dry sticks, and cloth.
            */

            var enchantment = Helper.CreateBlueprintWeaponEnchantment(
                "Flame Blade"
                ).SetComponents(
                new FlameBladeLogic(feat, DamageTypeMix.Fire, 10)
                );
            enchantment.WeaponFxPrefab = Helper.GetPrefabLink(Resource.Sfx.Weapon_Fire);
            enchantment.m_HiddenInUI = true;

            var weapon = Helper.CreateBlueprintItemWeapon(
                "FlameBladeWeapon",
                "Flame Blade",
                weaponType: Helper.ToRef<BlueprintWeaponTypeReference>("be24e972e8656514898dd335e983ea2c"), //MeleeTouchType
                cloneVisuals: "d9fbec4637d71bd4ebc977628de3daf3", //Scimitar
                damageOverride: new DiceFormula(1, DiceType.D8),
                form: Helper.CreateDamageTypeDescription(DamageEnergyType.Fire),
                enchantments: new BlueprintWeaponEnchantmentReference[] { enchantment.ToRef() }
                );

            var buff = Helper.CreateBlueprintBuff(
                "FlameBladeBuff"
                ).Flags(
                hidden: true
                ).SetComponents(
                new AddTemporaryWeapon(weapon),
                Helper.CreateAddStatBonusIfHasFact(StatType.Speed, ModifierDescriptor.Enhancement, 10, facts: feat),
                Helper.CreateAddStatBonusIfHasFact(StatType.SkillMobility, ModifierDescriptor.Competence, 4, facts: feat)
                );

            var ab = Helper.CreateBlueprintAbility(
                "FlameBladeAbility",
                "Flame Blade",
                "A 3-foot-long, blazing beam of red-hot fire springs forth from your hand. You wield this blade-like beam as if it were a scimitar. Attacks with the flame blade are melee touch attacks. The blade deals 1d8 points of fire damage + 1 point per two caster levels (maximum +10). Since the blade is immaterial, your Strength modifier does not apply to the damage. A flame blade can ignite combustible materials such as parchment, straw, dry sticks, and cloth.",
                icon: Helper.StealIcon("05b7cbe45b1444a4f8bf4570fb2c0208"),
                type: AbilityType.Spell,
                actionType: UnitCommand.CommandType.Free,
                range: AbilityRange.Personal,
                duration: Resource.Strings.MinutesPerLevel
                ).TargetSelf(
                ).SetComponents(
                Helper.CreateAbilityEffectRunAction(
                    SavingThrowType.Unknown,
                    Helper.CreateContextActionApplyBuff(buff, Helper.DurationMinutesPerLevel, fromSpell: true)),
                Helper.CreateSpellComponent(SpellSchool.Evocation),
                Helper.CreateSpellDescriptorComponent(SpellDescriptor.Fire),
                Helper.CreateCraftInfoComponent()
                );

            ab.Add(2,
                "bad8638d40639d04fa2f80a1cac67d6b", //DruidSpellList
                "c0c40e42f07ff104fa85492da464ac69"  //ShamanSpelllist
                );
        }

        [PatchInfo(Severity.Fix, "Various Tweaks", "life bubble is AOE again", false)]
        public static void PatchVarious()
        {
            // life bubble is AOE again
            var bubble = Helper.Get<BlueprintAbility>("265582bc494c4b12b5860b508a2f89a2"); //LifeBubble
            bubble.Range = AbilityRange.Personal;
            if (bubble.GetComponent<AbilityTargetsAround>() == null)
                bubble.AddComponents(new AbilityTargetsAround { m_Radius = 20.Feet(), m_TargetType = TargetType.Ally, m_Condition = new() });
        }
    }

    /*
    Chill Touch
    School necromancy; Level bloodrager 1, magus 1, shaman 1, sorcerer/wizard 1, witch 1; Mystery reaper 1
    Casting Time 1 standard action
    Range touch
    Targets creature or creatures touched (up to one/level)
    Saving Throw Fortitude partial or Will negates; see text; Spell Resistance yes

    A touch from your hand, which glows with blue energy, disrupts the life force of living creatures. Each touch channels negative energy that deals 1d6 points of damage. The touched creature also takes 1 point of Strength damage unless it makes a successful Fortitude saving throw. You can use this melee touch attack up to one time per level.
    An undead creature you touch takes no damage of either sort, but it must make a successful Will saving throw or flee as if panicked for 1d4 rounds + 1 round per caster level.


    Produce Flame
    School evocation [fire]; Level druid 1, shaman 1; Domain fire 2
    Casting Time 1 standard action
    Range personal
    Duration 1 min./level (D)
    Saving Throw none; Spell Resistance yes

    Flames as bright as a torch appear in your open hand. The flames harm neither you nor your equipment.
    In addition to providing illumination, the flames can be hurled or used to touch enemies. You can strike an opponent with a melee touch attack, dealing fire damage equal to 1d6 + 1 point per caster level (maximum +5). Alternatively, you can hurl the flames up to 120 feet as a thrown weapon. When doing so, you attack with a ranged touch attack (with no range penalty) and deal the same damage as with the melee attack. No sooner do you hurl the flames than a new set appears in your hand. Each attack you make reduces the remaining duration by 1 minute. If an attack reduces the remaining duration to 0 minutes or less, the spell ends after the attack resolves.
    

    Gozreh's Trident
    School evocation [electricity]; Level bloodrager 2, cleric 2, druid 2, hunter 2, oracle 2, warpriest 2, witch 2 (Gozreh)
    Casting Time 1 standard action
    Range personal
    Duration 1 minute/level (D)
    Saving Throw none; Spell Resistance yes

    A 4-foot-long, blazing, forked bolt of electricity springs forth from your hand. You wield this spear-like bolt as if it were a trident (you are considered proficient with the bolt). Attacks with Gozreh’s trident are melee touch attacks. The bolt deals 1d8 points of electricity damage + 1 point per 2 caster levels (maximum +10). Since the bolt is immaterial, your Strength modifier does not apply to the damage. The bolt can ignite combustible materials such as parchment, straw, dry sticks, and cloth.

    */
}
