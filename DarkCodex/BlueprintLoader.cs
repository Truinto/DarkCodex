using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Modding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    [HarmonyPatch]
    public class BlueprintLoader
    {
        public static readonly BlueprintLoader Instance = new();

        public Dictionary<Type, IList> ListByType;

        public BlueprintLoader()
        {
            ListByType = new();

            //foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var type in Assembly.Load("Assembly-CSharp").GetTypes())
                if (typeof(SimpleBlueprint).IsAssignableFrom(type))
                    ListByType[type] = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
        }

        public void Load()
        {
            var __instance = ResourcesLibrary.BlueprintsCache;
            var deserializer = __instance.m_PackSerializer;
            var reader = deserializer.m_Primitive.m_Reader;
            var stream = reader.BaseStream;

            lock (__instance.m_Lock)
            {
                foreach (var (guid, bpCache) in __instance.m_LoadedBlueprints.ToArray())
                {
                    // add if already loaded
                    if (bpCache.Blueprint != null)
                    {
                        ListByType[bpCache.Blueprint.GetType()].Add(bpCache.Blueprint);
                        continue;
                    }

                    // try load
                    uint offset = bpCache.Offset;
                    stream.Seek(offset, SeekOrigin.Begin);
                    SimpleBlueprint sbp = null;
                    deserializer.Blueprint(ref sbp);
                    if (sbp == null)
                        continue;

                    OwlcatModificationsManager.Instance.OnResourceLoaded(sbp, guid.ToString(), out object obj);
                    var blueprint = (obj as SimpleBlueprint) ?? sbp;
                    if (blueprint == null)
                        continue;

                    __instance.m_LoadedBlueprints[guid] = new() { Offset = offset, Blueprint = blueprint };

                    // add if could load
                    ListByType[blueprint.GetType()].Add(blueprint);
                }
            }
        }

        public List<T> Get<T>(bool includeDerived = true) where T : SimpleBlueprint
        {
            var result = new List<T>();

            if (!includeDerived)
                return ListByType.FirstOrDefault(f => f.Key == typeof(T)).Value as List<T>;

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

        public List<SimpleBlueprint> Get(Func<SimpleBlueprint, bool> predicate)
        {
            var result = new List<SimpleBlueprint>();

            foreach (var pair in ListByType)
                foreach (var bp in pair.Value)
                    if (predicate((SimpleBlueprint)bp))
                        result.Add((SimpleBlueprint)bp);

            return result;
        }

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.AddCachedBlueprint))]
        [HarmonyPostfix]
        public static void Postfix(SimpleBlueprint bp)
        {
            var type = bp.GetType();
            if (Instance.ListByType.TryGetValue(type, out var list))
            {
                list.Add(bp);
            }
        }

    }
}
