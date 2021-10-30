using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    public class ContextActionStopActivatables : ContextAction
    {
        public override string GetCaption()
        {
            return "";
        }

        public override void RunAction()
        {
            var unit = this.Context.MaybeCaster;

            foreach (var act in unit.ActivatableAbilities)
            {
                if (act.m_AppliedBuff == null && !act.Blueprint.DeactivateImmediately && act.GetComponent<DeactivateImmediatelyIfNoAttacksThisRound>() == null)
                {
                    unit.Buffs.GetBuff(act.Blueprint.Buff)?.Remove();
                }
            }
        }
    }
}
