using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic;
using Kingmaker.EntitySystem.Entities;

namespace DarkCodex.Components
{
    public class ContextActionCastSpellOnCaster : ContextAction
	{
		public BlueprintAbility Spell
		{
			get
			{
				BlueprintAbilityReference spell = this.m_Spell;
				if (spell == null)
				{
					return null;
				}
				return spell.Get();
			}
		}

		public override string GetCaption()
		{
			return string.Format("Cast spell {0} on caster", this.Spell);
		}

		public override void RunAction()
		{
			UnitEntityData caster = this.Context.MaybeCaster;
			if (caster == null)
			{
				Helper.Print("ContextActionCastSpellOnCaster Caster is missing");
				return;
			}

			if (!SkipCheck && !caster.Descriptor.HasFact(this.Spell))
            {
				//Helper.PrintDebug("ContextActionCastSpellOnCaster skipped");
                return;
            }

            AbilityData abilityData = new AbilityData(this.Spell, caster.Descriptor);
			Rulebook.Trigger(new RuleCastSpell(abilityData, caster));
			//Helper.PrintDebug("ContextActionCastSpellOnCaster casted spell");
		}

		public bool SkipCheck;

		[SerializeField]
		public BlueprintAbilityReference m_Spell;
	}
}
