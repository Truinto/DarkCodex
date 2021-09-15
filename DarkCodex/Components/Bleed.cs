using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    public class UnitPartBleed : UnitPart
    {
        [JsonProperty]
        public ContextDiceValue Value;

        [JsonProperty]
        public AnyBlueprintReference Source;

    }

    [AllowedOn(typeof(BlueprintUnitFact), false)]
    public class BleedBuff : UnitBuffComponentDelegate/*<BuffDamageEachRoundData>*/, ITickEachRound, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleHealDamage>, IRulebookHandler<RuleHealDamage>
    {
        // check out BuffOnArmor, BuffDamageEachRound
        public override void OnActivate()
        {
            base.OnActivate();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
        }

        public override void OnTurnOn()
        {
            base.OnTurnOn();
        }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
        }

        public void OnNewRound()
        {
            throw new NotImplementedException();
        }

        public void OnEventAboutToTrigger(RuleHealDamage evt)
        {
            throw new NotImplementedException();
        }

        public void OnEventDidTrigger(RuleHealDamage evt)
        {
            throw new NotImplementedException();
        }
    }
}
