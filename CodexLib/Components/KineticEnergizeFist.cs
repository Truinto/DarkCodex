using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class KineticEnergizeFist : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleAttackWithWeapon>
    {
        public BlueprintUnitFactReference Activatable;

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            var data = this.Owner.GetFact(Activatable).GetDataExt<IActionBarConvert, VariantSelectionData>();

            if (data?.Selected?.Get() is BlueprintAbility ab)
            {
                //ab.AbilityVariants.Variants[0].
            }
        }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
        }
    }
}
