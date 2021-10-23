using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    [AllowedOn(typeof(BlueprintParametrizedFeature), false)]
    public class PreferredSpell : UnitFactComponentDelegate, IUnitReapplyFeaturesOnLevelUpHandler, IUnitSubscriber, ISubscriber //ISpellBookCustomSpell
    {
        public override void OnTurnOn()
        {
            var spell = this.Param.Blueprint as BlueprintAbility;
            foreach (var spellbook in this.Owner.Spellbooks)
            {
                if (spellbook.Blueprint.Spontaneous)
                    continue;

                int min = 11;
                if (spellbook.m_KnownSpellLevels.TryGetValue(spell, out List<int> levels))
                    foreach (var level in levels)
                        min = Math.Min(min, level);
                if (min > 10)
                    continue;

                var variants = spell.GetComponent<AbilityVariants>();

                if (variants == null)
                    AddSpell(spellbook, min, spell);
                else
                    foreach (var variant in variants.Variants)
                        AddSpell(spellbook, min, variant);
            }
        }

        public void AddSpell(Spellbook spellbook, int level, BlueprintAbility spell)
        {
            var array = new BlueprintAbility[11];
            for (int i = level; i < array.Length; i++)
                array[i] = spell;

            spellbook.AddSpellConversionList("PreferredSpell#" + spell.name, array);
        }

        public override void OnTurnOff()
        {
            var spell = this.Param.Blueprint as BlueprintAbility;
            foreach (var spellbook in this.Owner.Spellbooks)
            {
                var variants = spell.GetComponent<AbilityVariants>();
                if (variants == null)
                    spellbook.RemoveSpellConversionList("PreferredSpell#" + spell.name);
                else
                    foreach (var variant in variants.Variants)
                        spellbook.RemoveSpellConversionList("PreferredSpell#" + variant.name);
            }
        }

        public void HandleUnitReapplyFeaturesOnLevelUp()
        {
            OnTurnOff();
            OnTurnOn();
        }
    }
}
