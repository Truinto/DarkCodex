using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.RuleSystem.RulebookEvent;

namespace CodexLib
{
    public class ChangeSpellElementalDamageFix : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IBeforeRule
    {
        public SpellDescriptor Descriptor;

        public ChangeSpellElementalDamageFix(DamageEnergyType element)
        {
            Descriptor = ChangeSpellElementalDamage.GetDamageEnergyDescriptor(element);
        }

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            evt.SetCustomData(Const.KeyChangeElement, this.Descriptor);
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }
}
