using HarmonyLib;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib.Patches
{
    [HarmonyPatch]
    public class Patch_AbilityIsFullRound
    {
        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.RequireFullRoundAction), MethodType.Getter)]
        public static void Postfix(AbilityData __instance, ref bool __result)
        {
            if (!__result)
                return;

            if (__instance.IsSpontaneous 
                && __instance.MetamagicData?.NotEmpty == true 
                && !__instance.Blueprint.IsFullRoundAction 
                && __instance.Caster?.Unit.HasFlag(MechanicFeature.SpontaneousMetamagicNoFullRound) == true)
            {
                __result = false;
            }

            if (__instance.Blueprint.SpellDescriptor.HasAnyFlag(SpellDescriptor.Summoning)
                && __instance.Caster?.Unit.HasFlag(MechanicFeature.SummoningNoFullRound) == true)
            {
                __result = false;
            }
        }
    }
}
