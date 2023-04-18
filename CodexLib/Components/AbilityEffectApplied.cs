using Kingmaker.ElementsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    // TODO
    /// <summary>
    /// Runs actions, if owner <see cref="BlueprintAbility"/> triggered <typeparamref name="TRule"/>.
    /// </summary>
    [AllowedOn(typeof(BlueprintAbility))]
    public class AbilityEffectApplied<TRule> : BlueprintComponent, IRulebookHandler<RulebookTargetEvent> where TRule : RulebookTargetEvent //, IAbilityRulebookHandler
    {
        /// <summary>When true, Actions are executed.</summary>
        public Func<TRule, AbilityExecutionContext, bool> Condition;
        /// <summary>Actions to perform, when Condition returns true.</summary>
        public ActionList Actions;

        private static bool Locked;

        /// <inheritdoc cref="AbilityEffectApplied{TRule}"/>
        public AbilityEffectApplied(Func<TRule, AbilityExecutionContext, bool> condition, params GameAction[] actions)
        {
            this.Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            this.Actions = new() { Actions = actions ?? throw new ArgumentNullException(nameof(actions)) };
        }

        /// <summary></summary>
        public void OnEventAboutToTrigger(RulebookTargetEvent evt)
        {
        }

        /// <summary></summary>
        public void OnEventDidTrigger(RulebookTargetEvent evt)
        {
            Helper.PrintDebug($"AbilityEffectApplied evt={evt.GetType().Name} source={evt.Reason?.Context?.AssociatedBlueprint} owner={this.OwnerBlueprint} lock={Locked}"); // TOCHECK

            if (Locked)
                return;
            if (evt is not TRule rule)
                return;
            if (evt.Reason?.Context is not AbilityExecutionContext context)
                return;
            if (context.AssociatedBlueprint != this.OwnerBlueprint)
                return;

            try
            {
                if (!Condition(rule, context))
                    return;
                Locked = true;
                using (context.GetDataScope(evt.Target))
                    Actions.Run();
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
            }
            finally
            {
                Locked = false;
            }
        }
    }

    public static class Patch_AbilityEffectApplied
    {

    }

    public class Event_AbilityEffectApplied : IBeforeRulebookEventTriggerHandler<RulebookTargetEvent>, IAfterRulebookEventTriggerHandler<RulebookTargetEvent>
    {
        public static Event_AbilityEffectApplied Instance;

        public static void Subscribe()
        {
            if (!EventBus.IsGloballySubscribed(Instance))
                EventBus.Subscribe(Instance = new());

            //EventBus.IsGloballySubscribed(Instance);
            //EventBus.Subscribe(Instance);
            //RulebookEventBus.GlobalRulebookSubscribers.m_Listeners.Values.Any(a => a is SubscribersList<RuleDealDamage> list && list.List.Contains(Instance));
            //RulebookEventBus.Subscribe(Instance);
        }

        public void OnBeforeRulebookEventTrigger(RulebookTargetEvent evt)
        {
            if (evt.Reason?.Context is AbilityExecutionContext context)
                context.AbilityBlueprint.CallComponents<IAbilityRulebookHandler>(a => a.OnEventAboutToTrigger(evt, context));
        }

        public void OnAfterRulebookEventTrigger(RulebookTargetEvent evt)
        {
            if (evt.Reason?.Context is AbilityExecutionContext context)
                context.AbilityBlueprint.CallComponents<IAbilityRulebookHandler>(a => a.OnEventDidTrigger(evt, context));
        }
    }
}
