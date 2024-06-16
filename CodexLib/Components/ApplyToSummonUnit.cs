using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Runs action list on units summoned by owner.
    /// </summary>
    public class ApplyToSummonUnit : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleSummonUnit>
    {
        /// <summary></summary>
        public ActionList Actions;

        /// <inheritdoc cref="ApplyToSummonUnit"/>
        public ApplyToSummonUnit(params GameAction[] gameActions)
        {
            this.Actions = Helper.CreateActionList(gameActions);
        }

        /// <summary></summary>
        public void OnEventAboutToTrigger(RuleSummonUnit evt)
        {
        }

        /// <summary></summary>
        public void OnEventDidTrigger(RuleSummonUnit evt)
        {
            using (ContextData<SpawnedUnitData>.Request().Setup(evt.SummonedUnit))
            {
                using (base.Context.GetDataScope(evt.SummonedUnit))
                {
                    this.Actions.Run();
                }
            }
        }
    }
}
