using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class PropertyMindShield : PropertyValueGetter
    {
        public override int GetBaseValue(UnitEntityData unit)
        {
			UnitPartKineticist unitPartKineticist = unit.Get<UnitPartKineticist>();
			if (!unitPartKineticist)
				return 0;

			int num = unitPartKineticist.AcceptedBurn;

			if (!unit.Descriptor.HasFact(Feature))
				num *= 2;

			return num;
		}

		public BlueprintFeature Feature;
    }
}
