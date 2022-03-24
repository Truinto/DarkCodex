using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects;
using System;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Fix Area Effect Double Damage", "fixes area effects triggering twice when cast", false)]
    [HarmonyPatch]
    public class Patch_FixAreaDoubleDamage
    {
        [HarmonyPatch(typeof(AreaEffectEntityData), MethodType.Constructor, typeof(AreaEffectView), typeof(MechanicsContext), typeof(BlueprintAbilityAreaEffect), typeof(TargetWrapper), typeof(TimeSpan), typeof(TimeSpan?), typeof(bool))]
        [HarmonyPostfix]
        public static void OnCast(AreaEffectEntityData __instance, ref float ___m_TimeToNextRound)
        {
            // start the OnRound timer at 6 seconds; this will fix the immediate trigger of OnEnter and OnRound at the same time
            if (__instance.Blueprint.GetComponent<AbilityAreaEffectRunAction>()?.UnitEnter?.HasActions == true)
                ___m_TimeToNextRound = 6f;
        }
    }
}
