using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    public class PropertyGetterSneakAttack : PropertyValueGetter
    {
        public override int GetBaseValue(UnitEntityData unit)
        {
            int value = unit.Stats.SneakAttack.ModifiedValue;
            Helper.PrintDebug("PropertyGetterSneakAttack " + value);
            return value;
        }

        public static void CreatePropertyGetterSneakAttack()
        {
            var prop = Helper.CreateBlueprintUnitProperty(
                "SneakAttackPropertyGetter"
                ).SetComponents(new PropertyGetterSneakAttack());

            Resource.Cache.PropertySneakAttackDice.SetReference(prop);
        }
    }
}
