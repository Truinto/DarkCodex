using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ModifyWeaponSize : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleCalculateWeaponStats>
    {
        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt.Weapon == base.Owner)
            {
                evt.IncreaseWeaponSize(SizeCategoryChange);
            }
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
        }

        public int SizeCategoryChange;
    }
}
