using CodexLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UI.MVVM;
using Kingmaker.UI.UnitSettings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CodexLib
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
        [JsonIgnore]
        private Sprite m_Icon;
        [JsonIgnore]
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


        public static bool Unlocked;

        public static HashSet<DefGroup> Groups;

        public static Sprite GroupBorder;

        public static bool IsValidSpellGroup(MechanicActionBarSlot mechanic)
        {
            if (mechanic is MechanicActionBarSlotSpellGroup)
                return false;

            if (mechanic is MechanicActionBarSlotSpell spell && !spell.Spell.IsVariable)
                return true;
            if (mechanic is MechanicActionBarSlotAbility ability && !ability.Ability.IsVariable)
                return true;
            if (mechanic is MechanicActionBarSlotSpontaneusConvertedSpell convert && !convert.Spell.IsVariable)
                return true;

            return false;
        }

        public static BlueprintUnitFact GetBlueprint(MechanicActionBarSlot slot)
        {
            if (slot is MechanicActionBarSlotActivableAbility act)
                return act.ActivatableAbility.Blueprint;
            else if (slot is MechanicActionBarSlotAbility ab)
                return ab.Ability.Blueprint;
            else if (slot is MechanicActionBarSlotPlaceholder place)
                return place.Blueprint.Get();
            return null;
        }

        public static BlueprintGuid GetGuid(MechanicActionBarSlot slot)
        {
            return GetBlueprint(slot)?.AssetGuid ?? BlueprintGuid.Empty;
        }

        public static void RefreshUI()
        {
            var actionbar = RootUIContext.Instance.InGameVM.StaticPartVM.ActionBarVM;
            if (actionbar == null)
                return;
            actionbar.m_NeedReset = true;
            actionbar.OnUpdateHandler();
        }
    }
}
