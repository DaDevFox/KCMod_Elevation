using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using UnityEngine;
using Harmony;

namespace Elevation
{
    class DebugExt : MonoBehaviour
    {

        private static List<int> IDs = new List<int>();
        private static int ID = 0;
        private static int loop = 1024;
        //private static List<LineRenderer> drawnLines = new List<LineRenderer>();
        //private static List<float> drawnLineDurations = new List<float>();

        public static void dLog(object message, bool repeatable = false, object GameObjectOrVector3 = null)
        {
            if (Settings.debug)
            {
                KingdomLog.TryLog(Mod.ModID + "_debugmsg-" + (repeatable ? (ID++).ToString() : ""), message.ToString(), KingdomLog.LogStatus.Neutral, (repeatable ? 0.1f : 2f), GameObjectOrVector3);
                //ID %= loop; // safety: Int.MaxValue???
                //IDs.Add(1);
            }
        }

        public static void Log(object message, bool repeatable = false, KingdomLog.LogStatus type = KingdomLog.LogStatus.Neutral, object GameObjectOrVector3 = null)
        {
            KingdomLog.TryLog(Mod.ModID + "_debugmsg-" + (repeatable ? (ID++).ToString() : ""), message.ToString(), type, (repeatable ? 1 : 20), GameObjectOrVector3);
            //IDs.Add(1);
        }

        public static void HandleException(Exception ex)
        {
            Mod.Log(ex.Message + "\n" + ex.StackTrace);
        }

        //public static void DrawLine(Vector3 from, Vector3 to, float duration = 10)
        //{
        //    LineRenderer lineRenderer = new LineRenderer();
        //    lineRenderer.transform.SetParent(World.inst.transform);

        //    lineRenderer.positionCount = 2;
        //    lineRenderer.SetPosition(0, from);
        //    lineRenderer.SetPosition(1, to);

        //    lineRenderer.endColor = Color.white;
        //    lineRenderer.startColor = Color.white;

        //    drawnLines.Add(lineRenderer);
        //    drawnLineDurations.Add(duration);
        //}


        //void Update()
        //{
        //    List<int> idsToDestroy = new List<int>();
        //    for (int i = 0; i < drawnLines.Count; i++)
        //    {

        //        drawnLineDurations[i] -= Time.deltaTime;
        //        if (drawnLineDurations[i] < 0)
        //            idsToDestroy.Add(i);
        //    }

        //    foreach (int idx in idsToDestroy)
        //    {
        //        GameObject.Destroy(drawnLines[idx]);
        //        drawnLines.RemoveAt(idx);
        //        drawnLineDurations.RemoveAt(idx);
        //    }
        //}

        //[HarmonyPatch(typeof(UnityEngine.Debug), new Type[]
        //{
        //    typeof(Vector3),
        //    typeof(Vector3),
        //    typeof(Color),
        //    typeof(float),
        //    typeof(bool)
        //})]
        //[HarmonyPatch("DrawLine")]
        //class DebugDrawLinePatch
        //{
        //    static bool Prefix(Vector3 start, Vector3 end, Color color, float duration)
        //    {
        //        Utils.FoxRenderer.CreateLine(start, end, 10f, duration, Color.white);

        //        return false;
        //    }
        //}
    }
}
