using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.BinaryFormat;
using Kingmaker.Blueprints.JsonSystem.Converters;
using Kingmaker.BundlesLoading;
using Kingmaker.Modding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BlueprintLoader
{
    [HarmonyPatch]
    public class BlueprintLoader
    {
        public static readonly BlueprintLoader Instance = new();

        /// <summary>Do not modify Dictionary or any of its values. Use 'AddBlueprints' to add new blueprints.</summary>
        public readonly Dictionary<Type, IList> ListByType;
        /// <summary>Loading progress between 0.0 and 1.0</summary>
        public float Progress;
        /// <summary>Whenever the loading process was started. Stays true afterwards.</summary>
        public bool WasStarted { get; private set; }
        /// <summary>Whenever the loading process is finished.</summary>
        public bool IsFinished { get; private set; }
        private Thread LoadingThread;
        private object Lock = new();
        private BlueprintGuid[] Guids;

        public BlueprintLoader()
        {
            ListByType = new();

            //foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var type in Assembly.Load("Assembly-CSharp").GetTypes())
                if (typeof(SimpleBlueprint).IsAssignableFrom(type))
                    ListByType[type] = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
        }

        /// <summary>Starts the loading process, if possible. Does not block.</summary>
        public void Start()
        {
            if (this.WasStarted || ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints.Count <= 0)
                return;

            this.WasStarted = true;
            lock (ResourcesLibrary.BlueprintsCache.m_Lock)
                this.Guids = ResourcesLibrary.BlueprintsCache.m_LoadedBlueprints.Keys.ToArray();
            LoadingThread = new Thread(Load);
            LoadingThread.Start();
        }

        private void Load()
        {
            var __instance = ResourcesLibrary.BlueprintsCache;
            int count = 0;
            if (this.Guids == null)
                lock (__instance.m_Lock)
                    this.Guids = __instance.m_LoadedBlueprints.Keys.ToArray();
            int total = this.Guids.Length;
            var watch = Stopwatch.StartNew();

            foreach (var guid in this.Guids)
            {
                var bp = __instance.Load(guid);
                if (bp != null)
                {
                    lock (this.Lock)
                        ListByType[bp.GetType()].Add(bp);
                    this.Progress = (float)++count / total;
                }
            }

            this.Progress = 1f;
            this.IsFinished = true;
            this.Guids = null;
            watch.Stop();
            Main.Print($"loaded {count} of {total} blueprints in {watch.ElapsedMilliseconds} milliseconds");
#if DEBUG
            foreach (var list in ListByType.Values)
                Main.Print($"List of '{list.GetType()}' holds {list.Count} blueprints");
#endif
        }

        /// <summary>Gets a new list of a specific blueprint type.</summary>
        /// <typeparam name="T">Use SimpleBlueprint to return all blueprints.</typeparam>
        /// <param name="includeDerived">Whenever the list should contain derived types like BlueprintSelection in List≺BlueprintFeature≻</param>
        public List<T> Get<T>(bool includeDerived = true) where T : SimpleBlueprint
        {
            if (!includeDerived)
                return ((List<T>)ListByType[typeof(T)]).ToList();

            var result = new List<T>();
            foreach (var (type, list) in ListByType)
            {
                if (typeof(T).IsAssignableFrom(type))
                {
                    result.Capacity = result.Count + list.Count;
                    foreach (var bp in list)
                        result.Add((T)bp);
                }
            }

            return result;
        }

        /// <summary>Gets a new list of blueprints which match a condition. Always includes derived types.</summary>
        /// <param name="predicate">Condition to match the blueprint.</param>
        public List<T> Get<T>(Func<T, bool> predicate) where T : SimpleBlueprint
        {
            var result = new List<T>();

            foreach (var (type, list) in ListByType)
                if (typeof(T).IsAssignableFrom(type))
                    foreach (var bp in list)
                        if (predicate((T)bp))
                            result.Add((T)bp);

            return result;
        }

        /// <summary>Gets a new list of blueprints which match a condition.</summary>
        /// <param name="predicate">Condition to match the blueprint.</param>
        public List<BlueprintScriptableObject> Get(Func<BlueprintScriptableObject, bool> predicate)
        {
            var result = new List<BlueprintScriptableObject>();

            foreach (var list in ListByType.Values)
                foreach (var bp in list)
                    if (bp is BlueprintScriptableObject bps)
                        if (predicate(bps))
                            result.Add(bps);

            return result;
        }

        /// <summary>Add a blueprint to ResourcesLibrary.BlueprintsCache</summary>
        public void AddBlueprint(SimpleBlueprint bp)
        {
            if (bp == null || bp.AssetGuid == BlueprintGuid.Empty)
                throw new ArgumentException("All blueprint must have a Guid.");

            ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(bp.AssetGuid, bp);
        }

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.AddCachedBlueprint))]
        [HarmonyPostfix]
        private static void Postfix1(SimpleBlueprint bp)
        {
            if (bp == null)
                return;

            var type = bp.GetType();
            if (Instance.ListByType.TryGetValue(type, out var list))
            {
                lock (Instance.Lock)
                    list.Add(bp);
            }
        }

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPriority(Priority.First)]
        [HarmonyPostfix]
        private static void Postfix2()
        {
#if true
            Instance.Start();   // multithreaded
#else
            Instance.Load();   // on main thread
#endif
        }
    }
}
