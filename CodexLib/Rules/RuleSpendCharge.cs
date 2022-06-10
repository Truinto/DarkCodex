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
    public class RuleSpendCharge : RulebookEvent
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
}
