using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace BlueprintLoader
{
    public class Main
    {
        internal static Harmony harmony;
        internal static UnityModManager.ModEntry.ModLogger logger;

        internal static bool Load(UnityModManager.ModEntry modEntry)
        {
            harmony = new Harmony(modEntry.Info.Id);
            logger = modEntry.Logger;

            PatchSafe(typeof(BlueprintLoader));

            return true;
        }

        internal static void PatchSafe(Type type)
        {
            try
            {
                harmony.CreateClassProcessor(type).Patch();
            }
            catch (Exception e)
            {
                logger.LogException(e);
            }
        }
    }
}
