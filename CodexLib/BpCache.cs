using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace CodexLib
{
    /// <summary>
    /// Fast blueprint cache for specified types. Preloads from base game guids. Also watches for manual additions from mods.
    /// </summary>
    [HarmonyPatch]
    public static class BpCache
    {
        /// <summary>
        /// Default types to export and cache.
        /// </summary>
        public static readonly Type[] DefaultTypes =
        [
            typeof(BlueprintAbility),
            typeof(BlueprintActivatableAbility),
            typeof(BlueprintBuff),
            typeof(BlueprintFeature),
            typeof(BlueprintItem),
            typeof(BlueprintItemEnchantment),
        ];

        private static readonly Dictionary<Type, IList> _blueprints = [];

        static BpCache()
        {
            foreach (var type in DefaultTypes)            
                _blueprints[type] = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));            
        }

        /// <summary>
        /// True if LoadResources was called before, otherwise false.
        /// </summary>
        public static bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets read-only collection of given type or empty collection.
        /// </summary>
        /// <param name="type">Type of blueprint</param>
        public static IList Get(Type type)
        {
            if (type == null)
                return Array.Empty<object>();
            if (_blueprints.TryGetValue(type, out var list))
                return (IList)Activator.CreateInstance(typeof(ReadOnlyCollection<>).MakeGenericType(type), list);
            return Get(type.BaseType);
        }

        /// <summary>
        /// Gets read-only collection of given type or empty collection.
        /// </summary>
        /// <typeparam name="T">Type of blueprint</typeparam>
        public static AmbigiousCollection<T> Get<T>() where T : SimpleBlueprint
        {
            return new AmbigiousCollection<T>(get(typeof(T)));

            IList get(Type type)
            {
                if (type == null)
                    return Array.Empty<object>();
                if (_blueprints.TryGetValue(type, out var list))
                    return list;
                return get(type.BaseType);
            }
        }

        /// <summary>
        /// Filters base game blueprints and exports guid into file. Can be embedded into 'CodexLib.Resources.Blueprints.bin' for loading.
        /// </summary>
        /// <param name="path">File path to save to</param>
        /// <param name="types">Types of blueprints to filter for</param>
        public static void ExportResources(string path = "Blueprints.bin", Type[] types = null)
        {
            try
            {
                var watch = Stopwatch.StartNew();
                Dictionary<Type, IList> dic = [];
                foreach (var type in types ?? DefaultTypes)
                    dic.Add(type, (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type)));

                var bpcache = ResourcesLibrary.BlueprintsCache;
                foreach (var (guid, entry) in bpcache.m_LoadedBlueprints.ToArray())
                {
                    if (entry.Offset != 0)
                    {
                        var bp = entry.Blueprint ?? bpcache.Load(guid);
                        if (bp == null)
                            continue;

                        //var key = dic.Keys.FirstOrDefault(f => f.IsAssignableFrom(bp.GetType()));
                        //if (key == null)
                        //    continue;
                        //dic[key].Add(bp);

                        var type = bp.GetType();
                        foreach (var entry2 in dic)
                        {
                            if (entry2.Key.IsAssignableFrom(type))
                                entry2.Value.Add(bp);
                        }
                    }
                }

                var buffer = new byte[16];
                using var writer = new FileStream(path, FileMode.Create, FileAccess.Write);
                foreach (var (type, list) in dic)
                {
                    // print length of type; print type
                    string name = type.GetFullName();
                    writer.Write(name.Length, buffer);
                    writer.Write(name, buffer);

                    // print number of guids; print guid
                    writer.Write(list.Count, buffer);
                    foreach (object obj in list)
                        writer.Write(((SimpleBlueprint)obj).AssetGuid.ToByteArray(), 0, 16);
                }
                writer.Dispose();

                watch.Stop();
                Helper.Print($"Finished export in milliseconds: {watch.ElapsedMilliseconds}");
            }
            catch (Exception e) { Helper.PrintException(e); }
        }

        /// <summary>
        /// Add a new type to be recorded. If a mod adds a blueprint of that type, it's added to the cache.
        /// </summary>
        public static void RegisterType(Type type)
        {
            _blueprints.Ensure(type, out _, typeof(List<>).MakeGenericType(type));
        }

        /// <summary>
        /// Load blueprints from stream. Format must be as ExportResources().<br/>
        /// Tries to load embedded CodexLib.Resources.Blueprints.bin, if null.
        /// </summary>
        public static void LoadResources(Stream reader = null, bool checkDupe = true)
        {
            try
            {
                var watch = Stopwatch.StartNew();
                var bpcache = ResourcesLibrary.BlueprintsCache;
                var guid = new byte[16];
                var buffer = new byte[1024];
                int length;

                //using (var reader = new FileStream("Blueprints.bin", FileMode.Open, FileAccess.Read))
                reader ??= Assembly.GetExecutingAssembly().GetManifestResourceStream("CodexLib.Resources.Blueprints.bin");

                while(true)
                {
                    // read length of type, exit if end of stream
                    length = reader.Read(buffer, 0, 4);
                    if (length == 0)
                        break;
                    length = BitConverter.ToInt32(buffer, 0);
                    if (length > 1024)
                        throw new FormatException($"Type length too large: {length}");

                    // read type
                    reader.Read(buffer, 0, length);
                    string typeName = Encoding.ASCII.GetString(buffer, 0, length);
                    var type = Type.GetType(typeName) ?? throw new FormatException($"Type couldn't be parsed '{typeName}', length={length}");

                    // get list
                    if (_blueprints.Ensure(type, out var list, typeof(List<>).MakeGenericType(type)))
                        Helper.PrintDebug($"Adding type list: {type.Name}");

                    // read number of guids
                    reader.Read(buffer, 0, 4);
                    length = BitConverter.ToInt32(buffer, 0);

                    // read guids and load them
                    for (int i = 0; i < length; i++)
                    {
                        if (reader.Read(guid, 0, 16) != 16)
                            throw new EndOfStreamException();

                        var bp = bpcache.Load(new BlueprintGuid(guid));
                        if (bp == null)
                        {
                            Helper.PrintDebug($"failed to load blueprint: {new Guid(guid):N}");
                            continue;
                        }

                        if (!type.IsAssignableFrom(bp.GetType()))
                        {
                            Helper.PrintDebug($"{bp.AssetGuid} type={bp.GetType().Name} is not {type.Name}");
                            continue;
                        }

                        if (list.Contains(bp))
                            Helper.PrintDebug("duplicate blueprint: " + bp.AssetGuid);
                        else
                            list.Add(bp);
                    }
                }
                reader.Dispose();

                watch.Stop();
                Helper.Print($"Loaded blueprints in {watch.ElapsedMilliseconds}ms {_blueprints.Join(f => $"{f.Key.Name}:{f.Value.Count}")}");
            }
            catch (Exception e) { Helper.PrintException(e); }
            IsLoaded = true;
        }

        /// <summary>
        /// Manual adding blueprint to cache. Should not be necessary as 'BlueprintsCache.AddCachedBlueprint' also adds this.
        /// </summary>
        public static void AddResource(SimpleBlueprint bp)
        {
            try
            {
                if (bp == null)
                    return;

                var type = bp.GetType();
                foreach (var entry in _blueprints)
                {
                    if (entry.Key.IsAssignableFrom(type) && !entry.Value.Contains(bp))
                        entry.Value.Add(bp);
                }
            }
            catch (Exception e) { Helper.PrintException(e); }
        }

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        private static void OnInit()
        {
            if (IsLoaded)
            {
                Helper.PrintError("BlueprintsCache already initiated!");
                return;
            }
            LoadResources(null, false);
        }

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.AddCachedBlueprint))]
        [HarmonyPostfix]
        private static void OnAddCachedBlueprint(SimpleBlueprint bp)
        {
            AddResource(bp);
        }
    }
}
