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
        private CountableFlagArray flags;

        public Dictionary<string, object> Data { get => data ??= new(); }
        public CountableFlagArray Flags { get => flags ??= new(); }

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
            var data = unit?.Get<PartCustomData>();
            if (data == null)
                return default;
            data.Data.TryGetValue(key, out object value);
            return value as T;
        }

        public static void AddData(this UnitEntityData unit, string key, object value)
        {
            var data = unit?.Ensure<PartCustomData>();
            data.Data[key] = value;
        }

        public static void RemoveData(this UnitEntityData unit, string key)
        {
            var data = unit?.Get<PartCustomData>();
            if (data == null)
                return;
            data.Data.Remove(key);
            if (data.IsEmpty())
                unit.Parts.Remove(data);
        }

        public static void Retain<T>(this UnitEntityData unit, T key) where T : Enum
        {
            var data = unit?.Ensure<PartCustomData>();
            data.Flags.Retain<T>(key);
        }

        public static void Release<T>(this UnitEntityData unit, T key) where T : Enum
        {
            var data = unit?.Get<PartCustomData>();
            if (data == null)
                return;
            data.Flags.Release<T>(key);
            if (data.IsEmpty())
                unit.Parts.Remove(data);
        }

        public static bool HasFlag<T>(this UnitEntityData unit, T key) where T : Enum
        {
            var data = unit?.Get<PartCustomData>();
            if (data == null)
                return false;
            return data.Flags.HasFlag<T>(key);
        }
    }

}
