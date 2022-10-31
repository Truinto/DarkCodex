using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityModManagerNet;

#pragma warning disable 649

namespace Shared
{
    public static partial class Main
    {
        #region Partial Methods

        static partial void OnLoad(UnityModManager.ModEntry modEntry);
        static partial void OnBlueprintsLoaded();
        static partial void OnBlueprintsLoadedLast();
        static partial void OnMainMenu();

        #endregion

        public static Harmony harmony;
        public static bool Enabled;
        public static string ModPath;
        internal static PatchInfoCollection patchInfos;
        internal static readonly List<string> appliedPatches = new();
        private static UnityModManager.ModEntry.ModLogger logger;
        private static bool applyNullFinalizer;

        #region UnityModManager

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            /// <summary>Loads on game start.</summary>
            /// <param name="modEntry.Info">Contains all fields from the 'Info.json' file.</param>
            /// <param name="modEntry.Path">The path to the mod folder e.g. '\Steam\steamapps\common\YourGame\Mods\TestMod\'.</param>
            /// <param name="modEntry.Active">Active or inactive.</param>
            /// <param name="modEntry.Logger">Writes logs to the 'Log.txt' file.</param>
            /// <param name="modEntry.OnToggle">The presence of this function will let the mod manager know that the mod can be safely disabled during the game.</param>
            /// <param name="modEntry.OnGUI">Called to draw UI.</param>
            /// <param name="modEntry.OnSaveGUI">Called while saving.</param>
            /// <param name="modEntry.OnUpdate">Called by MonoBehaviour.Update.</param>
            /// <param name="modEntry.OnLateUpdate">Called by MonoBehaviour.LateUpdate.</param>
            /// <param name="modEntry.OnFixedUpdate">Called by MonoBehaviour.FixedUpdate.</param>
            /// <returns>Returns true, if no error occurred.</returns>
            ModPath = modEntry.Path;
            logger = modEntry.Logger;
            modEntry.OnUnload = Unload;

            try
            {
                EnsureCodexLib(modEntry.Path);
                harmony = new Harmony(modEntry.Info.Id);
                Patch(typeof(Patches));
                OnLoad(modEntry);
                Enabled = true;
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogException(ex);
                return false;
            }
        }

        public static bool Unload(UnityModManager.ModEntry modEntry)
        {
            harmony?.UnpatchAll(modEntry.Info.Id);
            Enabled = false;
            return true;
        }

