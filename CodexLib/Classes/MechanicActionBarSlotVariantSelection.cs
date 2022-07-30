using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UI.UnitSettings;
using Owlcat.Runtime.UI.Tooltips;

namespace CodexLib
{
    public class MechanicActionBarSlotVariantSelection : MechanicActionBarSlot, IActionBarDisableDrag
    {
        public BlueprintUnitFactReference Blueprint;
        public VariantSelectionData SelectionData;

        public MechanicActionBarSlotVariantSelection(UnitEntityData unit, BlueprintUnitFactReference blueprint, VariantSelectionData selection)
        {
            this.Unit = unit;
            this.Blueprint = blueprint ?? new();
            this.SelectionData = selection;
        }

        public override void OnClick()
        {
            base.OnClick();
            if (IsActive())
                this.SelectionData.Selected = null;
            else
                this.SelectionData.Selected = this.Blueprint;
        }

        public override bool IsActive() => this.SelectionData.Selected == this.Blueprint;
        public override bool IsBad() => !this.Blueprint.NotEmpty();
        public override bool IsDisabled(int resourceCount) => false;
        public override bool CanUseIfTurnBasedInternal() => true;
        public override object GetContentData() => null;
        public override Color GetDecorationColor() => Color.white;
        public override Sprite GetDecorationSprite() => null;
        public override string GetTitle() => Blueprint.Get()?.Name;
        public override string GetDescription() => Blueprint.Get()?.Description;
        public override Sprite GetIcon() => Blueprint.Get()?.Icon;
        public override int GetResource() => -1;
        public override bool IsCasting() => false;
        public override TooltipBaseTemplate GetTooltipTemplate() => new TooltipTemplateDataProvider(new UIData(GetTitle(), GetDescription(), GetIcon()));
    }
}
