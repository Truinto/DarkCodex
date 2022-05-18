using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Kingmaker.Utility;

namespace CodexLib
{

    public class MechanicActionBarSlotSpellGroup : MechanicActionBarSlotAbility, IMechanicGroup // logic for spell groups
    {
        [JsonProperty]
        public List<MechanicActionBarSlot> Slots;
        private int[] cacheCountIndex;

        [JsonConstructor]
        public MechanicActionBarSlotSpellGroup(EntityRef<UnitEntityData> m_UnitRef, List<MechanicActionBarSlot> slots, AbilityData ability)
        {
            this.m_UnitRef = m_UnitRef;
            this.Slots = slots;
            this.Ability = ability;
        }
        public MechanicActionBarSlotSpellGroup(UnitEntityData unit, List<MechanicActionBarSlot> slots)
        {
            this.Unit = unit;
            this.Slots = slots;
            this.Ability = slots[0].GetContentData() as AbilityData;
            UpdateResourceCount();
        }

        List<MechanicActionBarSlot> IMechanicGroup.Slots => Slots;
        public void AddToGroup(MechanicActionBarSlot mechanic, MechanicActionBarSlot target = null, bool placeRight = true)
        {
            if (!DefGroup.IsValidSpellGroup(mechanic))
                return;

            Slots.Remove(f => f == mechanic || f.GetContentData() == mechanic.GetContentData());

            int placeAt = Slots.Count;
            if (target != null)
            {
                int index = Slots.IndexOf(target);
                if (index >= 0)
                    placeAt = index + (placeRight ? 1 : 0);
            }

            Slots.Insert(placeAt, mechanic);
            CalculateCache();
            DefGroup.RefreshUI();
        }
        public void RemoveFromGroup(MechanicActionBarSlot mechanic)
        {
            Slots.Remove(mechanic);
            CalculateCache();
            DefGroup.RefreshUI();
        }

        public override Color GetDecorationColor() => Color.gray;
        public override Sprite GetDecorationSprite() => DefGroup.GroupBorder;
        public override bool IsBad() => Ability == null || Slots.Count == 0;
        public override void OnClick()
        {
            base.OnClick();
        }
        public override void OnRightClick()
        {
            base.OnRightClick();
        }
        public override int GetResource()
        {
            int sum = 0;
            bool flag = true;
#if false   // slower alternative
            foreach (var slot in Slots)
            {
                slot.UpdateResourceCount();
                if (slot.ResourceCount != 0)
                {
                    var abilityData = slot.GetContentData() as AbilityData;
                    if (abilityData == null)
                        continue;
            
                    // update master and AutoUse, if necessary
                    if (flag)
                    {
                        flag = false;
                        if (!ReferenceEquals(this.Ability, abilityData))
                        {
                            if (Unit.Brain.IsAutoUseAbility(this.Ability) && abilityData.IsSuitableForAutoUse)
                                Unit.Brain.AutoUseAbility = abilityData;
                            this.Ability = abilityData;
                        }
                    }
            
                    // sum resources other than infinites
                    if (slot.ResourceCount > 0)
                    {
                        var spell = new SpellSlotLevel(abilityData);
                        if (!cacheSpellSlots.Contains(spell))
                        {
                            cacheSpellSlots.Add(spell);
                            sum += slot.ResourceCount;
                        }
                    }
                }
            }
            cacheSpellSlots.Clear();
            return sum;
#endif
            if (cacheCountIndex == null)
                CalculateCache();
            foreach (int index in cacheCountIndex)
            {
                var slot = Slots[index];
                int count = slot.GetResource();
                if (count > 0)
                    sum += count;

                // update master and AutoUse, if necessary
                if (flag && count != 0)
                {
                    flag = false;
                    var abilityData = slot.GetContentData() as AbilityData;
                    if (!ReferenceEquals(this.Ability, abilityData))
                    {
                        if (Unit.Brain.IsAutoUseAbility(this.Ability) && abilityData.IsSuitableForAutoUse)
                            Unit.Brain.AutoUseAbility = abilityData;
                        this.Ability = abilityData;
                    }
                }
            }

            return sum;
        }

        /// <returns>true if AutoUse is managed by this group</returns>
        public bool UpdateAutoUse()
        {
            if (!ReferenceEquals(this.Ability, Unit.Brain.m_AutoUseAbility))
                return false;

            UpdateResourceCount();
            return true;
        }

        //private static List<SpellSlotLevel> cacheSpellSlots = new();

        private void CalculateCache()
        {
            var list = new List<int>();
            var spells = new List<SpellSlotLevel>();

            for (int i = 0; i < Slots.Count; i++)
            {
                var abilityData = Slots[i].GetContentData() as AbilityData;
                if (abilityData == null)
                    continue;

                var spell = new SpellSlotLevel(abilityData);
                if (!spells.Contains(spell))
                {
                    spells.Add(spell);
                    list.Add(i);
                }
            }

            cacheCountIndex = list.ToArray();
        }
    }
}
