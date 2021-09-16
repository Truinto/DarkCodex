using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    public class DiceValue
    {
        public int Dice;
        public DiceType DiceType;
        public int Bonus;

        public DirectDamage GetDirect()
        {
            return new DirectDamage(new DiceFormula(Dice, DiceType), Bonus);
        }

        public void Increase(ContextDiceValue value, MechanicsContext context)
        {
            this.Dice += value.DiceCountValue.Calculate(context);
            if (this.DiceType < value.DiceType)
                this.DiceType = value.DiceType;
            this.Bonus += value.BonusValue.Calculate(context);
        }

        public static DiceValue Get(ContextDiceValue value, MechanicsContext context)
        {
            var result = new DiceValue();
            result.Dice = value.DiceCountValue.Calculate(context);
            result.DiceType = value.DiceType;
            result.Bonus = value.BonusValue.Calculate(context);
            return result;
        }
    }
}
