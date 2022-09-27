using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class FlatFootedForced : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCheckTargetFlatFooted>
    {
        public void OnEventAboutToTrigger(RuleCheckTargetFlatFooted evt)
        {
            evt.ForceFlatFooted = true;
        }

        public void OnEventDidTrigger(RuleCheckTargetFlatFooted evt)
        {
        }
    }
}
