//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using Roy_T.AStar.Grids;
//using Roy_T.AStar.Primitives;
//using Roy_T.AStar.Paths;
//using Roy_T.AStar.Graphs;

//namespace Elevation
//{
//    public class RoyLibraryPathfinder : ElevationPathfinder
//    {
//        public Roy_T.AStar.Grids.Grid grid;

//        public override void Init(int width, int height)
//        {
//            var gridSize = new GridSize(World.inst.GridWidth, World.inst.GridHeight);
//            var cellSize = new Size(Distance.FromMeters(1), Distance.FromMeters(1));
//            var traversalVelocity = Velocity.FromKilometersPerHour(1);

//            grid = Roy_T.AStar.Grids.Grid.CreateGridWithLateralAndDiagonalConnections(gridSize, cellSize, traversalVelocity);
//        }

//        public override void Path(Vector3 startPos, bool upperGridStart, Vector3 endPos, bool upperGridEnd, ref List<Vector3> path, Pathfinder.blocksPathTest blocksPath, Pathfinder.blocksPathTest pull, Pathfinder.applyExtraCost extraCost, int team, bool doDiagonal, bool doTrimming, bool allowIntergridTravel)
//        {
//            var finder = new PathFinder();
//            var royPath = finder.FindPath(new GridPosition((int)startPos.x, (int)startPos.z), new GridPosition((int)endPos.x, (int)endPos.z), grid);

//            var edges = royPath.Edges;

//            if (edges.Count > 0)
//            {
//                var start = edges.ElementAt(0).Start.Position;
//                Cell cell = World.inst.GetCellData(new Vector3(start.X, 0f, start.Y));
//                if (cell != null)
//                    path.Add(cell.Center);
//            }

//            foreach (var edge in edges)
//            {
//                var end = edge.End.Position;

//                Cell cell = World.inst.GetCellData(new Vector3(end.X, 0f, end.Y));
//                if(cell != null)
//                    path.Add(cell.Center);
//            }


//        }
//    }
//}
