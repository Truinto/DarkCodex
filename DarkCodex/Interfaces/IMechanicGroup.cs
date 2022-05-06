using Kingmaker.UI.UnitSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public interface IMechanicGroup
    {
        public List<MechanicActionBarSlot> Slots { get; }
        public void AddToGroup(MechanicActionBarSlot mechanic, MechanicActionBarSlot target = null, bool placeRight = true);
        public void RemoveFromGroup(MechanicActionBarSlot mechanic);
    }
}
