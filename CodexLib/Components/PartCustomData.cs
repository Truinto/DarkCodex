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
    /// <summary>
    /// EntityPart to store any kind of enum value.
    /// </summary>
    public class PartCustomData : EntityPart
    {
        [JsonProperty]
        private CountableFlagArray flags;

        /// <inheritdoc cref="CountableFlagArray"/>
        public CountableFlagArray Flags { get => flags ??= new(); }

        /// <inheritdoc cref="CountableFlagArray.IsEmpty"/>
        public bool IsEmpty()
        {
            return flags == null || flags.IsEmpty();
        }
    }

    /// <summary>
    /// Extension methods to work with <see cref="PartCustomData"/>.
    /// </summary>
    public static class PartExtensions
    {
        /// <summary>
        /// Removes all instances of <see cref="PartCustomData"/>.
        /// </summary>
        public static void ClearData(this UnitEntityData unit)
        {
            unit.Parts.RemoveAll<PartCustomData>(f => true);
        }

        /// <summary>
        /// Adds <see cref="PartCustomData"/>, if necessary. Increase give enum count by one.
        /// </summary>
        public static void Retain(this UnitEntityData unit, Enum key)
        {
            var data = unit?.Ensure<PartCustomData>();
            data.Flags.Retain(key);
        }

        /// <summary>
        /// Decreases enum count by one. If <see cref="PartCustomData"/> is empty, it will be removed.
        /// </summary>
        public static void Release(this UnitEntityData unit, Enum key)
        {
            var data = unit?.Get<PartCustomData>();
            if (data == null)
                return;
            data.Flags.Release(key);
            if (data.IsEmpty())
                unit.Parts.Remove(data);
        }

        /// <summary>
        /// Checks if given enum value is one or more.
        /// </summary>
        public static bool HasFlag(this UnitEntityData unit, Enum key)
        {
            var data = unit?.Get<PartCustomData>();
            if (data == null)
                return false;
            return data.Flags.HasFlag(key);
        }
    }

}
