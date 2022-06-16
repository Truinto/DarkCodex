using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class CountableFlagArray
    {
        [JsonProperty]
        public Dictionary<Enum, CountableFlag> Data = new();

        public void Retain<T>(T key) where T : Enum
        {
            Data.Ensure(key, out var flag);
            flag.Retain();
        }

        public void Release<T>(T key) where T : Enum
        {
            if (Data.TryGetValue(key, out var flag))
            {
                if (flag.Count <= 1)
                    Data.Remove(key);
                else
                    flag.Release();
            }
        }

        public void ReleaseAll<T>(T key) where T : Enum
        {
            Data.Remove(key);
        }

        public bool HasFlag<T>(T key) where T : Enum
        {
            return Data.ContainsKey(key);

            //if (Data.TryGetValue(key, out var flag))
            //    return flag.Value;
            //return false;
        }

        public int GetCount<T>(T key) where T : Enum
        {
            if (Data.TryGetValue(key, out var flag))
                return flag.Count;
            return 0;
        }
    }
}
