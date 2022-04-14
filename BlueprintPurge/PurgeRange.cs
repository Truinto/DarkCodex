using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueprintPurge
{
    public class PurgeRange
    {
        public bool Enabled { get; set; }
        public Guid Guid { get; set; }
        public string Type { get; set; }
        public string File { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public bool IsList { get; set; }
        public string Ref { get; set; }
        public string Peek { get; set; }
        public byte[] Data;
    }
}
