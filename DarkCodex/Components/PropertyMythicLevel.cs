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

namespace DarkCodex.Components
{
    public class PropertyMythicLevel : PropertyValueGetter
    {
        public static void CreateMythicDispelProperty()
        {
            var prop = Helper.CreateBlueprintUnitProperty(
                "MythicDispelPropertyGetter"
                ).SetComponents(new PropertyMythicLevel() {
                    Fact = Helper.ToRef<BlueprintUnitFactReference>("51b6b22ff184eef46a675449e837365d"),   //SpellPenetrationMythicFeat
                    Greater = Helper.ToRef<BlueprintUnitFactReference>("1978c3f91cfbbc24b9c9b0d017f4beec") //GreaterSpellPenetration
                });

            Resource.Cache.PropertyMythicDispel.SetReference(prop);
        }

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
