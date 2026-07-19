using CodexLib;
using Kingmaker.EntitySystem.Stats;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;

#pragma warning disable IDE1006 // ignore lower case fields

namespace DarkCodex
{
    public class Settings : BaseSettings<Settings>
    {
        public override string DefaultPath => Path.Combine(Main.ModPath, "settings.json");
        [JsonInclude] public override int Version { get; set; } = 6;

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

        [JsonProperty] public bool NewFeatureDefaultOn { get; set; }
        /// <summary>Collection of features explicitly turned off.</summary>
        [JsonProperty] public HashSet<string> Blacklist { get; protected set; } = [];
        /// <summary>Collection of features explicitly turned on.</summary>
        [JsonProperty] public HashSet<string> Whitelist { get; protected set; } = [];

        public static Settings State = TryLoad();

        protected override bool OnUpdate()
        {
            if (Version < 4)
            {
                showBootupWarning = true;
                var hash = new HashSet<string>();
                foreach (string str in Whitelist)
                    hash.Add(toUpper(str));
                Whitelist = hash;

                hash = [];
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

            if (Version < 5)
            {
                Whitelist.Remove("Patch.Patch_RespecPartially");
            }

            if (Version < 6)
            {
                if (!Blacklist.Contains("DEBUG.Enchantments"))
                    Whitelist.Add("DEBUG.Enchantments");
                if (!Blacklist.Contains("Enchantments.NameAll"))
                    Whitelist.Add("Enchantments.NameAll");
            }

            return true;
        }
    }
}
