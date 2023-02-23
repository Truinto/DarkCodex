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
            var data = new TranspilerTool(instructions, generator, original);

            data.InsertReturn(Trigger, true);
            data.ReplaceAllCalls(typeof(AbilityData), nameof(AbilityData.SpendMaterialComponent), Patch0);

            return data;
        }

        public static bool Trigger(AbilityData __instance)
        {
            var ruleSpend = Rulebook.Trigger(new RuleSpendCharge(__instance));

            if (ruleSpend.ShouldConsumeMaterial)
                __instance.SpendMaterialComponent();

            return ruleSpend.ShouldSpend;
        }

        public static void Patch0(AbilityData __instance)
        {
        }
    }
}
