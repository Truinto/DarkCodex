using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface ISettings
    {
        public int Version { get; }

        public bool NewFeatureDefaultOn { get; }

        public HashSet<string> Blacklist { get; }

        public HashSet<string> Whitelist { get; }
    }
}
