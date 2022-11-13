using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodexLib
{
    /// <summary>
    /// Read-only wrapper for any type of IList. Casts items. Throws if cast not possible.
    /// </summary>
    public class AmbigiousCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
    {
        private IList _list;
        [NonSerialized] private object _syncRoot;

        public AmbigiousCollection(IList list)
        {
            this._list = list ?? new List<object>();
        }

        public T this[int index]
        {
            get => (T)_list[index];
            set => throw new Exception();
        }

        object IList.this[int index]
        { 
            get => _list[index]; 
            set => throw new Exception(); 
        }

        public int Count => _list.Count;

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    if (_list is ICollection collection)
                        _syncRoot = collection.SyncRoot;
                    else
                        Interlocked.CompareExchange<object>(ref _syncRoot, new object(), default(object));
                }

                return _syncRoot;
            }
        }

        bool ICollection.IsSynchronized => false;

        public bool IsReadOnly => true;

        public bool IsFixedSize => true;

        public int Add(object value)
        {
            throw new Exception();
        }

        public void Add(T item)
        {
            throw new Exception();
        }

        public void Clear()
        {
            throw new Exception();
        }

        public bool Contains(object value)
        {
            return _list.Contains(value);
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(Array array, int index)
        {
            _list.CopyTo(array, index);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(object value)
        {
            return _list.IndexOf(value);
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, object value)
        {
            throw new Exception();
        }

        public void Insert(int index, T item)
        {
            throw new Exception();
        }

        public void Remove(object value)
        {
            throw new Exception();
        }

        public bool Remove(T item)
        {
            throw new Exception();
        }

        public void RemoveAt(int index)
        {
            throw new Exception();
        }

        public struct Enumerator : IEnumerator<T>
        {
            private IList<T> list;

            private uint _size;

            private uint index;

            private T current;

            public T Current => current;

            object IEnumerator.Current => current;

            internal Enumerator(IList<T> list)
            {
                this.list = list;
                _size = (uint)list.Count;
                index = 0;
                current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (index < _size)
                {
                    current = list[(int)index];
                    index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                index = _size + 1;
                current = default(T);
                return false;
            }

            void IEnumerator.Reset()
            {
                index = 0;
                current = default(T);
            }
        }
    }
}
