using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Shared
{
    public class PatchInfoAttribute : Attribute, IComparable<PatchInfoAttribute>, IEquatable<PatchInfoAttribute>
    {
        public PatchInfoAttribute() { }

        public PatchInfoAttribute(Severity PatchType, string DisplayName = null, string Description = null, bool Homebrew = true, int Priority = 400, Type Requirement = null)
        {
            this.PatchType = PatchType;
            this.DisplayName = DisplayName;
            this.Description = Description;
            this.Homebrew = Homebrew;
            this.Requirement = Requirement;
            this.Priority = Priority;
        }

        public Severity PatchType;
        public LocalizedStringCached DisplayName;
        public LocalizedStringCached Description;
        public bool Homebrew;
        public Type Requirement;
        public int Priority;    // 400 = normal, 300 = late, 500 = early, 200 = after other mods; currently only informative
        public string Class;
        public string Method;
        public int Hash;
        public bool Disabled;
        public bool DisabledAll;

        public bool IsWIP => (PatchType & Severity.WIP) != 0;
        public bool IsFaulty => (PatchType & Severity.Faulty) != 0;
        public bool IsDangerous => (PatchType & Severity.Create) != 0;
        public bool IsHarmony => (PatchType & Severity.Harmony) != 0;
        public bool IsEvent => (PatchType & Severity.Event) != 0;
        public bool IsHidden => (PatchType & Severity.Hidden) != 0;
        public bool IsDefaultOff => (PatchType & Severity.DefaultOff) != 0;

        public string FullName => Class + "." + Method;
        public string HomebrewStr => Homebrew ? ":house:" : ":book:";
        public string StatusStr => IsFaulty ? ":x:" : IsWIP ? ":construction:" : ":heavy_check_mark:";

        public int CompareTo(PatchInfoAttribute other)
        {
            // put last: Patch, Event
            int i = 0;
            if (this.Class == "Patch")
                i += 100;
            if (other.Class == "Patch")
                i -= 100;

            // sort by class name, then patch severity, then method name
            if (i == 0)
                i = this.Class.CompareTo(other.Class);
            if (i == 0)
                i = ((int)other.PatchType & 0xFF) - ((int)this.PatchType & 0xFF);
            if (i == 0)
                i = this.Method.CompareTo(other.Method);
            return i;
        }

        public bool Equals(PatchInfoAttribute other)
        {
            return this.Method == other.Method && this.Class == other.Class;
        }

        public override int GetHashCode()
        {
            return this.Hash;
        }
    }

    [Flags]
    public enum Severity
    {
        None = 0,
        Event = 1,      // for Events only, conflicts near impossible
        Harmony = 2,    // for HarmonyPatches only, conflicts improbable
        Fix = 4,        // low, no content
        Extend = 8,     // low, conflicts improbable
        Create = 16,    // high, permanent requirement for save

        WIP = 256,
        Faulty = 512,
        Hidden = 1024,
        DefaultOff = 2048,
    }

    public class PatchInfoCollection : IEnumerable<PatchInfoAttribute>
    {
        private List<PatchInfoAttribute> list = new();
        private ISettings state;

        public PatchInfoCollection(ISettings state)
        {
            this.state = state;
        }

        public void Add(PatchInfoAttribute attr, MemberInfo info)
        {
            attr.Class = info.DeclaringType?.Name ?? "Patch";
            attr.Method = info.Name;
            attr.Hash = (attr.Class + "." + attr.Method).GetHashCode();

            if (!list.Contains(attr))
                list.Add(attr);
        }

        public void SetEnable(bool value, string category, bool force = false)
        {
            if (category.Last() == '*')
            {
                if (value)
                    state.Blacklist.Remove(category);
                else
                    state.Blacklist.Add(category);
            }
            else
            {
                if (value)
                {
                    state.Whitelist.Add(category);
                    state.Blacklist.Remove(category);
                    if (force)
                        state.Blacklist.Remove(TrySubstring(category, '.') + ".*");
                }
                else
                {
                    state.Whitelist.Remove(category);
                    state.Blacklist.Add(category);
                }
            }
            Update();
        }

        public void SetEnable(bool value, PatchInfoAttribute attr, bool force = false)
        {
            attr.Disabled = !value;

            string fullName = attr.FullName;
            if (value)
            {
                state.Blacklist.Remove(fullName);
                state.Whitelist.Add(fullName);
                if (force)
                {
                    state.Blacklist.Remove(TrySubstring(fullName, '.') + ".*");
                    Update();
                }
            }
            else
            {
                state.Whitelist.Remove(fullName);
                state.Blacklist.Add(fullName);
            }
        }

        public void Update()
        {
            string category = null;
            bool disableAll = false;

            foreach (var info in list)
            {
                if (info.Class != category)
                {
                    category = info.Class;
                    disableAll = IsDisenabledCategory(category + ".*");
                }
                info.DisabledAll = disableAll;
                info.Disabled = IsDisenabledSingle(info.Class + "." + info.Method);
            }
        }

        private bool IsDisenabledCategory(string name)
        {
            if (state.Blacklist.Contains(name))
                return true;
            return false;
        }

        private bool IsDisenabledSingle(string name)
        {
            if (state.Blacklist.Contains(name))
                return true;

            if (state.Whitelist.Contains(name))
                return false;
#if !DEBUG
            int hash = name.GetHashCode();
            if (list.Find(f => f.Hash == hash)?.IsDefaultOff == true)
                return true;
#endif
            return !state.NewFeatureDefaultOn;
        }

        public bool IsDisenabled(string name)
        {
            if (state.Blacklist.Contains(name)) // blacklist has priority
                return true;

            if (name.Last() == '*') // categories are always enabled unless they are on the blacklist
                return false;

            if (state.Blacklist.Contains(TrySubstring(name, '.') + ".*")) // check if disabled by category
                return true;

            if (state.Whitelist.Contains(name)) // check whitelist
                return false;
#if !DEBUG
            int hash = name.GetHashCode();
            if (list.Find(f => f.Hash == hash)?.IsDefaultOff == true) // check default off flag
                return true;                
#endif
            return !state.NewFeatureDefaultOn;  // if setting was never set, check for generic default
        }

        public bool IsEnabled(string name)
        {
            return !IsDisenabled(name);
        }

        public IEnumerator<PatchInfoAttribute> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void Sort()
        {
            list.Sort();
        }

        public IEnumerable<string> GetCriticalPatches()
        {
            return list.Where(w => w.IsDangerous && !w.Disabled && !w.DisabledAll).Select(s => s.FullName);
        }

        public List<string> IsEnabledAll(List<string> source)
        {
            for (int i = source.Count - 1; i >= 0; i--)
            {
                //bool critical = this.list.Find(f => f.FullName == source[i])?.IsDangerous ?? false;
                if (IsEnabled(source[i]))
                    source.RemoveAt(i);
            }
            return source;
        }

        private static string TrySubstring(string str, char c, int start = 0, bool tail = false)
        {
            try
            {
                if (tail)
                {
                    if (start < 0)
                        return str.Substring(str.LastIndexOf(c) + 1);
                    return str.Substring(str.IndexOf(c, start) + 1);
                }

                if (start < 0)
                    return str.Substring(0, str.LastIndexOf(c));
                return str.Substring(start, str.IndexOf(c, start));
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
