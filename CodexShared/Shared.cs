using Kingmaker.Blueprints.Facts;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Shared
{
    public static class Shared
    {
        #region Strings/Localization

        //public static T SetStrings<T>(this T obj, string name, string description, string descriptionShort = null) where T : BlueprintUnitFact
        //{
        //    obj.m_DisplayName = name?.CreateString();
        //    obj.m_Description = description?.CreateString();
        //    obj.m_DescriptionShort = descriptionShort?.CreateString();
        //    return obj;
        //}

        //private static SHA1 _SHA = SHA1.Create();
        //private static StringBuilder _sb1 = new();
        //private static Locale _lastLocale = Locale.enGB;
        //private static Dictionary<string, string> _mappedStrings;
        //public static LocalizedString CreateString(this string value, string key = null)
        //{
        //    if (value == null || value == "")
        //        return new LocalizedString { Key = "" };

        //    if (key == null)
        //    {
        //        var sha = _SHA.ComputeHash(Encoding.UTF8.GetBytes(value));
        //        for (int i = 0; i < sha.Length; i++)
        //            _sb1.Append(sha[i].ToString("x2"));
        //        key = _sb1.ToString();
        //        _sb1.Clear();
        //    }

        //    var pack = LocalizationManager.CurrentPack;
        //    if (LocalizationManager.CurrentPack.Locale != _lastLocale)
        //    {
        //        _lastLocale = LocalizationManager.CurrentPack.Locale;
        //        try
        //        {
        //            string path = LocalizationManager.CurrentPack.Locale.ToString() + "enGB.json";
        //            path = Path.Combine(Main.ModPath, path);
        //            _mappedStrings = Deserialize<Dictionary<string, string>>(path: path);
        //            foreach (var entry in _mappedStrings)
        //                pack.PutString(entry.Key, entry.Value);
        //            _mappedStrings = null;
        //        }
        //        catch (Exception e)
        //        {
        //            Main.Print($"Could not read lanaguage file for {LocalizationManager.CurrentPack.Locale}: {e.Message}");
        //        }
        //    }

        //    if (!pack.m_Strings.ContainsKey(key))
        //    {
        //        pack.PutString(key, value);
        //        _saveString(key, value);
        //    }

        //    return new LocalizedString { Key = key };
        //}

        //[System.Diagnostics.Conditional("DEBUG")]
        //private static void _saveString(string key, string value)
        //{
        //    if (_mappedStrings == null)
        //        _mappedStrings = new Dictionary<string, string>();
        //    _mappedStrings[key] = value;
        //}

        //[System.Diagnostics.Conditional("DEBUG")]
        //public static void ExportStrings()
        //{
        //    if (_mappedStrings == null)
        //        return;

        //    Dictionary<string, string> oldmap = null;

        //    try
        //    {
        //        oldmap = Deserialize<Dictionary<string, string>>(path: Path.Combine(Main.ModPath, "enGB.json"));

        //        foreach (var entry in _mappedStrings)
        //            if (!oldmap.ContainsKey(entry.Key))
        //                oldmap.Add(entry.Key, entry.Value);
        //    }
        //    catch (Exception) { }

        //    try
        //    {
        //        Serialize(oldmap ?? _mappedStrings, path: Path.Combine(Main.ModPath, "enGB.json"));
        //        _mappedStrings = null;
        //    }
        //    catch (Exception e)
        //    {
        //        Main.Print($"Failed export lanaguage file: {e.Message}");
        //    }
        //}

        #endregion

        #region JsonSerializer

        private static JsonSerializerSettings _jsetting = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.Auto,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        };

        public static string Serialize(object value, bool indent = true, bool type = true, string path = null, bool append = false)
        {
            _jsetting.Formatting = indent ? Formatting.Indented : Formatting.None;
            string result = JsonConvert.SerializeObject(value, _jsetting);

            if (path != null)
            {
                //path = Path.Combine(Main.ModPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using var sw = new StreamWriter(path, append);
                sw.WriteLine(result);
                sw.Close();
            }

            return result;
        }

        public static string Serialize<T>(T value, bool indent = true, bool type = true, string path = null, bool append = false)
        {
            _jsetting.Formatting = indent ? Formatting.Indented : Formatting.None;
            _jsetting.TypeNameHandling = type ? TypeNameHandling.Auto : TypeNameHandling.None;
            string result = JsonConvert.SerializeObject(value, typeof(T), _jsetting);

            if (path != null)
            {
                //path = Path.Combine(Main.ModPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
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
                return JsonConvert.DeserializeObject(value);
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
                return JsonConvert.DeserializeObject<T>(value);
            return default;
        }

        public static T TryDeserialize<T>(string path = null, string value = null)
        {
            try
            {
                return Deserialize<T>(path, value);
            }
            catch (Exception e) { Main.PrintException(e); }
            return default;
        }

        public static void TryPrintFile(string path, string content, bool append = true)
        {
            try
            {
                //path = Path.Combine(Main.ModPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using var sw = new StreamWriter(path, append);
                sw.WriteLine(content);
            }
            catch (Exception e) { Main.PrintException(e); }
        }

        public static void TryPrintBytes(string path, byte[] data)
        {
            try
            {
                //path = Path.Combine(Main.ModPath, path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, data);
            }
            catch (Exception e) { Main.PrintException(e); }
        }

        public static byte[] TryReadBytes(string path)
        {
            try
            {
                //path = Path.Combine(Main.ModPath, path);
                return File.ReadAllBytes(path);
            }
            catch (Exception e) { Main.PrintException(e); }
            return new byte[0];
        }

        public static void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                Main.PrintException(e);
            }
        }

        #endregion
    }
}
