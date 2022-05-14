using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class LevelUpAddSelectionHasFact : UnitFactComponentDelegate
    {
        public BlueprintUnitFact[] Facts;
        public IFeatureSelection Selection;

        public override void OnActivate()
        {
            try
            {
                var levelUp = Game.Instance.LevelUpController;
                if (levelUp == null)
                    return;
                if (levelUp.Preview != this.Owner && levelUp.Unit != this.Owner)
                    return;

                foreach (var f in Facts)
                    if (!this.Owner.HasFact(f))
                        return;

                levelUp.State.AddSelection(null, default, this.Selection, 0);
            }
            catch (Exception e)
            {
                Helper.PrintException(e);
            }
        }
    }
}
