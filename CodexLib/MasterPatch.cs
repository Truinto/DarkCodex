using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodexLib.Patches;
using Kingmaker.Enums;

namespace CodexLib
{
    /// <summary>
    /// If your mod uses components, run this code. Some components will not work properly without these patches.<br/>
    /// <br/>
    /// <code>MasterPatch.Run();</code>
    /// </summary>
    public static class MasterPatch
    {
        private static bool IsRun;
        private static Harmony harmony;

        /// <summary>
        /// Call at least once, if you plan to use components.
        /// </summary>
        public static void Run()
        {
            if (IsRun)
                return;
            IsRun = true;
            harmony = new Harmony("CodexLib");

            PatchSafe(typeof(Patch_AbilityAtWill));
            PatchSafe(typeof(Patch_ActivatableActionBar));
            PatchSafe(typeof(Patch_AOEAttackRolls));
            PatchSafe(typeof(Patch_ConditionExemption));
            PatchSafe(typeof(Patch_ContextStatValue));
            PatchSafe(typeof(Patch_DebugReport));
            PatchSafe(typeof(Patch_FixAbilityTargets));
            PatchSafe(typeof(Patch_GetTargetProjectileFix));
            PatchSafe(typeof(Patch_RulebookEventBusPriority));
            PatchSafe(typeof(Patch_SpellSelectionParametrized));
            PatchSafe(typeof(Patch_WeaponCategory));
            PatchSafe(typeof(Patch_AbilityIsFullRound));
            PatchSafe(typeof(Patch_RuleSpendCharge));

            Helper.EnumCreateModifierDescriptor(Const.Intelligence, "Intelligence", "");
            Helper.EnumCreateModifierDescriptor(Const.Charisma, "Charisma", "");

            harmony = null;
        }

        private static void PatchSafe(Type patch)
        {
            try
            {
                Helper.Print("Patching " + patch.Name);
                harmony.CreateClassProcessor(patch).Patch();
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
            }
        }
    }
}
