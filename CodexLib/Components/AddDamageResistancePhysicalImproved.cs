using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Improves existing Damage reduction
    /// </summary>
    public class AddDamageResistancePhysicalImproved : AddDamageResistancePhysical
    {
        private static PhysicalDamage physical = new(new DiceFormula(0, 0), 0);

        public override int CalculateValue(ComponentRuntime runtime)
        {
            var partDR = runtime.Owner.Get<UnitPartDamageReduction>();
            if (partDR == null)
                return base.CalculateValue(runtime);

            int value = 0;
            foreach (var dr in partDR.m_Chunks.Select(s => s.DR))
            {
                if (!dr.Bypassed(physical, null))
                    value = Math.Max(value, dr.SourceBlueprintComponent.Value.Calculate(runtime.Fact.MaybeContext));
            }

            return value;
        }
    }
}
