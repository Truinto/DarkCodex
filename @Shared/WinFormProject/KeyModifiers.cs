using System;

namespace Shared
{
    [Flags]
    public enum KeyModifiers : Int16
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        /// <summary>reserved by WINDOWS</summary>
        Windows = 8
    }
}