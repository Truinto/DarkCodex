using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib.Patches
{
    /// <summary>
    /// Allow for more <see cref="AbilityParameterType"/> ContextValues.<br/>
    /// 3=CasterLevel, 4=RankBonus, 5=DC, 6=Concentraction
    /// </summary>
    [HarmonyPatch(typeof(ContextValue), nameof(ContextValue.GetAbilityParameter))]
    public class Patch_ContextValueParams
    {
        /// <summary></summary>
        public static void Postfix(AbilityExecutionContext context, ContextValue __instance, ref int __result)
        {
            switch (__instance.m_AbilityParameter)
            {
                case (AbilityParameterType)3:
                    __result = context.Params.CasterLevel;
                    break;
                case (AbilityParameterType)4:
                    __result = context.Params.RankBonus;
                    break;
                case (AbilityParameterType)5:
                    __result = context.Params.DC;
                    break;
                case (AbilityParameterType)6:
                    __result = context.Params.Concentration;
                    break;
            }
        }
    }
}
