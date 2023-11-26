//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using Heuristic;
//using Heuristic.Linq;
//using Heuristic.Linq.Algorithms;

//namespace Elevation
//{
//    public class ExternalPathfinding : ElevationPathfinder
//    {
//        public static int width {get; private set; }
//        public static int height {get; private set; }

//        public override void Init(int width, int height)
//        {
//            ExternalPathfinding.width = width;
//            ExternalPathfinding.height = height;
//        }

//        public override void Path(Vector3 startPos, bool upperGridStart, Vector3 endPos, bool upperGridEnd, ref List<Vector3> path, Pathfinder.blocksPathTest blocksPath, Pathfinder.blocksPathTest pull, Pathfinder.applyExtraCost extraCost, int team, bool doDiagonal, bool doTrimming, bool allowIntergridTravel)
//        {
//            // Reformat end position to be within same region as start cell
//            Cell startCell = World.inst.GetCellDataClamped(startPos);
//            Cell endCell = World.inst.GetCellDataClamped(endPos);
//            if (!WorldRegions.Reachable(startCell, endCell))
//            {
//                endCell = World.inst.FindMatchingSurroundingCell(World.inst.GetCellDataClamped(endPos), false, 2, c =>
//                {
//                    return !(blocksPath(c, team) && WorldRegions.Reachable(startCell, c));
//                });
//            }


//            Func<Vector3, int, IEnumerable<Vector3>> expander = (position, lv) => 
//            {
//                CellMeta meta = Grid.Cells.Get((int)position.x, (int)position.z);
//                if (meta == null)
//                    return new List<Vector3>();
//                CellMeta[] neighbors = doDiagonal ? meta.neighborsPlusFast : meta.neighborsFast;
//                List<Vector3> result = new List<Vector3>();
//                for (int i = 0; i < neighbors.Length; i++)
//                    if (Math.Abs(neighbors[i].elevationTier - meta.elevationTier) <= PrebakedPathfinder.ElevationClimbThreshold && !blocksPath(neighbors[i].cell, team))
//                        result.Add(neighbors[i].Center);
//                return result;
//            };
            
//            var queryable = HeuristicSearch.AStar(startPos, endPos, expander);
//            path = queryable.ToList();
//        }
//    }
//}
