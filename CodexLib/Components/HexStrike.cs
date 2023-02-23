using Kingmaker.UI.UnitSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class HexStrike : ActivatableVariants, IInitiatorRulebookHandler<RuleAttackWithWeapon>
    {
        public HexStrike(params AnyRef[] facts) : base(facts)
        {
        }

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            if (!this.IsOn || this.Data.Selected is not BlueprintAbility)
                return;

            //evt.IsFirstAttack
        }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
        }
    }
}
