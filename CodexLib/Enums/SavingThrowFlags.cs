using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    [Flags]
    public enum SavingThrowFlags
    {
        None = 0,

        All = Fortitude | Reflex | Will,
        Fortitude = 1 << 0,
        Reflex = 1 << 1,
        Will = 1 << 2,
    }
}
