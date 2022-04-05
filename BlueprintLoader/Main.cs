using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace BlueprintLoader
{
    public class Main
    {
        internal static Harmony harmony;
        internal static UnityModManager.ModEntry.ModLogger logger;
        public static readonly string version;

        static Main()
        {
            version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
        }

        public static bool Load()
        {
            logger = new("BlueprintLoader." + version);
            harmony = new Harmony("BlueprintLoader." + version);

            PatchSafe(typeof(BlueprintLoader));
            Print("Patched");
            return true;
        }

        internal static bool ExLoad(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            harmony = new Harmony(modEntry.Info.Id);

            PatchSafe(typeof(BlueprintLoader));
            Print("Patched");
            return true;
        }

        internal static void Print(string text)
        {
            logger.Log(text);
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
