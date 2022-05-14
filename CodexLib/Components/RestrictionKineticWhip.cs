using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Class.Kineticist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class RestrictionKineticWhip : ActivatableAbilityRestriction
    {
        public override bool IsAvailable()
        {
            return this.Owner.Body.PrimaryHand.MaybeWeapon?.Blueprint.GetComponent<WeaponKineticBlade>() != null;
        }
    }
}
