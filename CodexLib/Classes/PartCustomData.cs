using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class PartCustomData : EntityPart
    {
        [JsonProperty]
        private Dictionary<string, object> data;
        [JsonProperty]
        private FlagArray flags;

        public Dictionary<string, object> Data { get => data ??= new(); }
        public FlagArray Flags { get => flags ??= new(); }

        public bool IsEmpty()
        {
            return (data == null || data.Count == 0) && (flags == null || flags.Data == null || flags.Data.Count == 0);
        }
    }

    public static class PartExtensions
    {
        public static void ClearData(this UnitEntityData unit)
        {
            unit.Parts.RemoveAll<PartCustomData>(f => true);
        }

        public static T GetData<T>(this UnitEntityData unit, string key) where T : class
        {
            var data = unit.Get<PartCustomData>();
            if (data == null)
                return default;
            data.Data.TryGetValue(key, out object value);
            return value as T;
        }

        public static void AddData(this UnitEntityData unit, string key, object value)
        {
            var data = unit.Ensure<PartCustomData>();
            data.Data[key] = value;
        }

        public static void RemoveData(this UnitEntityData unit, string key)
        {
            var data = unit.Get<PartCustomData>();
            if (data == null)
                return;
            data.Data.Remove(key);
            if (data.IsEmpty())
                unit.Parts.Remove(data);
        }

        public static T GetFlags<T>(this UnitEntityData unit) where T : Enum
        {
            var data = unit.Get<PartCustomData>();
            if (data == null)
                return default;
            return data.Flags.GetFlags<T>();
        }

        public static void AddFlags<T>(this UnitEntityData unit, T flags) where T : Enum
        {
            var data = unit.Ensure<PartCustomData>();
            data.Flags.AddFlags<T>(flags);
        }

        public static void RemoveFlags<T>(this UnitEntityData unit, T flags) where T : Enum
        {
            var data = unit.Get<PartCustomData>();
            if (data == null)
                return;
            data.Flags.RemoveFlags<T>(flags);
            if (data.IsEmpty())
                unit.Parts.Remove(data);
        }

        public static bool HasAnyFlags<T>(this UnitEntityData unit, T flags) where T : Enum
        {
            var data = unit.Get<PartCustomData>();
            if (data == null)
                return false;
            return data.Flags.HasAnyFlags<T>(flags);
        }

        public static bool HasAllFlags<T>(this UnitEntityData unit, T flags) where T : Enum
        {
            var data = unit.Get<PartCustomData>();
            if (data == null)
                return false;
            return data.Flags.HasAllFlags<T>(flags);
        }
    }

}
