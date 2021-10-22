using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    public class AddFactOnlyParty : UnitFactComponentDelegate
    {
        public override void OnActivate()
        {
            if (!(bool)Game.Instance.Player?.Party.Any())
                return;

            bool isParty = this.Owner.IsPet || this.Owner.IsMainCharacter || this.Owner.IsCustomCompanion() || this.Owner.IsStoryCompanion();
            if (!isParty || this.Owner.HasFact(this.Feature))
                return;

            Helper.PrintDebug($"Applying {Feature.NameSafe()} for {this.Owner} is IsPet={Owner.IsPet} IsMainCharacter={Owner.IsMainCharacter} IsCustomCompanion={Owner.IsCustomCompanion()} IsStoryCompanion={Owner.IsStoryCompanion()}");
            this.Owner.AddFact(this.Feature, null, this.Parameter);
        }

        public BlueprintUnitFactReference Feature;
        public FeatureParam Parameter;
    }
}
