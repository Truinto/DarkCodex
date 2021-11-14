using HarmonyLib;
using Kingmaker;
using Kingmaker.Achievements;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Modding;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [PatchInfo(Severity.Harmony, "Patch: Allow Achievements", "clears the 'has used mods before' flag and also pretends that no mods are active", false)]
    [HarmonyPatch(typeof(OwlcatModificationsManager), nameof(OwlcatModificationsManager.IsAnyModActive), MethodType.Getter)]
    public class Patch_AllowAchievements    // note: must be manually patched; it crashes with PatchAll
    {
        public static bool Patched;
        public static bool Prefix(ref bool __result)
        {
            if (!Settings.StateManager.State.allowAchievements)
                return true;

            if (Game.Instance?.Player != null)
                Game.Instance.Player.ModsUser = false;
            __result = false;
            return false;
        }
    }

    #region Activatables

    // Note: CallOfTheWild also patches this, but in a different manner. Not sure if there is potential in error. So far I haven't found any bugs in playtesting.
    // Also, the enum will change, if the order of calls are changed, e.g. new mods are added. I think that's not a problem. I can't think of a reason why it would be saved in the save file. Needs further testing.
    //[HarmonyPatch(typeof(EnumUtils), nameof(EnumUtils.GetMaxValue))] since this is a generic method, we need to patch this manually, see Main.Load
    public static class Patch_ActivatableAbilityGroup
    {
        public static int ExtraGroups = 0;
        public static bool GameAlreadyRunning = false;

        ///<summary>Calls this to register a new group. Returns your new enum.</summary>
        public static ActivatableAbilityGroup GetNewGroup()
        {
            if (GameAlreadyRunning)
                return 0;

            ExtraGroups++;
            Helper.PrintDebug("GetNewGroup new: " + (Enum.GetValues(typeof(ActivatableAbilityGroup)).Cast<int>().Max() + ExtraGroups).ToString());
            return (ActivatableAbilityGroup)(Enum.GetValues(typeof(ActivatableAbilityGroup)).Cast<int>().Max() + ExtraGroups);
        }

        public static void Postfix(ref int __result)
        {
            __result += ExtraGroups;
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Activatable OnNewRound", "uses up move action when triggered; deactivates activatable if no action left", false)]
    [HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.OnNewRound))]
    public static class Patch_ActivatableOnNewRound
    {
        public static void Postfix(ActivatableAbility __instance)
        {
            var cd = __instance.Owner.Unit.CombatState.Cooldown;
            if (cd.StandardAction > 0f && cd.MoveAction > 3f || cd.MoveAction > 6f)
                __instance.IsOn = false;
        }
    }

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

    [PatchInfo(Severity.Harmony, "Patch: Activatable OnTurnOn", "fixes activatable not being allowed to be active when they have the same action (like 2 move actions)", false)]
    [HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.OnDidTurnOn))]
    public static class Patch_ActivatableOnTurnOn
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            List<CodeInstruction> list = instr.ToList();
            MethodInfo original = AccessTools.Method(typeof(EntityFact), nameof(EntityFact.GetComponent), null, typeof(ActivatableAbilityUnitCommand).ObjToArray());
            MethodInfo replacement = AccessTools.Method(typeof(Patch_ActivatableOnTurnOn), nameof(NullReplacement));

            for (int i = 0; i < list.Count; i++)
            {
                var mi = list[i].operand as MethodInfo;
                if (mi != null && mi == original)
                {
                    Helper.PrintDebug("ActivatableAbility_OnTurnOnPatch at " + i);
                    list[i].operand = replacement;
                }
            }

            return list;
        }

        public static object NullReplacement(object something)
        {
            return null;
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Activatable ApplyValidation", "removes validation", false)]
    [HarmonyPatch(typeof(ActivatableAbilityUnitCommand), nameof(ActivatableAbilityUnitCommand.ApplyValidation))]
    public static class Patch_ActivatableApplyValidation
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Activatable TryStart", "fixes activatable not starting the second time, while being outside of combat", false)]
    [HarmonyPatch(typeof(ActivatableAbility), nameof(ActivatableAbility.TryStart))]
    public static class Patch_ActivatableTryStart
    {
        public static void Prefix(ActivatableAbility __instance)
        {
            if (!__instance.Owner.Unit.IsInCombat)
            {
                __instance.Owner.Unit.CombatState.Cooldown.SwiftAction = 0f;
                __instance.Owner.Unit.CombatState.Cooldown.MoveAction = 0f;
            }

        }
    }

    [PatchInfo(Severity.Harmony, "Patch: Activatable ActionBar", "adds logic for automatic-only activatable", false)]
    [HarmonyPatch(typeof(MechanicActionBarSlotActivableAbility), nameof(MechanicActionBarSlotActivableAbility.OnClick))]
    public static class Patch_ActivatableActionBar
    {
        public static readonly int NoManualOn = 788704819;
        public static readonly int NoManualOff = 788704820;
        public static readonly int NoManualAny = 788704821;

        public static bool Prefix(MechanicActionBarSlotActivableAbility __instance)
        {
            if (!__instance.ActivatableAbility.IsOn && __instance.ActivatableAbility.Blueprint.WeightInGroup == NoManualOn)
            {
                return false;
            }
            if (__instance.ActivatableAbility.IsOn && __instance.ActivatableAbility.Blueprint.WeightInGroup == NoManualOff)
            {
                return false;
            }
            if (__instance.ActivatableAbility.Blueprint.WeightInGroup == NoManualAny)
            {
                return false;
            }
            return true;
        }
    }

    #endregion

    /// <summary>If a component causes an exception, it will try to resolve the asset name.
    /// If that name is null or wrong formated, it will crash the report and hide any meaningful log entries.
    /// This patch fills the missing data with nonsense to prevent that bug.</summary>
    [PatchInfo(Severity.Harmony, "Patch: Debug Report", "fixes error log crashes due to unnamed components", false)]
    [HarmonyPatch(typeof(Element), nameof(Element.AssetGuidShort), MethodType.Getter)]
    public class Patch_DebugReport
    {
        public static void Prefix(Element __instance)
        {
            if (__instance.name == null)
                __instance.name = "$$empty";
        }
    }
}
