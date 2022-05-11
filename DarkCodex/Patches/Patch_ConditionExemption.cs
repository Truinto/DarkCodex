using HarmonyLib;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarkCodex.Components;
using Shared;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Status Effect Exemptions", "Adds logic to ignore status effects under certain conditions.", false)]
    [HarmonyPatch]
    public class Patch_ConditionExemption
    {
        [HarmonyPatch(typeof(UnitState), nameof(UnitState.AddCondition))]
        [HarmonyPrefix]
        public static void Prefix1(UnitCondition condition, ref Buff source, UnitState __instance)
        {
            var exceptions = __instance.m_ConditionsExceptions[(int)condition];
            if (exceptions == null)
                return;

            foreach (var exception in exceptions)
            {
                if (exception is UnitConditionExceptionsFromBuff buffException && buffException.IsException(source))
                {
                    Helper.PrintDebug($"ConditionExemption {condition} from {source}");
                    __instance.m_Conditions[(int)condition]--;
                    source = null;
                    break;
                }
            }
        }
    }
}
