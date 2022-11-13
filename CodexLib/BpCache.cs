﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items;
using System.IO;
using Kingmaker.Blueprints.JsonSystem;
using System.Collections.ObjectModel;

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
        public static Type[] DefaultTypes = new Type[]
        {
            typeof(BlueprintAbility),
            typeof(BlueprintActivatableAbility),
            typeof(BlueprintBuff),
            typeof(BlueprintFeature),
            typeof(BlueprintItem),
            typeof(BlueprintItemEnchantment),
        };

        private static Dictionary<Type, IList> _blueprints = new();

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
                return new List<object>();
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
                    return new List<object>();
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
                Dictionary<Type, IList> dic = new();
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

                using (var writer = new BinaryWriter(new FileStream(path, FileMode.CreateNew, FileAccess.Write), Encoding.ASCII))
                {
                    foreach (var (type, list) in dic)
                    {
                        // print length of type; print type
                        string name = type.GetFullName();
                        writer.Write(name.Length);
                        writer.Write(name);

                        // print number of guids; print guid
                        writer.Write(list.Count);
                        foreach (object obj in list)
                            writer.Write(((SimpleBlueprint)obj).AssetGuid.ToByteArray());
                    }
                    writer.Close();
                }
            }
            catch (Exception e) { Helper.PrintException(e); }
        }

        private static void LoadResources()
        {
            if (IsLoaded)
                return;
            try
            {
                var bpcache = ResourcesLibrary.BlueprintsCache;
                var guid = new byte[16];
                var buffer = new byte[1024];
                int length;

                //using (var reader = new FileStream("Blueprints.bin", FileMode.Open, FileAccess.Read))
                using (var reader = Assembly.GetExecutingAssembly().GetManifestResourceStream("CodexLib.Resources.Blueprints.bin"))
                {
                    // read length of type
                    reader.Read(buffer, 0, 4);
                    length = BitConverter.ToInt32(buffer, 0);
                    if (length > 1024)
                        throw new FormatException("Type length too large");

                    // read type
                    reader.Read(buffer, 0, length);
                    var type = Type.GetType(Encoding.ASCII.GetString(buffer, 0, length));
                    if (type == null)
                        throw new FormatException("Type couldn't be parsed");

                    // get list
                    if (_blueprints.Ensure(type, out var list, typeof(List<>).MakeGenericType(type)))
                        Helper.Print($"_blueprint missing type: {type.Name}");

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
#if DEBUG
                        if (list.Contains(bp))
                            Helper.PrintDebug("duplicate blueprint: " + bp.AssetGuid);
                        else
#endif
                            list.Add(bp);
                    }
                }
            }
            catch (Exception e) { Helper.PrintException(e); }
            IsLoaded = true;
        }

        /// <summary>
        /// Manual adding blueprint to cache. Should not be necessary as 'BlueprintsCache.AddCachedBlueprint' also adds this.
        /// </summary>
        public static void AddResource(SimpleBlueprint bp)
        {
            if (bp == null)
                return;

            var type = bp.GetType();
            foreach (var entry in _blueprints)
                if (entry.Key.IsAssignableFrom(type) && !entry.Value.Contains(bp))
                    entry.Value.Add(bp);
        }

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        private static void OnInit()
        {
            LoadResources();
        }

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.AddCachedBlueprint))]
        [HarmonyPostfix]
        private static void OnAddCachedBlueprint(SimpleBlueprint bp)
        {
            try
            {
                AddResource(bp);
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
            }
        }
    }
}