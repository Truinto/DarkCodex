using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Triggers before AbilityData consumes charges or material. May skip charge and/or material consumption.
    /// </summary>
    public class RuleSpendCharge : RulebookEvent // TODO: RuleSpendCharge
    {
        public bool ShouldSpend = true;
        public bool ShouldConsumeMaterial = true;
        public readonly AbilityData Spell;

        public RuleSpendCharge([NotNull] AbilityData abilityData) : base(abilityData.Caster.Unit)
        {
            this.Spell = abilityData;
        }

        public override void OnTrigger(RulebookEventContext context)
        {
            // patch AbilityData.Spend
            // check SpendChargesOnSpellCast, ActivatableAbilityResourceLogic.SpendResource
        }
    }

    /// <summary>
    /// Consumes spell slots in place of item charges.
    /// </summary>
    public class ConvertSpellSlots : EntityFactComponentDelegate, IInitiatorRulebookHandler<RuleSpendCharge>
    {
        public BlueprintSpellbook Spellbook;
        public BlueprintBuff Buff;

        public void OnEventAboutToTrigger(RuleSpendCharge evt)
        {
            var item = evt.Spell.SourceItem;
            var caster = evt.Initiator;
            if (item == null || item.IsSpendCharges || !caster.Buffs.HasFact(this.Buff))
                return;

            var spellbook = caster.GetSpellbook(Spellbook);
            if (spellbook == null)
                return;

            for (int i = 3; i < spellbook.m_SpontaneousSlots.Length; i++)
            {
                if (spellbook.m_SpontaneousSlots[i] > 0)
                {
                    spellbook.m_SpontaneousSlots[i]--;
                    evt.ShouldSpend = false;
                    evt.ShouldConsumeMaterial = false;
                    return;
                }
            }
        }

        public void OnEventDidTrigger(RuleSpendCharge evt)
        {
        }
    }

    /// <summary>
    /// Patch to trigger custom rule RuleSpendCharge.
    /// </summary>
    [HarmonyPatch]
    public class Patch_RuleSpendCharge
    {
        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.Spend))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var code = instructions as List<CodeInstruction> ?? instructions.ToList();
            int index = 0;

            code.AddCondition(ref index, Trigger, generator);
            code.RemoveMethods(typeof(AbilityData), nameof(AbilityData.SpendMaterialComponent));

            return code;
        }

        public static bool Trigger(object obj)
        {
            if (obj is not AbilityData __instance)
                throw new ArgumentException();

            var ruleSpend = Rulebook.Trigger(new RuleSpendCharge(__instance));

            if (ruleSpend.ShouldConsumeMaterial)
                __instance.SpendMaterialComponent();

            return ruleSpend.ShouldSpend;
        }
    }
}
