using Kingmaker.Controllers;

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
            // reduce count by 1; remove at 0
            var part = context.MaybeCaster?.Get<UnitPartTouchPersist>();
            if (part != null && part.Ability.Blueprint == context.AbilityBlueprint)
            {
                if (--part.Count > 0)
                    return false;
                part.RemoveSelf();
            }

            return true;
        }

        [HarmonyPatch(typeof(AbilityCastRateUtils), nameof(AbilityCastRateUtils.GetChargesCount), typeof(AbilityData))]
        [HarmonyPostfix]
        public static void Postfix2(AbilityData ability, ref int __result)
        {
            // display correct count
            if (ability.StickyTouch != null)
            {
                var part = ability.Caster.Unit.Get<UnitPartTouchPersist>();
                if (part != null)
                    __result = part.Count;
            }
        }
    }
}
