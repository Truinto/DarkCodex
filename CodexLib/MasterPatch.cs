using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodexLib.Patches;

namespace CodexLib
{
    /// <summary>
    /// Patch Control
    /// </summary>
    public static class MasterPatch
    {
        private static bool IsRun;
        private static Harmony harmony;

        /// <summary>
        /// Call at least once, if you plan to use any components.
        /// </summary>
        public static void Run()
        {
            if (IsRun)
                return;
            IsRun = true;
            harmony = new Harmony("CodexLib");

            PatchSafe(typeof(Patch_ContextStatValue));
            PatchSafe(typeof(Patch_GetTargetProjectileFix));

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
