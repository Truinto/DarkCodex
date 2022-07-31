using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class KineticEnergizeFist : UnitFactComponentDelegate<VariantSelectionData>, IActionBarConvert, IInitiatorRulebookHandler<RuleAttackWithWeapon>
    {
        public BlueprintUnitFactReference[] Facts;

        public List<MechanicActionBarSlot> GetConverts()
        {
            var data = this.Data;

            var list = new List<MechanicActionBarSlot>();
            foreach (var fact in this.Facts)
            {
                if (fact.NotEmpty() && this.Owner.HasFact(fact))
                    list.Add(new MechanicActionBarSlotVariantSelection(this.Owner, fact, data));
            }

            return list;
        }

        public Sprite GetIcon()
        {
            return this.Data.Selected?.Get()?.Icon;
        }

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            if (this.Fact is not ActivatableAbility act || !act.IsOn)
                return;

            if (this.Data.Selected?.Get() is not BlueprintAbility ab)
                return;

        }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
        }
    }
}
