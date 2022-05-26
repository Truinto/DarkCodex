using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    //AttackOfOpportunityAttackBonus

    [AllowedOn(typeof(BlueprintUnitFact), false)]
    public class AddAttackBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAttackBonus>
    {
        public ModifierDescriptor Descriptor;
        public ContextValue[] Bonus;
        public bool IsSumBonus;

        public AddAttackBonus(ModifierDescriptor descriptor, params ContextValue[] bonus)
        {
            this.Descriptor = descriptor;
            this.Bonus = bonus;
        }

        public AddAttackBonus(ModifierDescriptor descriptor, params ContextStatValue[] bonus)
        {
            this.Descriptor = descriptor;
            this.Bonus = bonus;
        }

        public void OnEventAboutToTrigger(RuleCalculateAttackBonus evt)
        {
            int bonus = this.Bonus[0].Calculate(this.Context);
            for (int i = 1; i < this.Bonus.Length; i++)
                if (IsSumBonus)
                    bonus += this.Bonus[i].Calculate(this.Context);
                else
                    bonus = Math.Max(bonus, this.Bonus[i].Calculate(this.Context));

            evt.AddModifier(bonus, this.Fact, this.Descriptor);

            Helper.PrintDebug($"triggered AddAttackBonus {bonus}");
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonus evt)
        {
        }
    }
}
