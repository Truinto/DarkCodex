using Kingmaker.UI.Models.Log;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class AbilityRestrictionDuelist : BlueprintComponent, IAbilityCasterRestriction
    {
        public bool CheckBucklerOrNoShield;

        public AbilityRestrictionDuelist(bool checkBucklerOrNoShield = false)
        {
            this.CheckBucklerOrNoShield = checkBucklerOrNoShield;
        }

        public bool IsCasterRestrictionPassed(UnitEntityData caster)
        {
            if (caster == null)
                return false;

            if (this.CheckBucklerOrNoShield
                && caster.Body.SecondaryHand.HasShield
                && caster.Body.SecondaryHand.MaybeShield.Blueprint.Type.ProficiencyGroup != ArmorProficiencyGroup.Buckler)
                return false;

            var weapon = caster.GetFirstWeapon();
            if (weapon == null)
                return false;

            var category = weapon.Blueprint.Category;

            if (caster.Ensure<UnitPartDamageGrace>().HasEntry(category))
                return true;

            if (category.HasSubCategory(WeaponSubCategory.Light)
                || category.HasSubCategory(WeaponSubCategory.OneHandedPiercing)
                || category == WeaponCategory.DuelingSword && caster.State.Features.DuelingMastery)
                return true;

            return false;
        }

        public string GetAbilityCasterRestrictionUIText()
        {
            return LocalizedTexts.Instance.Reasons.SpecificWeaponRequired.ToString(() => GameLogContext.Text = DuelistName.ToString());
        }

        public static LocalizedString DuelistName = Helper.CreateString("Duelist Weapon");
    }
}
