using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Config
{
    public class JsonManager
    {
        public JsonSerializer Serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        });
        
        public T Deserialize<T>(string path)
        {
            T result;

            using (StreamReader streamReader = new(path))
            {
                using (JsonTextReader jsonReader = new(streamReader))
                {
                    result = this.Serializer.Deserialize<T>(jsonReader);

                    jsonReader.Close();
                }

                streamReader.Close();
            }

            return result;
        }
        public void Serialize<T>(T value, string path)
        {
            using (StreamWriter streamReader = new(path))
            {
                using (JsonTextWriter jsonReader = new(streamReader))
                {
                    this.Serializer.Serialize(jsonReader, value);

                    jsonReader.Close();
                }

                streamReader.Close();
            }
        }
    }

    public class JsonFileManager
    {
        private readonly JsonManager _jsonManager;


        public JsonManager GetJsonManager()
        {
            return _jsonManager;
        }

        public JsonFileManager(JsonManager jsonManager)
        {
            this._jsonManager = jsonManager;
        }


        public bool TryLoadConfiguration<T>(string path, out T state)
        {
            try
            {
                state = _jsonManager.Deserialize<T>(path);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
                Debug.LogWarning("Can't load configuration!");

                state = (T)Activator.CreateInstance(typeof(T));

                return false;
            }
        }

        public bool TrySaveConfiguration<T>(string path, T state)
        {
            try
            {
                _jsonManager.Serialize(state, path);
                return true;
            }
            catch (Exception ex)
            {
                const string Message = "Can't save configuration!";

                Debug.LogWarning(ex);
                Debug.LogWarning(Message);

                return false;
            }
        }
    }

    public class Manager<T>
    {
        public readonly string StateFilePath;

        public readonly JsonFileManager JsonLoader;

        private T _state;

        public Func<T, bool> updateCallback;

        public T State
        {
            get
            {
                if (_state != null)
                {
                    return _state;
                }
                Debug.Log("Loading: " + this.StateFilePath);

                if (!File.Exists(this.StateFilePath))
                {
                    Debug.Log(this.StateFilePath + " not found. Creating a default config file...");
                    EnsureDirectoryExists(new FileInfo(this.StateFilePath).Directory.FullName);

                    JsonLoader.TrySaveConfiguration(this.StateFilePath, (T)Activator.CreateInstance(typeof(T)));
                }
                JsonLoader.TryLoadConfiguration(this.StateFilePath, out _state);
                return _state;
            }

            private set
            {
                _state = value;
            }
        }

        public bool TryReloadConfiguratorState()
        {
            T state;
            if (JsonLoader.TryLoadConfiguration(this.StateFilePath, out state))
            {
                State = state;
                return true;
            }

            return false;
        }

        public bool TrySaveConfigurationState()
        {
            if (_state != null)
                return JsonLoader.TrySaveConfiguration(this.StateFilePath, _state);

            return false;
        }

        /// <summary>
        /// if not isAbsolute then path is the mods name
        /// </summary>
        public Manager(string path, Func<T, bool> updateCallback = null)
        {
            bool errorFlag;
            string resultPath;
            
            resultPath = path;
            EnsureDirectoryExists(new FileInfo(resultPath).Directory.FullName);
            errorFlag = !Directory.Exists(new FileInfo(resultPath).Directory.FullName);

            this.StateFilePath = resultPath;
            this.JsonLoader = new JsonFileManager(new JsonManager());

            this.updateCallback = updateCallback;

            UpdateVersion();
        }

        public void UpdateVersion()
        {
            try
            {
                object newObj = Activator.CreateInstance(typeof(T));
                int newVersion = (int)typeof(T).GetField("version").GetValue(newObj);
                int savedVersion = (int)typeof(T).GetField("version").GetValue(State);

                if (savedVersion != 0 && newVersion != 0)
                {
                    if (savedVersion != newVersion)
                    {
                        Debug.Log("Updating version...");
                        bool shouldSave = true;
                        if (updateCallback != null) shouldSave = updateCallback(State);
                        typeof(T).GetField("version").SetValue(State, newVersion);
                        if (shouldSave) this.TrySaveConfigurationState();
                    }
                }
            }
            catch (Exception e)
            {
                if (this.StateFilePath != null)
                    Debug.Log("Config.Manager could not check version of: " + Path.GetFileName(this.StateFilePath) + "\n" + e.Message);
                return;
            }
        }

        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }

    public class Helper
    {
        //https://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }

}