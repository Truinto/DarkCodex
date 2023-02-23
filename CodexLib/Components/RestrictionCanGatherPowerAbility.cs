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
                GameLogContext.Text = LocalizedTexts.Instance.WeaponCategories.GetText(Kingmaker.Enums.WeaponCategory.KineticBlast);
            });
        }

        public bool IsCasterRestrictionPassed(UnitEntityData caster)
        {
            UnitPartKineticist unitPartKineticist = caster.Get<UnitPartKineticist>();
            if (!unitPartKineticist)
                return false;

            UnitBody body = caster.Body;

            ItemEntity maybeItem = body.PrimaryHand.MaybeItem;
            bool flag = maybeItem is ItemEntityWeapon itemEntityWeapon && itemEntityWeapon.Blueprint.Category == WeaponCategory.KineticBlast;
            ItemEntity maybeItem2 = body.SecondaryHand.MaybeItem;
            ItemEntityShield maybeShield = body.SecondaryHand.MaybeShield;
            ArmorProficiencyGroup? armorProficiencyGroup = (maybeShield != null) ? new ArmorProficiencyGroup?(maybeShield.Blueprint.Type.ProficiencyGroup) : default(ArmorProficiencyGroup?);
            if (maybeItem != null && !(maybeItem as ItemEntityWeapon).IsMonkUnarmedStrike && !flag)
                return false;

            if (maybeItem2 != null)
            {
                if (armorProficiencyGroup != null)
                {
                    ArmorProficiencyGroup? armorProficiencyGroup2 = armorProficiencyGroup;
                    ArmorProficiencyGroup armorProficiencyGroup3 = ArmorProficiencyGroup.TowerShield;
                    if (!(armorProficiencyGroup2.GetValueOrDefault() == armorProficiencyGroup3 & armorProficiencyGroup2 != null))
                        return unitPartKineticist.CanGatherPowerWithShield;
                }
                return false;
            }

            return true;
        }
    }
}