        private static void EnsureCodexLib(string modPath)
        {
            if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith("CodexLib, ")))
            {
                PrintDebug("CodexLib already loaded.");
                return;
            }

            string path = null;
            Version version = null;
            modPath = new DirectoryInfo(modPath).Parent.FullName;
            PrintDebug("Looking for CodexLib in " + modPath);

            foreach (string cPath in Directory.GetFiles(modPath, "CodexLib.dll", SearchOption.AllDirectories))
            {
                try
                {
                    var cVersion = new Version(FileVersionInfo.GetVersionInfo(cPath).FileVersion);
                    PrintDebug($"Found: newer={version == null || cVersion > version} version={cVersion} @ {cPath}");
                    if (version == null || cVersion > version)
                    {
                        path = cPath;
                        version = cVersion;
                    }
                }
                catch (Exception) { }
            }

            if (path != null)
            {
                try
                {
                    Print("Loading CodexLib " + path);
                    AppDomain.CurrentDomain.Load(File.ReadAllBytes(path));
                }
                catch (Exception) { }
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                PrintDebug("Requested " + args.Name);

                if (ModPath != null && args.Name.StartsWith("CodexLib, "))
                {
                    string path = null;
                    Version version = null;

                    foreach (string cPath in Directory.GetFiles(Directory.GetParent(ModPath).FullName, "CodexLib.dll"))
                    {
                        var cVersion = new Version(FileVersionInfo.GetVersionInfo(cPath).FileVersion);
                        if (version == null || cVersion > version)
                        {
                            path = cPath;
                            version = cVersion;
                        }
                    }

                    if (path != null)
                    {
                        Print("AssemblyResolve " + path);
                        return Assembly.LoadFrom(path);
                    }
                }
            }
            catch (Exception ex) { logger?.LogException(ex); }
            return null;
        }

        [HarmonyPatch]
        private static class Patches
        {
            //[HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))] // used by some mods
            //[HarmonyPostfix]
            //[HarmonyPriority(Priority.First + 5)]
            private static void Postfix1()
            {
                try
                {
                }
                catch (Exception ex) { logger?.LogException(ex); }
            }

            [HarmonyPatch(typeof(StartGameLoader), nameof(StartGameLoader.LoadAllJson))]
            [HarmonyPriority(Priority.Normal)]
            [HarmonyPostfix]
            private static void Postfix2()
            {
                try
                {
                    OnBlueprintsLoaded();
                }
                catch (Exception ex) { logger?.LogException(ex); }
            }

            [HarmonyPatch(typeof(StartGameLoader), nameof(StartGameLoader.LoadAllJson))]
            [HarmonyPriority(Priority.Last - 5)]
            [HarmonyPostfix]
            private static void Postfix3()
            {
                try
                {
                    OnBlueprintsLoadedLast();
                    RunLastNow();

                    if (applyNullFinalizer)
                    {
                        var nullFinalizer = new HarmonyMethod(AccessTools.Method(typeof(Main), nameof(Main.NullFinalizer)));
                        foreach (var patch in harmony.GetPatchedMethods().ToArray())
                        {
                            if (Harmony.GetPatchInfo(patch).Finalizers.Count == 0)
                                harmony.Patch(patch, finalizer: nullFinalizer);
                        }
                    }
                }
                catch (Exception ex) { logger?.LogException(ex); }
            }

            [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Start))]
            [HarmonyPostfix]
            private static void Postfix4()
            {
                try
                {
                    OnMainMenu();
                }
                catch (Exception ex) { logger?.LogException(ex); }
            }
        }

        #endregion

        #region Helper

        private static List<(string, Action)> _patchLast = new();
        internal static void RunLast(string message, Action action)
        {
            if (_patchLast == null)
                action();
            else
                _patchLast.Add((message, action));
        }
        private static void RunLastNow()
        {
            if (_patchLast == null)
                return;

            if (_patchLast.Count > 0)
                PrintDebug("Running _patchLast");

            foreach (var (message, action) in _patchLast)
            {
                try
                {
                    PrintDebug(message);
                    action();
                }
                catch (Exception e) { PrintException(e); }
            }

            _patchLast = null;
        }

        internal static void Print(string msg) => logger?.Log(msg);

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void PrintDebug(string msg) => logger?.Log(msg);

        private static int _exceptionCount;
        internal static void PrintException(Exception ex)
        {
            if (_exceptionCount > 1000)
                return;

            logger?.LogException(ex);

            _exceptionCount++;
            if (_exceptionCount > 1000)
                logger?.Log("-- too many exceptions, future exceptions are suppressed");
        }

        internal static void PrintError(string msg) => logger?.Log("[Error/Exception] " + msg);

        internal static void Patch(Type patch)
        {
            Print("Patching " + patch.Name);
            var processor = harmony.CreateClassProcessor(patch);
            processor.Patch();
#if DEBUG
            var patches = GetConflictPatches(processor);
            if (patches != null && patches.Count > 0)
                PrintDebug("warning: potential conflict\n\t" + patches.Join(s => $"{s.owner}, {s.PatchMethod.Name}", "\n\t"));
#endif
        }

        /// <summary>
        /// Only works if all harmony attributes are on the class. Does not support bulk patches.
        /// </summary>
        internal static bool IsPatched(Type patch)
        {
            try
            {
                if (patch.GetCustomAttributes(false).FirstOrDefault(f => f is HarmonyPatch) is not HarmonyPatch attr)
                    throw new ArgumentException("Type must have HarmonyPatch attribute");

                MethodBase orignal = attr.info.GetOriginalMethod();
                if (orignal == null)
                    throw new Exception($"GetOriginalMethod returned null {attr.info}");
                var info = Harmony.GetPatchInfo(orignal);
                return info != null && (info.Prefixes.Any() || info.Postfixes.Any() || info.Transpilers.Any());
            }
            catch (Exception e) { PrintException(e); }
            return true;
        }

        internal static List<Patch> GetConflictPatches(PatchClassProcessor processor)
        {
            var list = new List<Patch>();

            try
            {
                foreach (var patch in processor.patchMethods)
                {
                    var orignal = patch.info.GetOriginalMethod();
                    if (orignal == null)
                        throw new Exception($"GetOriginalMethod returned null {patch.info}");

                    // if unpatched, no conflict
                    var info = Harmony.GetPatchInfo(orignal);
                    if (info == null)
                        continue;

                    // if foreign transpilers, warn conflict
                    list.AddRange(info.Transpilers.Where(a => a.owner != harmony.Id));

                    // if foreign prefixes with return type and identical priority as own prefix, warn conflict
                    var prio = info.Prefixes.Where(w => w.owner == harmony.Id).Select(s => s.priority);
                    list.AddRange(info.Prefixes.Where(w => w.owner != harmony.Id && w.PatchMethod.ReturnType != typeof(void) && prio.Contains(w.priority)));
                }
            }
            catch (Exception e) { PrintException(e); }

            return list;
        }

        internal static bool LoadSafe(Action action)
        {
            ProcessInfo(action.Method);
#if DEBUG
            var watch = Stopwatch.StartNew();
#endif
            string name = action.Method.DeclaringType.Name + "." + action.Method.Name;

            if (CheckSetting(name))
            {
                Print($"Skipped loading {name}");
                return false;
            }

            try
            {
                Print($"Loading {name}");
                action();
#if DEBUG
                watch.Stop();
                PrintDebug("Loaded in milliseconds: " + watch.ElapsedMilliseconds);
#endif
                return true;
            }
            catch (Exception e)
            {
#if DEBUG
                watch.Stop();
#endif
                PrintException(e);
                return false;
            }
        }

        internal static bool LoadSafe(Action<bool> action, bool flag)
        {
            ProcessInfo(action.Method);
#if DEBUG
            var watch = Stopwatch.StartNew();
#endif
            string name = action.Method.DeclaringType.Name + "." + action.Method.Name;

            if (CheckSetting(name))
            {
                Print($"Skipped loading {name}");
                return false;
            }

            try
            {
                Print($"Loading {name}:{flag}");
                action(flag);
#if DEBUG
                watch.Stop();
                PrintDebug("Loaded in milliseconds: " + watch.ElapsedMilliseconds);
#endif
                return true;
            }
            catch (Exception e)
            {
#if DEBUG
                watch.Stop();
#endif
                PrintException(e);
                return false;
            }
        }

        internal static void PatchUnique(Type patch)
        {
            /// needs to have field: static bool Patched
            ProcessInfo(patch);
            if (IsPatched(patch))
            {
                Print("Skipped patching because not unique " + patch.Name);
                return;
            }

            if (PatchSafe(patch))
                patch.GetField("Patched", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(null, true);
        }

        internal static bool PatchSafe(Type patch)
        {
            ProcessInfo(patch);
            if (CheckSetting("Patch." + patch.Name))
            {
                Print("Skipped patching " + patch.Name);
                return false;
            }

            try
            {
                Patch(patch);
                return true;
            }
            catch (Exception e)
            {
                PrintException(e);
                return false;
            }
        }

        internal static void SubscribeSafe(Type type)
        {
            ProcessInfo(type);
            if (CheckSetting("Patch." + type.Name))
            {
                Print("Skipped subscribing to " + type.Name);
                return;
            }

            try
            {
                Print("Subscribing to " + type.Name);
                EventBus.Subscribe(Activator.CreateInstance(type));
            }
            catch (Exception e)
            {
                PrintException(e);
            }
        }

        internal static MethodBase GetOriginalMethod(this HarmonyMethod attr)
        {
            try
            {
                switch (attr.methodType)
                {
                    case MethodType.Normal:
                        if (attr.methodName is null)
                            return null;
                        return AccessTools.DeclaredMethod(attr.declaringType, attr.methodName, attr.argumentTypes);

                    case MethodType.Getter:
                        if (attr.methodName is null)
                            return null;
                        return AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetGetMethod(true);

                    case MethodType.Setter:
                        if (attr.methodName is null)
                            return null;
                        return AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetSetMethod(true);

                    case MethodType.Constructor:
                        return AccessTools.DeclaredConstructor(attr.declaringType, attr.argumentTypes);

                    case MethodType.StaticConstructor:
                        return AccessTools.GetDeclaredConstructors(attr.declaringType)
                            .Where(c => c.IsStatic)
                            .FirstOrDefault();
                }
            }
            catch (AmbiguousMatchException ex)
            {
                throw new Exception("GetOriginalMethod", ex);
            }

            return null;
        }

        private static bool CheckSetting(string name)
        {
            bool skip = patchInfos?.IsDisenabled(name) ?? false;

            if (!skip && !appliedPatches.Contains(name))
                appliedPatches.Add(name);

            return skip;
        }

        private static void ProcessInfo(MemberInfo info)
        {
            if (info == null)
                return;

            if (Attribute.GetCustomAttribute(info, typeof(PatchInfoAttribute)) is not PatchInfoAttribute attr)
            {
                PrintDebug(info.Name + " has no PatchInfo");
                return;
            }

            patchInfos?.Add(attr, info);
        }

        internal static Exception NullFinalizer(Exception __exception)
        {
#if !DEBUG
            return null;
#else
            if (__exception == null)
                return null;
            try
            {
                PrintException(__exception);
            }
            catch (Exception) { }
            return null;
#endif
        }

        #endregion

        #region GUI

        private static void Checkbox(ref bool value, string label, Action<bool> action = null)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(value ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(20)))
            {
                value = !value;
                action?.Invoke(value);
            }
            GUILayout.Space(5);
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        #endregion
    }
}

