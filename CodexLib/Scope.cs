using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace CodexLib
{
    public class Scope : IDisposable
    {
        public string modPath;
        public UnityModManager.ModEntry.ModLogger logger;

        public Scope(string modPath, UnityModManager.ModEntry.ModLogger logger)
        {
            this.modPath = modPath;
            this.logger = logger;
            Stack.Push(this);
        }

        public void Dispose()
        {
            Stack.Pop();
        }

        public static Stack<Scope> Stack = new(new Scope[] { new Scope("", new UnityModManager.ModEntry.ModLogger("CodexLib")) });
        public static string ModPath => Stack.Last().modPath;
        public static UnityModManager.ModEntry.ModLogger Logger => Stack.Last().logger;
    }
}
