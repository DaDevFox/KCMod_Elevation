using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Elevation.Utils
{

    public static class ArrayExtensions
    {
        public static T[] Add<T>(this T[] array, T element)
        {
            List<T> list = array.ToList();
            list.Add(element);
            return list.ToArray();
        }

        public static T[] AddRange<T>(this T[] array, IEnumerable<T> collection)
        {
            int count = collection.Count();
            T[] result = new T[array.Length + count];
            array.CopyTo(result, 0);
            for (int i = array.Length; i < result.Length; i++)
            {
                result[i] = collection.ElementAt(i - array.Length);
            }

            return result;

            // List<T> list = array.ToList();
            // list.AddRange(collection);
            // return list.ToArray();
        }

        public static T[] AddRange<T>(this T[] array, T[] other)
        {
            T[] result = new T[array.Length + other.Length];
            array.CopyTo(result, 0);
            for (int i = array.Length; i < result.Length; i++)
            {
                result[i] = other[i - array.Length];
            }

            return result;
        }

        /// <summary>
        /// Sets the last [<c>other.Length</c>] elements of the array <c>array</C> to other's elements. The completed array will always be of length <c>length</c>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="other"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static T[] SetLast<T>(this T[] array, T[] other, int length)
        {
            T[] result = new T[length];
            for (int i = 0; i < result.Length; i++)
                if (i < array.Length)
                    result[i] = array[i];

            for (int i = result.Length - 1; i >= result.Length - other.Length; i--)
            {
                result[i] = other[result.Length - 1 - i];
            }

            return result;
        }
    }

    public static class EnumerableExtensions
    {
        public static string ToStringList<T>(this T[] array, bool returnLine = false)
        {
            string result = "[";
            for (int i = 0; i < array.Length; i++)
                result += array[i].ToString() + (i + 1 < array.Length ? "," : "") + (returnLine ? "\n" : "");

            result += "]";
            return result;
        }

        public static T[] Add<T>(this T[] array)
        {
            Array.Resize(ref array, array.Length + 1);
            return array;
        }

        public static T[] Add<T>(this T[] array, T item)
        {
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = item;
            //Debug.Log($"array length {array.Length}");
            return array;
        }

        public static void AddRange<T>(this T[] array, IEnumerable<T> collection)
        {
            List<T> list = array.ToList();
            list.AddRange(collection);
            array = list.ToArray();
        }

        public static T Find<T>(this IEnumerable<T> enumrable, T toFind)
        {
            foreach (T item in enumrable)
                if (item.Equals(toFind))
                    return item;
            return default(T);
        }

        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            for (int i = 0; i < array.Length; i++)
            {
                action(array[i]);
            }
        }

        public static void ForEach<T>(this T[] array, Action<int, T> action)
        {
            for (int i = 0; i < array.Length; i++)
            {
                action(i, array[i]);
            }
        }

        public static void ForEach<T>(this T[] array, Action<int> action)
        {
            for (int i = 0; i < array.Length; i++)
            {
                action(i);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            enumerable.ToList().ForEach(action);
        }

        public static int GetNextFreeKey<T>(this Dictionary<int, T> dict)
        {
            int found = -1;
            int i = 0;
            while (found == -1)
            {
                if (!dict.ContainsKey(i))
                    found = i;
                i++;
            }
            return found;
        }
    }
}
