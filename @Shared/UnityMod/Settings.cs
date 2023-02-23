using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shared
{
    /// <summary>
    /// Inherit this class in your settings class. Implements basic load/save functions.
    /// </summary>
    /// <typeparam name="T">Class that contains settings.</typeparam>
    public abstract class BaseSettings<T> : ISettings where T : BaseSettings<T>, new()
    {
        /// <summary>File version. Overwrite this in the constructor.</summary>
        [JsonProperty] public int Version { get; set; }
        /// <summary>Whenever new features should be considered on by default.</summary>
        [JsonProperty] public bool NewFeatureDefaultOn { get; set; }
        /// <summary>Collection of features explicitly turned off.</summary>
        [JsonProperty] public HashSet<string> Blacklist { get; protected set; } = new();
        /// <summary>Collection of features explicitly turned on.</summary>
        [JsonProperty] public HashSet<string> Whitelist { get; protected set; } = new();
        /// <summary>File path to load from / save to.</summary>
        [JsonIgnore] public string FilePath;
        /// <summary>Serialization settings.</summary>
        [JsonIgnore]
        public JsonSerializerSettings JSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.Auto,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        };

        /// <summary>
        /// Called, if file version is different from current version. Return true, if file should be saved.
        /// </summary>
        protected virtual bool OnUpdate() => true;

        /// <summary>
        /// Try save file.
        /// </summary>
        public virtual void TrySave()
        {
            if (FilePath == null)
                return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                using var sw = new StreamWriter(FilePath, false);
                sw.WriteLine(JsonConvert.SerializeObject(this, typeof(T), JSettings));
                sw.Close();
            }
            catch (Exception e) { Logger.PrintException(e); }
        }

        /// <summary>
        /// Try load file. Creates new file, if loading failed for any reason.
        /// </summary>
        /// <param name="path">Path settings should be saved.</param>
        /// <param name="file">Name of settings file. Should include file extension.</param>
        public static T TryLoad(string path, string file = "settings.json")
        {
            string filePath = Path.Combine(path, file);
            try
            {
                using var sr = new StreamReader(filePath);
                T state = JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
                sr.Close();
                state.FilePath = filePath;

                T refState = new();
                if (state.Version != refState.Version)
                {
                    if (state.OnUpdate())
                    {
                        state.Version = refState.Version;
                        state.TrySave();
                    }
                }
                return state;
            }
            catch (Exception)
            {
                Logger.Print("Could not load setting, creating new.");
                T state = new() { FilePath = filePath };
                state.TrySave();
                return state;
            }
        }
    }

    /// <summary>
    /// Abstract settings interface.
    /// </summary>
    public interface ISettings
    {
        /// <summary>File version.</summary>
        public int Version { get; }

        /// <summary>Whenever new features should be considered on by default.</summary>
        public bool NewFeatureDefaultOn { get; }

        /// <summary>Collection of features explicitly turned off.</summary>
        public HashSet<string> Blacklist { get; }

        /// <summary>Collection of features explicitly turned on.</summary>
        public HashSet<string> Whitelist { get; }
    }
}
