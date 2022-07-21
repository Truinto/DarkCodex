

namespace CodexLib
{
    public class ContextRankBonus : BlueprintComponent, IContextBonus
    {
        public int Bonus;
        public AbilityRankType RankType;

        public ContextRankBonus(int bonus, AbilityRankType rankType = AbilityRankType.Default)
        {
            this.Bonus = bonus;
            this.RankType = rankType;
        }

        public void Apply(MechanicsContext context)
        {
            context[RankType] += Bonus;
        }
    }

}
