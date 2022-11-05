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
    [AllowedOn(typeof(BlueprintActivatableAbility))]
    [AllowedOn(typeof(BlueprintAbility))]
    public class ActivatableVariants : UnitFactComponentDelegate<VariantSelectionData>, IActionBarConvert
    {
        public bool NeedFact = true;
        public BlueprintUnitFactReference[] Facts;

        /// <param name="facts">type: <b>BlueprintUnitFact</b></param>
        public ActivatableVariants(params AnyRef[] facts)
        {
            this.Facts = facts.ToRef<BlueprintUnitFactReference>();
        }

        public virtual List<MechanicActionBarSlot> GetConverts()
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

        public virtual Sprite GetIcon()
        {
            return this.Data.Selected?.Icon;
        }

        public bool IsOn => this.Fact is ActivatableAbility act && act.IsOn;

        public BlueprintUnitFact Selected => this.Data.Selected as BlueprintUnitFact;

        /// <summary>Example</summary>
        private void OnEventAboutToTrigger()
        {
            if (this.Fact is not ActivatableAbility act || !act.IsOn)
                return;

            if (this.Data.Selected is not BlueprintUnitFact feature)
                return;
        }
    }

    public sealed class VariantSelectionData
    {
        [JsonProperty]
        public IUIDataProvider Selected;
    }
}
