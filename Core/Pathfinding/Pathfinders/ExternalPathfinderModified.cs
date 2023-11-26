using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
//using CHusse.Pathfinding;
using DeenGames.Utils.AStarPathFinder;
using DeenGames.Utils;

namespace Elevation
{
    public class DebugPathVisualizer
    {
        public List<Frame> CurrentFrames { get; } = new List<Frame>();

        public static bool showNumbers = false;

        public static Color open = Color.green;
        public static Color closed = Color.red;

        public static Color start = Color.white;
        public static Color end = Color.white;
        public static Color current = Color.grey;

        public static Color path = Color.blue;

        public static Material openMat;
        public static Material closedMat;

        public static Material startMat;
        public static Material endMat;
        public static Material currentMat;

        public static Material pathMat;

        public static Transform container { get; } = new GameObject("PathDebugContainer").transform;

        public enum Marker
        {
            OpenDot,
            ClosedDot,

            StartDot,
            EndDot,
            CurrentDot,

            PathDot,
            PathLine
        }

        public static List<Marker> forNextFrame = new List<Marker>();
        private static List<Vector3> posForNextFrame = new List<Vector3>();

        public static Dictionary<int, DebugPathVisualizer> visualizers { get; } = new Dictionary<int, DebugPathVisualizer>();
        public static ElevationPathfinder tester;

        public static Vector3? startPos = null;
        public static Vector3? endPos = null;

        public static KeyCode AssignPosKey = KeyCode.J;
        public static KeyCode CancelKey = KeyCode.Semicolon;

        public static KeyCode pathKey = KeyCode.Quote;

        public static KeyCode frameDownKey = KeyCode.Comma;
        public static KeyCode frameUpKey = KeyCode.Period;

        public static int frame = 0;



        private bool active = false;
        public bool Active
        { 
            get => active;
            set
            {
                if (active != value)
                {
                    SetActive(value);
                    active = value;
                }
            }
        }

        public static void Init()
        {
            openMat = new Material(Shader.Find("Standard"));
            openMat.color = open;

            closedMat = new Material(Shader.Find("Standard"));
            closedMat.color = closed;

            startMat = new Material(Shader.Find("Standard"));
            startMat.color = start;

            endMat = new Material(Shader.Find("Standard"));
            endMat.color = end;

            currentMat = new Material(Shader.Find("Standard"));
            currentMat.color = current;

            pathMat = new Material(Shader.Find("Standard"));
            pathMat.color = path;

            if(!visualizers.ContainsKey(1))
                visualizers.Add(1, new DebugPathVisualizer());

            if (tester == null)
            {
                tester = Activator.CreateInstance(ElevationPathfinder.current) as ElevationPathfinder;
                tester.Init(World.inst.GridWidth, World.inst.GridHeight);
                tester.aliasHashCode = 1;
            }
        }

        public static void TickAll()
        {
            foreach (DebugPathVisualizer visualizer in visualizers.Values)
                if (visualizer.Active)
                    visualizer.Tick();

            visualizers[1].Tick();

            if(Input.GetKeyDown(CancelKey))
            {
                startPos = null;
                endPos = null;

                DebugExt.dLog("Path Cancelled");
            }

            if (Input.GetKeyDown(AssignPosKey))
            {
                if (Physics.Raycast(Cam.inst.cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000f)) 
                {
                    if (startPos == null)
                    {
                        startPos = hit.point;
                        DebugExt.dLog($"Path Start: {startPos}", false, startPos);
                    }
                    else if (endPos == null)
                    {
                        endPos = hit.point;
                        DebugExt.dLog($"Path End: {endPos}", false, endPos);
                    }
                    else
                        DebugExt.dLog("Path already set");
                }
            }

            List<Vector3> path = new List<Vector3>();

            if (Input.GetKeyDown(pathKey) && startPos != null && endPos != null)
            {
                if (DebugPathVisualizer.tester.aliasHashCode != 1)
                    DebugPathVisualizer.tester.aliasHashCode = 1;

                DebugPathVisualizer.tester.Path(startPos.Value, false, endPos.Value, false, ref path, World.inst.pather.blocksPath, World.inst.pather.blocksPath, World.inst.pather.extraCost, 1, true, true, true);
                DebugExt.dLog($"Path from {startPos.Value} to {endPos.Value}: {path.Count} nodes");
            }


            // Frame Controls
            int frameChange = 0;
            if (Input.GetKeyDown(frameUpKey))
                frameChange = 1;
            if (Input.GetKeyDown(frameDownKey))
                frameChange = -1;
            if (Input.GetKey(KeyCode.LeftShift))
                frameChange *= 5;
            if (Input.GetKey(KeyCode.LeftControl))
                frameChange *= 10;

            int newFrame = Mathf.Clamp(frame + frameChange, -2, visualizers[1].CurrentFrames.Count);

            if(newFrame != frame)
            {
                DebugExt.dLog($"frame: {frame} to {newFrame}");

                if(frame != -1 && frame != -2)
                {
                    visualizers[1].CurrentFrames[frame].Active = false;
                }
                else
                {
                    if (frame == -2)
                        visualizers[1].Active = false;
                }

                if(newFrame != -1 && newFrame != -2)
                {
                    visualizers[1].CurrentFrames[newFrame].Active = true;
                }
                else
                {
                    if(newFrame == -1)
                    {
                        visualizers[1].Active = false;
                    }
                    if (newFrame == -2)
                        visualizers[1].Active = true;
                }

                frame = newFrame;
            }
        }



