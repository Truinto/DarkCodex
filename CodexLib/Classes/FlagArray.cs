using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class FlagArray
    {
        [JsonProperty]
        public Dictionary<Type, long> Data = new();

        public T GetFlags<T>() where T : Enum
        {
            if (this.Data.TryGetValue(typeof(T), out long value))
                return (T)Enum.ToObject(typeof(T), value);
            return default;
        }

        public void AddFlags<T>(T value) where T : Enum
        {
            if (!this.Data.ContainsKey(typeof(T)))
                this.Data[typeof(T)] = Convert.ToInt64(value);
            this.Data[typeof(T)] |= Convert.ToInt64(value);
        }

        public void RemoveFlags<T>(T value) where T : Enum
        {
            if (!this.Data.ContainsKey(typeof(T)))
                return;
            if (0 == (this.Data[typeof(T)] &= ~Convert.ToInt64(value)))
                this.Data.Remove(typeof(T));
        }

        public bool HasAnyFlags<T>(T value) where T : Enum
        {
            if (!this.Data.ContainsKey(typeof(T)))
                return false;
            return (this.Data[typeof(T)] & Convert.ToInt64(value)) != 0;
        }

        public bool HasAllFlags<T>(T value) where T : Enum
        {
            if (!this.Data.ContainsKey(typeof(T)))
                return false;
            return (this.Data[typeof(T)] | ~Convert.ToInt64(value)) == -1;
        }

        public void AddFlags(FlagArray other)
        {
            foreach (var flags in other.Data)
            {
                if (!this.Data.ContainsKey(flags.Key))
                    this.Data[flags.Key] = flags.Value;
                else
                    this.Data[flags.Key] |= flags.Value;
            }
        }

        public void RemoveFlags(FlagArray other)
        {
            foreach (var flags in other.Data)
            {
                if (this.Data.ContainsKey(flags.Key))
                    this.Data[flags.Key] &= ~flags.Value;
            }
        }

        //[OnSerializing]
        private void OnSerializing()
        {
            foreach (var key in this.Data.Keys.ToArray())
            {
                if (this.Data[key] == 0)
                    this.Data.Remove(key);
            }
        }

        //[OnDeserialized]
        private void OnDeserialized() 
        {
            Helper.PrintDebug("OnDeserialized FlagArray");
        }

        //[OnError]
        private void OnError(StreamingContext context, ErrorContext errorContext)
        {
            Helper.PrintException(errorContext.Error);
            errorContext.Handled = true;
        }

        private static void Debug()
        {
            var f = new FlagArray();

            f.AddFlags(Test.None);
        }

        private enum Test
        {
            None
        }
    }
}
