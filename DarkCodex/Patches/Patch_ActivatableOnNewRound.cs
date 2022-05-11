using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.TurnBasedMode.Controllers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurnBased.Controllers;
using Shared;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Activatable OnNewRound", "uses up move action when triggered; deactivates activatable if no action left", false)]
    [HarmonyPatch]
    public static class Patch_ActivatableOnNewRound
    {
        [HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.OnNewRound))]
        [HarmonyPostfix]
        public static void Postfix(ActivatableAbility __instance)
        {
            var cd = __instance.Owner.Unit.CombatState.Cooldown;
            if (cd.StandardAction > 0f && cd.MoveAction > 3f || cd.MoveAction > 6f)
                __instance.IsOn = false;
        }

        [HarmonyPatch(typeof(TurnController), nameof(TurnController.CalculatePredictionForActivatableAbilities))]
        [HarmonyPostfix]
        public static void RecalcPrediction(TurnController __instance)
        {
            foreach (var activatable in __instance.SelectedUnit.ActivatableAbilities.RawFacts)
            {
                if (!activatable.IsOn || !activatable.IsStarted)
                    continue;

                var command = activatable.Blueprint.GetComponent<ActivatableAbilityUnitCommand>();
                if (command == null)
                    continue;

                var states = __instance.GetActionsStates(__instance.SelectedUnit);
                var unit = activatable.Owner.Unit;
                switch (command.Type)
                {
                    case UnitCommand.CommandType.Standard:
                        if (states.Standard.PredictedAbility == null && unit.UsedStandardAction())
                            states.Standard.SetPrediction(CombatAction.UsageType.UseAbility, CombatAction.ActivityType.Ability, CombatAction.ActivityState.Used, activatable);
                        break;
                    case UnitCommand.CommandType.Move:
                        if (states.Move.PredictedAbility == null && unit.UsedOneMoveAction())
                            states.Move.SetPrediction(CombatAction.UsageType.UseAbility, CombatAction.ActivityType.Ability, CombatAction.ActivityState.Used, activatable);
                        break;
                    case UnitCommand.CommandType.Swift:
                        if (states.Swift.PredictedAbility == null && !unit.HasSwiftAction())
                            states.Swift.SetPrediction(CombatAction.UsageType.UseAbility, CombatAction.ActivityType.Ability, CombatAction.ActivityState.Used, activatable);
                        break;
                }
            }
        }
    }
}
