using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Custom extensions.
    /// </summary>
    public static class Extensions
    {
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
    }
}
