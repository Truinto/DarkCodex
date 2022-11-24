using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ContextSharedBonus : BlueprintComponent, IMechanicRecalculate
    {
        public int Bonus;
        public AbilitySharedValue SharedType;

        public int Priority => 300;

        public ContextSharedBonus(int bonus, AbilitySharedValue sharedType = AbilitySharedValue.Damage)
        {
            this.Bonus = bonus;
            this.SharedType = sharedType;
        }

        public void PreCalculate(MechanicsContext context)
        {
        }

        public void PostCalculate(MechanicsContext context)
        {
            context[SharedType] += Bonus;
        }
    }
}
