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
        private CountableFlagArray flags;

        public CountableFlagArray Flags { get => flags ??= new(); }

        public bool IsEmpty()
        {
            return flags == null || flags.IsEmpty();
        }
    }

    public static class PartExtensions
    {
        public static void ClearData(this UnitEntityData unit)
        {
            unit.Parts.RemoveAll<PartCustomData>(f => true);
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
