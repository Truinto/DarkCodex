using System.Collections.Generic;
using System.Linq;
using CodexLib;
using HarmonyLib;
using Kingmaker.RuleSystem.Rules;

namespace CodexLib.Patches
{
    /// <summary>
    /// Logic for ContextConditionAttackRoll to make AOE attack rolls.
    /// </summary>
    [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.OnTrigger))]
    [HarmonyPriority(380)]
    public class Patch_AOEAttackRolls
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var original1 = AccessTools.PropertySetter(typeof(RuleAttackRoll), nameof(RuleAttackRoll.D20));
            var original2 = AccessTools.PropertySetter(typeof(RuleAttackRoll), nameof(RuleAttackRoll.CriticalConfirmationD20));
            var data = new TranspilerTool(instructions, generator, original);

            if (data.ReplaceAllCalls(original1, SetD20) == 0)
                throw new Exception("Unable to patch " + original1);

            if (data.ReplaceAllCalls(original2, SetD20Crit) == 0)
                throw new Exception("Unable to patch " + original2);

            return data;
        }

        public static void SetD20(RuleAttackRoll instance, RuleRollD20 value)
        {
            instance.D20 ??= value;
        }

        public static void SetD20Crit(RuleAttackRoll instance, RuleRollD20 value)
        {
            instance.CriticalConfirmationD20 ??= value;
        }
    }

    //[HarmonyPatch(typeof(KineticistController), nameof(KineticistController.TryRunKineticBladeActivationAction))]
    //public class Patch_KineticistWhipReach
    //{
    //    public static void Postfix(UnitPartKineticist kineticist, UnitCommand cmd, bool __result)
    //    {
    //        if (!__result)
    //            return;
    //        if (kineticist.Owner.Buffs.GetBuff(Patch_KineticistAllowOpportunityAttack2.whip_buff) == null) 
    //            return;
    //        cmd.ApproachRadius += 5f * 0.3048f;
    //    }
    //}
}
