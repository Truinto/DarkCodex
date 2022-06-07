using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib.Patches
{
    /// <summary>
    /// Override of ContextValue by ContextStatValue.<br/>
    /// This patch makes ContextValue.Calculate quasi virtual.
    /// </summary>
    [HarmonyPatch]
    public class Patch_ContextStatValue
    {
        [HarmonyPatch(typeof(ContextValue), nameof(ContextValue.Calculate), typeof(MechanicsContext), typeof(BlueprintScriptableObject), typeof(UnitEntityData))]
        [HarmonyPrefix]
        public static bool Prefix(MechanicsContext context, BlueprintScriptableObject blueprint, UnitEntityData caster, ContextValue __instance, ref int __result)
        {
            if (__instance is ContextStatValue c)
            {
                __result = c.Calculate(context, blueprint, caster);
                return false;
            }
            return true;
        }
    }
}
