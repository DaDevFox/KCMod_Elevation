using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Elevation
{
    public static class GameObjectExtensions
    {
        public static void ClearChildren(this GameObject obj)
        {
            List<Transform> children = new List<Transform>();

            for (int i = 0; i < obj.transform.childCount; i++)
            {
                children.Add(obj.transform.GetChild(i));
            }

            foreach (Transform child in children)
                GameObject.Destroy(child.gameObject);
        }

        public static void ClearChildren(this Transform obj)
        {
            List<Transform> children = new List<Transform>();

            for (int i = 0; i < obj.childCount; i++)
            {
                children.Add(obj.GetChild(i));
            }

            foreach (Transform child in children)
                GameObject.Destroy(child.gameObject);
        }

        public static void ForEachChildRecursive(this Transform obj, Action<Transform> action)
        {
            action(obj);
            for(int i = 0; i < obj.childCount; i++)
                ForEachChildRecursive(obj.GetChild(i), action);
        }

        public static string LabelForEachChildRecursive(this Transform obj, Func<Transform, string> printFunc)
        {
            return LabelForEachChildInternal(obj, printFunc, "");
        }

        private static string LabelForEachChildInternal(Transform obj, Func<Transform, string> printFunc, string baseString)
        {
            baseString += "--";
            string result = baseString + $" {obj.name}: \t{printFunc(obj)}{Environment.NewLine}";
            for (int i = 0; i < obj.childCount; i++)
                result += LabelForEachChildInternal(obj.GetChild(i), printFunc, baseString);
            return result;
        }

    }
}
