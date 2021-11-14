using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public class PatchInfoAttribute : Attribute, IComparable
    {
        public PatchInfoAttribute() { }

        public PatchInfoAttribute(Severity PatchType, string Name = null, string Description = null, bool Homebrew = true, int Priority = 400, Type Requirement = null) 
        {
            this.PatchType = PatchType;
            this.Name = Name;
            this.Description = Description;
            this.Homebrew = Homebrew;
            this.Requirement = Requirement;
            this.Priority = Priority;
        }

        public Severity PatchType;
        public string Name;
        public string Description;
        public bool Homebrew;
        public Type Requirement;
        public int Priority;    // 400 = normal, 300 = late, 500 = early, 200 = after other mods; currently only informative
        public string Class;
        public string Method;

        public bool IsWIP => (PatchType & Severity.WIP) > 0;
        public bool IsFaulty => (PatchType & Severity.Faulty) > 0;
        public bool IsDangerous => (PatchType & Severity.Create) > 0;

        public int CompareTo(object obj)
        {
            if (this == null && obj == null)
                return 0;
            if (this == null)
                return -1;
            if (obj is not PatchInfoAttribute other)
                return 1;

            int i = this.Class.CompareTo(other.Class);
            if (i == 0)
                i = ((int)this.PatchType & 0xFF) - ((int)other.PatchType & 0xFF);
            if (i == 0)
                i = this.Method.CompareTo(other.Method);
            return i;
        }
    }

    [Flags]
    public enum Severity
    {
        None = 0,
        Event = 1,      // for Events only, conflicts near impossible
        Harmony = 2,    // for HarmonyPatches only, conflicts improbable
        Fix = 3,        // low, no content
        Extend = 4,     // low, conflicts improbable
        Create = 5,     // high, permanent requirement for save

        WIP = 256,
        Faulty = 512,
    }
}
