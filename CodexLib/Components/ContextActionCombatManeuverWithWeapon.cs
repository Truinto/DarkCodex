using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ContextActionCombatManeuverWithWeapon : ContextAction
    {
        public CombatManeuver Type;
        public bool CanBeRanged;
        public bool OnlyDuelistWeapon;

        public ContextActionCombatManeuverWithWeapon(CombatManeuver type, bool canBeRanged = false, bool onlyDuelistWeapon = false)
        {
            this.Type = type;
            this.CanBeRanged = canBeRanged;
            this.OnlyDuelistWeapon = onlyDuelistWeapon;
        }

        public override string GetCaption()
        {
            return "Weapon Maneuver";
        }

        public override void RunAction()
        {
            var attacker = this.Context.MaybeCaster;
            if (attacker == null)
                return;

            var target = this.Target.Unit;
            if (target == null)
                return;

            var weapon = this.CanBeRanged ? attacker.GetFirstWeapon() : attacker.GetThreatHandMelee()?.MaybeWeapon;
            if (weapon == null)
                return;

            bool suitable = !this.OnlyDuelistWeapon 
                || weapon.Blueprint.Category.HasSubCategory(WeaponSubCategory.Light) 
                || weapon.Blueprint.Category.HasSubCategory(WeaponSubCategory.OneHandedPiercing) 
                || attacker.Descriptor.Ensure<UnitPartDamageGrace>().HasEntry(weapon.Blueprint.Category) 
                || (weapon.Blueprint.Category == WeaponCategory.DuelingSword && attacker.Descriptor.State.Features.DuelingMastery);
            if (!suitable)
                return;

            this.Context.TriggerRule(new RuleCombatManeuverWithWeapon(attacker, target, this.Type, weapon, targetAC: true));
        }
    }
}
