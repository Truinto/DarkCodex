using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
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
            var attack = new RuleAttackRoll(this.Context.MaybeCaster, this.Target.Unit, weapon, 0);
            attack.DoNotProvokeAttacksOfOpportunity = IgnoreAoO;
            attack.SuspendCombatLog = false;
            this.Context.TriggerRule(attack);

            return attack.IsHit;
        }

        [NotNull]
        public BlueprintItemWeaponReference Weapon;
        public bool IgnoreAoO;
    }
}
