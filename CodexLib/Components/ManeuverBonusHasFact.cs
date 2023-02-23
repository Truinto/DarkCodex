using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Grants bonus to a combat maneuver while owner has Fact.
    /// </summary>
    public class ManeuverBonusHasFact : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateCMB>
    {
        public BlueprintUnitFactReference Feature;
        public int Bonus;
        public CombatManeuver Type;
        public ModifierDescriptor Descriptor;

        /// <summary>
        /// Grants bonus to a combat maneuver while owner has Fact.
        /// </summary>
        /// <param name="feature">type: <b>BlueprintUnitFact</b></param>
        /// <param name="bonus">Maneuver bonus amount.</param>
        /// <param name="type">Type of combat maneuver.</param>
        /// <param name="descriptor">ModifierDescriptor of maneuver bonus.</param>
        public ManeuverBonusHasFact(AnyRef feature, int bonus, CombatManeuver type, ModifierDescriptor descriptor = ModifierDescriptor.UntypedStackable)
        {
            this.Feature = feature ?? throw new ArgumentNullException(nameof(feature));
            this.Bonus = bonus;
            this.Type = type;
            this.Descriptor = descriptor;
        }

        public void OnEventAboutToTrigger(RuleCalculateCMB evt)
        {
            if (evt.Type == this.Type && evt.Initiator.HasFact(this.Feature))
            {
                evt.AddModifier(this.Bonus * this.Fact.GetRank(), this.Fact, this.Descriptor);
            }
        }

        public void OnEventDidTrigger(RuleCalculateCMB evt)
        {
        }
    }
}
