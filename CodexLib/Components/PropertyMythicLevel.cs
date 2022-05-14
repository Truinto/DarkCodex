using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class PropertyMythicLevel : PropertyValueGetter
    {
        public override int GetBaseValue(UnitEntityData unit)
        {
            if (!unit.HasFact(Fact))
                return 0;

            if (Greater == null || unit.HasFact(Greater))
                return unit.Progression.MythicLevel;
            else
                return unit.Progression.MythicLevel / 2;
        }

        [NotNull]
        public BlueprintUnitFactReference Fact;
        [CanBeNull]
        public BlueprintUnitFactReference Greater;
    }
}
