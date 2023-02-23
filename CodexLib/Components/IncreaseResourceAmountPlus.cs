using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Grants extra charges for each Fact a creature has.
    /// </summary>
    public class IncreaseResourceAmountPlus : UnitFactComponentDelegate, IResourceAmountBonusHandler
    {
        public BlueprintAbilityResourceReference Resource;

        public AnyRef[] IncreasingFacts;

        public int Multiplier;

        /// <summary>
        /// Grants extra charges for each Fact a creature has.
        /// </summary>
        /// <param name="resource">type: <b>BlueprintAbilityResource</b></param>
        /// <param name="multiplier">Extra resources per matching fact.</param>
        /// <param name="increasingFacts">type: <b>BlueprintUnitFact</b></param>
        public IncreaseResourceAmountPlus(AnyRef resource, int multiplier = 1, params AnyRef[] increasingFacts)
        {
            this.Resource = resource;
            this.Multiplier = multiplier;
            this.IncreasingFacts = increasingFacts;
        }

        public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
        {
            if (this.Fact.Active && this.Resource.Is(resource))
            {
                foreach (var fact in this.IncreasingFacts)
                {
                    if (fact.GetBlueprint() is BlueprintUnitFact fact2 && this.Owner.HasFact(fact2))
                        bonus += Multiplier;
                }
            }
        }
    }
}
