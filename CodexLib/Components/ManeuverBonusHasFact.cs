using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class ManeuverBonusHasFact : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateCMB>
    {
        public BlueprintUnitFactReference Feature;
        public int Bonus;
        public CombatManeuver Type;
        public ModifierDescriptor Descriptor;

        /// <param name="feature">type: <b>BlueprintUnitFact</b></param>
        public ManeuverBonusHasFact(AnyRef feature, int bonus, CombatManeuver type, ModifierDescriptor descriptor = ModifierDescriptor.UntypedStackable)
        {
            this.Feature = feature ?? throw new ArgumentNullException();
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
