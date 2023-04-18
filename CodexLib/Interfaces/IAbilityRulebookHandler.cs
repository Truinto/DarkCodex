using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public interface IAbilityRulebookHandler
    {
        void OnEventAboutToTrigger(RulebookTargetEvent evt, AbilityExecutionContext context);

        void OnEventDidTrigger(RulebookTargetEvent evt, AbilityExecutionContext context);
    }
}
