using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Log;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodexLib
{
	[ComponentName("Predicates/Target has fact")]
	[AllowedOn(typeof(BlueprintAbility), false)]
	[AllowMultipleComponents]
	[TypeId("266a47455a4547dfb964feb6fbb260ca")]
	public class AbilityTargetHasFactExcept : BlueprintComponent, IAbilityTargetRestriction
	{
        public ReferenceArrayProxy<BlueprintUnitFact, BlueprintUnitFactReference> CheckedFacts => this.m_CheckedFacts;

        public bool IsTargetRestrictionPassed(UnitEntityData caster, TargetWrapper target)
		{
			UnitEntityData unit = target.Unit;
			if (unit == null)
				return false;

			if (caster.Descriptor.HasFact(PassIfFact))
				return true;

			foreach (BlueprintUnitFact blueprint in this.CheckedFacts)
			{
				if (unit.Descriptor.HasFact(blueprint))
					return true ^ this.Inverted;
			}
			return false ^ this.Inverted;
		}

		public string GetAbilityTargetRestrictionUIText(UnitEntityData caster, TargetWrapper target)
		{
			string facts = string.Join(", ", Enumerable.Select<BlueprintUnitFactReference, string>(this.m_CheckedFacts, delegate (BlueprintUnitFactReference i)
			{
				BlueprintUnitFact blueprintUnitFact = i.Get();
				if (blueprintUnitFact == null)
				{
					return null;
				}
				return blueprintUnitFact.Name;
			}).NotNull<string>());
			return (this.Inverted ? BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetHasNoFact : BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetHasFact).ToString(delegate ()
			{
				GameLogContext.Text = facts;
			});
		}

		[SerializeField]
		[FormerlySerializedAs("CheckedFacts")]
		public BlueprintUnitFactReference[] m_CheckedFacts;

		public BlueprintUnitFactReference PassIfFact; // passes check if unit has this fact

		public bool Inverted;
	}
}
