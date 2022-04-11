using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueprintPurge
{
    public class PurgeRange
    {
        public bool Enabled;
        public Guid Guid;
        public string Type;
        public string File;
        public int Start;
        public int End;
        public bool NullReplace;
        public string Ref;
        public string Peek;
        public byte[] Data;
    }
}
