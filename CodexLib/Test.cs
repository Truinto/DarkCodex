using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityModManagerNet.UnityModManager;

namespace CodexLib
{
    public class Test
    {
        public static string Text = "Hello World! 3";

        public static string VersionP { get => "VersionP 3"; }
        public static string VersionV = "VersionV 3";
        
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Debug(ModEntry.ModLogger logger)
        {
            logger.Log("is in DEBUG");
        }

        [System.Diagnostics.Conditional("ASDIAREGIN")]
        public static void Debug2(ModEntry.ModLogger logger)
        {
            logger.Log("is in ASDIAREGIN");
        }
    }
}
