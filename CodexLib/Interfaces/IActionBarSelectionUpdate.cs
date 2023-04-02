using Kingmaker.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Triggers when player clicks on a MechanicActionBarSlotVariantSelection variant.
    /// </summary>
    public interface IActionBarSelectionUpdate
    {
        public void Update([NotNull] AbilityData ability, [CanBeNull] IUIDataProvider blueprint);
    }
}
