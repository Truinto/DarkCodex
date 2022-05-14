using Kingmaker.UnitLogic.Mechanics.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ContextConditionMoreHitDice : ContextCondition
    {
        public override string GetConditionCaption()
        {
            return "";
        }

        public override bool CheckCondition()
        {
            if (this.Context.MaybeCaster == null || this.Target.Unit == null)
                return false;

            return this.Context.MaybeCaster.Progression.CharacterLevel > this.Target.Unit.Progression.CharacterLevel;
        }
    }
}
