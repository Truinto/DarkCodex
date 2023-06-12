using HarmonyLib;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Fix Quicken Metamagic", "fixed quickened abilities taking more time than normal, if you already have used your swift action", false)]
    [HarmonyPatch]
    public class Patch_FixQuickenMetamagic
    {
        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.RuntimeActionType), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Postfix(AbilityData __instance, ref UnitCommand.CommandType __result)
        {
            if (__instance.HasMetamagic(Metamagic.Quicken) && __instance.Caster.Unit.CombatState.HasCooldownForCommand(UnitCommand.CommandType.Swift))
            {
                __result = __instance.Blueprint.ActionType;
            }
        }
    }
}
