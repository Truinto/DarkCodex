using CodexLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
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
}
