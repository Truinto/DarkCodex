using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// unfinished
    /// </summary>
    public class RuleSpendCharge : RulebookEvent
    {
        public bool ShouldSpend = true;
        public readonly AbilityData AbilityData;

        public RuleSpendCharge([NotNull] AbilityData abilityData) : base(abilityData.Caster.Unit)
        {
            this.AbilityData = abilityData;
        }

        public override void OnTrigger(RulebookEventContext context)
        {
            // patch AbilityData.Spend, AbilityData.SpendFromSpellbook
            // check SpendChargesOnSpellCast, ActivatableAbilityResourceLogic.SpendResource

        }
    }
}
