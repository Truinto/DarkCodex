using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.Kineticist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class AutoMetakinesis : UnitFactComponentDelegate, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>
    {
        private static readonly BlueprintFeatureReference _master_maximize = Helper.ToRef<BlueprintFeatureReference>("5cda9f923cb35ea4a957b0e899420ec5");

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            var ability = (evt.Blueprint as BlueprintAbility)?.GetComponent<AbilityKineticist>();
            if (ability == null) return;

            var burn = ability.CalculateBurnCost(evt.AbilityData);
            int pool = burn.GatherPower;
            KineticistAbilityBurnCost.DecreaseWithPool(burn.BlastBase + burn.BlastIncrease - burn.BlastDecrease, ref pool);
            KineticistAbilityBurnCost.DecreaseWithPool(burn.InfusionBase + burn.InfusionIncrease - burn.InfusionDecrease, ref pool);
            KineticistAbilityBurnCost.DecreaseWithPool(burn.MetakinesisBase + burn.MetakinesisIncrease - burn.MetakinesisDecrease, ref pool);

            int costmax = evt.Initiator.Descriptor.HasFact(_master_maximize) ? 1 : 2;

            if (pool >= costmax && !evt.HasMetamagic(Metamagic.Maximize))
            {
                evt.AddMetamagic(Metamagic.Maximize);
                pool -= costmax;
            }

            if (pool >= 1)
            {
                evt.AddMetamagic(Metamagic.Empower);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }
}
