using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Shared
{
    public static class CollectionHelper
    {
        public static T[] ObjToArray<T>(this T obj)
        {
            if (obj == null) return null;
            return new T[] { obj };
        }

        public static T[] ToArray<T>(params T[] objs)
        {
            return objs;
        }

        public static List<T> AsList<T>(this IEnumerable<T> values)
        {
            return values as List<T> ?? values.ToList();
        }

        /// <summary>Appends objects on array.</summary>
        public static T[] Append<T>(this T[] orig, params T[] objs)
        {
            orig ??= Array.Empty<T>();
            objs ??= Array.Empty<T>();

            int i, j;
            T[] result = new T[orig.Length + objs.Length];
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            for (j = 0; i < result.Length; i++)
                result[i] = objs[j++];
            return result;
        }

        public static List<T> Append<T>(this List<T> orig, List<T> objs)
        {
            var result = new List<T>(orig);
            result.AddRange(objs);
            return result;
        }

        /// <summary>Appends objects on array and overwrites the original.</summary>
        public static void AppendAndReplace<T>(ref T[] orig, params T[] objs)
        {
            orig ??= Array.Empty<T>();
            objs ??= Array.Empty<T>();

            int i, j;
            T[] result = new T[orig.Length + objs.Length];
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            for (j = 0; i < result.Length; i++)
                result[i] = objs[j++];
            orig = result;
        }

        public static void AppendAndReplace<T>(ref T[] orig, List<T> objs)
        {
            orig ??= Array.Empty<T>();

            T[] result = new T[orig.Length + objs.Count];
            int i;
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            foreach (var obj in objs)
                result[i++] = obj;
            orig = result;
        }

        public static void AppendAndReplace<T>(ref T[] orig, IEnumerable<T> objs)
        {
            orig ??= Array.Empty<T>();

            T[] result = new T[orig.Length + objs.Count()];
            int i;
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            foreach (var obj in objs)
                result[i++] = obj;
            orig = result;
        }

        public static void InsertAt<T>(ref T[] orig, T obj, int index = -1)
        {
            orig ??= Array.Empty<T>();
            if (index < 0 || index > orig.Length) index = orig.Length;

            T[] result = new T[orig.Length + 1];
            for (int i = 0, j = 0; i < result.Length; i++)
            {
                if (i == index)
                    result[i] = obj;
                else
                    result[i] = orig[j++];
            }
            orig = result;
        }

        public static void RemoveAt<T>(ref T[] orig, int index)
        {
            orig ??= Array.Empty<T>();
            if (index < 0 || index >= orig.Length) return;

            T[] result = new T[orig.Length - 1];
            for (int i = 0, j = 0; i < result.Length; i++)
            {
                if (i != index)
                    result[i] = orig[j++];
            }
            orig = result;
        }

        public static void RemoveGet<T>(this List<T> list, List<T> result, Func<T, bool> predicate)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    result.Add(list[i]);
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        public static void RemoveGet<T1, T2>(this List<T1> list, List<T2> result, Func<T1, bool> predicate, Func<T1, T2> select)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    result.Add(select(list[i]));
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Get dictionary by key and create new value with standard constructor, if it did not exist.
        /// </summary>
        /// <returns>true if new value was created</returns>
        public static bool Ensure<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, out TValue value) where TValue : new()
        {
            if (dic.TryGetValue(key, out value))
                return false;
            dic[key] = value = new();
            return true;
        }

        public static bool Ensure<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, out TValue value, Type type)
        {
            if (dic.TryGetValue(key, out value))
                return false;
            dic[key] = value = (TValue)Activator.CreateInstance(type);
            return true;
        }

        public static bool Ensure<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, out TValue value, Func<TValue> getter)
        {
            if (dic.TryGetValue(key, out value))
                return false;
            dic[key] = value = getter();
            return true;
        }

        public static List<T> AddUnique<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
                list.Add(item);
            return list;
        }

        public static List<T> AddUnique<T>(this List<T> list, Func<T, bool> pred, Func<T> getter)
        {
            if (!list.Any(pred))
                list.Add(getter());
            return list;
        }

        public static IEnumerable<TResult> SelectNotNull<T, TResult>(this IEnumerable<T> array, Func<T, TResult> func) where TResult : class
        {
            if (array is null)
                yield break;

            foreach (var result in array.Select(func))
                if (result is not null)
                    yield return result;
        }

        private static readonly List<object> _list = new();

        /// <summary>
        /// Gets a static list object. Do not save reference.
        /// Call <b>Flush&lt;T&gt;()</b> to receive output.
        /// </summary>
        public static List<object> GetList()
        {
            if (_list.Count != 0)
            {
                Debug.WriteLine("Warning: List wasn't flushed!");
                _list.Clear();
            }
            return _list;
        }

        /// <summary>
        /// Use when finished with <b>GetList()</b>
        /// </summary>
        public static T[] Flush<T>() where T : class
        {
            var result = new T[_list.Count];
            _list.CopyTo(result);
            _list.Clear();
            return result;
        }
    }
}
