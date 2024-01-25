using Shared.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Shared.Collections
{
    public static class Collection
    {
        public static T Get<T>(this IEnumerable enumerable)
        {
            if (enumerable == null)
                return default;
            foreach (var item in enumerable)
                if (item is T t)
                    return t;
            return default;
        }

        public static IEnumerable<T> GetAll<T>(this IEnumerable enumerable)
        {
            if (enumerable == null)
                yield break;
            foreach (var item in enumerable)
                if (item is T t)
                    yield return t;
        }

        #region Search

        /// <summary>
        /// Checks if right type can be assigned to left type. <br/>
        /// Works similiar to IsAssignableFrom, but will returns false for ValueTypes (which need boxing) and void (which overflows the stack).     
        /// </summary>
        public static bool IsTypeCompatible(this Type left, Type right)
        {
            if (left == null || right == null)
                return false;
            if (left == right)
                return true;
            if (left.IsValueType || right.IsValueType) // value types must match exactly
                return false;
            return left.IsAssignableFrom(right);
        }

        /// <summary>
        /// Find index in a collection by a predicate.
        /// </summary>
        public static int FindIndex<T>(this IEnumerable<T> enumerable, Func<T, bool> pred) where T : class
        {
            int num = 0;
            foreach (T item in enumerable)
            {
                if (pred(item))
                    return num;
                num++;
            }
            return -1;
        }

        /// <summary>
        /// Get element at index or default.
        /// </summary>
        public static T AtIndex<T>(this IEnumerable<T> enumerable, int index)
        {
            return enumerable.ElementAtOrDefault(index);
        }

        #endregion

        #region AddUnique

        /// <summary>
        /// Adds items to a list, which are not already in the list. Compares with Equal().
        /// </summary>
        public static void AddUnique<T>(this IList<T> list, IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                foreach (var item in list)
                {
                    if (item.Equals(value))
                        goto next;
                }
                list.Add(value);
            next:;
            }
        }

        /// <summary>
        /// Adds items to a list, which are not already in the list. Types can be different. Compares with Equal() of <typeparamref name="T1"/>.
        /// </summary>
        public static void AddUnique<T1, T2>(this IList<T1> list, IEnumerable<T2> values, Func<T2, T1> converter)
        {
            foreach (var value in values)
            {
                foreach (var item in list)
                {
                    if (item.Equals(value))
                        goto next;
                }
                list.Add(converter(value));
            next:;
            }
        }

        /// <summary>
        /// Adds items to a list, which are not already in the list. Types can be different. Compares with Equal() of <typeparamref name="T2"/>.
        /// </summary>
        public static void AddUnique2<T1, T2>(this IList<T1> list, IEnumerable<T2> values, Func<T2, T1> converter)
        {
            foreach (var value in values)
            {
                foreach (var item in list)
                {
                    if (value.Equals(item))
                        goto next;
                }
                list.Add(converter(value));
            next:;
            }
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

        #endregion

        #region As X

        /// <summary>
        /// Returns itself, if value is an array. Otherwise creates a new array.
        /// </summary>
        public static T[] AsArray<T>(this IEnumerable<T> values)
        {
            return values as T[] ?? values.ToArray();
        }

        /// <summary>
        /// Returns itself, if value is a list. Otherwise creates a new list.
        /// </summary>
        public static List<T> AsList<T>(this IEnumerable<T> values)
        {
            return values as List<T> ?? values.ToList();
        }

        #endregion

        #region Append

        /// <summary>Appends objects on array.</summary>
        public static T[] Append<T>(this T[] orig, params T[] objs)
        {
            orig ??= [];
            objs ??= [];

            int i, j;
            T[] result = new T[orig.Length + objs.Length];
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            for (j = 0; i < result.Length; i++)
                result[i] = objs[j++];
            return result;
        }

        public static List<T> Append<T>(this IList<T> orig, IList<T> objs)
        {
            var result = new List<T>(orig);
            result.AddRange(objs);
            return result;
        }

        /// <summary>Appends objects on array and overwrites the original.</summary>
        public static void AppendAndReplace<T>(ref T[] orig, params T[] objs)
        {
            AppendAndReplace(ref orig, (IEnumerable<T>)objs);
        }

        /// <summary>Appends objects on array and overwrites the original.</summary>
        public static void AppendAndReplace<T>(ref T[] orig, IEnumerable<T> objs)
        {
            orig ??= [];

            T[] result = new T[orig.Length + objs.Count()];
            int i;
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            foreach (var obj in objs)
                result[i++] = obj;
            orig = result;
        }

        #endregion

        #region Remove

        public static void RemoveAll<T>(ref T[] orig, Func<T, bool> predicate)
        {
            orig = orig.Where(w => !predicate(w)).ToArray();
        }

        public static void InsertAt<T>(ref T[] orig, T obj, int index = -1)
        {
            orig ??= [];
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
            orig ??= [];
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

        #endregion Remove

        #region Ensure

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

        #endregion

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
