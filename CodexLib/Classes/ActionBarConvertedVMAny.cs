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
    /// Overwrites logic to use any MechanicActionBarSlot.
    /// </summary>
    public class ActionBarConvertedVMAny : ActionBarConvertedVM
    {
        /// <inheritdoc cref="ActionBarConvertedVMAny"/>
        public ActionBarConvertedVMAny(ActionBarSlotVM parent, List<MechanicActionBarSlot> list, Action onClose) : base([], onClose)
        {
            foreach (var item in list)
                this.Slots.Add(new ActionBarSlotVMChild(parent, item));
        }
    }
}
