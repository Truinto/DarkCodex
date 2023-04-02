using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UI.Log;
using Kingmaker.UI.Models.Log;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Class.Kineticist;

namespace CodexLib
{
    [AllowedOn(typeof(BlueprintAbility), false)]
    public class RestrictionCanGatherPowerAbility : BlueprintComponent, IAbilityCasterRestriction
    {
        public string GetAbilityCasterRestrictionUIText()
        {
            return LocalizedTexts.Instance.Reasons.SpecificWeaponRequired.ToString(() =>
            {
                GameLogContext.Text = LocalizedTexts.Instance.WeaponCategories.GetText(WeaponCategory.KineticBlast);
            });
        }

        public bool IsCasterRestrictionPassed(UnitEntityData caster)
        {
            UnitPartKineticist unitPartKineticist = caster.Get<UnitPartKineticist>();
            if (!unitPartKineticist)            
                return false;
            
            UnitBody body = caster.Body;            
            ItemEntity maybeItem = body.PrimaryHand.MaybeItem;
            ItemEntityWeapon obj = maybeItem as ItemEntityWeapon;
            bool flag = obj != null && obj.Blueprint.Category == WeaponCategory.KineticBlast;
            ItemEntity maybeItem2 = body.SecondaryHand.MaybeItem;
            ArmorProficiencyGroup? armorProficiencyGroup = body.SecondaryHand.MaybeShield?.Blueprint.Type.ProficiencyGroup;

            if (maybeItem != null && !(maybeItem as ItemEntityWeapon).IsMonkUnarmedStrike && !flag)            
                return false;
            
            if (maybeItem2 != null)
            {
                if (armorProficiencyGroup.HasValue && armorProficiencyGroup != ArmorProficiencyGroup.TowerShield)                
                    return unitPartKineticist.CanGatherPowerWithShield;                
                return false;
            }
            return true;
        }
    }
}
