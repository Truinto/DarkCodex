using HarmonyLib;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CodexLib.Patches
{
    [HarmonyPatch]
    public class Patch_AbilityIsFullRound
    {
        public static bool OverwriteFullRound(AbilityData __instance)
        {
            var flags = __instance.Caster?.Unit.Get<PartCustomData>()?.Flags;
            if (flags == null)
                return false;

            if (flags.HasFlag(MechanicFeature.SpontaneousMetamagicNoFullRound)
                    && __instance.IsSpontaneous
                    && __instance.MetamagicData?.NotEmpty == true
                    && !__instance.Blueprint.IsFullRoundAction)
                return true;
            if (flags.HasFlag(MechanicFeature.SummoningNoFullRound)
                   && __instance.Blueprint.SpellDescriptor.HasAnyFlag(SpellDescriptor.Summoning))
                return true;

            return false;
        }

        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.RequireFullRoundAction), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Postfix1(AbilityData __instance, ref bool __result)
        {
            if (!__result)
                return;

            if (OverwriteFullRound(__instance))
                __result = false;
        }

        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.GetDefaultActionType))]
        [HarmonyPrefix]
        public static void Postfix2(AbilityData __instance, ref CommandType __result)
        {
            if (__result != CommandType.Standard)
                return;

            if (OverwriteFullRound(__instance))
                __result = __instance.Blueprint.ActionType;
        }
    }
}
