using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Zat.Shared.Rendering;

namespace Elevation.Utils
{
    public static class FoxRenderer
    {
        public class LineData
        {
            public Vector3[] points;
            public Color color;
            public float duration;
            public float width;
            public float timeConsumed;
        }

        private static Dictionary<int, LineData> lines = new Dictionary<int, LineData>();

        static FoxRenderer()
        {
            Mod.Tick += Tick;
        }

        private static void Tick()
        {
            foreach (KeyValuePair<int, LineData> pair in lines)
            {
                LineData data = pair.Value;
                if (data.timeConsumed < data.duration)
                    DrawZatLine(Cam.inst.cam, data.points, data.width, data.color);
                else
                    lines.Remove(pair.Key);

                data.timeConsumed += Time.unscaledDeltaTime;
            }
        }

        /// <summary>
        /// Draws a zat line for 1 frame
        /// </summary>
        /// <param name="cam"></param>
        /// <param name="points"></param>
        /// <param name="width"></param>
        /// <param name="color"></param>
        public static void DrawZatLine(Camera cam, Vector3[] points, float width, Color color)
        {
            bool firstPoint = true;
            Vector3 lastPoint = Vector3.zero;
            foreach(Vector3 point in points)
            {
                if (!firstPoint)
                {
                    Vector3 a = cam.WorldToScreenPoint(lastPoint);
                    Vector3 b = cam.WorldToScreenPoint(point);

                    Vector2 a2D = new Vector2(a.x, a.y);
                    Vector2 b2D = new Vector2(a.x, a.y);

                    ZatsRenderer.DrawLine(a2D, b2D, width, color);

                    firstPoint = true;
                }

                lastPoint = point;
            }
        }

        /// <summary>
        /// Draws a zat line for 1 frame
        /// </summary>
        /// <param name="cam"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="width"></param>
        /// <param name="color"></param>
        public static void DrawZatLine(Camera cam, Vector3 start, Vector3 end, float width, Color color)
        {
            Vector3[] points = new Vector3[2];

            points[0] = start;
            points[1] = end;

            DrawZatLine(cam, points, width, color);
        }

        public static int AddLine(Vector3 start, Vector3 end, float width = 0.1f, float duration = 10f, Color color = new Color()) => AddLine(new Vector3[] { start, end }, width, duration, color);


        public static int AddLine(Vector3[] points, float width = 0.1f, float duration = 10f, Color color = new Color())
        {
            int key = lines.GetNextFreeKey();

            lines.Add(key, new LineData()
            {
                points = points,
                width = width,
                duration = duration,
                color = color
            });

            return key;
        }

        /// <summary>
        /// Creates line using a GameObject and LineRenderer
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="width"></param>
        /// <param name="duration"></param>
        /// <param name="color"></param>
        public static void CreateLine(Vector3 start, Vector3 end, float width = 0.1f, float duration = 10f, Color color = new Color())
        {
            Vector3[] vertices = new Vector3[2];

            vertices[0] = start;
            vertices[1] = end;

            GameObject GO = GameObject.Instantiate(new GameObject());
            Line l = GO.AddComponent<Line>();
            l.Init(vertices, width, duration, color);
        }

        /// <summary>
        /// Creates a line using a GameObject and LineRenderer
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="width"></param>
        /// <param name="duration"></param>
        /// <param name="color"></param>
        public static void CreateLine(Vector3[] vertices, float width = 0.1f, float duration = 10f, Color color = new Color())
        {
            GameObject GO = GameObject.Instantiate(new GameObject());
            Line l = GO.AddComponent<Line>();
            l.Init(vertices, width, duration, color);
        }

        public class Line : MonoBehaviour
        {
            public LineData data = new LineData();

            private Material _material;



            // Got help (pretty much copied) from: 
            // https://docs.unity3d.com/ScriptReference/GL.html
            public void Init(Vector3 start, Vector3 end, float width = 0.1f, float duration = 10f, Color color = new Color())
            {
                data.points = new Vector3[2];
                data.points[0] = start;
                data.points[1] = end;

                data.width = width;
                data.duration = duration;

                data.color = color;

                Create();
            }

            public void Init(Vector3[] vertices, float width = 0.1f, float duration = 10f, Color color = new Color())
            {
                data.points = vertices;

                data.width = width;
                data.duration = duration;

                data.color = color;

                Create();
            }

            void Update()
            {
                if(data.timeConsumed > data.duration)
                    GameObject.Destroy(gameObject);

                data.timeConsumed += Time.deltaTime;
            }

            void Create()
            {
                SetupMaterial();
                _material.SetPass(0);


                GL.PushMatrix();
                GL.MultMatrix(transform.localToWorldMatrix);

                GL.Begin(GL.LINES);

                foreach (Vector3 point in data.points)
                    GL.Vertex(point);

                GL.End();
                GL.PopMatrix();
            }

            void SetupMaterial()
            {
                if (!_material)
                {
                    Shader shader = Shader.Find("Standard");
                    _material = new Material(shader);

                    _material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    _material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);

                    _material.SetInt("_Cull", (int)CullMode.Off);
                    _material.SetInt("_ZWrite", 0);

                    _material.color = data.color;
                }
            }

        }

    }
}
