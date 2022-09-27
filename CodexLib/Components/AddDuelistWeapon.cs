using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// WeaponCategory counts as duelist weapon (one-handed piercing weapon).
    /// </summary>
    public class AddDuelistWeapon : UnitFactComponentDelegate
    {
        public WeaponCategory WeaponCategory;

        public AddDuelistWeapon(WeaponCategory weaponCategory)
        {
            this.WeaponCategory = weaponCategory;
        }

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartDamageGrace>().AddEntry(this.WeaponCategory, this.Fact);
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartDamageGrace>().RemoveEntry(this.Fact);
        }

        //public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        //{
        //    if (evt.Weapon.Blueprint.Type.Category != this.WeaponCategory)
        //        return;
        //    if (evt.Initiator.Body.SecondaryHand.MaybeShield?.Blueprint.Type.ProficiencyGroup != ArmorProficiencyGroup.Buckler)
        //        return;
        //    if (evt.Initiator.Body.SecondaryHand.HasWeapon && evt.Initiator.Body.SecondaryHand.MaybeWeapon != evt.Initiator.Body.EmptyHandWeapon)
        //        return;

        //    var dexterity = evt.Initiator.Descriptor.Stats.Dexterity;
        //    var stat = (evt.DamageBonusStat != null) ? (this.Owner.Descriptor.Stats.GetStat(evt.DamageBonusStat.Value) as ModifiableValueAttributeStat) : null;
        //    if (dexterity != null && (stat == null || dexterity.Bonus > stat.Bonus))
        //        evt.OverrideDamageBonusStat(StatType.Dexterity);
        //}
    }
}
