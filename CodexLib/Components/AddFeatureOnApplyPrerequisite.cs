using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class AddFeatureOnApplyPrerequisite : UnitFactComponentDelegate
    {
        public int Amount;
        public BlueprintFeatureReference[] Prerequisites;
        public BlueprintFeatureReference Feature;

        /// <summary>
        /// Same as <see cref="AddFeatureOnApply"/>, but prerequisites are required.
        /// </summary>
        /// <param name="amount">Count of prerequisites that must be fulfilled.</param>
        /// <param name="prerequisites">type: <b>BlueprintFeatureReference</b></param>
        /// <param name="feature">type: <b>BlueprintFeatureReference</b></param>
        public AddFeatureOnApplyPrerequisite(int amount, AnyRef feature, params AnyRef[] prerequisites)
        {
            this.Amount = amount;
            this.Prerequisites = prerequisites.ToRef<BlueprintFeatureReference>();
            this.Feature = feature;
        }

        public override void OnActivate() // see AddFeatureSelection
        {
            int amount = 0;
            foreach (var preq in this.Prerequisites)
            {
                if (this.Owner.HasFact(preq))
                    amount++;
            }

            if (amount < this.Amount)
                return;

            var feature = this.Feature.Get();

            if (feature is BlueprintProgression progression) // this code is from AddFeatureOnApply
            {
                this.Owner.Progression.Features.AddFeature(progression, null);
                var state = (Game.Instance.LevelUpController?.State) ?? new LevelUpState(this.Owner, LevelUpState.CharBuildMode.LevelUp, false);
                if (progression.ExclusiveProgression && this.Owner.Progression.GetClassData(progression.ExclusiveProgression) != null)
                    state.SelectedClass = progression.ExclusiveProgression;
                LevelUpHelper.UpdateProgression(state, this.Owner, progression);
            }
            else if (feature is BlueprintFeatureSelection selection) // this code is from SelectFeature.Apply
            {
                if (this.Fact is not Feature f)
                    return;

                var state = (Game.Instance.LevelUpController?.State) ?? new LevelUpState(this.Owner, LevelUpState.CharBuildMode.LevelUp, false);
                state.AddSelection(null, f.Source.GetValueOrDefault(), selection, f.SourceLevel);
            }
            else if (feature is not null)
            {
                this.Owner.Progression.Features.AddFeature(feature, null);
            }
        }
    }
}
