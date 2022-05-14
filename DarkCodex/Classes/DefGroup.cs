using CodexLib;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkCodex
{
    public class DefGroup : IEquatable<DefGroup>, IEquatable<string>
    {
        [JsonProperty]
        public string Title;
        [JsonProperty]
        public string Description;
        [JsonProperty]
        public List<BlueprintGuid> Guids;

        [JsonProperty]
        private string icon;
        private Sprite m_Icon;
        private int hash;

        [JsonConstructor]
        public DefGroup(string title, string description, string icon, List<BlueprintGuid> guids)
        {
            this.Title = title;
            this.Description = description;
            this.Guids = guids ?? new();
            this.icon = icon;
            this.hash = title.GetHashCode();
        }

        public DefGroup(string title, string description, string icon, params string[] guids)
        {
            this.Title = title;
            this.Description = description;
            this.Guids = guids.Select(s => BlueprintGuid.Parse(s)).ToList();
            this.icon = icon;
            this.hash = title.GetHashCode();
        }

        public DefGroup()
        {
            this.Guids = new();
        }

        public Sprite GetIcon()
        {
            if (m_Icon == null && this.icon != null)
                m_Icon = Helper.StealIcon(this.icon);
            return m_Icon;
        }

        public override int GetHashCode()
        {
            return hash;
        }

        public bool Equals(DefGroup other)
        {
            return this.hash == other.hash;
        }
        public bool Equals(string other)
        {
            return this.Title == other;
        }

        public bool Equals(DefGroup x, DefGroup y)
        {
            return x.hash == y.hash;
        }
    }
}
