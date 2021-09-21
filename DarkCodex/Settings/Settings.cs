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
        public List<string> doNotLoad = new List<string>();

        [JsonProperty]
        public bool debug_block_unrecruit = false;

        [JsonProperty]
        public List<string> debug_block_quest = new List<string>() { "guid here", "or add more", "as much as you like" };

        public static Config.Manager<Settings> StateManager = new Config.Manager<Settings>(Path.Combine(Main.ModPath, "settings.json"));
    }
}
