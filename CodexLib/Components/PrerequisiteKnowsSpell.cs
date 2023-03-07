using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.UI;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Check spell is known in any spellbook.
    /// </summary>
    public class PrerequisiteKnowsSpell : Prerequisite
    {
        /// <summary>type: <b>BlueprintAbility</b></summary>
        public AnyRef Spell;

        /// <inheritdoc cref="PrerequisiteKnowsSpell"/>
        /// <param name="spell">type: <b>BlueprintAbility</b></param>
        /// <param name="any">Prerequisite.GroupType.Any or .All</param>
        public PrerequisiteKnowsSpell(AnyRef spell, bool any = false)
        {
            this.Spell = spell;
            this.Group = any ? GroupType.Any : GroupType.All;
        }

        /// <inheritdoc cref="PrerequisiteKnowsSpell"/>
        public override bool CheckInternal([CanBeNull] FeatureSelectionState selectionState, [NotNull] UnitDescriptor unit, [CanBeNull] LevelUpState state)
        {
            return unit.Spellbooks.Any(a => a.m_KnownSpells.Any(b => b.Any(c => this.Spell.Is(c.Blueprint))));
        }

        /// <summary>
        /// UI text: "Able to cast {spell}"
        /// </summary>
        public override string GetUITextInternal(UnitDescriptor unit)
        {
            return Text + Spell.Get<IUIDataProvider>()?.Name;
        }

        /// <summary>UI text: "Able to cast "</summary>
        public static LocalizedString Text = Helper.CreateString("Able to cast ");
    }
}
