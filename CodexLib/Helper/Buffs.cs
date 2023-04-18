using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public static partial class Helper
    {
        /// <summary>
        /// Applies buff to unit or increases its duration, if unit is already under the effect of that buff.
        /// </summary>
        public static void AddBuffStacking(this UnitEntityData unit, BlueprintBuff blueprintBuff, MechanicsContext parentContext, TimeSpan? duration = null)
        {
            var buff = unit.Buffs.GetBuff(blueprintBuff);
            if (buff != null)
            {
                if (duration.HasValue)
                    buff.IncreaseDuration(duration.Value);
                else
                    buff.MakePermanent();
            }
            else
            {
                unit.AddBuff(blueprintBuff, parentContext, duration);
            }
        }
    }
}
