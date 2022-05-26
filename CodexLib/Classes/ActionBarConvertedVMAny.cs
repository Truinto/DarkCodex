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
    /// overwrites logic to use any MechanicActionBarSlot
    /// </summary>
    public class ActionBarConvertedVMAny : ActionBarConvertedVM
    {
        public ActionBarConvertedVMAny(ActionBarSlotVM parent, List<MechanicActionBarSlot> list, Action onClose) : base(new(), onClose)
        {
            foreach (var item in list)
                this.Slots.Add(new ActionBarSlotVMChild(parent, item));
        }
    }
}
