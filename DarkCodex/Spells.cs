using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class Spells
    {
        [PatchInfo(Severity.Create | Severity.Faulty, "Bladed Dash", "spell: Bladed Dash", false)]
        public static void CreateBladedDash()
        {
            // sfx from CallLightningAbility
            // StormwalkerFlashStepAbility AbilityCustomFlashStep
            // DragonsBreathBlue AbilityDeliverProjectile
        }
    }
}
