using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// This is an example on how to get the variant data from a buff applied from activatable ability.
    /// </summary>
    [AllowedOn(typeof(BlueprintBuff))]
    public class ActivatableVariantsExample : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleAttackWithWeapon>
    {
        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            if (this.Fact is not Buff buff)
                return;

            if (buff.m_Context.AssociatedBlueprint is not BlueprintUnitFact parent)
                return;

            var data = this.Owner.GetFact(parent).GetDataExt<IActionBarConvert, VariantSelectionData>();

            if (data?.Selected is not BlueprintAbility selection)
                return;

            // stuff here
        }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
        }
    }
}
