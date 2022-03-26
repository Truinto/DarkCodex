using HarmonyLib;
using Kingmaker.UnitLogic.Class.Kineticist.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Dark Elementalist Burn", "for Wild Talents your current amount of burn includes the number of successful Soul Power uses")]
    [HarmonyPatch(typeof(KineticistBurnPropertyGetter), nameof(KineticistBurnPropertyGetter.GetBaseValue))]
    public class Patch_DarkElementalistBurn
    {
        public static void Postfix(ref int __result)
        {
            // calculate maximum and current resource DarkElementalistSoulPowerResource
        }
    }
}
