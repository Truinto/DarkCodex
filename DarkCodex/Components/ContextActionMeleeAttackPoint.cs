using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;

namespace DarkCodex.Components
{
    public class ContextActionMeleeAttackPoint : ContextAction
    {
        public override string GetCaption()
        {
            return "Melee attack the target closest to a point.";
        }

        public override void RunAction()
        {
            UnitEntityData caster = this.Context.MaybeCaster;
            if (caster == null)
                return;

            WeaponSlot threatHandMelee = caster.GetThreatHandMelee();
            if (threatHandMelee == null)
                return;

            UnitEntityData unitEntityData = SelectTarget(caster, threatHandMelee.Weapon.AttackRange.Meters, this.Target);
            if (unitEntityData == null)
                return;

            Helper.PrintDebug($"Blade Rush: {unitEntityData}");
            RuleAttackWithWeapon ruleAttackWithWeapon = new(caster, unitEntityData, threatHandMelee.Weapon, 0)
            {
                Reason = this.Context,
                AutoHit = false,
                AutoCriticalThreat = false,
                AutoCriticalConfirmation = false,
                ExtraAttack = true
            };
            this.Context.TriggerRule(ruleAttackWithWeapon);
        }

        private UnitEntityData SelectTarget(UnitEntityData caster, float range, TargetWrapper target)
        {
            var point = target.Point;

            range += caster.View.Corpulence * 2;
            UnitEntityData closest = null;
            foreach (UnitGroupMemory.UnitInfo unitInfo in caster.Memory.Enemies)
            {
                UnitEntityData unit = unitInfo.Unit;
                if (unit == null || unit.View == null || !unit.Descriptor.State.IsConscious)
                    continue;

                if (caster.DistanceTo(unit) <= range)
                {
                    if (closest == null || unit.DistanceTo(point) < closest.DistanceTo(point))
                    {
                        closest = unit;
                    }
                }
            }
            return closest;
        }
    }
}
