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
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                harmony = new Harmony(modEntry.Info.Id);
                Patch(typeof(StartGameLoader_LoadAllJson));
                Patch(typeof(MainMenu_Start));
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

        [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Start))]
        private static class MainMenu_Start
        {
            private static void Postfix()
            {
                try
                {
                    OnMainMenu();
                }
                catch (Exception ex) { logger?.LogException(ex); }
            }
        }

        [HarmonyPatch(typeof(StartGameLoader), "LoadAllJson")]
        private static class StartGameLoader_LoadAllJson
        {
            private static void Postfix()
            {
                try
                {
                    OnBlueprintsLoaded();

                    if (applyNullFinalizer)
                    {
                        var nullFinalizer = new HarmonyMethod(typeof(Main).GetMethod(nameof(Main.NullFinalizer)));
                        foreach (var patch in harmony.GetPatchedMethods().ToArray())
                        {
                            if (Harmony.GetPatchInfo(patch).Finalizers.Count == 0)
                                harmony.Patch(patch, finalizer: nullFinalizer);
                        }
                    }
                }
                catch (Exception ex) { logger?.LogException(ex); }
            }
        }

        #endregion

        #region Helper

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

        public static void Patch(Type patch)
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
        public static bool IsPatched(Type patch)
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

        public static List<Patch> GetConflictPatches(PatchClassProcessor processor)
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

        public static bool LoadSafe(Action action)
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

        public static bool LoadSafe(Action<bool> action, bool flag)
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

        public static void PatchUnique(Type patch)
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

        public static bool PatchSafe(Type patch)
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

        public static void SubscribeSafe(Type type)
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

        public static MethodBase GetOriginalMethod(this HarmonyMethod attr)
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

        public static Exception NullFinalizer(Exception __exception)
        {
            if (__exception == null)
                return null;
#if DEBUG
            try
            {
                PrintException(__exception);
            }
            catch (Exception) { }
#endif
            return null;
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