        public void Tick()
        {
            if (forNextFrame.Count > 0)
                CurrentFrames.Add(new Frame(forNextFrame, posForNextFrame));
        }

        public void Notify(Marker marker, Vector3 position, bool singleFrame = false)
        {
            if (singleFrame)
            {
                CurrentFrames.Add(new Frame(new List<Marker>() { marker }, new List<Vector3>() { position }));
            }
            else
            {
                forNextFrame.Add(marker);
                posForNextFrame.Add(position);
            }
        }

        public void Clear()
        {
            foreach (Frame frame in CurrentFrames)
                frame.Destroy();

            CurrentFrames.Clear();
        }

        public void SetActive(bool active)
        {
            foreach(Frame frame in CurrentFrames)
            {
                if (!frame.generated)
                    frame.GenerateAll();

                frame.Active = true;
            }
        }

        public class Frame
        {
            private List<GameObject> objects;
            private List<Marker> markers;
            private List<Vector3> positions;

            private bool active = true;
            public bool Active
            {
                get => active;
                set
                {
                    if (active != value)
                    {
                        foreach (GameObject obj in objects)
                            if(obj != null)
                                obj.SetActive(value);

                        active = value;
                    }
                }
            }

            public bool generated = false;

            public Frame(List<Marker> markers, List<Vector3> positions)
            {
                this.markers = markers;
                this.positions = positions;
            }

            public void GenerateAll()
            {
                if(objects.Count > 0)
                    Destroy();
                for (int i = 0; i < markers.Count; i++)
                    objects.Add(Generate(markers[i], positions[i]));

                generated = true;
            }

            private GameObject Generate(Marker marker, Vector3 position)
            {
                GameObject obj = new GameObject();
                obj.transform.SetParent(container);
                obj.transform.localPosition = position;

                switch (marker)
                {
                    case Marker.OpenDot:
                        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                        obj.AddComponent<MeshFilter>().mesh = temp.GetComponent<MeshFilter>().mesh;
                        obj.AddComponent<MeshRenderer>().material = openMat;

                        GameObject.Destroy(temp);
                        break;
                    case Marker.ClosedDot:
                        GameObject temp2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                        obj.AddComponent<MeshFilter>().mesh = temp2.GetComponent<MeshFilter>().mesh;
                        obj.AddComponent<MeshRenderer>().material = closedMat;

                        GameObject.Destroy(temp2);
                        break;
                    case Marker.StartDot:
                        GameObject temp3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                        obj.AddComponent<MeshFilter>().mesh = temp3.GetComponent<MeshFilter>().mesh;
                        obj.AddComponent<MeshRenderer>().material = startMat;

                        GameObject.Destroy(temp3);
                        break;
                    case Marker.EndDot:
                        GameObject temp4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                        obj.AddComponent<MeshFilter>().mesh = temp4.GetComponent<MeshFilter>().mesh;
                        obj.AddComponent<MeshRenderer>().material = endMat;

                        GameObject.Destroy(temp4);
                        break;
                    case Marker.CurrentDot:
                        GameObject temp5 = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                        obj.AddComponent<MeshFilter>().mesh = temp5.GetComponent<MeshFilter>().mesh;
                        obj.AddComponent<MeshRenderer>().material = currentMat;

                        GameObject.Destroy(temp5);
                        break;
                    case Marker.PathDot:
                        GameObject temp6 = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                        obj.AddComponent<MeshFilter>().mesh = temp6.GetComponent<MeshFilter>().mesh;
                        obj.AddComponent<MeshRenderer>().material = pathMat;

                        GameObject.Destroy(temp6);
                        break;
                }

                return obj;
            }

