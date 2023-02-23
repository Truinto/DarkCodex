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
    /// Remembers parent ActionBarSlotVM of converted slots.
    /// </summary>
    public class ActionBarSlotVMChild : ActionBarSlotVM
    {
        /// <summary>Parent slot that has this open in a convert box.</summary>
        public ActionBarSlotVM Parent;

        /// <inheritdoc cref="ActionBarSlotVMChild"/>
        public ActionBarSlotVMChild(ActionBarSlotVM parent, MechanicActionBarSlot abs, int index = -1, int spellLevel = -1) : base(abs, index, spellLevel)
        {
            this.Parent = parent;
        }

        /// <inheritdoc/>
        public override void DisposeImplementation()
        {
            Parent = null;
            base.DisposeImplementation();
        }

        /// <summary>
        /// Updates icon and border for <see cref="MechanicActionBarSlotVariantSelection"/>.
        /// </summary>
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
