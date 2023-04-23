using Kingmaker.Controllers;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using TurnBased.Controllers;

namespace CodexLib.Patches
{
    /// <summary>
    /// Patches to handle touch attacks with multiple charges.<br/>
    /// See also: AbilityEffectStickyTouchPersist, UnitPartTouchPersist
    /// </summary>
    [HarmonyPatch]
    public class Patch_TouchPersist
    {
        [HarmonyPatch(typeof(TouchSpellsController), nameof(TouchSpellsController.OnAbilityEffectApplied))]
        [HarmonyPrefix]
        public static bool Prefix1(AbilityExecutionContext context, TouchSpellsController __instance)
        {
            if (context.MaybeCaster?.Get<UnitPartTouch>() is not UnitPartTouchPersist part)
                return true;

            // reduce count by 1; remove at 0
            if (part.Ability.Blueprint == context.AbilityBlueprint)
            {
                if (--part.Count <= 0)
                {
                    part.RemoveSelf();
                    part.Dispose();
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(AbilityCastRateUtils), nameof(AbilityCastRateUtils.GetChargesCount), typeof(AbilityData))]
        [HarmonyPostfix]
        public static void Postfix2(AbilityData ability, ref int __result)
        {
            // display correct count
            if (ability.Caster.Unit.Get<UnitPartTouch>() is UnitPartTouchPersist part
                && part.Ability.Data == ability)
                __result = part.Count;
        }

        [HarmonyPatch(typeof(MagusController), nameof(MagusController.OnEventDidTrigger), typeof(RuleAttackWithWeapon))]
        [HarmonyPrefix]
        public static bool Prefix3(RuleAttackWithWeapon evt)
        {
            if (!evt.AttackRoll.IsHit)
                return false;

            var partTouch = evt.Initiator.Get<UnitPartTouch>();
            if (partTouch == null || evt.Initiator.Get<UnitPartMagus>() == null)
                return false;

            // force cast spell through weapon attack
            if (evt.AttackRoll.Weapon.Blueprint.IsMelee && !evt.AttackRoll.Weapon.Blueprint.IsNatural) // natural attacks already resolved in TouchSpellsController
                Rulebook.Trigger(new RuleCastSpell(partTouch.Ability, evt.AttackRoll.Target));

            return false;
        }

        [HarmonyPatch(typeof(UnitUseAbility), nameof(UnitUseAbility.CreateCastCommand))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler4(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var data = new TranspilerTool(instructions, generator, original);

            // normal: spell cast is replaced with held spell; fix: cast spell as is while in TurnBased
            data.Seek(typeof(AbilityEffectStickyTouch), nameof(AbilityEffectStickyTouch.TouchDeliveryAbility));
            data++;
            var label = (Label)data.Current.operand; // TODO: UnityMod add method 'JumpTo'
            data.InsertAfter(OpCodes.Ldc_I4_0);
            data.InsertAfter(patch);
            data.InsertAfter(OpCodes.Brtrue_S, label);

            return data;

            static bool patch(bool __stack, [LocalParameter(indexByType: 0)] UnitPartTouch partTouch)
            {
                return partTouch is UnitPartTouchPersist && CombatController.IsInTurnBasedCombat();
            }
        }
    }
}
