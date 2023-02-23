using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Local selection for TranspilerTool delegate.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class LocalParameterAttribute : Attribute
    {
        /// <summary>Name of local.</summary>
        public string Name;
        /// <summary>Type of local. If missing, will assume by type of parameter.</summary>
        public Type Type;
        /// <summary>Index of local.</summary>
        public int Index;
        /// <summary>Index of local, filtered by exactly matching type.</summary>
        public int IndexByType;

        /// <summary>
        /// Parameter will be called with a local. By indices is only valid for original locals. Use name for self declared locals. <br/>
        /// If <b>name</b> is set, will look for self declared local or create one. <br/>
        /// If <b>index</b> is 0 or greater, the absolute index will be chosen. If the type is incompatable an exception is thrown. <br/>
        /// If <b>indexByType</b> is 0 or greater, the index in a collection of exact matching type will be chosen. <br/>
        /// If both indices are below zero, the current CodeInstruction will determine the local. 
        /// </summary>
        public LocalParameterAttribute(string name = null, Type type = null, int index = -1, int indexByType = -1)
        {
            this.Name = name;
            this.Type = type;
            this.Index = index;
            this.IndexByType = indexByType;
        }
    }
}
