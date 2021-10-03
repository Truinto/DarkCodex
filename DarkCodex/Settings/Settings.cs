using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace DarkCodex
{
    public class Settings
    {
        [JsonProperty]
        public int version = 1;
        
        [JsonProperty]
        public bool allowAchievements = true;

        [JsonProperty]
        public List<string> doNotLoad = new List<string>() { 
            "General.patchBasicFreebieFeats",
        };

        public static Config.Manager<Settings> StateManager = new Config.Manager<Settings>(Path.Combine(Main.ModPath, "settings.json"));
    }
}
