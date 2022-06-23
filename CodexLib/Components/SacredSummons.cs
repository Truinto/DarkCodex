using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class SacredSummons : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleSummonUnit>
    {
        public void OnEventAboutToTrigger(RuleSummonUnit evt)
        {
        }

        public void OnEventDidTrigger(RuleSummonUnit evt)
        {
            if (Game.Instance.Player.IsTurnBasedModeOn())
            {
                evt.SummonedUnit.Descriptor.RemoveFact(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff);
                Helper.PrintDebug($"{Owner} summoned {evt.SummonedUnit} with SacredSummons");
            }
        }
    }
}
