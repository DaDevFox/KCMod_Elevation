using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Fox.Utils
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Resize the array to be able to include 1 more element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T[] Add<T>(this T[] array)
        {
            Array.Resize(ref array, array.Length + 1);
            return array;
        }

        /// <summary>
        /// Adds an element to the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="item">element to add</param>
        /// <returns></returns>
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

        public static T Find<T>(this IEnumerable<T> enumerable, T toFind)
        {
            List<T> list = enumerable.ToList();
            for (int i = 0; i < list.Count; i++)
                if (list[i].Equals(toFind))
                    return list[i];

            return default(T);
        }

        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            for (int i = 0; i < array.Length; i++)
                action(array[i]);
        }

        public static void ForEach<T>(this T[] array, Action<int, T> action)
        {
            for (int i = 0; i < array.Length; i++)
                action(i, array[i]);
        }

        public static void ForEach<T>(this T[] array, Action<int> action)
        {
            for (int i = 0; i < array.Length; i++)
                action(i);
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            for (int i = 0; i < enumerable.Count(); i++)
                action(enumerable.ElementAt(i));
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

        public static T Random<T>(this IEnumerable<T> enumerable) => enumerable.ElementAt(UnityEngine.Random.Range(0, enumerable.Count()));

        public static T Min<T>(this T[] array, Func<T, float> match)
        {
            float min = float.MaxValue;
            T found = default(T);
            for (int i = 0; i < array.Length; i++)
            {
                float current = match(array[i]);
                if (current < min)
                {
                    min = current;
                    found = array[i];
                }
            }

            return found;
        }

        public static T Max<T>(this T[] array, Func<T, float> match)
        {
            float max = float.MinValue;
            T found = default(T);
            for (int i = 0; i < array.Length; i++)
            {
                float current = match(array[i]);
                if (current > max)
                {
                    max = current;
                    found = array[i];
                }
            }

            return found;
        }
    }
}
