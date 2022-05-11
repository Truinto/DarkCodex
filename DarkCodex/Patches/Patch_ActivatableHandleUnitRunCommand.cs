using HarmonyLib;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
using Shared;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Activatable HandleUnitRunCommand", "fixes move actions disabling the activatable (since we have 2 of them)", false)]
    [HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.HandleUnitRunCommand))]
    public static class Patch_ActivatableHandleUnitRunCommand
    {
        public static bool Prefix(UnitCommand command, ActivatableAbility __instance)
        {
            if (command.Type == UnitCommand.CommandType.Move) // skip this logic, if the trigger came from a move action
                return false;

            return true;
        }
    }
}
