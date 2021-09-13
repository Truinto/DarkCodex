using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace DarkCodex
{
    class ManualPatchAttribute : System.Attribute
    {
        public Type declaringType;
        public string methodName;
        public MethodType methodType;

        public ManualPatchAttribute(Type declaringType, string methodName, MethodType methodType = 0)
        {
            this.declaringType = declaringType;
            this.methodName = methodName;
            this.methodType = methodType;
        }
    }
}
