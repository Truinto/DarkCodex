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
        public bool stopAreaEffectsDuringCutscenes = true;

        [JsonProperty]
        public bool reallyFreeCost = false;

        [JsonProperty]
        public bool newFeatureDefaultOn = true;

        [JsonProperty]
        private HashSet<string> blacklist = new();

        [JsonProperty]
        private HashSet<string> whitelist = new();

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

        public bool IsDisenabledCategory(string name)
        {
            if (blacklist.Contains(name))
                return true;
            return false;
        }

        public bool IsDisenabledSingle(string name)
        {
            if (blacklist.Contains(name))
                return true;

            if (whitelist.Contains(name))
                return false;

            int hash = name.GetHashCode();
            if (Main.patchInfos.Find(f => f.Hash == hash)?.IsDefaultOff == true)
                return true;

            return !newFeatureDefaultOn;
        }

        public bool IsDisenabled(string name)
        {
            if (blacklist.Contains(name))
                return true;

            if (blacklist.Contains(name.TrySubstring('.') + ".*"))
                return true;

            if (whitelist.Contains(name))
                return false;

            int hash = name.GetHashCode();
            if (Main.patchInfos.Find(f => f.Hash == hash)?.IsDefaultOff == true)
                return true;

            return !newFeatureDefaultOn;
        }

        [JsonIgnore] public bool NewFeatureDefaultOn => newFeatureDefaultOn;
        [JsonIgnore] public HashSet<string> Blacklist => blacklist;
        [JsonIgnore] public HashSet<string> Whitelist => whitelist;

        public static Config.Manager<Settings> StateManager = new(Path.Combine(Main.ModPath, "settings.json"), OnUpdate);

        private static bool OnUpdate(Settings settings)
        {
            if (settings.version < 4 && settings.whitelist != null && settings.blacklist != null)
            {
                settings.showBootupWarning = true;
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
