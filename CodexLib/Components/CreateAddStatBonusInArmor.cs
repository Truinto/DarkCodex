using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class CreateAddStatBonusInArmor : UnitFactComponentDelegate, IUnitActiveEquipmentSetHandler, IUnitEquipmentHandler
    {
        public ContextValue Value;
        public StatType Stat;
        public ModifierDescriptor Descriptor;
        public ArmorProficiencyGroup[] Category;

        public CreateAddStatBonusInArmor(ContextValue value, StatType stat, ModifierDescriptor descriptor = ModifierDescriptor.UntypedStackable, params ArmorProficiencyGroup[] category)
        {
            this.Value = value;
            this.Stat = stat;
            this.Descriptor = descriptor;
            this.Category = category;
        }

        public override void OnTurnOn()
        {
            CheckArmor();
        }

        public override void OnTurnOff()
        {
            DeactivateModifier();
        }

        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        {
            if (unit.Unit != this.Owner)
                return;

            CheckArmor();
        }

        public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
        {
            if (slot.Owner != this.Owner || slot is not ArmorSlot)
                return;

            CheckArmor();
        }

        private void CheckArmor()
        {
            if (!this.Fact.IsTurnedOn)
                return;

            var type = this.Owner.Body.Armor.MaybeArmor?.ArmorType();
            if (type == null && this.Category.Contains(ArmorProficiencyGroup.None) || this.Category.Contains(type.Value))
                ActivateModifier();
            else
                DeactivateModifier();
        }

        private void ActivateModifier()
        {
            this.Owner.Stats.GetStat(this.Stat)?.AddModifier(this.Value.Calculate(this.Context), this.Runtime, this.Descriptor);
        }

        private void DeactivateModifier()
        {
            this.Owner.Stats.GetStat(this.Stat)?.RemoveModifiersFrom(this.Runtime);
        }
    }
}
