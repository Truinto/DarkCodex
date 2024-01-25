using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Shared.Loggers;

#pragma warning disable CS1591

// TODO: update to use System.Text.Json JsonSerializer.Serialize
//private static JsonSerializerOptions jsonOptions = new() { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve, AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip, IncludeFields = false, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals }; //[JsonInclude] [JsonIgnore]
//private static JsonSerializerOptions jsonOptionsRef = new() { WriteIndented = true, ReferenceHandler = ReferenceHandler.IgnoreCycles, AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip, IncludeFields = false, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals };


namespace Shared.Serialize
{
    /// <summary>
    /// Tool to handle (de-)serialization.
    /// </summary>
    public static class SerializationTool
    {
        public static List<JsonConverter> DefaultConverters = [];

        public static JsonSerializerSettings JSettings = new()
        {
            Converters = DefaultConverters,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.Auto,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        };

        public static string Serialize(this object value, bool indent = true, bool type = true, string path = null, bool append = false)
        {
            JSettings.Formatting = indent ? Formatting.Indented : Formatting.None;
            JSettings.TypeNameHandling = type ? TypeNameHandling.Auto : TypeNameHandling.None;
            string result = JsonConvert.SerializeObject(value, JSettings);

            if (path != null)
            {
                //path = Path.Combine(Main.ModPath, path);
                EnsureDir(path);
                using var sw = new StreamWriter(path, append);
                sw.WriteLine(result);
                sw.Close();
            }

            return result;
        }

        public static string Serialize<T>(this T value, bool indent = true, bool type = true, string path = null, bool append = false)
        {
            JSettings.Formatting = indent ? Formatting.Indented : Formatting.None;
            JSettings.TypeNameHandling = type ? TypeNameHandling.Auto : TypeNameHandling.None;
            string result = JsonConvert.SerializeObject(value, typeof(T), JSettings);

            if (path != null)
            {
                //path = Path.Combine(Main.ModPath, path);
                EnsureDir(path);
                using var sw = new StreamWriter(path, append);
                sw.WriteLine(result);
                sw.Close();
            }

            return result;
        }

        public static object Deserialize(string path = null, string value = null)
        {
            if (path != null)
            {
                //path = Path.Combine(Main.ModPath, path);
                using var sr = new StreamReader(path);
                value = sr.ReadToEnd();
                sr.Close();
            }

            if (value != null)
                return JsonConvert.DeserializeObject(value, JSettings);
            return null;
        }

        public static T Deserialize<T>(string path = null, string value = null)
        {
            if (path != null)
            {
                //path = Path.Combine(Main.ModPath, path);
                using var sr = new StreamReader(path);
                value = sr.ReadToEnd();
                sr.Close();
            }

            if (value != null)
                return (T)JsonConvert.DeserializeObject(value, typeof(T), JSettings);
            return default;
        }

        public static string TrySerialize<T>(this T value, bool indent = true, bool type = true, string path = null, bool append = false)
        {
            try
            {
                return Serialize<T>(value: value, indent: indent, type: type, path: path, append: append);
            }
            catch (Exception e) { Shared.Loggers.Logger.PrintException(e); }
            return null;
        }

        public static bool TryDeserialize<T>(out T result, string path = null, string value = null)
        {
            try
            {
                result = Deserialize<T>(path, value);
                return true;
            }
            catch (Exception e) { Shared.Loggers.Logger.PrintException(e); }
            result = default;
            return false;
        }

        public static void TryPrintFile(string path, string content, bool append = true)
        {
            try
            {
                //path = Path.Combine(Main.ModPath, path);
                EnsureDir(path);
                using var sw = new StreamWriter(path, append);
                sw.WriteLine(content);
            }
            catch (Exception e) { Shared.Loggers.Logger.PrintException(e); }
        }

        public static void TryPrintBytes(string path, byte[] data)
        {
            try
            {
                //path = Path.Combine(Main.ModPath, path);
                EnsureDir(path);
                File.WriteAllBytes(path, data);
            }
            catch (Exception e) { Shared.Loggers.Logger.PrintException(e); }
        }

        public static byte[] TryReadBytes(string path)
        {
            try
            {
                //path = Path.Combine(Main.ModPath, path);
                return File.ReadAllBytes(path);
            }
            catch (Exception e) { Shared.Loggers.Logger.PrintException(e); }
            return new byte[0];
        }

        public static void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception e) { Shared.Loggers.Logger.PrintException(e); }
        }

        private static void EnsureDir(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            path = System.IO.Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(path))
                return;
            Directory.CreateDirectory(path);
        }
    }
}
