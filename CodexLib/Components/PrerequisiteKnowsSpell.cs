using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class PrerequisiteKnowsSpell : Prerequisite
    {
        public BlueprintAbilityReference Spell;

        public override bool CheckInternal([CanBeNull] FeatureSelectionState selectionState, [NotNull] UnitDescriptor unit, [CanBeNull] LevelUpState state)
        {
            return unit.Spellbooks.Any(a => a.m_KnownSpells.Any(b => b.Any(c => this.Spell.Is(c.Blueprint))));
        }

        public override string GetUITextInternal(UnitDescriptor unit)
        {
            return Text + Spell.Get().Name;
        }

        public LocalizedString Text = Helper.CreateString("Can not cast ");
    }
}
