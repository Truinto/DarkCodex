using Kingmaker.Blueprints;
using HarmonyLib;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: True Gather Power Level", "Normal: The level of gathering power is determined by the mode (none, low, medium, high) selected. If the mode is lower than the already accumulated gather level, then levels are lost.\nPatched: The level of gathering is true to the accumulated level or the selected mode, whatever is higher.", false)]
    [HarmonyPatch(typeof(KineticistController), nameof(KineticistController.TryApplyGatherPower))]
    public class Patch_TrueGatherPowerLevel
    {
        public static BlueprintBuff buff1 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("e6b8b31e1f8c524458dc62e8a763cfb1");
        public static BlueprintBuff buff2 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("3a2bfdc8bf74c5c4aafb97591f6e4282");
        public static BlueprintBuff buff3 = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("82eb0c274eddd8849bb89a8e6dbc65f8");

        public static bool Prefix(UnitPartKineticist kineticist, BlueprintAbility abilityBlueprint, ref KineticistAbilityBurnCost cost)
        {
            if (kineticist == null || abilityBlueprint.GetComponent<AbilityKineticist>() == null || kineticist.GatherPowerAbility == null)
                return false;

            int buffRank = kineticist.TargetGatherPowerRank; // get the target power rank

            // check if stronger buff exists and if so apply it instead
            if (buffRank < 1 && kineticist.Owner.Buffs.GetBuff(buff1) != null)
                buffRank = 1;
            else if (buffRank < 2 && kineticist.Owner.Buffs.GetBuff(buff2) != null)
                buffRank = 2;
            else if (buffRank < 3 && kineticist.Owner.Buffs.GetBuff(buff3) != null)
                buffRank = 3;

            int value = KineticistUtils.CalculateGatherPowerBonus(kineticist.GatherPowerBaseValue, buffRank); // add increase from Supercharge

            cost.IncreaseGatherPower(value); // apply value

            return false;
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
