using Kingmaker.EntitySystem.Stats;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;

namespace DarkCodex
{
    public class Settings : BaseSettings<Settings>
    {
        public Settings() => version = 4;

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
        [JsonConverter(typeof(StringEnumConverter))]
        public StatType PsychokineticistStat = StatType.Wisdom;

        [JsonProperty]
        public bool polymorphKeepInventory = false;

        [JsonProperty]
        public bool polymorphKeepModel = false;

        [JsonProperty]
        public bool verbose = true;
        [JsonProperty]
        public bool debug_1 = false;
        [JsonProperty]
        public bool debug_2 = false;
        [JsonProperty]
        public bool debug_3 = false;
        [JsonProperty]
        public bool debug_4 = false;

        public static Settings State = TryLoad(Main.ModPath);

        protected override bool OnUpdate()
        {
            if (version < 4 && Whitelist != null && Blacklist != null)
            {
                showBootupWarning = true;
                var hash = new HashSet<string>();
                foreach (string str in Whitelist)
                    hash.Add(toUpper(str));
                Whitelist = hash;

                hash = new HashSet<string>();
                foreach (string str in Blacklist)
                    hash.Add(toUpper(str));
                Blacklist = hash;

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
