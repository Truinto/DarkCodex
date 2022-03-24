using Kingmaker.EntitySystem.Stats;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;

namespace DarkCodex
{
    public class Settings
    {
        [JsonProperty]
        public int version = 3;

        [JsonProperty]
        public bool showBootupWarning = true;

        [JsonProperty]
        public bool allowAchievements = true;

        [JsonProperty]
        public bool stopAreaEffectsDuringCutscenes = true;

        [JsonProperty]
        public bool reallyFreeCost = false;

        [JsonProperty]
        public bool newFeatureDefaultOn = true;

        [JsonProperty]
        [Obsolete]
        private HashSet<string> doNotLoad = null;

        [JsonProperty]
        private HashSet<string> blacklist = new()
        {
            "General.patchBasicFreebieFeats",
        };

        [JsonProperty]
        private HashSet<string> whitelist = new()
        {
        };

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

        public void SetEnable(bool value, string name)
        {
            if (value)
            {
                blacklist.Remove(name);
                if (!name.Contains(".*"))
                    whitelist.Add(name);
            }
            else
            {
                whitelist.Remove(name);
                blacklist.Add(name);
            }
        }

        public bool IsDisenabledSingle(string name)
        {
            if (blacklist.Contains(name))
                return true;
            return !newFeatureDefaultOn && !whitelist.Contains(name);
        }

        public bool IsDisenabledCategory(string name)
        {
            if (blacklist.Contains(name))
                return true;
            return false;
        }

        public bool IsDisenabled(string name)
        {
            string all = name.TrySubstring('.') + ".*";

            if (blacklist.Contains(name) || blacklist.Contains(all))
                return true;
            return !newFeatureDefaultOn && !whitelist.Contains(name);
        }

        public static Config.Manager<Settings> StateManager = new(Path.Combine(Main.ModPath, "settings.json"), OnUpdate);

        private static bool OnUpdate(Settings settings)
        {
            if (settings.version < 3 && settings.doNotLoad != null)
            {
                settings.showBootupWarning = true;
                settings.blacklist = settings.doNotLoad;
                settings.doNotLoad = null;
            }

            if (settings.version < 4 && settings.whitelist != null && settings.blacklist != null)
            {
                var hash = new HashSet<string>();
                foreach (string str in settings.whitelist)
                    hash.Add(toUpper(str));
                settings.whitelist = hash;

                hash = new HashSet<string>();
                foreach (string str in settings.blacklist)
                    hash.Add(toUpper(str));
                settings.blacklist = hash;

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
