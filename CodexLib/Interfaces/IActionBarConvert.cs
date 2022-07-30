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
    /// Allow custom action bar convertions (unfoldable variants).
    /// </summary>
    public interface IActionBarConvert
    {
        public List<MechanicActionBarSlot> GetConverts();

        [CanBeNull]
        public Sprite GetIcon();
    }
}
