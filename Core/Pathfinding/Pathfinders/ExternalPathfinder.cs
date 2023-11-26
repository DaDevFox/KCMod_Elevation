using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CHusse.Pathfinding;

namespace Elevation
{
    public class ExternalPathfinder : ElevationPathfinder
    {
        public int width {get; private set; }
        public int height {get; private set; }

        public SpatialAStar<Node, Cell> aStar;

        public bool allowingDiagonals = false;

        public Stopwatch pathing = new Stopwatch();

        public override void Init(int width, int height)
        {
            this.width = width;
            this.height = height;

            Node[,] grid = new Node[width, height];
            for (int x = 0; x < width; x++)
                for (int z = 0; z < height; z++)
                    grid[x, z] = new Node(World.inst.GetCellDataClamped(x, z), this);

            aStar = new SpatialAStar<Node, Cell>(grid);
        }

        public override void Path(Vector3 startPos, bool upperGridStart, Vector3 endPos, bool upperGridEnd, ref List<Vector3> path, Pathfinder.blocksPathTest blocksPath, Pathfinder.blocksPathTest pull, Pathfinder.applyExtraCost extraCost, int team, bool doDiagonal, bool doTrimming, bool allowIntergridTravel)
        {
            pathing.Restart();

            allowingDiagonals = doDiagonal;
            //try
            //{

            //    // Reformat end position to be within same region as start cell
            Cell startCell = World.inst.GetCellDataClamped(startPos);
            Cell endCell = World.inst.GetCellDataClamped(endPos);
            if (
                //!WorldRegions.Reachable(startCell, endCell) || 
                blocksPath(World.inst.GetPathCell(endCell), team))
            {
                endCell = World.inst.FindMatchingSurroundingCell(World.inst.GetCellDataClamped(endPos), false, 2, c =>
                {
                    return !(blocksPath(World.inst.GetPathCell(c), team)
                    //&& WorldRegions.Reachable(startCell, c)
                    );
                });
            }


            //    Func<Vector3, int, IEnumerable<Vector3>> expander = (position, lv) =>
            //    {
            //        CellMeta meta = Grid.Cells.Get((int)position.x, (int)position.z);
            //        if (meta == null)
            //            return new List<Vector3>();
            //        CellMeta[] neighbors = doDiagonal ? meta.neighborsPlusFast : meta.neighborsFast;
            //        List<Vector3> result = new List<Vector3>();
            //        for (int i = 0; i < neighbors.Length; i++)
            //            if (Math.Abs(neighbors[i].elevationTier - meta.elevationTier) <= PrebakedPathfinder.ElevationClimbThreshold && !blocksPath(neighbors[i].cell, team))
            //                result.Add(neighbors[i].Center);
            //        return result;
            //    };

            //    var queryable = HeuristicSearch.AStar(startPos, endPos, expander);
            //    path = queryable.ToList();

            //}catch(Exception ex)
            //{
            //    DebugExt.HandleException(ex);
            //}

            LinkedList<Node> pathNodes = aStar.Search(new Node(startCell, this), new Node(endCell, this), startCell);

            if(path.Count > 0)
                path.Clear();

            path.Add(startPos);

            foreach (Node node in pathNodes)
                path.Add(node.cell.Center);

            path.Add(endPos);

            //DebugExt.dLog($"path {startPos}{(upperGridStart ? "u" : "l")} to {endPos}{(upperGridEnd ? "u" : "l")} size {path.Count}");

            allowingDiagonals = false;

            pathing.Stop();
        }

        public class Node : IPathNode<Cell>
        {
            public ExternalPathfinder pathfinder;

            public int X => cell.x;
            public int Y => cell.z;

            public Cell cell;

            public Node(Cell cell, ExternalPathfinder pathfinder)
            {
                this.cell = cell;
                this.pathfinder = pathfinder;
            }

            public bool IsWalkable(Cell other)
            {
                CellMeta meta = Grid.Cells.Get(cell);

                if (meta)
                {
                    CellMeta otherMeta = Grid.Cells.Get(other);
                    if (otherMeta && Math.Abs(meta.elevationTier - otherMeta.elevationTier) <= 1)
                        return true;
                }

                if (!pathfinder.allowingDiagonals && Pathing.GetDiagonal(other, cell, out Diagonal diagonal))
                    return false;

                if (Pathing.BlockedCompletely(cell))
                    return false;

                return false;
            }

            public Cell GetContext()
            {
                return cell;
            }

        }
    }
}
