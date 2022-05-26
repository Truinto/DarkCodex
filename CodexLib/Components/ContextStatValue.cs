using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ContextStatValue : ContextValue
    {
        public StatType Stat;
        public ModifierDescriptor SpecificModifier; // Use \"None\" value for bonus from all modifiers.
        public bool GetRawValue;

        public new int Calculate(MechanicsContext context, BlueprintScriptableObject blueprint = null, UnitEntityData caster = null)
        {
            if (caster == null)
                caster = context.MaybeCaster;
            var stat = caster.Descriptor.Stats.GetStat(this.Stat);

            if (this.SpecificModifier != ModifierDescriptor.None)
                return stat.GetDescriptorBonus(this.SpecificModifier);
            else if (!GetRawValue && stat is ModifiableValueAttributeStat attributeStat)
                return attributeStat.Bonus;
            else
                return stat.ModifiedValue;
        }

        public static implicit operator ContextStatValue(StatType stat)
        {
            return new ContextStatValue
            {
                Stat = stat
            };
        }
    }
}
