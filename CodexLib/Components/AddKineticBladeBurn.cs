using Kingmaker.UnitLogic.Class.Kineticist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Adds equipped kinetic blade blast burn to the current burn cost.
    /// </summary>
    public class AddKineticBladeBurn : UnitFactComponentDelegate, IKineticistCalculateAbilityCostHandler
    {
        /// <inheritdoc cref="AddKineticBladeBurn"/>
        public AddKineticBladeBurn()
        {
        }

        public void HandleKineticistCalculateAbilityCost(UnitDescriptor caster, BlueprintAbility abilityBlueprint, ref KineticistAbilityBurnCost cost)
        {
            int burn = caster?.Body?.PrimaryHand?.MaybeWeapon?.Blueprint?.GetComponent<WeaponKineticBlade>()?.ActivationAbility?.AbilityKineticist?.BlastBurnCost ?? 0;
            cost.Increase(burn, KineticistBurnType.Blast);
        }
    }
}
