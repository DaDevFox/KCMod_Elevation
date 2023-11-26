using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;
using Elevation.Utils;
using Elevation;
using Zat.Shared.Rendering;

namespace Fox.Debugging
{
    [HarmonyPatch(typeof(Debug), "DrawLine",
        new Type[] {
            typeof(Vector3),
            typeof(Vector3),
            typeof(Color),
            typeof(float),
            typeof(bool)
        })]
    public class DebugLines : MonoBehaviour
    {
        // UNFINISHED

        public static LineGUI gui { get; private set; }

        internal static Dictionary<int, Line> _lines = new Dictionary<int, Line>();
        internal static KCModHelper _helper;

        public static bool active { get; } = false;

        // KC Mod Helper
        private void Preload(KCModHelper helper)
        {
            _helper = helper;
        }


        // Harmony Postfix
        static void Postfix(Vector3 start, Vector3 end, Color color, float duration)
        {
            if (active)
                AddLine(start, end, color, duration);
        }

        public static Line AddLine(Vector3 start, Vector3 end, Color color, float duration) => new Line(start, end, color, duration);
        
        public static void Tick()
        {
            if (Settings.debug)
                DebugPathVisualizer.TickAll();

            if (active)
            {
                List<Line> toDispose = new List<Line>();
                foreach (Line line in _lines.Values)
                    if (!line.Tick())
                        toDispose.Add(line);
                toDispose.ForEach((line) => line.Dispose());


                if (!gui)
                    gui = new GameObject("LineGUI").AddComponent<LineGUI>();
            }
        }
    }

    public class LineGUI : MonoBehaviour
    {
        void OnGUI()
        {
            Camera cam = Cam.inst.cam;

            Mod.dLog(DebugLines._lines.Count);

            DebugLines._lines.Values.ForEach((Line line) =>
            {
                if (line.points.Length == 2)
                    ZatsRenderer.DrawLine(cam.FixedWorldToScreenPoint((Vector3)line.start), cam.FixedWorldToScreenPoint((Vector3)line.end), line.width, line.color);


                Vector3? last = null;
                line.points.ForEach((Vector3 current) =>
                {
                    if (last != null)
                        ZatsRenderer.DrawLine(cam.FixedWorldToScreenPoint((Vector3)last), cam.FixedWorldToScreenPoint(current), line.width, line.color);
                    last = (Vector3?)current;
                });
            });
        }
    }


    public class Line
    {
        private static string _containerName { get; } = "LineContainer";
        private static Transform _container;

        public int id { get; private set; }

        public float duration { get; private set; } = 10f;
        public float timeElapsed { get; internal set; } = 0f;

        public Vector3? start { get => points.Length > 0 ? (Vector3?)points[0] : null; }
        public Vector3? end { get => points.Length > 0 ? (Vector3?)points[points.Length - 1] : null; }
        public Vector3[] points { get; set; }
        public float width = 1f;
        public Color color { get; set; }

        internal Line(Vector3 start, Vector3 end, Color color, float duration) => new Line(new Vector3[] { start, end }, color, duration);

        internal Line(Vector3[] points, Color color, float duration)
        {
            int id = DebugLines._lines.GetNextFreeKey();
            DebugLines._lines.Add(id, this);

            this.id = id;
            this.duration = duration;
            this.points = points;
            this.color = color;
            Tick();
        }

        public bool Tick()
        {
            if (duration > 1f)
                Mod.dLog(duration);

            if (timeElapsed > duration)
                return false;

            timeElapsed += Time.unscaledDeltaTime;
            return true;
        }


        public void Dispose()
        {
            DebugLines._lines.Remove(id);
        }

        public static implicit operator bool(Line obj) => obj != null;
    }

    public static class CameraExtensions
    {
        public static Vector3 FixedWorldToScreenPoint(this Camera cam, Vector3 position)
        {
            Vector3 original = cam.WorldToScreenPoint(position);
            return new Vector2(original.x, Screen.height - original.y);
        }
    }
}
