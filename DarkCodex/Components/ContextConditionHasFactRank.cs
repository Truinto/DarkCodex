using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    public class ContextConditionHasFactRank : ContextCondition
	{
		public BlueprintUnitFactReference Fact;

		public ContextValue RankValue;

		public override string GetConditionCaption()
        {
            return "";
        }

        public override bool CheckCondition()
        {
			if (this.Target.Unit == null)
				return false;

			EntityFact entityFact = this.Target.Unit.Facts.Get(this.Fact);
			if (entityFact == null)
				return false;

			return entityFact.GetRank() >= this.RankValue.Calculate(this.Context);
		}
	}
}
