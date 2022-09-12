using Kingmaker.UI;
using Kingmaker.UI.UnitSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Component to add foldable to any BlueprintAbility or BlueprintActivatableAbility. Takes priority over existing conversions, if any.
    /// </summary>
    public class ActivatableVariants : UnitFactComponentDelegate<VariantSelectionData>, IActionBarConvert
    {
        public bool NeedFact = true;
        public BlueprintUnitFactReference[] Facts;

        public ActivatableVariants(params AnyRef[] facts)
        {
            this.Facts = facts.ToRef<BlueprintUnitFactReference>();
        }

        public List<MechanicActionBarSlot> GetConverts()
        {
            var data = this.Data;

            var list = new List<MechanicActionBarSlot>();
            foreach (var fact in this.Facts)
            {
                if (fact.NotEmpty() && (!this.NeedFact || this.Owner.HasFact(fact)))
                    list.Add(new MechanicActionBarSlotVariantSelection(this.Owner, fact.Get(), data));
            }

            return list;
        }

        public Sprite GetIcon()
        {
            return this.Data.Selected?.Icon;
        }
    }

    public sealed class VariantSelectionData
    {
        [JsonProperty]
        public IUIDataProvider Selected;
    }
}
