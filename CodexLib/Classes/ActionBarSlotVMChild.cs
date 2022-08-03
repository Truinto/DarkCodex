using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UI.UnitSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// remembers parent ActionBarSlotVM
    /// </summary>
    public class ActionBarSlotVMChild : ActionBarSlotVM
    {
        public ActionBarSlotVM Parent;

        public ActionBarSlotVMChild(ActionBarSlotVM parent, MechanicActionBarSlot abs, int index = -1, int spellLevel = -1) : base(abs, index, spellLevel)
        {
            this.Parent = parent;
        }

        public override void DisposeImplementation()
        {
            Parent = null;
            base.DisposeImplementation();
        }

        public override void OnMainClick()
        {
            base.OnMainClick();

            if (this.MechanicActionBarSlot is MechanicActionBarSlotVariantSelection selection)
            {
                bool isOn = selection.IsActive();

                if (isOn)
                    this.Parent.ForeIcon.Value = selection.GetIcon();
                else
                    this.Parent.ForeIcon.Value = null;

                if (this.Parent.MechanicActionBarSlot is MechanicActionBarSlotActivableAbility act)
                    act.ActivatableAbility.IsOn = isOn;
            }

        }
    }
}
