using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UI.UnitSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ActionBarSlotVMChild : ActionBarSlotVM // remembers parent ActionBarSlotVM
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
    }
}
