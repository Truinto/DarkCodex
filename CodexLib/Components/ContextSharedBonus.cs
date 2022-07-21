using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ContextSharedBonus : BlueprintComponent, IContextBonus
    {
        public int Bonus;
        public AbilitySharedValue SharedType;

        public ContextSharedBonus(int bonus, AbilitySharedValue sharedType = AbilitySharedValue.Damage)
        {
            this.Bonus = bonus;
            this.SharedType = sharedType;
        }

        public void Apply(MechanicsContext context)
        {
            context[SharedType] += Bonus;
        }
    }
}
