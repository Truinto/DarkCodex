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
        public bool CanBeRanged;

        public ContextConditionAttackRoll(AnyRef weapon = null, bool ignoreAoO = true, bool canBeRanged = true)
        {
            this.Weapon = weapon;
            this.IgnoreAoO = ignoreAoO;
            this.CanBeRanged = canBeRanged;
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

            var weapon = this.Weapon?.Get()?.CreateEntity<ItemEntityWeapon>() ?? (this.CanBeRanged ? caster.GetFirstWeapon() : caster.GetThreatHandMelee()?.MaybeWeapon);
            if (weapon == null)
                return false;

            var attackRoll = new RuleAttackRoll(this.Context.MaybeCaster, this.Target.Unit, weapon, 0);
            
            if (ApplyBladedBonus)
            {
                int intelligence = caster.Descriptor.Stats.Intelligence.Bonus;
                int charisma = caster.Descriptor.Stats.Charisma.Bonus;

                if (intelligence > charisma)
                    attackRoll.AddModifier(intelligence, Const.Intelligence);
                else
                    attackRoll.AddModifier(charisma, Const.Charisma);
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
    }
}
