using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Kingmaker.RuleSystem.Rules;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: AOE Attack Rolls", "allows Impale Infusion and other AOE attacks to roll once for all", false)]
    [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.OnTrigger))]
    public class Patch_AOEAttackRolls
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            var line = instr.ToList();
            var original1 = AccessTools.PropertySetter(typeof(RuleAttackRoll), nameof(RuleAttackRoll.D20));
            var original2 = AccessTools.PropertySetter(typeof(RuleAttackRoll), nameof(RuleAttackRoll.CriticalConfirmationD20));

            for (int i = 0; i < line.Count; i++)
            {
                if (line[i].Calls(original1))
                    line[i].ReplaceCall(typeof(Patch_AOEAttackRolls), nameof(SetD20));
                else if(line[i].Calls(original2))
                    line[i].ReplaceCall(typeof(Patch_AOEAttackRolls), nameof(SetD20Crit));
            }

            return line;
        }

        public static void SetD20(RuleAttackRoll instance, RuleRollD20 d20)
        {
            if (instance.D20 == null)
                instance.D20 = d20;
        }

        public static void SetD20Crit(RuleAttackRoll instance, RuleRollD20 d20)
        {
            if (instance.CriticalConfirmationD20 == null)
                instance.CriticalConfirmationD20 = d20;
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
