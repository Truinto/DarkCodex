using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// List of enums with a counter of retains. Count is tracked individually by type and value.<br/>
    /// E.g. SpellSchool.Conjuration (0b10) and SpellSchool.Divination (0b11) do not overlap.<br/>
    /// All values are boxed, because they are cast into <see cref="Enum"/>.
    /// </summary>
    public class CountableFlagArray
    {
        [JsonProperty]
        private readonly Dictionary<Enum, CountableFlag> Data = new();

        /// <summary>
        /// Increases count for a specific enum value.
        /// </summary>
        public void Retain(Enum key)
        {
            Data.Ensure(key, out var flag);
            flag.Retain();
        }

        /// <summary>
        /// Decreases count for a specific enum value.
        /// </summary>
        public void Release(Enum key)
        {
            if (Data.TryGetValue(key, out var flag))
            {
                if (flag.Count <= 1)
                    Data.Remove(key);
                else
                    flag.Release();
            }
        }

        /// <summary>
        /// Remove a specific enum value. Same as setting count to zero.
        /// </summary>
        public void ReleaseAll(Enum key)
        {
            Data.Remove(key);
        }

        /// <summary>
        /// True if enum value count is one or more. Otherwise false.
        /// </summary>
        public bool HasFlag(Enum key)
        {
            return Data.ContainsKey(key);
        }

        /// <summary>
        /// True if no values are saved in this instance.
        /// </summary>
        public bool IsEmpty()
        {
            return Data.Count == 0;
        }

        /// <summary>
        /// Accessor to count of a specific enum value.
        /// </summary>
        public int this[Enum key]
        {
            get
            {
                if (Data.TryGetValue(key, out var flag))
                    return flag.Count;
                return 0;
            }
            set
            {
                if (value <= 0)
                    Data.Remove(key);
                else
                {
                    Data.Ensure(key, out var flag);
                    flag.m_Count = (short)value;
                }
            }
        }
    }
}
