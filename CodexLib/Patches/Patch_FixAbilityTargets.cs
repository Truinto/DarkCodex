using HarmonyLib;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Designers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib.Patches
{
    /// <summary>
    /// If no AoERadiusProvider component is present, it will always default to TargetType.Any.
    /// This patch checks the blueprint's settings CanTargetFriends and CanTargetEnemies instead.
    /// </summary>
    [HarmonyPatch]
    public class Patch_FixAbilityTargets
    {
        [HarmonyPatch(typeof(BlueprintAbility), nameof(BlueprintAbility.AoETargets), MethodType.Getter)]
        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPrefix]
        public static bool Prefix(BlueprintAbility __instance, ref TargetType __result)
        {
            var provider = __instance.GetAoERadiusProvider(TacticalCombatHelper.IsActive);
            if (provider != null)
            {
                __result = provider.Targets;
                //Helper.PrintDebug($"AoETargets {provider.Targets}");
                return false;
            }

            if (__instance.CanTargetEnemies && !__instance.CanTargetFriends)
                    __result = TargetType.Enemy;
            else if (!__instance.CanTargetEnemies && __instance.CanTargetFriends)
                __result = TargetType.Ally;
            else
                __result = TargetType.Any;

            //Helper.PrintDebug($"AoETargets override {provider.Targets}");
            return false;
        }
    }
}
