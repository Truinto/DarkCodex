using HarmonyLib;
using Kingmaker;
using Kingmaker.Achievements;
using Kingmaker.Modding;
using Kingmaker.UnitLogic.ActivatableAbilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    /// <summary>
    /// Events for combat start and end.
    /// </summary>
    [HarmonyPatch(typeof(GameHistoryLog), nameof(GameHistoryLog.HandlePartyCombatStateChanged))]
    public class Patch_CombatState
    {
        public delegate void Handler();
        public static event Handler Start;
        public static event Handler End;

        public static void Postfix(bool inCombat)
        {
            if (inCombat)
                Start?.Invoke();
            else
                End?.Invoke();
        }
    }

    /// <summary>
    /// Is manually patched. It crashes otherwise. Clears the 'has used mods before' flag and also pretends that no mods are active.
    /// </summary>
    [ManualPatch(typeof(OwlcatModificationsManager), nameof(OwlcatModificationsManager.IsAnyModActive), MethodType.Getter)]
    public class Patch_AllowAchievements
    {
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

    [HarmonyPatch(typeof(AchievementEntity), nameof(AchievementEntity.IsDisabled), MethodType.Getter)]
    public static class Patch_AllowAchievements2
    {
        public static bool Prepare()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        public static void Postfix(bool __result)
        {
            bool modactive = OwlcatModificationsManager.Instance.IsAnyModActive;
            bool usermarked = Game.Instance.Player.ModsUser;

            Helper.PrintDebug($"Achievements: dis={__result} modactive={modactive} usermarked={usermarked}");
        }
    }

    // Note: CallOfTheWild also patches this, but in a different manner. Not sure if there is potential in error. So far I haven't found any bugs in playtesting.
    // Also, the enum will change, if the order of calls are changed, e.g. new mods are added. I think that's not a problem. I can't think of a reason why it would be saved in the save file. Needs further testing.
    //[HarmonyLib.HarmonyPatch(typeof(EnumUtils), nameof(EnumUtils.GetMaxValue))] since this is a generic method, we need to patch this manually, see Main.Load
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
}
