using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Replace ResourceLogic of specific abilities.
    /// </summary>
    public class OverrideResourceLogic : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>
    {
        public IAbilityResourceLogic ResourceLogic;
        public BlueprintAbilityReference[] Spells;

        /// <summary>
        /// Replace ResourceLogic of specific abilities.
        /// </summary>
        /// <param name="resourceLogic">Resource logic to take precedence.</param>
        /// <param name="spells">type: <b>BlueprintAbility</b></param>
        public OverrideResourceLogic(IAbilityResourceLogic resourceLogic, params AnyRef[] spells)
        {
            this.ResourceLogic = resourceLogic;
            this.Spells = spells.ToRef<BlueprintAbilityReference>();
        }

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            var ability = evt.AbilityData;
            if (ability == null)
                return;

            if (this.Spells.Contains(ability.Blueprint))
            {
                ability.OverridenResourceLogic = this.ResourceLogic;
            }
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }

    /// <summary>
    /// Resource-free use of ability, if not on cooldown.
    /// </summary>
    public class AbilityResourceLogicCooldown : AbilityResourceLogic, IAbilityRestriction, IAbilityResourceLogic
    {
        public int Cooldown;

        /// <summary>
        /// Resource-free use of ability, if not on cooldown.
        /// </summary>
        /// <param name="resource">type: <b>BlueprintAbilityResource</b></param>
        /// <param name="cooldown">Cooldown in rounds.</param>
        /// <param name="isSpend">If false, all uses are free without cooldown.</param>
        public AbilityResourceLogicCooldown(AnyRef resource, int cooldown, bool isSpend = true)
        {
            this.m_RequiredResource = resource;
            this.m_IsSpendResource = isSpend;
            this.Cooldown = cooldown;
        }

        public override string GetAbilityRestrictionUIText()
        {
            return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
        }

        public override bool IsAbilityRestrictionPassed(AbilityData ability)
        {
            if (!this.IsSpendResource)
                return true;
            return ability.Caster.Resources.HasEnoughResource(RequiredResource, CalculateCost(ability));
        }

        public override int CalculateCost(AbilityData ability)
        {
            if (ability.Caster.Unit.Ensure<PartCooldown>().IsReady(ability.Blueprint))
                return 0;

            return base.CalculateCost(ability);
        }

        public override void Spend(AbilityData ability)
        {
            if (!this.IsSpendResource)
                return;
            var unit = ability.Caster.Unit;
            if (unit == null || unit.Blueprint.IsCheater)
                return;

            int cost = CalculateCost(ability);
            unit.Ensure<PartCooldown>().Apply(ability.Blueprint, this.Cooldown);

            if (cost > 0 && ability.Caster.Resources.HasEnoughResource(this.RequiredResource, cost))
                unit.Descriptor.Resources.Spend(this.RequiredResource, cost);
        }
    }
}
