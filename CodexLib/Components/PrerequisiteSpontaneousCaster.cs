using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class PrerequisiteSpontaneousCaster : Prerequisite
    {
        public override bool CheckInternal(FeatureSelectionState selectionState, UnitDescriptor unit, LevelUpState state)
        {
            foreach (ClassData classData in unit.Progression.Classes)
            {
                if (classData.Spellbook != null && classData.Spellbook.Spontaneous)
                    return true;
            }
            return false;
        }

        public override string GetUITextInternal(UnitDescriptor unit)
        {
            return TextSpontaneousCaster;
        }

        public static LocalizedString TextSpontaneousCaster = "Spontaneous Caster".CreateString();
    }
}
