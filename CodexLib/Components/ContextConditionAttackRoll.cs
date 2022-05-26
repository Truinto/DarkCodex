using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ContextConditionAttackRoll : ContextCondition
    {
        public static RuleRollD20 LastAttack;
        public static RuleRollD20 LastCrit;

        public BlueprintItemWeaponReference Weapon;
        public bool IgnoreAoO;
        public bool ShareD20 = true;
        public bool ApplyBladedBonus;

        public ContextConditionAttackRoll() { }

        public ContextConditionAttackRoll([CanBeNull] BlueprintItemWeaponReference weapon, bool ignoreAoO = true)
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
            var caster = this.Context.MaybeCaster;
            if (caster == null)
                return false;

            var weapon = Weapon?.Get()?.CreateEntity<ItemEntityWeapon>() ?? caster.GetThreatHandMelee()?.Weapon;
            if (weapon == null)
                return false;

            var attackRoll = new RuleAttackRoll(this.Context.MaybeCaster, this.Target.Unit, weapon, 0);
            
            if (ApplyBladedBonus)
            {
                int bonus = Math.Max(caster.Descriptor.Stats.Intelligence.ModifiedValue, caster.Descriptor.Stats.Charisma.ModifiedValue);
                attackRoll.AddModifier(bonus, ModifierDescriptor.UntypedStackable);
            }

            if (!ShareD20 || this.AbilityContext.AttackRoll == null)
            {
                //Helper.PrintDebug("first attack roll");
                attackRoll.DoNotProvokeAttacksOfOpportunity = IgnoreAoO;
                this.Context.TriggerRule(attackRoll);

                this.AbilityContext.AttackRoll = attackRoll;
                return attackRoll.IsHit;
            }
            else
            {
                //Helper.PrintDebug("successive attack roll");
                attackRoll.DoNotProvokeAttacksOfOpportunity = true;
                attackRoll.D20 = this.AbilityContext.AttackRoll?.D20;
                attackRoll.CriticalConfirmationD20 = this.AbilityContext.AttackRoll.CriticalConfirmationD20;
                this.Context.TriggerRule(attackRoll);

                return attackRoll.IsHit;
            }
        }

        public static GameAction GameAction(BlueprintItemWeaponReference weapon = null, bool ignoreAoO = true)
        {
            return Helper.CreateConditional(new ContextConditionAttackRoll(weapon, ignoreAoO));
        }
    }
}
