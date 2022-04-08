using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkCodex
{
    public interface ISettings
    {
        public bool NewFeatureDefaultOn { get; }

        public HashSet<string> Blacklist { get; }

        public HashSet<string> Whitelist { get; }
    }
}
