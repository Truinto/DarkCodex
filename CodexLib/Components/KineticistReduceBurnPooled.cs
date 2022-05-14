using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodexLib
{
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    public class KineticistReduceBurnPooled : UnitFactComponentDelegate, IGlobalSubscriber, ISubscriber, IKineticistCalculateAbilityCostHandler
    {
        public void HandleKineticistCalculateAbilityCost(UnitDescriptor caster, BlueprintAbility abilityBlueprint, ref KineticistAbilityBurnCost cost)
        {
            if (caster != this.Owner)
                return;
            int value = this.ReduceBurn.Calculate(this.Context);
            cost.IncreaseGatherPower(value);
        }

        public ContextValue ReduceBurn;
    }
}
