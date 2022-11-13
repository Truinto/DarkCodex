using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ContextConditionDuelistWeapon : ContextCondition
    {
        public bool CheckBucklerOrNoShield;

        public ContextConditionDuelistWeapon(bool checkBucklerOrNoShield = false)
        {
            this.CheckBucklerOrNoShield = checkBucklerOrNoShield;
        }

        public override string GetConditionCaption() => nameof(ContextConditionDuelistWeapon);

        public override bool CheckCondition()
        {
            var owner = this.Context.MaybeOwner;
            if (owner == null)
                return false;

            if (this.CheckBucklerOrNoShield 
                && owner.Body.SecondaryHand.HasShield 
                && owner.Body.SecondaryHand.MaybeShield.Blueprint.Type.ProficiencyGroup != ArmorProficiencyGroup.Buckler)
                return false;

            var weapon = owner.GetFirstWeapon();
            if (weapon == null)
                return false;

            var category = weapon.Blueprint.Category;

            if (owner.Ensure<UnitPartDamageGrace>().HasEntry(category))
                return true;

            if (category.HasSubCategory(WeaponSubCategory.Light)
                || category.HasSubCategory(WeaponSubCategory.OneHandedPiercing)
                || category == WeaponCategory.DuelingSword && owner.State.Features.DuelingMastery)
                return true;

            return false;
        }
    }
}
