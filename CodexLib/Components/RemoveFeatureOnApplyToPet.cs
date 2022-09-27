using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class RemoveFeatureOnApplyToPet : UnitFactComponentDelegate
    {
        public BlueprintUnitFactReference Feature;

        /// <param name="feature">type: <b>BlueprintUnitFact</b></param>
        public RemoveFeatureOnApplyToPet(AnyRef feature)
        {
            this.Feature = feature;
        }

        public override void OnActivate()
        {
            TryRemove();
        }

        public override void OnTurnOn()
        {
            TryRemove();
        }

        private void TryRemove()
        {
            foreach (var pet in this.Owner.Pets)
            {
                pet.Entity?.Descriptor.Progression.Features.RemoveFact(this.Feature);
            }
        }
    }
}
