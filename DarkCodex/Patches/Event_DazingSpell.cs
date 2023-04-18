using Kingmaker.UnitLogic;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    /// <summary>
    /// Logic for Dazing Spell Metamagic.
    /// </summary>
    [PatchInfo(Severity.Event, "Event: Dazing Spell", "logic for Dazing Spell Metamagic", false)]
    public class Event_DazingSpell : IAfterRulebookEventTriggerHandler<RuleDealDamage>
    {
        /// <summary>DazeBuff: 9934fedff1b14994ea90205d189c8759</summary>
        public static BlueprintBuffReference Buff = "9934fedff1b14994ea90205d189c8759".ToRef<BlueprintBuffReference>();  //DazeBuff

        /// <summary></summary>
        public void OnAfterRulebookEventTrigger(RuleDealDamage evt)
        {
            var context = evt.Reason?.Context;
            if (context == null)
                return;

            if (!context.HasMetamagic(Const.Dazing))
                return;

            if (!evt.HasDealtDamage())
                return;

            var save = context.SourceAbilityContext?.RulebookContext?.LastEvent<RuleSavingThrow>();
            save ??= context.TriggerRule(new RuleSavingThrow(evt.Target, SavingThrowType.Will, context.Params.DC));

            if (!save.Success)
            {
                int duration = Math.Max(1, context.Params.SpellLevel);
                evt.Target.AddBuffStacking(Buff, context, duration.Rounds().Seconds);
            }
        }
    }
}
