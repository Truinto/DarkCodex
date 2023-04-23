using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Attaches <see cref="EntityPart{PartCustomData}"/> to unit and sets the <see cref="MechanicFeature"/>.<br/>
    /// To check use <see cref="PartExtensions.HasFlag(UnitEntityData, Enum)"/>.
    /// </summary>
    public class AddMechanicFeatureCustom : UnitFactComponentDelegate
    {
        /// <summary>Custom mechanic flag to set.</summary>
        public MechanicFeature Feature;

        /// <inheritdoc cref="AddMechanicFeatureCustom"/>
        public AddMechanicFeatureCustom(MechanicFeature feature)
        {
            this.Feature = feature;
        }

        /// <summary></summary>
        public override void OnTurnOn()
        {
            this.Owner.Retain(Feature);
        }

        /// <summary></summary>
        public override void OnTurnOff()
        {
            this.Owner.Release(Feature);
        }
    }
}
