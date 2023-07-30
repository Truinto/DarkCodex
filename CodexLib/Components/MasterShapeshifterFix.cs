using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Ensures Master Shapeshifter gets its bonuses, even when the spell has no <see cref="Polymorph"/> component (non-visual).
    /// </summary>
    public class MasterShapeshifterFix : UnitFactComponentDelegate<MasterShapeshifterFix.RuntimeData>, IUnitBuffHandler
    {
        public void HandleBuffDidAdded(Buff buff)
        {
            var context = buff.MaybeContext;
            if (context == null)
                return;

            if (context.MaybeCaster != this.Owner)
                return;

            if (buff.Blueprint == null || buff.Blueprint.GetComponent<Polymorph>() != null)
                return;

            if (!buff.m_Context.SpellDescriptor.HasAnyFlag(SpellDescriptor.Polymorph))
                return;

            this.Data.Clear();
            this.Data.SourceBuff = buff;
            this.Data.AppliedModifiers = new()
            {
                this.Owner.Stats.GetStat(StatType.Strength).AddModifier(4, this.Runtime, ModifierDescriptor.MasterShapeshifter),
                this.Owner.Stats.GetStat(StatType.Dexterity).AddModifier(4, this.Runtime, ModifierDescriptor.MasterShapeshifter),
                this.Owner.Stats.GetStat(StatType.Constitution).AddModifier(4, this.Runtime, ModifierDescriptor.MasterShapeshifter)
            };
        }

        public void HandleBuffDidRemoved(Buff buff)
        {
            if (buff != this.Data.SourceBuff)
                return;

            this.Data.Clear();
        }

        public class RuntimeData
        {
            public void Clear()
            {
                SourceBuff = null;
                if (AppliedModifiers != null)
                {
                    foreach (var mod in AppliedModifiers)
                        mod.Remove();
                    AppliedModifiers = null;
                }
            }

            [JsonProperty]
            public Buff SourceBuff;
            [JsonProperty]
            public List<ModifiableValue.Modifier> AppliedModifiers;
        }
    }
}
