using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurnBased.Controllers;

namespace DarkCodex.Components
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class TurnBasedBuffMovementSpeed : UnitFactComponentDelegate
	{
		public TurnBasedBuffMovementSpeed(float multiplier = 1f)
        {
			this.Multiplier = multiplier;
        }

		public override void OnTurnOn()
		{
			if (CombatController.IsInTurnBasedCombat() ^ InTurnBasedCombat)
				return;

            int newSpeed = (int)(this.Owner.Stats.Speed.BaseValue * this.Multiplier + this.Bonus);
			newSpeed = newSpeed.MinMax(this.Min, this.Max);

			int bonus = newSpeed - this.Owner.Stats.Speed.BaseValue;

			this.Owner.Stats.Speed.AddModifierUnique(bonus, this.Runtime, this.Descriptor);
		}

		public override void OnTurnOff()
		{
			base.Owner.Stats.Speed.RemoveModifiersFrom(this.Runtime);
		}

		public bool InTurnBasedCombat;
		public float Multiplier;
		public int Bonus;
		public int Min = 0;
		public int Max = 60;

		public ModifierDescriptor Descriptor = ModifierDescriptor.Enhancement;
	}
}
