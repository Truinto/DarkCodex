﻿using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    public class PropertyMaxAttribute : PropertyValueGetter
    {
        public static void createPropertyMaxMentalAttribute()
        {
            Resource.Cache.PropertyMaxMentalAttribute = Helper.CreateBlueprintUnitProperty(
                "MaxMentalAttributePropertyGetter"
                ).SetComponents(new PropertyMaxAttribute() { PhysicalStat = false, MentalStat = true })
                .ToRef();
        }

        public override int GetBaseValue(UnitEntityData unit)
        {
            int max = 0;
            if (this.PhysicalStat)
            {
                Math.Max(max, unit.Stats.GetStat(StatType.Strength));
                Math.Max(max, unit.Stats.GetStat(StatType.Dexterity));
                Math.Max(max, unit.Stats.GetStat(StatType.Constitution));
            }
            if (this.MentalStat)
            {
                Math.Max(max, unit.Stats.GetStat(StatType.Charisma));
                Math.Max(max, unit.Stats.GetStat(StatType.Intelligence));
                Math.Max(max, unit.Stats.GetStat(StatType.Wisdom));
            }
            return max;
        }

        public bool PhysicalStat;
        public bool MentalStat;
    }
}