            public void Destroy(bool immediate = false)
            {
                foreach (GameObject obj in objects)
                {
                    if (!immediate)
                        GameObject.Destroy(obj);
                    else
                        GameObject.DestroyImmediate(obj);
                }

                objects.Clear();
            }
        }
    }


    public class ExternalPathfinderModified : ElevationPathfinder
    {
        public int width { get; private set; }
        public int height { get; private set; }

        public PathFinder pathfinder;

        public bool allowingDiagonals = false;

        public Stopwatch pathing = new Stopwatch();

        private bool cancelledCurrentPath = false;

        public override void Init(int width, int height)
        {
            this.width = width;
            this.height = height;

            byte[,] grid = new byte[width, height];
            for (int x = 0; x < width; x++)
                for (int z = 0; z < height; z++)
                    grid[x, z] = 1;

            pathfinder = new PathFinder(grid);
            pathfinder.PathCancel += () => cancelledCurrentPath = true;

            pathfinder.DebugProgress = true;
            pathfinder.DebugFoundPath = true;

            pathfinder.PathFinderDebug += Pathfinder_PathFinderDebug;
        }

        private void Pathfinder_PathFinderDebug(int fromX, int fromY, int x, int y, PathFinderNodeType type, int totalCost, int cost)
        {
            if (DebugPathVisualizer.visualizers.ContainsKey(aliasHashCode))
            {
                DebugPathVisualizer visualizer = DebugPathVisualizer.visualizers[aliasHashCode];
                switch (type)
                {
                    case PathFinderNodeType.Open:
                        visualizer.Notify(DebugPathVisualizer.Marker.OpenDot, new Vector3(x, 0f, y));
                        break;
                    case PathFinderNodeType.Close:
                        visualizer.Notify(DebugPathVisualizer.Marker.ClosedDot, new Vector3(x, 0f, y));
                        break;
                    case PathFinderNodeType.Start:
                        visualizer.Notify(DebugPathVisualizer.Marker.StartDot, new Vector3(x, 0f, y));
                        break;
                    case PathFinderNodeType.End:
                        visualizer.Notify(DebugPathVisualizer.Marker.EndDot, new Vector3(x, 0f, y));
                        break;
                    case PathFinderNodeType.Current:
                        visualizer.Notify(DebugPathVisualizer.Marker.CurrentDot, new Vector3(x, 0f, y));
                        break;
                    case PathFinderNodeType.Path:
                        visualizer.Notify(DebugPathVisualizer.Marker.PathDot, new Vector3(x, 0f, y));
                        break;
                }
            }
        }

