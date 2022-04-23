using Kingmaker.EntitySystem.Stats;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;

namespace DarkCodex
{
    public class Settings : ISettings
    {
        [JsonProperty]
        public int version = 4;

        [JsonProperty]
        public bool showBootupWarning = true;

        [JsonProperty]
        public bool allowAchievements = true;

        [JsonProperty]
        public bool saveMetadata = true;

        [JsonProperty]
        public bool stopAreaEffectsDuringCutscenes = true;

        [JsonProperty]
        public bool reallyFreeCost = false;

        [JsonProperty]
        public bool newFeatureDefaultOn = true;

        [JsonProperty]
        public HashSet<string> Blacklist { get; private set; } = new();

        [JsonProperty]
        public HashSet<string> Whitelist { get; private set; } = new();

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public StatType PsychokineticistStat = StatType.Wisdom;

        [JsonProperty]
        public bool polymorphKeepInventory = false;

        [JsonProperty]
        public bool polymorphKeepModel = false;

        [JsonProperty]
        public bool debug_1 = false;
        [JsonProperty]
        public bool debug_2 = false;
        [JsonProperty]
        public bool debug_3 = false;
        [JsonProperty]
        public bool debug_4 = false;

        [JsonIgnore] public bool NewFeatureDefaultOn => newFeatureDefaultOn;

        public static Config.Manager<Settings> StateManager = new(Path.Combine(Main.ModPath, "settings.json"), OnUpdate);

        private static bool OnUpdate(Settings settings)
        {
            if (settings.version < 4 && settings.Whitelist != null && settings.Blacklist != null)
            {
                settings.showBootupWarning = true;
                var hash = new HashSet<string>();
                foreach (string str in settings.Whitelist)
                    hash.Add(toUpper(str));
                settings.Whitelist = hash;

                hash = new HashSet<string>();
                foreach (string str in settings.Blacklist)
                    hash.Add(toUpper(str));
                settings.Blacklist = hash;

                string toUpper(string text)
                {
                    if (text[0].IsLowercase())
                    {
                        Resource.sb.Clear();
                        Resource.sb.Append(text);
                        Resource.sb[0] -= (char)0x20;
                        return Resource.sb.ToString();
                    }
                    return text;
                }
            }

            return true;
        }
    }
}
