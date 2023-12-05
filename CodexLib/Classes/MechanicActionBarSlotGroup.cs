using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UI.UnitSettings;
using Kingmaker.Utility;
using Newtonsoft.Json;
using Owlcat.Runtime.UI.Tooltips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CodexLib
{
    /// <summary>
    /// Logic for Ability Group
    /// </summary>
    public class MechanicActionBarSlotGroup : MechanicActionBarSlot, IMechanicGroup
    {
        [JsonProperty]
        private int hash;
        [JsonProperty]
        public List<MechanicActionBarSlot> Slots;

        [JsonConstructor]
        public MechanicActionBarSlotGroup(EntityRef<UnitEntityData> m_UnitRef, int hash, List<MechanicActionBarSlot> slots)
        {
            this.m_UnitRef = m_UnitRef;
            this.hash = hash;
            this.Slots = slots;

            if (this.hash != 0)
                return;

            // find and set Index
            var slot = this.Slots.FirstOrDefault();
            var guid = DefGroup.GetGuid(slot);
            if (guid != BlueprintGuid.Empty)
                this.hash = DefGroup.Groups?.FindOrDefault(f => f.Guids.Contains(guid))?.GetHashCode() ?? 0;
        }

        public MechanicActionBarSlotGroup(UnitEntityData unit, int hash, List<MechanicActionBarSlot> slots)
        {
            this.Unit = unit;
            this.hash = hash;
            this.Slots = slots ?? [];
        }

        List<MechanicActionBarSlot> IMechanicGroup.Slots => Slots;
        public void AddToGroup(MechanicActionBarSlot mechanic, MechanicActionBarSlot target = null, bool placeRight = true)
        {
            if (mechanic == null)
                return;

            var guid = DefGroup.GetGuid(mechanic);
            if (guid == BlueprintGuid.Empty)
                return;

            Group.Guids.Remove(guid);

            int placeAt = Slots.Count;
            if (target != null)
            {
                int index = Group.Guids.IndexOf(DefGroup.GetGuid(target));
                if (index >= 0)
                    placeAt = index + (placeRight ? 1 : 0);
            }

            Group.Guids.Insert(placeAt, guid);
            if (!Slots.Contains(mechanic))
                Slots.Add(mechanic);
            DefGroup.RefreshUI();
        }
        public void RemoveFromGroup(MechanicActionBarSlot mechanic)
        {
            if (mechanic == null)
                return;
            var guid = DefGroup.GetGuid(mechanic);
            if (guid == BlueprintGuid.Empty)
                return;

            Group.Guids.Remove(guid);
            Slots.Remove(mechanic);
            DefGroup.RefreshUI();
        }

        public override int GetHashCode()
        {
            return hash;
        }

        public DefGroup Group => DefGroup.Groups?.FindOrDefault(w => w.GetHashCode() == this.hash) ?? new();

        public override bool CanUseIfTurnBasedInternal() => true;
        public override object GetContentData() => this;
        public override Color GetDecorationColor() => Color.black;
        public override Sprite GetDecorationSprite() => DefGroup.GroupBorder;
        public override string GetTitle()
        {
            return Group.Title;
        }

        public override string GetDescription()
        {
            return Group.Description;
        }

        public override Sprite GetIcon()
        {
            return Group.GetIcon()
                ?? Slots.FirstOrDefault(f => f.IsActive())?.GetIcon()
                ?? Slots.FirstOrDefault()?.GetIcon();
        }

        public override int GetResource()
        {
            if (Slots == null)
                return -1;

            int count = 0;
            foreach (var slot in Slots)
                if (slot.IsActive())
                    count++;
            if (count == 0)
                return -1;
            return count;
        }
        public override bool IsCasting() => false;

        public override string KeyName => GetTitle();

        public override bool IsActive() => Slots.Any(a => a.IsActive());
        public override bool IsDisabled(int resourceCount) => false;
        public override bool IsPossibleActive(int? resource = null) => true;
        public override void UpdateSlotInternal(ActionBarSlot slot)
        {
            if (slot.ActiveMark != null && IsActive())
            {
                slot.ActiveMark.color = slot.RunningColor;
                slot.ActiveMark.gameObject.SetActive(true);
            }
        }
        public override bool IsBad() => hash == 0 || Group.Title == null; // use this to remove invalid entries

        public override TooltipBaseTemplate GetTooltipTemplate()
        {
            var group = Group;
            string title = group.Title ?? "MISSING TITLE";
            string description = group.Description;
            Sprite icon = group.GetIcon();
            return new TooltipTemplateDataProvider(new UIData(title, description, icon));
        }
    }
}
