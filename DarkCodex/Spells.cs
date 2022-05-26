using CodexLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands.Base;
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
            var flashstep = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("e10424c1afe70cb4384090f4dab8d437"); //StormwalkerFlashStepAbility

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
                AbilityRange.Projectile
                ).SetComponents(
                new AbilityDeliverTeleportTrample()
                {
                    Projectile = "4990cdb96ea77b5439afbc804f12d922".ToRef<BlueprintProjectileReference>(),
                    //DisappearFx = Helper.GetPrefabLink("b2c32c1999a42d547a8a0e3e7e958fc6"),
                    //AppearFx = Helper.GetPrefabLink("b2c32c1999a42d547a8a0e3e7e958fc6"),
                    m_Length = 30.Feet()
                },
                Helper.CreateAbilitySpawnFx("503b78b507366cc4da0f462cb40131f6"),
                Helper.CreateAbilityEffectRunAction(SavingThrowType.Unknown, Helper.CreateConditional(new Condition[] {
                    //Helper.CreateContextConditionIsEnemy(),
                    new ContextConditionAttackRoll { IgnoreAoO = true, ApplyBladedBonus = true }})),
                checkWeapon,
                school,
                craft
                ).TargetEnemy(point: true);

            // TODO: apply attack bonus; AddAttackBonus
            var applyBuff = Helper.CreateAbilityExecuteActionOnCast();

            var normal = Helper.CreateBlueprintAbility(
                "BladedDash",
                "Bladed Dash",
                "When you cast this spell, you immediately move up to 30 feet in a straight line any direction, momentarily leaving a multi-hued cascade of images behind you. This movement does not provoke attacks of opportunity. You may make a single melee attack at your highest base attack bonus against any one creature you are adjacent to at the end of your movement. You gain a circumstance bonus on your attack roll equal to your Intelligence or Charisma modifier, whichever is higher. You must end the bonus movement granted by this spell in an unoccupied space. Despite the name, the spell works with any melee weapon.",
                icon: icon,
                AbilityType.Spell,
                UnitCommand.CommandType.Standard,
                AbilityRange.Close
                ).SetComponents(
                new AddAttackBonus(ModifierDescriptor.UntypedStackable, StatType.Intelligence, StatType.Charisma),
                flashstep.GetComponent<AbilityCustomFlashStep>(),
                checkWeapon,
                school,
                craft,
                Helper.CreateSpellListComponent("4d72e1e7bd6bc4f4caaea7aa43a14639".ToRef<BlueprintSpellListReference>(), 2)
                ).TargetEnemy(point: true);

            normal.Add(2, "4d72e1e7bd6bc4f4caaea7aa43a14639", "25a5013493bdcf74bb2424532214d0c8"); // Magus, Bard
            greater.Add(5, "4d72e1e7bd6bc4f4caaea7aa43a14639", "25a5013493bdcf74bb2424532214d0c8"); // Magus, Bard
        }
    }
}
