using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Mechanics.Properties;

namespace CodexLib
{
    public class PropertyKineticistBurn : PropertyValueGetter
    {
        public override int GetBaseValue(UnitEntityData unit)
        {
            var unitPartKineticist = unit.Get<UnitPartKineticist>();
            if (!unitPartKineticist)
                return 0;

            int num = unitPartKineticist.AcceptedBurn;

            if (ElementalEmbodiment != null && unit.HasFact(ElementalEmbodiment))
                num++;

            return num;
        }

        public static BlueprintFeature ElementalEmbodiment;
    }
}
