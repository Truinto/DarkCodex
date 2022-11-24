

namespace CodexLib
{
    /// <summary>
    /// Manipulate calculation for ranks/ability params.<br/>
    /// Greater priority runs first.<br/>
    /// Use priority:<br/>
    /// 400 = change rank value<br/>
    /// 300 = change shared value
    /// </summary>
    public interface IMechanicRecalculate
    {
        public int Priority { get; }

        public void PreCalculate(MechanicsContext context);

        public void PostCalculate(MechanicsContext context);
    }

}
