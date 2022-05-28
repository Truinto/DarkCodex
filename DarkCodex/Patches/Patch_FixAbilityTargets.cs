using HarmonyLib;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Designers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [HarmonyPatch]
    public class Patch_FixAbilityTargets
    {
        [HarmonyPatch(typeof(BlueprintAbility), nameof(BlueprintAbility.AoETargets), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool Prefix(BlueprintAbility __instance, ref TargetType __result)
        {
            var provider = __instance.GetAoERadiusProvider(TacticalCombatHelper.IsActive);
            if (provider != null)
            {
                __result = provider.Targets;
                //Main.PrintDebug($"AoETargets {provider.Targets}");
                return false;
            }

            if (__instance.CanTargetEnemies && !__instance.CanTargetFriends)
                    __result = TargetType.Enemy;
            else if (!__instance.CanTargetEnemies && __instance.CanTargetFriends)
                __result = TargetType.Ally;
            else
                __result = TargetType.Any;

            //Main.PrintDebug($"AoETargets override {provider.Targets}");
            return false;
        }
    }
}
