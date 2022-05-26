using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
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
    [HarmonyPatch]
    public class Patch_RulebookEventBusPriority
    {
        [HarmonyPatch(typeof(RulebookSubscribersList<RulebookEvent>), "Kingmaker.PubSubSystem.IRulebookSubscribersList.AddSubscriber")]
        [HarmonyPrefix]
        public static void Prefix(object subscriber)
        {
            Helper.PrintDebug($"SubscribersList adding {subscriber.GetType()}");
        }
    }

    // remove, instead redo IncreaseSpellDescriptorDC, IncreaseSpellContextDescriptorDC, IncreaseSpellDescriptorCasterLevel, AddOutgoingDamageBonus
    public class ChangeSpellElementalDamageFix : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulePriority
    {
        public int Priority => 400;

        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            var caster = evt.Initiator;
            evt.SetCustomData(new CustomDataKey("ChangeElement"), SpellDescriptor.Fire);
            evt.m_CustomData.FirstOrDefault(f => f.Key.m_Name == "ChangeElement");
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }
}
