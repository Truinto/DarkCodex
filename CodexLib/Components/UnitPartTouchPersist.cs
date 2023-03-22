using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Replacement for <see cref="UnitPartTouch"/>. Also remembers the number of uses per cast.
    /// </summary>
    public class UnitPartTouchPersist : UnitPartTouch
    {
        /// <summary>Number of uses before effect wears off.</summary>
        public int Count;
    }

    /// <summary>
    /// Logic to handle touch attacks with multiple charges. E.g. Chill Touch.
    /// </summary>
    public class AbilityEffectStickyTouchPersist : AbilityEffectStickyTouch
    {
        /// <summary>Number of uses before effect wears off.</summary>
        public ContextValue Count;

        /// <inheritdoc cref="AbilityEffectStickyTouchPersist"/>
        /// <param name="blueprintAbility">type: <b>BlueprintAbility</b></param>
        /// <param name="count">Number of uses before effect wears off.</param>
        public AbilityEffectStickyTouchPersist(AnyRef blueprintAbility, ContextValue count)
        {
            this.m_TouchDeliveryAbility = blueprintAbility;
            this.Count = count;
        }

        /// <summary>
        /// Implementation of AbilityApplyEffect.Apply.
        /// </summary>
        public override void Apply(AbilityExecutionContext context, TargetWrapper target)
        {
            var caster = context.MaybeCaster;
            if (caster == null)
                return;

            var part = caster.Ensure<UnitPartTouch, UnitPartTouchPersist>();
            part.Count = this.Count.Calculate(context);
            part.Init(this.TouchDeliveryAbility, context.Ability, context.SourceAbilityContext);

            if (caster == target.Unit)
            {
                Rulebook.Trigger(new RuleCastSpell(part.Ability.Data, target));
                return;
            }

            caster.Brain.AutoUseAbility = part.Ability.Data;
            var unitCommand = (part.AutoCastCommand = UnitUseAbility.CreateCastCommand(part.Ability.Data, target));
            unitCommand.IgnoreCooldown(part.IgnoreCooldownBeforeTime);
            caster.Commands.AddToQueueFirst(unitCommand);
        }
    }
}
