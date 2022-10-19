using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class CacheData<T> where T: class
    {
        const int Size = 5;

        public int Pointer;
        public T[] Cache = new T[Size];
        public object[][] Arguments = new object[Size][];
        public Func<object[], T> Getter;

        public CacheData(Func<object[], T> getter)
        {
            this.Getter = getter;
        }

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
