using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public struct PoisonType
    {
        public string Name;

        public int DC;

        public StatType Stat;

        public DiceFormula Value;

        public int Bonus;

        public int Ticks;

        public int SuccesfullSaves;

        public SavingThrowType SaveType;

        public ModifierDescriptor Descriptor;

        public bool NoEffectOnFirstTick;

        public ActionList Actions;

        public PoisonType(string name, int dc, StatType stat, DiceFormula value, int bonus = 0, int ticks = 6, int succesfullSaves = 1, SavingThrowType saveType = SavingThrowType.Fortitude, ModifierDescriptor descriptor = ModifierDescriptor.UntypedStackable, bool noEffectOnFirstTick = false, ActionList actions = null)
        {
            this.Name = name;
            this.DC = dc;
            this.Stat = stat;
            this.Value = value;
            this.Bonus = bonus;
            this.Ticks = ticks;
            this.SuccesfullSaves = succesfullSaves;
            this.SaveType = saveType;
            this.Descriptor = descriptor;
            this.NoEffectOnFirstTick = noEffectOnFirstTick;
            this.Actions = actions;
        }
    }
}
