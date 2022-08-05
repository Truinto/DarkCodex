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
            return this.Data.Wrapper.Selected?.Icon;
        }
    }

    public sealed class VariantSelectionData
    {
        [JsonProperty]
        public VariantSelectionWrapper Wrapper = new();
    }

    /// <summary>
    /// It is necessary to wrap this value, because the json converter for UnitFactComponentDelegate does save $type.
    /// Without $type it is necessary that my converter can resolve UIDataProvider, which could be used by other converters causing incompatabilities.
    /// By using a wrapper I can ensure my JsonConverter will not try and fail to convert foreign types.
    /// </summary>
    public sealed class VariantSelectionWrapper
    {
        [JsonProperty]
        public IUIDataProvider Selected;
    }
}
