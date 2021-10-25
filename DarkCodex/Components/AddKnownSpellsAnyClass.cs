using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex.Components
{
    public class AddKnownSpellsAnyClass : UnitFactComponentDelegate, ILevelUpCompleteUIHandler, IGlobalSubscriber, ISubscriber
    {
        public override void OnActivate()
        {
            AddSpells();
        }

        private void AddSpells()
        {
            if (Spells == null || Levels == null || Spells.Length != Levels.Length)
            {
                Helper.Print("AddKnownSpellAnyClass invalid Blueprint");
            }

            for (int i = 0; i < Spells.Length; i++)
            {
                int level = Levels[i];
                var spell = Spells[i].Get();

                foreach (var spellbook in this.Owner.Spellbooks)
                {
                    if (spellbook.MaxSpellLevel >= level && !spellbook.GetKnownSpells(level).Any(p => p.Blueprint == spell))
                        spellbook.AddKnown(level, spell);
                }
            }
        }

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {
            AddSpells();
        }

        public BlueprintAbilityReference[] Spells;
        public int[] Levels;
    }
}
