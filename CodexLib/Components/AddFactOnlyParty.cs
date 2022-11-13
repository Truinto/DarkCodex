using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class AddFactOnlyParty : UnitFactComponentDelegate, IUnitLevelUpHandler
    {
        public BlueprintUnitFactReference Feature;
        public int Level;
        public FeatureParam Parameter;

        /// <param name="feature">type: <b>BlueprintUnitFact</b></param>
        public AddFactOnlyParty(AnyRef feature, int level = 0, FeatureParam parameter = null)
        {
            this.Feature = feature;
            this.Level = level;
            this.Parameter = parameter;
        }

        public override void OnActivate()
        {
            if (!(Game.Instance.Player?.Party?.Any() ?? false) || this.Owner?.Progression == null)
                return;

            bool isParty = this.Owner.IsPet || this.Owner.IsMainCharacter || this.Owner.IsCustomCompanion() || this.Owner.IsStoryCompanion();
            if (!isParty || this.Owner.Progression.CharacterLevel < this.Level || this.Owner.HasFact(this.Feature))
                return;

            Helper.Print($"Applying {Feature.NameSafe()} for {this.Owner} is IsPet={Owner.IsPet} IsMainCharacter={Owner.IsMainCharacter} IsCustomCompanion={Owner.IsCustomCompanion()} IsStoryCompanion={Owner.IsStoryCompanion()}");
            this.Owner.AddFact(this.Feature, null, this.Parameter);
        }

        public void HandleUnitBeforeLevelUp(UnitEntityData unit)
        {
            OnActivate();
        }

        public void HandleUnitAfterLevelUp(UnitEntityData unit, LevelUpController controller)
        {
            if (unit.Progression.CharacterLevel == this.Level)
                OnActivate();
        }
    }
}
