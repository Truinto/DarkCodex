

namespace CodexLib
{
    public class ContextRankBonus : BlueprintComponent, IMechanicRecalculate
    {
        public int Bonus;
        public AbilityRankType RankType;

        public int Priority => 400;

        public ContextRankBonus(int bonus, AbilityRankType rankType = AbilityRankType.Default)
        {
            this.Bonus = bonus;
            this.RankType = rankType;
        }

        public void PreCalculate(MechanicsContext context)
        {
        }

        public void PostCalculate(MechanicsContext context)
        {
            context[RankType] += Bonus;
        }
    }

}
