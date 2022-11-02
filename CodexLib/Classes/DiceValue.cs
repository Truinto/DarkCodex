using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// A simplified ContextDiceValue without context.
    /// </summary>
    public class DiceValue
    {
        public int Dice;
        public DiceType DiceType;
        public int Bonus;

        public PhysicalDamage GetPhysical()
        {
            return new PhysicalDamage(new(new DiceFormula(Dice, DiceType)), Bonus, PhysicalDamageForm.Slashing);
        }

        public DirectDamage GetDirect()
        {
            return new DirectDamage(new DiceFormula(Dice, DiceType), Bonus);
        }

        public int Roll()
        {
            return RulebookEvent.Dice.D(new DiceFormula(this.Dice, this.DiceType)) + this.Bonus;
        }
        
        public DiceValue Increase(ContextDiceValue value, MechanicsContext context) => Increase(Get(value, context));
        public DiceValue Increase(DiceValue value)
        {
            this.Dice += value.Dice;
            if (this.DiceType < value.DiceType)
                this.DiceType = value.DiceType;
            this.Bonus += value.Bonus;
            return this;
        }

        public DiceValue Max(ContextDiceValue value, MechanicsContext context) => Max(Get(value, context));
        public DiceValue Max(DiceValue value)
        {
            this.Dice = Math.Max(this.Dice, value.Dice);
            if (this.DiceType < value.DiceType)
                this.DiceType = value.DiceType;
            this.Bonus = Math.Max(this.Bonus, value.Bonus);
            return this;
        }

        public static DiceValue Get(ContextDiceValue value, MechanicsContext context)
        {
            var result = new DiceValue();
            result.Dice = value.DiceCountValue.Calculate(context);
            result.DiceType = value.DiceType;
            result.Bonus = value.BonusValue.Calculate(context);
            return result;
        }

        public override string ToString()
        {
            return $"{Dice}d{(int)DiceType}+{Bonus}";
        }

        public static implicit operator DiceValue(ContextDiceValue value)
        {
            var context = ContextData<MechanicsContext.Data>.Current?.Context;
            if (context == null)
            {
                Helper.PrintException(new ExceptionDebug("DiceValue missing Context"));
                return new();
            }

            return Get(value, context);
        }
    }
}
