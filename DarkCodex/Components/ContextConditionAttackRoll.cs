using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    public class ContextConditionAttackRoll : ContextCondition
    {
        public ContextConditionAttackRoll(BlueprintItemWeaponReference weapon, bool ignoreAoO = true)
        {
            this.Weapon = weapon;
            this.IgnoreAoO = ignoreAoO;
        }

        public override string GetConditionCaption()
        {
            return "Caster hits target with an attack roll";
        }

        public override bool CheckCondition()
        {
            var weapon = Weapon.Get().CreateEntity<ItemEntityWeapon>();

            if (ShareD20 && this.AbilityContext.AttackRoll == null)
            {
                //Helper.PrintDebug("first attack roll");
                var attack1 = new RuleAttackRoll(this.Context.MaybeCaster, this.Target.Unit, weapon, 0);
                attack1.DoNotProvokeAttacksOfOpportunity = IgnoreAoO;
                this.Context.TriggerRule(attack1);

                this.AbilityContext.AttackRoll = attack1;
                return attack1.IsHit;
            }
            else
            {
                //Helper.PrintDebug("successive attack roll");
                var attack2 = new RuleAttackRoll(this.Context.MaybeCaster, this.Target.Unit, weapon, 0);
                attack2.DoNotProvokeAttacksOfOpportunity = true;
                attack2.D20 = this.AbilityContext.AttackRoll?.D20;
                //attack2.CriticalConfirmationD20 = this.AbilityContext.AttackRoll.CriticalConfirmationD20;
                this.Context.TriggerRule(attack2);

                return attack2.IsHit;
            }
        }

        public static RuleRollD20 LastAttack;
        public static RuleRollD20 LastCrit;

        [NotNull]
        public BlueprintItemWeaponReference Weapon;
        public bool IgnoreAoO;
        public bool ShareD20 = true;
    }
}
