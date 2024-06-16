using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Set the remaining duration of a buff. Does nothing if buff is not on target.
    /// </summary>
    public class ContextActionSetBuffDuration : ContextAction
    {
        public bool ToTarget;
        /// <summary><b>type: BlueprintBuffReference</b></summary>
        public AnyRef TargetBuff; 
        public ContextDurationValue DurationValue;
        public TimeSpan DurationSpan;

        public ContextActionSetBuffDuration(AnyRef targetBuff, double seconds, bool toTarget = false)
        {
            this.ToTarget = toTarget;
            this.TargetBuff = targetBuff;
            this.DurationSpan = TimeSpan.FromSeconds(seconds);
        }

        public ContextActionSetBuffDuration(AnyRef targetBuff, ContextDurationValue durationValue, bool toTarget = false)
        {
            this.ToTarget = toTarget;
            this.TargetBuff = targetBuff;
            this.DurationValue = durationValue;
        }

        public override string GetCaption() => "";

        public override void RunAction()
        {
            MechanicsContext mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;
            if (mechanicsContext == null)
            {
                Helper.PrintError("ContextActionSetBuffDuration: no context found");
                return;
            }

            var seconds = this.DurationValue?.Calculate(mechanicsContext).Seconds ?? DurationSpan;
            var buff = this.ToTarget ? this.Target.Unit.Buffs.GetBuff(this.TargetBuff) : mechanicsContext.MaybeCaster?.Buffs.GetBuff(this.TargetBuff);
            if (buff != null)
            {
                buff.SetDuration(seconds);
                buff.Owner.Buffs.UpdateNextEvent();
            }
        }
    }
}
