using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ContextActionSubstituteBuff : ContextAction
    {
        public BlueprintBuffReference Buff;
        public BlueprintBuffReference Replacement;
        public bool MergeDuration;

        public ContextActionSubstituteBuff(BlueprintBuffReference buff, BlueprintBuffReference replacement, bool mergeDuration = true)
        {
            this.Buff = buff;
            this.Replacement = replacement;
            this.MergeDuration = mergeDuration;
        }

        public override string GetCaption()
        {
            return "";
        }

        public override void RunAction()
        {
            var unit = this.Context.MainTarget?.Unit;
            if (unit == null)
                return;

            var buff1 = unit.Buffs.GetBuff(this.Buff);
            if (buff1 == null)
                return;

            var buff2 = unit.Buffs.GetBuff(this.Replacement);

            if (this.MergeDuration && buff2 != null)
                buff2.IncreaseDuration(buff1.TimeLeft);
            else
            {
                unit.AddBuff(this.Replacement, buff1.Context, buff1.TimeLeft);
                buff1.Remove();
            }
        }
    }
}
