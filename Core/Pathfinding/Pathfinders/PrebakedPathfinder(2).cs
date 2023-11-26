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



    public class PrebakedPathfinder : ElevationPathfinder
    {
        private static Dictionary<string, Node> _pathGrid;
        private static Dictionary<string, Node> _upperGrid;

        /// <summary>
        /// Multiplier of the additional cost applied for the difference between two elevated cells' tiers
        /// </summary>
        public static readonly float ElevationClimbCostMultiplier = 1.5f;
        /// <summary>
        /// Maximum difference between two elevated cells' tiers that can still be climbed
        /// </summary>
        public static readonly int ElevationClimbThreshold = 1;
        /// <summary>
        /// Base cost of pathfinding
        /// </summary>
        public static float BasePathfindingCost { get; } = 1f;
        /// <summary>
        /// G score appended per node
        /// </summary>
        public static float CostPerNode { get; } = 1f;

        /// <summary>
        /// Cost to travel between grids
        /// </summary>
        public static float IntergridTraversalCost { get; } = 0f;
        /// <summary>
        /// If this funciton returns true on a node, it can be used to travel from the path to the upper grid or vice versa
        /// </summary>
        public static Func<Node, bool> CheckIntergridTravel { get; } = (n) => n.cell.StructureCompareUniqueNameAll(World.castlestairsHash);

        public static Func<PathCell, int, bool> BlocksPath { get; } = (cell, team) =>
        {
            return cell.Type == ResourceType.Water;
        };

        #region Initialization

        public override void Init(int width, int height)
        {
            Mod.dLog("Pathfinding Initializing");

            // Init both grids

            // Letting a dictionary know its capacaity before adding any elements to it can increase efficiency. 
            if (_pathGrid == null)
                _pathGrid = new Dictionary<string, Node>(width * height);
            if (_upperGrid == null)
                _upperGrid = new Dictionary<string, Node>(width * height);


            // Path grid is the base grid; terrain tiles, this will never change
            // Upper grid is a grid exclusively for structures units can travel on top of
            // such as castle blocks or gates, these y-coordinates will change dynamically

            Mod.dLog("Creating");

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    Cell cell = World.inst.GetCellData(x, z);

                    if (cell == null)
                        continue;

                    Node pathGridNode = new Node()
                    {
                        cell = cell,
                        meta = Grid.Cells.Get(cell),
                        upperGrid = false
                    };

                    Node upperGridNode = new Node()
                    {
                        cell = cell,
                        meta = Grid.Cells.Get(cell),
                        upperGrid = true
                    };

                    string id = CellMetadata.GetPositionalID(cell);

                    if (!_pathGrid.ContainsKey(id))
                        _pathGrid.Add(id, pathGridNode);
                    else
                        _pathGrid[id] = pathGridNode;
                    //Mod.Log($"Duplicate key for path grid {id}");
                    if (!_upperGrid.ContainsKey(id))
                        _upperGrid.Add(id, upperGridNode);
                    else
                        _upperGrid[id] = upperGridNode;
                    //Mod.Log($"Duplicate key for upper grid {id}");
                }
            }

            Mod.dLog("Connecting");

            List<CellMeta> all = Grid.Cells.GetAll();

            Mod.dLog($"To loop thorugh {all.Count}");

            Stopwatch timer = Stopwatch.StartNew();

            // TODO: Optimize loop below
            // Try checking all connections at runtime?

            foreach (Node node in _pathGrid.Values) {
                node.ClearConnections();
            }
            foreach(Node node in _upperGrid.Values) {
                node.ClearConnections();
            }

            List<int> neighborsThatMatter = new List<int>() { 0, 1, 2, 7 };

            // Mark connections
            float timeStart = Time.time;
            float timeOld = Time.time;
            List<float> operationTimes = new List<float>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int operationIndex = 0;
            int neighbourStartingIndex = 0;
            int connectionCount = 0;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    operationIndex = 0;
                    timeOld = Time.time;

                    CellMeta mark = Grid.Cells.Get(x, z);

                    operationTimes[operationIndex] += Time.time - timeOld;
                    operationIndex += 1;
                    timeOld = Time.time;

                    CellMeta[] neighbors = mark.neighborsPlusFast;

                    operationTimes[operationIndex] += Time.time - timeOld;
                    operationIndex += 1;
                    timeOld = Time.time;

                    Node current_path = _pathGrid[CellMetadata.GetPositionalID(x, z)];

                    operationTimes[operationIndex] += Time.time - timeOld;
                    operationIndex += 1;
                    timeOld = Time.time;

                    Node current_upper = _upperGrid[CellMetadata.GetPositionalID(x, z)];

                    operationTimes[operationIndex] += Time.time - timeOld;
                    operationIndex += 1;
                    timeOld = Time.time;

                    neighbourStartingIndex = operationIndex;

                    foreach (int n in neighborsThatMatter)
                    {
                        operationIndex = neighbourStartingIndex;
                        timeOld = Time.time;

                        CellMeta neighbor = neighbors[n];
                        if (neighbor == null)
                            continue;

                        operationTimes[operationIndex] += Time.time - timeOld;
                        operationIndex += 1;
                        timeOld = Time.time;

                        // Upper grid should not be innately blocked under any circumstances; as circumstances can change and nodes that 
                        // were once disconnected aren't anymore; height difference between upper grid will be checked when finding path

                        Node neighborNode_upper = _upperGrid[CellMetadata.GetPositionalID(neighbor.cell)];

                        operationTimes[operationIndex] += Time.time - timeOld;
                        operationIndex += 1;
                        timeOld = Time.time;

                        current_upper.AddConnection(neighborNode_upper, BasePathfindingCost);

                        operationTimes[operationIndex] += Time.time - timeOld;
                        operationIndex += 1;
                        timeOld = Time.time;

                        neighborNode_upper.AddConnection(current_upper, BasePathfindingCost);

                        operationTimes[operationIndex] += Time.time - timeOld;
                        operationIndex += 1;
                        timeOld = Time.time;

                        int difference = Math.Abs(mark.elevationTier - neighbor.elevationTier);

                        operationTimes[operationIndex] += Time.time - timeOld;
                        operationIndex += 1;
                        timeOld = Time.time;

                        if (difference <= ElevationClimbThreshold)
                        {
                            operationTimes[operationIndex] += Time.time - timeOld;
                            operationIndex += 1;
                            timeOld = Time.time;

                            // The y-coordinates of the path grid on the other hand, will never change, and as such, there is no need
                            // to check nodes' y-coordinates when finding path, so these can be permanently tracked in the node itself

                            Node neighborNode_path = _pathGrid[CellMetadata.GetPositionalID(neighbor.cell)];

                            operationTimes[operationIndex] += Time.time - timeOld;
                            operationIndex += 1;
                            timeOld = Time.time;

                            current_path.AddConnection(neighborNode_path, BasePathfindingCost
                                + (difference * ElevationClimbCostMultiplier)
                                );

                            operationTimes[operationIndex] += Time.time - timeOld;
                            operationIndex += 1;
                            timeOld = Time.time;

                            neighborNode_path.AddConnection(current_path, BasePathfindingCost
                                + (difference * ElevationClimbCostMultiplier)
                                );

                            operationTimes[operationIndex] += Time.time - timeOld;
                            operationIndex += 1;
                            timeOld = Time.time;

                            connectionCount += 1;
                        }
                    }
                }
            }
            Mod.Log("CUSTOM VERSION");
            Mod.Log("TOTAL CONNECTIONS: " + connectionCount.ToString());
            Mod.Log("TOTAL TIME: " + (Time.time - timeStart).ToString());
            int operationIndexB = 0;
            foreach (float operationTime in operationTimes) {
                Mod.Log(operationIndexB.ToString() + " - " + operationTime.ToString());
                operationIndexB += 1;
            }
            Mod.Log("----------------------------------------------------------------------------------------------------");


            timer.Stop();

            Mod.dLog($"Connections parsed in {timer.Elapsed}");

            Mod.Log("Pathfinding Initialized");
        }

        #endregion

        #region Utils

        /// <summary>
        /// Gets the node at the given cell coordinate in either the upper or lower grid depending on the param upperGrid
        /// <para>Time Complexity: O(1)</para>
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="upperGrid"></param>
        /// <returns></returns>
        public static Node GetAt(Cell cell, bool upperGrid = false)
        {
            if (cell == null)
                return null;
            string id = CellMetadata.GetPositionalID(cell);
            return upperGrid ?
                (_upperGrid.ContainsKey(id) ? _upperGrid[id] : null) :
                (_pathGrid.ContainsKey(id) ? _pathGrid[id] : null);
        }

        /// <summary>
        /// Gets the node at the given world-bound coordinate in either the upper or lower grid depending on the param upperGrid
        /// <para>Time Complexity: O(1)</para>
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="upperGrid"></param>
        /// <returns></returns>
        public static Node GetAt(int x, int z, bool upperGrid = false)
        {
            //if (new Vector3(x, 0f, z) != World.inst.ClampToWorld(new Vector3(x, 0f, z)))
            //    return null;
            string id = CellMetadata.GetPositionalID(x, z);
            return upperGrid ?
                (_upperGrid.ContainsKey(id) ? _upperGrid[id] : null) :
                (_pathGrid.ContainsKey(id) ? _pathGrid[id] : null);
        }

        public static Node GetClosestUnblocked(Node node, int team, Pathfinder.blocksPathTest blocks, bool upperGrid)
        {
            Node found = null;
            //if (blocks(node.cell, team) || node.connected.Count == 0)
            //{
            //    Cell cell = World.inst.FindMatchingSurroundingCell(node.cell, false, 2, c =>
            //    {
            //        Node n = GetAt(c);
            //        return !(blocks(c, team) && n.connected.Count != 0 && n.upperGrid == upperGrid);
            //    });

            //    found = cell != null ? GetAt(cell, upperGrid) : null;
            //}
            //else
            //    found = node;
            return found;
        }

        private static Node GetNextCandidate(List<Node> openSet)
        {
            if (openSet.Count > 0)
            {
                float lowest = float.MaxValue;
                Node candidate = null;
                for (int i = 0; i < openSet.Count; i++)
                {
                    Node n = openSet[i];
                    if (n.f < lowest)
                    {
                        lowest = n.f;
                        candidate = n;
                    }
                }
                return candidate;
            }
            return null;
        }

        private static float Dist(Vector3 start, Vector3 end)
        {
            return (start - end).sqrMagnitude;
        }

        private static float Dist(Node start, Node end)
        {
            return Dist(start.cell.Center.xz(), end.cell.Center.xz());
        }

        public static bool Connected(Cell a, Cell b)
        {
            Node nA = GetAt(a);
            Node nB = GetAt(b);

            return nA.connected.ContainsKey(nB);
        }

        private static void Clear()
        {
            foreach(KeyValuePair<string, Node> pair in _pathGrid)
            {
                pair.Value.g = 0f;
                pair.Value.h = 0f;
                pair.Value.parent = null;
            }

            foreach(KeyValuePair<string, Node> pair in _upperGrid)
            {
                pair.Value.g = 0f;
                pair.Value.h = 0f;
                pair.Value.parent = null;
            }
        }


        #endregion

        #region Pathing

        // Implementation of A* pathfinding algorithm; modified slightly for KC and Elevation
        public override void Path(
            Vector3 startPos,
            bool upperGridStart,
            Vector3 endPos,
            bool upperGridEnd,

            ref List<Vector3> path,

            Pathfinder.blocksPathTest blocksPath,
            Pathfinder.blocksPathTest pull,
            Pathfinder.applyExtraCost extraCost,

            int team,

            bool doDiagonal,
            bool doTrimming,
            bool allowIntergridTravel)
        {
            Clear();

            // Blocks water as well as other obstacles
            Pathfinder.blocksPathTest blocks = (c, teamId) =>
            {
                return blocksPath(c, teamId) || BlocksPath(c, teamId);
            };

            // Init vars
            List<Node> openSet = new List<Node>();
            List<Node> closedSet = new List<Node>();

            Cell startCell = World.inst.GetCellDataClamped((startPos));
            Cell endCell = World.inst.GetCellDataClamped((endPos));

            Node start = GetAt(startCell, upperGridStart);
            Node end = GetAt(endCell, upperGridEnd);

            // Check for blocked start and/or end cell
            start = GetClosestUnblocked(start, team, blocks, upperGridStart);
            end = GetClosestUnblocked(end, team, blocks, upperGridEnd);

            List<string> visited = new List<string>();

            if (start == null || end == null)
            {
                path.Add(startPos);
                path.Add(endPos);

                return;
            }

            // Begin searching by adding start to open set
            start.g = 0;
            start.Heuristic(end);
            start.parent = null;

            openSet.Add(start);

            Node current = null;

            while (openSet.Count > 0)
            {
                // Find element with lowest f cost
                current = GetNextCandidate(openSet);

                openSet.Remove(current);
                closedSet.Add(current);


                // Check if reached destination
                if (current == end)
                {
                    try
                    {
                        path = RetracePath(current);
                    }
                    catch
                    {
                        Mod.dLog($"Path retrace looped on path {startPos}{(upperGridStart? "u" : "l")} to {endPos}{(upperGridEnd ? "u" : "l")}");
                    }
                    return;
                }

                // Find nodes that are connected
                Dictionary<Node, float> connected = current.connected;

                // Check for intergrid travel
                if (CheckIntergridTravel(current) && allowIntergridTravel)
                    connected.Add(
                        (current.upperGrid ? _pathGrid[current.meta.id] : _upperGrid[current.meta.id]),
                        IntergridTraversalCost);

                // Check for diagonal travel
                if (!doDiagonal)
                    connected = ExcludeDiagonals(current, connected);


                // Add all connected to openset to be later evaluated
                foreach(KeyValuePair<Node, float> connection in connected)
                {
                    if (blocks(World.inst.GetPathCell(connection.Key.cell), team) || closedSet.Contains(connection.Key))
                        continue;

                    string id = connection.Key.id;

                    //if (visited.Contains(id))
                    //    continue;

                    float connectionCost = current.g +
                        (CostPerNode + connection.Value + extraCost(World.inst.GetPathCell(connection.Key.cell), team));

                    if (!openSet.Contains(connection.Key))
                        openSet.Add(connection.Key);

                    if (connectionCost < connection.Key.g || !connection.Key.visited)
                    {
                        connection.Key.g = connectionCost;
                        connection.Key.Heuristic(end);
                        connection.Key.parent = current;
                        connection.Key.visited = true;
                    }

                    //visited.Add(id);
                }
            }

            if (doTrimming)
            {
                StringPull(start, end, team, pull, extraCost);
            }
        }

        /// <summary>
        /// Extracts which nodes are diagonal connections from the origin's connections and returns the new list of connections without diagonals
        /// </summary>
        /// <param name="origin">node used to detremine position and diagonals</param>
        /// <param name="connections">Dictionary of connections that will be checked for diagonals</param>
        /// <returns></returns>
        private static Dictionary<Node, float> ExcludeDiagonals(Node origin, Dictionary<Node, float> connections)
        {
            Dictionary<Node, float> _new = new Dictionary<Node, float>(connections);

            foreach (KeyValuePair<Node, float> connected in connections)
                if (Pathing.GetDiagonal(origin.cell, connected.Key.cell, out Diagonal diag))
                    _new.Remove(connected.Key);

            return _new;
        }

        private static List<Vector3> RetracePath(Node final)
        {
            List<Vector3> path = new List<Vector3>();
            //path.Add(final.cell.Center);

            int iterations = 0;
            int kill = World.inst.GridWidth * World.inst.GridHeight;

            Node current = final;
            while (current.parent != null)
            {
                if (path.Contains(current.cell.Center))
                    throw new Exception("path loop detected");

                path.Insert(0, current.cell.Center);
                current = current.parent;


                iterations++;
                if (iterations > kill)
                    break;
            }


            return path;
        }

        #region Source

        // Directly taken from source (with slight modifications) at Pathfinder.StringPull
        private static void StringPull(Node start, Node end, int teamId, Pathfinder.blocksPathTest blocksPath, Pathfinder.applyExtraCost extraCost)
        {
            Pathfinder.blocksPathTest blocks = (c, team) =>
            {
                return blocksPath(c, team) || BlocksPath(c, team);
            };

            Node node = end;
            if (node.parent != null)
            {
                Node parent = node.parent.parent;
                while (parent != null)
                {
                    bool flag = LineBlock(node, parent, extraCost, blocks, teamId);
                    if (World.inst.GetPathCell(node.cell).isUpperGridBlocked && flag)
                    {
                        node.parent = parent;
                        if (parent == start)
                        {
                            break;
                        }
                        parent = parent.parent;
                    }
                    else
                    {
                        node = node.parent;
                        parent = node.parent.parent;
                    }
                }
            }
        }

        // Directly taken from source (with slight modifications) at Pathfinder.rasterLine
        private static bool LineBlock(Node a, Node b, Pathfinder.applyExtraCost extraCost, Pathfinder.blocksPathTest blockFunc, int teamId)
        {
            bool result;
            if (a.upperGrid != b.upperGrid)
            {
                result = false;
            }
            else
            {
                int x1 = a.cell.x;
                int z1 = a.cell.z;
                int x2 = b.cell.x;
                int z2 = b.cell.z;
                float y = a.cell.Center.y;
                int startCost = extraCost(World.inst.GetPathCell(a.cell), teamId);
                int num4 = Mathf.Abs(x2 - x1);
                int num5 = (x1 >= x2) ? -1 : 1;
                int num6 = Mathf.Abs(z2 - z1);
                int num7 = (z1 >= z2) ? -1 : 1;
                int num8 = num4 - num6;
                float num9 = (num4 + num6 != 0) ? Mathf.Sqrt((float)num4 * (float)num4 + (float)num6 * (float)num6) : 1f;
                for (; ; )
                {
                    Node node = a.upperGrid ? _upperGrid[a.id] : _pathGrid[a.id];
                    if (GetPathBlocked(node, blockFunc, teamId) || startCost != extraCost(World.inst.GetPathCell(node.cell), teamId) || Mathf.Abs(node.cell.Center.y - y) > 0.01f)
                    {
                        break;
                    }
                    int num10 = num8;
                    int num11 = x1;
                    if (2 * num10 >= -num4)
                    {
                        if (x1 == x2)
                        {
                            goto Block_8;
                        }
                        if ((float)(num10 + num6) < num9)
                        {

                            node = GetAt(x1, z1 + num7, a.upperGrid);
                            if (node == null || GetPathBlocked(node, blockFunc, teamId) || startCost != extraCost(World.inst.GetPathCell(node.cell), teamId) || Mathf.Abs(node.cell.Center.y - y) > 0.01f)
                            {
                                goto IL_1C3;
                            }
                        }
                        num8 -= num6;
                        x1 += num5;
                    }
                    if (2 * num10 <= num6)
                    {
                        if (z1 == z2)
                        {
                            goto Block_13;
                        }
                        if ((float)(num4 - num10) < num9)
                        {
                            node = GetAt(num11 + num5, z1, a.upperGrid);
                            if (node == null || GetPathBlocked(node, blockFunc, teamId) || startCost != extraCost(World.inst.GetPathCell(node.cell), teamId) || Mathf.Abs(node.cell.Center.y - y) > 0.01f)
                            {
                                goto IL_25C;
                            }
                        }
                        num8 += num4;
                        z1 += num7;
                    }
                }
                return false;
            Block_8:
                goto IL_277;
            IL_1C3:
                return false;
            Block_13:
                goto IL_277;
            IL_25C:
                return false;
            IL_277:
                result = true;
            }
            return result;
        }

        // Directly taken from source (with slight modifications) at Pathfinder.GetPathBlocked
        private static bool GetPathBlocked(Node n, Pathfinder.blocksPathTest bt, int teamId)
        {
            if (n.upperGrid != true)
            {
                if (bt(World.inst.GetPathCell(n.cell), teamId))
                {
                    return true;
                }
            }
            else if (World.inst.GetPathCell(n.cell).isUpperGridBlocked)
            {
                return true;
            }
            return false;
        }

        #endregion

        #endregion

        // TODO: Upper grid connection checks during find path

        public class Node : IComparable<Node>
        {
            /// <summary>
            /// Previous node in path
            /// </summary>
            public Node parent { get; set; } = null;
            /// <summary>
            /// Whether or not this node has been visited
            /// </summary>
            public bool visited = false;

            public float h;
            public float g;
            /// <summary>
            /// Cumulative cost factoring the heuristic and g cost
            /// </summary>
            public float f => h + g; 

            /// <summary>
            /// Dictionary of the connected nodes to this node and the travel costs of each connection
            /// </summary>
            public Dictionary<Node, float> connected = new Dictionary<Node, float>(9);
            public Cell cell;
            public CellMeta meta;

            public bool upperGrid;

            /// <summary>
            /// Positional id of this node's cell
            /// </summary>
            public string id => CellMetadata.GetPositionalID(cell);

            public bool Connected(Node node) => connected.ContainsKey(node);

            public void AddConnection(Node connection, float weight = 1f)
            {
                connected.Add(connection, weight);
            }

            public void ClearConnections()
            {
                connected.Clear();
            }

            public float Heuristic(Node other)
            {
                h = PrebakedPathfinder.Dist(cell.Center.xz(), other.cell.Center.xz());
                return h;
            }

            public override string ToString()
            {
                string text = $"Node: {(meta != null ? meta.id : "No Mark")}; Grid: {(upperGrid ? "Upper" : "Path")}";

                return text;
            }

            public int CompareTo(Node other)
            {
                if (other == null)
                    return 1;

                return this.f.CompareTo(other.f);
            }
        }


    }

}
