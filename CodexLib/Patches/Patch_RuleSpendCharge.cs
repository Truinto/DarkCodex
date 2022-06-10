using HarmonyLib;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib.Patches
{
    /// <summary>
    /// Patch to trigger custom rule RuleSpendCharge.
    /// </summary>
    [HarmonyPatch]
    public class Patch_RuleSpendCharge
    {
        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.Spend))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var code = instructions as List<CodeInstruction> ?? instructions.ToList();
            int index = 0;

            code.AddCondition(ref index, Trigger, generator);
            code.RemoveMethods(typeof(AbilityData), nameof(AbilityData.SpendMaterialComponent));

            //Helper.PrintDebug(code.Join(null, "\n"));
            return code;
        }

        public static bool Trigger(object obj)
        {
            if (obj is not AbilityData __instance)
                return true;

            var ruleSpend = Rulebook.Trigger(new RuleSpendCharge(__instance));

            if (ruleSpend.ShouldConsumeMaterial)
                __instance.SpendMaterialComponent();

            return ruleSpend.ShouldSpend;
        }
    }
}
