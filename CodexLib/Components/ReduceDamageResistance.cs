using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Owner ignores specified amount of DR.
    /// </summary>
    public class ReduceDamageResistance : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>
    {
        public DamageTypeMix Type;
        public ContextValue Amount;

        public ReduceDamageResistance(DamageTypeMix type, ContextValue amount)
        {
            this.Type = type;
            this.Amount = amount;
        }

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
            foreach (var damage in evt.CalculatedDamage)
            {
                var source = damage.Source;
                if (source.Type >= DamageType.Direct)
                    continue;

                if ((source.ToDamageTypeMix() & this.Type) == 0)
                    continue;

                int reduction = Math.Min(source.ReductionBecauseResistance.TotalValue, this.Amount.Calculate(evt.Reason.Context));
                if (reduction <= 0)
                    continue;

                // reduce DR
                source.ReductionBecauseResistance.Add(new Modifier(-reduction, this.Fact, ModifierDescriptor.UntypedStackable));
            }
        }
    }
}
