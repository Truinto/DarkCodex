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

        public const SpellDescriptor AnyElement = SpellDescriptor.Fire | SpellDescriptor.Acid | SpellDescriptor.Cold | SpellDescriptor.Electricity | SpellDescriptor.Sonic;

        public ChangeSpellElementalDamageFix(DamageEnergyType element)
        {
            Descriptor = ChangeSpellElementalDamage.ElementToSpellDescriptor(element);
        }

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            var context = this.Context;
            context.SpellDescriptor |= evt.Spell.SpellDescriptor;

            if (this.Context.SpellDescriptor.HasAnyFlag(AnyElement))
            {
                this.Context.RemoveSpellDescriptor(AnyElement);
                this.Context.AddSpellDescriptor(Descriptor);
            }

            //var caster = evt.Initiator;
            //evt.SetCustomData(new CustomDataKey("ChangeElement"), SpellDescriptor.Fire);
            //evt.m_CustomData.FirstOrDefault(f => f.Key.m_Name == "ChangeElement");

            //var spellDescriptor = evt.Spell.GetComponent<SpellDescriptorComponent>().Descriptor;
            //if (spellDescriptor.HasAnyFlag(AnyElement) && !spellDescriptor.HasAnyFlag(Descriptor))
            //{
            //    if (FakeSpell == null)
            //        FakeSpell = new BlueprintAbility() { name = "FakeSpell" };
            //    ((SpellDescriptorComponent)FakeSpell.Components[0]).Descriptor = Descriptor;
            //
            //    var ruleParams = new RuleCalculateAbilityParams(this.Context.MaybeCaster, FakeSpell, evt.Spellbook);
            //    this.Context.TriggerRule(ruleParams);
            //
            //    var param = ruleParams.Result;
            //}

            //var changeElement = Helper.GetISubscriber<ChangeSpellElementalDamage, RuleCastSpell>(evt.Initiator);
            //if (changeElement != null)
            //{
            //    ChangeSpellElementalDamage.ElementToSpellDescriptor(changeElement.Element);
            //}

            //Helper.PrintDebug(Helper.GetIInitiator<RuleCalculateAbilityParams>(evt.Initiator).Join(f => f.name));
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }
}
