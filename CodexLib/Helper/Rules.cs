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
        /// Returns true if <i><paramref name="rule"/></i> dealt at least one point of damage of every type in <i><paramref name="damageType"/></i>.<br/>
        /// If <i><paramref name="any"/></i> is true, any damage type is sufficient, instead of all.
        /// </summary>
        public static bool HasDealtDamage(this RuleDealDamage rule, DamageTypeMix damageType = DamageTypeMix.None, bool any = false)
        {
            if (rule.ResultList == null)
                return false;

            var actual = DamageTypeMix.None;
            foreach (var damage in rule.ResultList)
            {
                if (damage.FinalValue > 0)
                {
                    if (damageType == DamageTypeMix.None)
                        return true;
                    actual |= damage.Source.ToDamageTypeMix();
                }
            }

            if (any)
                return (actual & damageType) == damageType;
            else
                return (actual & damageType) != 0;
        }
    }
}
