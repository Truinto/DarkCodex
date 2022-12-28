using Kingmaker.Controllers;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class UnitPartTouchPersist : UnitPartTouch
    {
        public int Count;
    }

    public class AbilityEffectStickyTouchPersist : AbilityEffectStickyTouch
    {
        public ContextValue Count;

        /// <param name="blueprintAbility">type: <b>BlueprintAbility</b></param>
        public AbilityEffectStickyTouchPersist(AnyRef blueprintAbility, ContextValue count)
        {
            this.m_TouchDeliveryAbility = blueprintAbility;
            this.Count = count;
        }

        public override void Apply(AbilityExecutionContext context, TargetWrapper target)
        {
            if (context.MaybeCaster == null)
                return;

            var part = context.MaybeCaster.Ensure<UnitPartTouchPersist>();
            part.Init(this.TouchDeliveryAbility, context.Ability, context.SourceAbilityContext);
            part.Count = this.Count.Calculate(context);

            if (context.MaybeCaster == target.Unit)
            {
                Rulebook.Trigger(new RuleCastSpell(part.Ability.Data, target));
                return;
            }

            var unitCommand = (part.AutoCastCommand = UnitUseAbility.CreateCastCommand(part.Ability.Data, target));
            unitCommand.IgnoreCooldown(part.IgnoreCooldownBeforeTime);
            context.MaybeCaster.Commands.AddToQueueFirst(unitCommand);
        }
    }

    [HarmonyPatch]
    public class Patch_TouchPersist
    {
        [HarmonyPatch(typeof(TouchSpellsController), nameof(TouchSpellsController.OnAbilityEffectApplied))]
        public static bool Prefix1(AbilityExecutionContext context, TouchSpellsController __instance)
        {
            // reduce count by 1; remove at 0
            var part = context.MaybeCaster?.Get<UnitPartTouchPersist>();
            if (part != null)
            {
                if (--part.Count > 0)
                    return false;
                part.RemoveSelf();
            }

            return true;
        }

        [HarmonyPatch(typeof(AbilityCastRateUtils), nameof(AbilityCastRateUtils.GetChargesCount), typeof(AbilityData))]
        public static void Postfix2(AbilityData ability, ref int __result)
        {
            // display correct count
            if(ability.StickyTouch != null)
            {
                var part = ability.Caster.Unit.Get<UnitPartTouchPersist>();
                if (part != null)
                    __result = part.Count;
            }
        }
    }
}
