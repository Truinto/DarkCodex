using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Cache for slow operations. Will return known solution to specified arguments. Otherwise calls getter and remembers solution in cache.
    /// </summary>
    public class CacheData<T> where T : class
    {
        private readonly int Size;
        private int Pointer;
        private readonly T[] Cache;
        private readonly object[][] Arguments;
        private readonly Func<object[], T> Getter;

        /// <summary>
        /// Cache for slow operations. Will return known solution to specified arguments. Otherwise calls getter and remembers solution in cache.
        /// </summary>
        /// <param name="getter">Function to resolve unknown solution.</param>
        /// <param name="size">Cache size.</param>
        public CacheData(Func<object[], T> getter, int size = 5)
        {
            this.Size = size;
            this.Cache = new T[size];
            this.Arguments = new object[size][];
            this.Getter = getter;
        }

        /// <summary>
        /// Get solution for specific argument collection. Calls getter, if solution not in cache.
        /// </summary>
        public T Get(params object[] args)
        {
            for (int i = 0; i < Size; i++)
            {
                if (Arguments[i]?.SequenceEqual(args) == true)
                    return Cache[i];
            }

            if (Pointer >= Size)
                Pointer = 0;
            Arguments[Pointer] = args;
            return Cache[Pointer++] = Getter.Invoke(args);
        }
    }
}
