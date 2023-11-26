using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using Harmony;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(MaxRangeDisplay), "LateUpdate")]
    public class TowerRangePlaneCorrection
    {
        static void Prefix(MaxRangeDisplay __instance, Building b, GameObject ___rangePlane, ref float ___maxRadius)
        {
            object rTexture = typeof(MaxRangeDisplay).GetField("rTexture", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
            if (rTexture == null) // slight optimization as opposed to whole contents postfixing SetEnabled?
            {
                ___maxRadius = 50f + 8f;
                Mesh mesh = ___rangePlane.GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = new Vector3[]
                {
                new Vector3(-___maxRadius / 2f, 0f, -___maxRadius / 2f),
                new Vector3(___maxRadius / 2f, 0f, -___maxRadius / 2f),
                new Vector3(-___maxRadius / 2f, 0f, ___maxRadius / 2f),
                new Vector3(___maxRadius / 2f, 0f, ___maxRadius / 2f)
                };
                mesh.vertices = vertices;
            }
        }
    }
}
