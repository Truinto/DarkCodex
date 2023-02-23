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

        /// <inheritdoc cref="AmbigiousCollection{T}"/>
        public AmbigiousCollection(IList list)
        {
            this._list = list ?? new List<object>();
        }

        /// <inheritdoc/>
        public T this[int index]
        {
            get => (T)_list[index];
            set => throw new InvalidCastException();
        }

        object IList.this[int index]
        { 
            get => _list[index]; 
            set => throw new InvalidCastException(); 
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool IsReadOnly => true;

        /// <inheritdoc/>
        public bool IsFixedSize => true;

        /// <inheritdoc/>
        public int Add(object value)
        {
            throw new Exception();
        }

        /// <inheritdoc/>
        public void Add(T item)
        {
            throw new Exception();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            throw new Exception();
        }

        /// <inheritdoc/>
        public bool Contains(object value)
        {
            return _list.Contains(value);
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(Array array, int index)
        {
            _list.CopyTo(array, index);
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <inheritdoc/>
        public int IndexOf(object value)
        {
            return _list.IndexOf(value);
        }

        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        /// <inheritdoc/>
        public void Insert(int index, object value)
        {
            throw new Exception();
        }

        /// <inheritdoc/>
        public void Insert(int index, T item)
        {
            throw new Exception();
        }

        /// <inheritdoc/>
        public void Remove(object value)
        {
            throw new Exception();
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            throw new Exception();
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            throw new Exception();
        }

        /// <inheritdoc/>
        public struct Enumerator : IEnumerator<T>
        {
            private IList<T> list;

            private uint _size;

            private uint index;

            private T current;

            /// <inheritdoc/>
            public T Current => current;

            object IEnumerator.Current => current;

            internal Enumerator(IList<T> list)
            {
                this.list = list;
                _size = (uint)list.Count;
                index = 0;
                current = default(T);
            }

            /// <inheritdoc/>
            public void Dispose()
            {
            }

            /// <inheritdoc/>
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
