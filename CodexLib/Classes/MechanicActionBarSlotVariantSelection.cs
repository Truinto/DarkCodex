using Kingmaker.UI;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UI.UnitSettings;
using Owlcat.Runtime.UI.Tooltips;

namespace CodexLib
{
    public class MechanicActionBarSlotVariantSelection : MechanicActionBarSlot, IActionBarDisableDrag
    {
        public IUIDataProvider Blueprint;
        public VariantSelectionData SelectionData;

        public MechanicActionBarSlotVariantSelection(UnitEntityData unit, IUIDataProvider blueprint, VariantSelectionData selection)
        {
            this.Unit = unit;
            this.Blueprint = blueprint;
            this.SelectionData = selection;
        }

        public override void OnClick()
        {
            base.OnClick();
            if (IsActive())
                this.SelectionData.Wrapper.Selected = null;
            else
                this.SelectionData.Wrapper.Selected = this.Blueprint;
        }

        public override bool IsActive() => this.SelectionData.Wrapper.Selected == this.Blueprint;
        public override bool IsBad() => this.Blueprint == null;
        public override bool IsDisabled(int resourceCount) => false;
        public override bool CanUseIfTurnBasedInternal() => true;
        public override object GetContentData() => null;
        public override Color GetDecorationColor() => Color.white;
        public override Sprite GetDecorationSprite() => null;
        public override string GetTitle() => Blueprint.Name;
        public override string GetDescription() => Blueprint.Description;
        public override Sprite GetIcon() => Blueprint.Icon;
        public override int GetResource() => -1;
        public override bool IsCasting() => false;
        public override TooltipBaseTemplate GetTooltipTemplate() => new TooltipTemplateDataProvider(new UIData(GetTitle(), GetDescription(), GetIcon()));
    }
}