        public override void Path(Vector3 startPos, bool upperGridStart, Vector3 endPos, bool upperGridEnd, ref List<Vector3> path, Pathfinder.blocksPathTest blocksPath, Pathfinder.blocksPathTest pull, Pathfinder.applyExtraCost extraCost, int team, bool doDiagonal, bool doTrimming, bool allowIntergridTravel)
        {
            //pathing.Restart();

            //allowingDiagonals = doDiagonal;
            ////try
            ////{

            ////    // Reformat end position to be within same region as start cell
            //Cell startCell = World.inst.GetCellDataClamped(startPos);
            //Cell endCell = World.inst.GetCellDataClamped(endPos);

            //if (startCell == null || endCell == null)
            //    Mod.dLog($"Invalid positions; start: {startPos}, end: {endPos}");

            //if (
            //    //!WorldRegions.Reachable(startCell, endCell) || 
            //    blocksPath(endCell, team))
            //{
            //    endCell = World.inst.FindMatchingSurroundingCell(World.inst.GetCellDataClamped(endPos), false, 2, c =>
            //    {
            //        return !(blocksPath(c, team)
            //        //&& WorldRegions.Reachable(startCell, c)
            //        );
            //    });
            //}


            ////    Func<Vector3, int, IEnumerable<Vector3>> expander = (position, lv) =>
            ////    {
            ////        CellMeta meta = Grid.Cells.Get((int)position.x, (int)position.z);
            ////        if (meta == null)
            ////            return new List<Vector3>();
            ////        CellMeta[] neighbors = doDiagonal ? meta.neighborsPlusFast : meta.neighborsFast;
            ////        List<Vector3> result = new List<Vector3>();
            ////        for (int i = 0; i < neighbors.Length; i++)
            ////            if (Math.Abs(neighbors[i].elevationTier - meta.elevationTier) <= PrebakedPathfinder.ElevationClimbThreshold && !blocksPath(neighbors[i].cell, team))
            ////                result.Add(neighbors[i].Center);
            ////        return result;
            ////    };

            ////    var queryable = HeuristicSearch.AStar(startPos, endPos, expander);
            ////    path = queryable.ToList();

            ////}catch(Exception ex)
            ////{
            ////    DebugExt.HandleException(ex);
            ////}

            //if (pathfinder == null)
            //    Init(World.inst.GridWidth, World.inst.GridHeight);

            //pathfinder.Diagonals = doDiagonal;

            //int searchFactor = 2;

            //PathData data = new PathData(startCell.x, startCell.z, endCell.x, endCell.z);
            ////if (gamePathRetries.ContainsKey(data))
            ////    searchFactor += gamePathRetries[data];

            //pathfinder.SearchLimit = (int)((startPos - endPos).sqrMagnitude) * searchFactor;

            //List<PathFinderNode> pathNodes = pathfinder.FindPath(new Point(startCell.x, startCell.z), new Point(endCell.x, endCell.z));


            //if (path.Count > 0)
            //    path.Clear();

            //if (pathNodes != null)
            //{
            //    path.Add(startPos);

            //    foreach (PathFinderNode node in pathNodes)
            //    {
            //        if (World.inst.GetCellData(node.X, node.Y) != null)
            //            path.Add(World.inst.GetCellData(node.X, node.Y).Center);
            //        else
            //            path.Add(new Vector3(node.X, 0f, node.Y));
            //    }

            //    path.Add(endPos);
            //}
            //else if(cancelledCurrentPath)
            //    path.Add(new Vector3(-1f, -1f, -1f));

            //// If the path gets cancelled the returned path will have 1 coordinate: (-1, -1, -1). 
            //// This is a message to the entity requesting a path to try to repath if they need to. 

            ////DebugExt.dLog($"path {startPos}{(upperGridStart ? "u" : "l")} to {endPos}{(upperGridEnd ? "u" : "l")} size {path.Count}");

            //allowingDiagonals = false;
            //cancelledCurrentPath = false;

            //pathing.Stop();
        }

        //public class Node : IPathNode<Cell>
        //{
        //    public ExternalPathfinderModified pathfinder;

        //    public int X => cell.x;
        //    public int Y => cell.z;

        //    public Cell cell;

        //    public Node(Cell cell, ExternalPathfinderModified pathfinder)
        //    {
        //        this.cell = cell;
        //        this.pathfinder = pathfinder;
        //    }

        //    public bool IsWalkable(Cell other)
        //    {
        //        CellMeta meta = Grid.Cells.Get(cell);

        //        if (meta)
        //        {
        //            CellMeta otherMeta = Grid.Cells.Get(other);
        //            if (otherMeta && Math.Abs(meta.elevationTier - otherMeta.elevationTier) <= 1)
        //                return true;
        //        }

        //        if (!pathfinder.allowingDiagonals && Pathing.GetDiagonal(other, cell, out Diagonal diagonal))
        //            return false;

        //        if (Pathing.BlockedCompletely(cell))
        //            return false;

        //        return false;
        //    }

        //    public Cell GetContext()
        //    {
        //        return cell;
        //    }

        //}
    }
}
