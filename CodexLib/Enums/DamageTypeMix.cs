using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    [Flags]
    public enum DamageTypeMix
    {
        None = 0,

        Physical = Bludgeoning | Piercing | Slashing,
        Bludgeoning = 1 << 0,
        Piercing = 1 << 1,
        Slashing = 1 << 2,

        Energy = Fire | Cold | Sonic | Electricity | Acid | NegativeEnergy | PositiveEnergy | Holy | Unholy | Divine | Magic,
        Fire = 1 << 3,
        Cold = 1 << 4,
        Sonic = 1 << 5,
        Electricity = 1 << 6,
        Acid = 1 << 7,
        NegativeEnergy = 1 << 8,
        PositiveEnergy = 1 << 9,
        Holy = 1 << 10,
        Unholy = 1 << 11,
        Divine = 1 << 12,
        Magic = 1 << 13,

        Alignment = Good | Evil | Chaotic | Lawful,
        Good = 1 << 14,
        Evil = 1 << 15,
        Chaotic = 1 << 16,
        Lawful = 1 << 17,

        Ghost = 1 << 18,
        Force = 1 << 19,
        Direct = 1 << 20,
        Untyped = 1 << 21,

        _23 = 1 << 22,
        _24 = 1 << 23,
        _25 = 1 << 24,
        _26 = 1 << 25,
        _27 = 1 << 26,
        _28 = 1 << 27,
        _29 = 1 << 28,
        _30 = 1 << 29,
        _31 = 1 << 30,
        _32 = 1 << 31,
    }
}
