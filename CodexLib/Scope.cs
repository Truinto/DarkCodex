using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace CodexLib
{
    /// <summary>
    /// Override logger and path for blueprint resolution. Helper calls that generate guids must be called in your scope, otherwise guids will be dumped outside your project.<br/>
    /// <br/>
    /// <code>using var scope = new Scope(Main.ModPath, Main.logger);</code>
    /// </summary>
    public class Scope : IDisposable
    {
        public string modPath;
        public UnityModManager.ModEntry.ModLogger logger;
        public Harmony harmony;
        public bool allowGuidGeneration;

        [Obsolete]
        public Scope(string modPath, UnityModManager.ModEntry.ModLogger logger)
        {
            this.modPath = modPath;
            this.logger = logger;
            Stack.Push(this);
        }

        [Obsolete]
        public Scope(string modPath, UnityModManager.ModEntry.ModLogger logger, Harmony harmony)
        {
            this.modPath = modPath;
            this.logger = logger;
            this.harmony = harmony;
            Stack.Push(this);
        }

        public Scope(string modPath, UnityModManager.ModEntry.ModLogger logger, Harmony harmony, bool allowGuidGeneration)
        {
            this.modPath = modPath;
            this.logger = logger;
            this.harmony = harmony;
            this.allowGuidGeneration = allowGuidGeneration;
            Stack.Push(this);
        }

        public void Dispose()
        {
            Stack.Pop();
        }

        static Scope()
        {
            Stack = new();
            new Scope("Mods", new UnityModManager.ModEntry.ModLogger("CodexLib"), new Harmony("CodexLib"), false);
        }

        public static Stack<Scope> Stack;

        public static string ModPath => Stack.Peek().modPath;
        public static UnityModManager.ModEntry.ModLogger Logger => Stack.Peek().logger;
        public static Harmony Harmony => Stack.Peek().harmony; // TODO: use this in Helper
        public static bool AllowGuidGeneration => Stack.Peek().allowGuidGeneration;
    }
}
