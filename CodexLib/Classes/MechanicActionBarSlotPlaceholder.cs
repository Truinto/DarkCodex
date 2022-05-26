using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
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
    /// <summary>
    /// Logic to display not available ability
    /// </summary>
    public class MechanicActionBarSlotPlaceholder : MechanicActionBarSlot
    {
        [JsonProperty]
        public BlueprintUnitFactReference Blueprint;

        [JsonConstructor]
        public MechanicActionBarSlotPlaceholder(EntityRef<UnitEntityData> m_UnitRef, BlueprintUnitFactReference blueprint)
        {
            this.m_UnitRef = m_UnitRef;
            this.Blueprint = blueprint;
        }

        public MechanicActionBarSlotPlaceholder(UnitEntityData unit, BlueprintUnitFactReference blueprint)
        {
            this.Unit = unit;
            this.Blueprint = blueprint ?? new();
        }

        public override bool CanUseIfTurnBasedInternal() => false;
        public override object GetContentData() => null;
        public override Color GetDecorationColor() => Color.white;
        public override Sprite GetDecorationSprite() => null;
        public override string GetTitle() => Blueprint.Get()?.Name;
        public override string GetDescription() => Blueprint.Get()?.Description;
        public override Sprite GetIcon() => Blueprint.Get()?.Icon;
        public override int GetResource() => -1;
        public override bool IsCasting() => false;
        public override bool IsDisabled(int resourceCount) => true;
        public override bool IsBad() => !DefGroup.Unlocked;
    }
}
