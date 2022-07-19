using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Adds another feature when gaining a feature. Can be a BlueprintFeatureSelection, in which case a new tab is generated.
    /// </summary>
    public class AddFeatureSelection : UnitFactComponentDelegate
    {
        public BlueprintFeatureReference Feature;

        public override void OnActivate()
        {
            var feature = this.Feature.Get();

            if (feature is BlueprintProgression progression) // this code is from AddFeatureOnApply
            {
                this.Owner.Progression.Features.AddFeature(feature, null);
                var state = (Game.Instance.LevelUpController?.State) ?? new LevelUpState(this.Owner, LevelUpState.CharBuildMode.LevelUp, false);
                if (progression.ExclusiveProgression && this.Owner.Progression.GetClassData(progression.ExclusiveProgression) != null)
                    state.SelectedClass = progression.ExclusiveProgression;
                LevelUpHelper.UpdateProgression(state, this.Owner, progression);
            }
            else if (feature is BlueprintFeatureSelection selection) // this code is from SelectFeature.Apply
            {
                var state = (Game.Instance.LevelUpController?.State) ?? new LevelUpState(this.Owner, LevelUpState.CharBuildMode.LevelUp, false);

                if (this.Fact is not Feature f)
                {
                    Helper.PrintDebug("Add selection is not Feature");
                    return;
                }
                Helper.PrintDebug($"Add selection source={f.Source} level={f.SourceLevel}");
                state.AddSelection(null, f.Source.GetValueOrDefault(), selection, f.SourceLevel); // enhancement: could set parent here
            }
            else if (feature is not null)
            {
                this.Owner.Progression.Features.AddFeature(feature, null);
            }
        }
    }
}
