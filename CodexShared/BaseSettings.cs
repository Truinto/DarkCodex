using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shared
{
    public abstract class BaseSettings<T> : ISettings where T : BaseSettings<T>, new()
    {
        [JsonProperty] public int version;
        [JsonProperty] public bool newFeatureDefaultOn = true;

        [JsonIgnore] public int Version => version;
        [JsonIgnore] public bool NewFeatureDefaultOn => newFeatureDefaultOn;
        [JsonProperty] public HashSet<string> Blacklist { get; protected set; } = new();
        [JsonProperty] public HashSet<string> Whitelist { get; protected set; } = new();

        [JsonIgnore] public string FilePath;
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

        protected virtual bool OnUpdate() => true;

        public void TrySave()
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
            catch (Exception e) { Main.PrintException(e); }
        }

        public static T TryLoad(string modPath, string file = "settings.json")
        {
            string filePath = Path.Combine(modPath, file);
            try
            {
                using var sr = new StreamReader(Path.Combine(Main.ModPath, modPath));
                T state = JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
                sr.Close();
                state.FilePath = filePath;

                T refState = new();
                if (state.version != refState.version)
                {
                    if (state.OnUpdate())
                    {
                        state.version = refState.version;
                        state.TrySave();
                    }
                }
                return state;
            }
            catch (Exception)
            {
                Main.Print("Could not load setting, creating new.");
                T state = new();
                state.FilePath = filePath;
                state.TrySave();
                return state;
            }
        }
    }
}
