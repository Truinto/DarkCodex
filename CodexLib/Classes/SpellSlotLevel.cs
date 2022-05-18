using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public readonly struct SpellSlotLevel : IEquatable<SpellSlotLevel>
    {
        public readonly AbilityData AbilityData;
        public readonly BlueprintSpellbook Spellbook;
        public readonly int Level;

        public SpellSlotLevel(AbilityData abilityData)
        {
            this.AbilityData = abilityData;
            this.Spellbook = abilityData.SpellbookBlueprint;
            this.Level = abilityData.SpellLevel;
        }

        public bool Equals(SpellSlotLevel other)
        {
            if (this.Spellbook.Spontaneous
                && this.Level == other.Level
                && this.Spellbook == other.Spellbook)
                return true;

            return ReferenceEquals(this.AbilityData, other.AbilityData)
                || ReferenceEquals(this.AbilityData.m_ConvertedFrom, other.AbilityData) 
                || ReferenceEquals(this.AbilityData, other.AbilityData.m_ConvertedFrom)
                || ReferenceEquals(this.AbilityData.m_ConvertedFrom, other.AbilityData.m_ConvertedFrom) && this.AbilityData.m_ConvertedFrom != null;
        }

        public override bool Equals(object obj)
        {
            return obj is SpellSlotLevel s2
                && Equals(s2);
        }

        public override int GetHashCode()
        {
            return this.AbilityData.GetHashCode();
        }

        public static bool operator ==(SpellSlotLevel s1, SpellSlotLevel s2)
        {
            return s1.Equals(s2);
        }

        public static bool operator !=(SpellSlotLevel s1, SpellSlotLevel s2)
        {
            return !s1.Equals(s2);
        }

    }
}
