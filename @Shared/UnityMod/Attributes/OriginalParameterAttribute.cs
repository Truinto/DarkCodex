using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Original parameter selection. If you need to define a different name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class OriginalParameterAttribute : Attribute
    {
        /// <summary>Original method name this parameter should be loaded with.</summary>
        public string Name;

        /// <summary></summary>
        public OriginalParameterAttribute(string name)
        {
            this.Name = name;
        }
    }
}
