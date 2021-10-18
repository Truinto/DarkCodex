using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    [AllowedOn(typeof(BlueprintParametrizedFeature), false)]
    public class PreferredSpell : UnitFactComponentDelegate, IUnitReapplyFeaturesOnLevelUpHandler, ISpellBookCustomSpell, IUnitSubscriber, ISubscriber
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

                var array = new BlueprintAbility[10];
                for (int i = min; i < array.Length; i++)
                    array[i] = spell;

                spellbook.AddSpellConversionList("PreferredSpell#" + spell.name, array);

                if (spellbook.m_CustomSpells == null)
                    continue;

                List<AbilityData> metamagics = new List<AbilityData>();
                foreach (var customSL in spellbook.m_CustomSpells)
                    foreach (var custom in customSL)
                        if (custom.Blueprint == spell)
                            metamagics.Add(custom);
            }
        }

        public override void OnTurnOff()
        {
            var spell = this.Param.Blueprint as BlueprintAbility;
            foreach (var spellbook in this.Owner.Spellbooks)
                spellbook.RemoveSpellConversionList("PreferredSpell#" + spell.name);
        }

        public void HandleUnitReapplyFeaturesOnLevelUp()
        {
            OnTurnOff();
            OnTurnOn();
        }

        public void AddSpellHandler(AbilityData ability)
        {
            Helper.PrintDebug("PreferredSpell AddSpellHandler");
            OnTurnOff();
            OnTurnOn();
        }

        public void RemoveSpellHandler(AbilityData ability)
        {
            OnTurnOff();
            OnTurnOn();
        }
    }
}
