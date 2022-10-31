namespace CodexLib
{
    /// <summary>
    /// Simple rule for delayed execution. E.g for projectiles.
    /// </summary>
    public class RuleResolveFunction : RulebookEvent
    {
        public RulebookEvent Parent;
        public Action<RulebookEvent> OnResolve;

        public RuleResolveFunction([NotNull] UnitEntityData initiator, RulebookEvent parent, Action<RulebookEvent> onResolve = null) : base(initiator)
        {
            this.Parent = parent;
            this.OnResolve = onResolve;
        }

        public override void OnTrigger(RulebookEventContext context)
        {
            try
            {
                this.OnResolve?.Invoke(this.Parent);
            }
            catch (Exception e) { Helper.PrintException(e); }
        }
    }
}
