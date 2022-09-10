using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public delegate bool ILFunction(object instance);
    public delegate bool ILFunctionRet(object instance, ref object result);

    public delegate void ILFunctionLocal<T>(object instance, ref T value);
}
