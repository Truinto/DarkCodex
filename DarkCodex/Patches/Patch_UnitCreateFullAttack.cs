using Kingmaker.UnitLogic.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_UnitCreateFullAttack
    {
        [HarmonyPatch(typeof(UnitAttack), nameof(UnitAttack.CreateFullAttack))]
        [HarmonyPostfix]
        public static void Postfix1(UnitAttack __instance, List<AttackHandInfo> __result)
        {

        }
    }
}
