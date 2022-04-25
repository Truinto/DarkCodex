using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [AllowMultipleComponents]
    public class AddConditionExceptions : UnitFactComponentDelegate
    {
        public UnitCondition Condition;
        public UnitConditionExceptions Exception;

        public override void OnTurnOn()
        {
            int index = (int)Condition;
            var exceptions = Owner.State.m_ConditionsExceptions;
            if (exceptions[index] == null)
                exceptions[index] = new();
            exceptions[index].Add(Exception);
        }

        public override void OnTurnOff()
        {
            int index = (int)Condition;
            var exceptions = Owner.State.m_ConditionsExceptions;
            if (exceptions[index] != null)
                exceptions[index].Remove(Exception);
        }
    }
}
