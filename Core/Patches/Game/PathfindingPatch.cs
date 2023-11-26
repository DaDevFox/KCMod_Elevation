using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Harmony;
using UnityEngine;
using System.Reflection;
using Fox.Profiling;
using Fox.Debugging;
using Elevation;

namespace Elevation.Patches
{
	public class OldPathfinding
	{
		//	public class NodeData
		//	{
		//		public List<Direction> visited = new List<Direction>();
		//	}

		//	private static ArrayExt<Pathfinder.Node> openSet = new ArrayExt<Pathfinder.Node>(500);
		//	private static Dictionary<string, NodeData> nodeMetaData = new Dictionary<string, NodeData>();

		//	private static List<Pathfinder.Node> scratchPathNode = new List<Pathfinder.Node>();

		//       #region Reflected Utils

		//	private static Vector3 GetPathPoint(Pathfinder.Node node, Pathfinder.Node[,] upperGrid)
		//	{
		//		Vector3 result = node.cell.Center;
		//		if (node.grid == upperGrid)
		//		{
		//			result = World.GetCellHeightPos(node.cell);
		//		}
		//		return result;
		//	}

		//	private static Pathfinder.Node SearchForClosestUnblockedCell(int sx, int sz, Vector3 start, Vector3 end, int teamId, Pathfinder pathfinder)
		//	{
		//		return (Pathfinder.Node) 
		//			typeof(Pathfinder)
		//			.GetMethod("SearchForClosestUnblockedCell", BindingFlags.NonPublic | BindingFlags.Instance)
		//			.Invoke(pathfinder, new object[] { sx, sz, start, end, teamId });
		//	}

		//	private static void ClearGrid(Pathfinder pathfinder)
		//	{
		//		typeof(Pathfinder)
		//			.GetMethod("ClearGrid", BindingFlags.NonPublic | BindingFlags.Instance)
		//			.Invoke(pathfinder, new object[0]);
		//	}

		//	private static void ClearOpenSet()
		//	{
		//		openSet.ClearFast();
		//	}

		//	private static void RemoveOpenSetAt(int idx)
		//	{
		//		openSet.RemoveAt(idx);
		//	}

		//	private static bool CheckVisited(Pathfinder.Node parent, Pathfinder.Node node)
		//	{
		//		string id = ElevationManager.GetCellMarkID(node.cell);
		//		if (!nodeMetaData.ContainsKey(id))
		//			nodeMetaData.Add(id, new NodeData());

		//		Direction dir;
		//		if (!PathingManager.GetCardinal(node.cell, parent.cell, out dir))
		//			Mod.dLog("Error: invalid direction");


		//		return nodeMetaData[id].visited.Contains(dir);
		//	}

		//	private static bool DetermineRemoveCell(int idx)
		//	{
		//		Pathfinder.Node node = openSet.data[idx];
		//		Cell cell = node.cell;
		//		CellMark meta = ElevationManager.GetCellMark(cell);

		//		string id = ElevationManager.GetCellMarkID(node.cell);
		//		if (!nodeMetaData.ContainsKey(id))
		//			nodeMetaData.Add(id, new NodeData());


		//		if (meta != null)
		//		{
		//			bool flag = true;
		//			foreach (Direction dir in meta.inverseBlockers)
		//				if (!nodeMetaData[id].visited.Contains(dir))
		//					flag = false;

		//			return flag;
		//		}
		//		return false;
		//	}

		//	private static void TrackNodeVisit(Pathfinder.Node parent, Pathfinder.Node node)
		//	{
		//		string id = ElevationManager.GetCellMarkID(node.cell);
		//		if (!nodeMetaData.ContainsKey(id))
		//			nodeMetaData.Add(id, new NodeData());

		//		Direction dir;
		//		if (!PathingManager.GetCardinal(node.cell, parent.cell, out dir))
		//			Mod.dLog("Error: invalid direction");
		//		if(!nodeMetaData[id].visited.Contains(dir))
		//			nodeMetaData[id].visited.Add(dir);
		//	}


		//	private static void AddToOpenSet(Pathfinder.Node node)
		//	{
		//		openSet.Add(node);
		//	}

		//	private static void AddToOpenSet(
		//		Pathfinder.Node[,] upperPathGrid,
		//		Pathfinder.blocksPathTest blocksPath,
		//		Pathfinder.applyExtraCost extraCost,

		//		int endX,
		//		int endZ,



		//		Pathfinder.Node node, 
		//		Pathfinder.Node parent, 
		//		int sG, 
		//		int teamId)
		//	{
		//		if (!node.visited)
		//		{
		//			Cell cell = node.cell;
		//			bool flag = node.grid == upperPathGrid;
		//			if (!GetPathBlocked(upperPathGrid, node, blocksPath, teamId))
		//			{
		//				int num = sG;
		//				if (!node.isFreeCell && !flag)
		//				{
		//					num += extraCost(cell, teamId);
		//				}
		//				num += parent.G;
		//				if (!node.inOpenList || node.G > num)
		//				{
		//					int num2 = cell.x - endX;
		//					if (num2 < 0)
		//					{
		//						num2 *= -1;
		//					}
		//					int num3 = cell.z - endZ;
		//					if (num3 < 0)
		//					{
		//						num3 *= -1;
		//					}
		//					int num4 = (num2 + num3) * 10;
		//					int f = num + num4;
		//					node.G = num;
		//					node.F = f;
		//					node.parent = parent;
		//					if (!node.inOpenList)
		//					{
		//						AddToOpenSet(node);
		//						node.inOpenList = true;
		//					}
		//				}
		//			}
		//		}
		//	}

		//	private static bool GetPathBlocked(
		//		Pathfinder.Node[,] upperPathGrid,

		//		Pathfinder.Node n, 
		//		Pathfinder.blocksPathTest bt, 
		//		int teamId)
		//	{
		//		if (n.grid != upperPathGrid)
		//		{
		//			if (bt(n.cell, teamId))
		//			{
		//				return true;
		//			}
		//		}
		//		else if (n.cell.isUpperGridBlocked)
		//		{
		//			return true;
		//		}
		//		return false;
		//	}


		//	#endregion



		//	private static void FindPath(
		//		Pathfinder __instance,

		//		int ___numCellsDown,
		//		int ___numCellsAcross,

		//		Pathfinder.Node[,] ___pathGrid,
		//		Pathfinder.Node[,] ___upperPathGrid,

		//		Vector3 startPos,
		//		bool startUseUpperGrid,
		//		Vector3 endPos,
		//		bool endUseUpperGrid,

		//		ref List<Vector3> path,

		//		Pathfinder.blocksPathTest bt,
		//		Pathfinder.blocksPathTest pullBlock,
		//		Pathfinder.applyExtraCost ec,

		//		int teamId,

		//		bool doDiagonal,
		//		bool doTrimming,
		//		bool allowUpperGrid)
		//	{
		//		path.Clear();
		//		Mod.dLog("finding path");

		//		// Clamp world space positions to grid positions
		//		int sx = Mathf.FloorToInt(startPos.x);
		//		int sz = Mathf.FloorToInt(startPos.z);
		//		int ex = Mathf.FloorToInt(endPos.x);
		//		int ez = Mathf.FloorToInt(endPos.z);

		//		// Check if position is inside grid
		//		if (
		//			sx >= 0 && sx < ___numCellsAcross &&
		//			sz >= 0 && sz < ___numCellsDown &&
		//			ex >= 0 && ex < ___numCellsAcross &&
		//			ez >= 0 && ez < ___numCellsDown)
		//		{
		//			Pathfinder.Node start = (!startUseUpperGrid) ? ___pathGrid[sx, sz] : ___upperPathGrid[sx, sz];
		//			Pathfinder.Node end = (!endUseUpperGrid) ? ___pathGrid[ex, ez] : ___upperPathGrid[ex, ez];

		//			// Check for redundant path
		//			if (start.cell == end.cell)
		//			{
		//				if (startUseUpperGrid == endUseUpperGrid)
		//				{
		//					path.Add(GetPathPoint(end, ___upperPathGrid));
		//					return;
		//				}
		//			}

		//			// Check for blocked start cell
		//			if(bt(start.cell, teamId) || PathingManager.BlockedCompletely(start.cell))
		//			{
		//				// NOTE: Changed [endPos, startPos] to [startPos, endPos]
		//				Pathfinder.Node newStart = SearchForClosestUnblockedCell(sx, sz, startPos, endPos, teamId, __instance);
		//				if (newStart != null)
		//				{
		//					start = newStart;
		//				}
		//			}

		//			// Check for blocked end cell
		//			if (bt(end.cell, teamId) || PathingManager.BlockedCompletely(end.cell))
		//			{
		//				// NOTE: above change here also, but vice versa
		//				Pathfinder.Node newEnd = SearchForClosestUnblockedCell(ex, ez, endPos, startPos, teamId, __instance);
		//				if (newEnd != null)
		//				{
		//					end = newEnd;
		//				}
		//			}

		//			ClearGrid(__instance);

		//			// Check for destination building bounding footprint
		//			if (!endUseUpperGrid)
		//			{
		//				Cell cellDataUnsafe = World.inst.GetCellDataUnsafe(ex, ez);
		//				if (cellDataUnsafe.OccupyingStructure.Count > 0)
		//				{
		//					int minX;
		//					int minZ;
		//					int maxX;
		//					int maxZ;
		//					cellDataUnsafe.OccupyingStructure[0].GetBoundingFootprint(out minX, out minZ, out maxX, out maxZ);
		//					for (int i = minZ; i <= maxZ; i++)
		//					{
		//						for (int j = minX; j <= maxX; j++)
		//						{
		//							if (j != ex || i != ez)
		//							{
		//								___pathGrid[j, i].isFreeCell = true;
		//							}
		//						}
		//					}
		//				}
		//			}

		//			// Check for start building bounding footprint
		//			if (!startUseUpperGrid)
		//			{
		//				Cell cellDataUnsafe2 = World.inst.GetCellDataUnsafe(sx, sz);
		//				if (cellDataUnsafe2.OccupyingStructure.Count > 0)
		//				{
		//					int minX;
		//					int minZ;
		//					int maxX;
		//					int maxZ;
		//					cellDataUnsafe2.OccupyingStructure[0].GetBoundingFootprint(out minX, out minZ, out maxX, out maxZ);
		//					for (int k = minZ; k <= maxZ; k++)
		//					{
		//						for (int l = minX; l <= maxX; l++)
		//						{
		//							if (l != start.cell.x || k != start.cell.z)
		//							{
		//								___pathGrid[l, k].isFreeCell = true;
		//							}
		//						}
		//					}
		//				}
		//			}

		//			// Preperation
		//			ClearOpenSet();
		//			AddToOpenSet(start);

		//			start.F = 2147483646;

		//			int cellsAcross = ___numCellsAcross;
		//			int cellsDown = ___numCellsDown;

		//			// Main loop
		//			while(openSet.Count > 0)
		//			{
		//				int idx = -1;
		//				int min = int.MaxValue;
		//				int max = 0;
		//				int i = 0;
		//				int count = openSet.Count;

		//				// Find the node with the lowest F cost; the path of least resistance evaluated so far
		//				while (i < count)
		//				{
		//					Pathfinder.Node current = openSet.data[i];
		//					if (current.F < min)
		//					{
		//						min = current.F;
		//						max = current.G;
		//						idx = i;
		//					}
		//					else if (current.F == min)
		//					{
		//						if (current.G > max)
		//						{
		//							min = current.F;
		//							max = current.G;
		//							idx = i;
		//						}
		//					}
		//					i++;
		//				}

		//				Pathfinder.Node bestNode = openSet.data[idx];

		//				// Track the direction from which this node has been visitied; it can later be used to cull out blocked tiles
		//				TrackNodeVisit(bestNode.parent, bestNode);

		//				// Remove the node from the open set if it has been visited from all possible directions using data from tracking node visits
		//				if (DetermineRemoveCell(idx))
		//				{
		//					RemoveOpenSetAt(idx);

		//					// NOTE: visited and inOpenList here actually mean 'visited multiple times and deemed unpathable'
		//					bestNode.inOpenList = false;
		//					bestNode.visited = true;
		//				}

		//				// Finish if this is the destination
		//				if (bestNode == end)
		//					break;

		//				Cell bestCell = bestNode.cell;
		//				int x = bestCell.x;
		//				int z = bestCell.z;

		//				Pathfinder.Node[,] grid = bestNode.grid;

		//				// Add neighbors cells to open list
		//				if (x + 1 < cellsAcross)
		//				{
		//					AddToOpenSet(
		//						___upperPathGrid,
		//						bt,
		//						ec,
		//						ex,
		//						ez,

		//						grid[x + 1, z], bestNode, 10, teamId);
		//				}
		//				if (x - 1 >= 0)
		//				{
		//					AddToOpenSet(
		//						___upperPathGrid,
		//						bt,
		//						ec,
		//						ex,
		//						ez,

		//						grid[x - 1, z], bestNode, 10, teamId);
		//				}
		//				if (z + 1 < cellsDown)
		//				{
		//					AddToOpenSet(
		//						___upperPathGrid,
		//						bt,
		//						ec,
		//						ex,
		//						ez,

		//						grid[x, z + 1], bestNode, 10, teamId);
		//				}
		//				if (z - 1 >= 0)
		//				{
		//					AddToOpenSet(
		//						___upperPathGrid,
		//						bt,
		//						ec,
		//						ex,
		//						ez, 

		//						grid[x, z - 1], bestNode, 10, teamId);
		//				}
		//				if (doDiagonal && !bestCell.isPathCell && bestNode.grid == ___pathGrid)
		//				{
		//					if (x + 1 < cellsAcross)
		//					{
		//						if (z + 1 < cellsDown)
		//						{
		//							AddToOpenSet(___upperPathGrid,
		//								bt,
		//								ec,
		//								ex,
		//								ez,

		//								grid[x + 1, z + 1], bestNode, 14, teamId);
		//						}
		//						if (z - 1 >= 0)
		//						{
		//							AddToOpenSet(
		//								___upperPathGrid,
		//								bt,
		//								ec,
		//								ex,
		//								ez,

		//								grid[x + 1, z - 1], bestNode, 14, teamId);
		//						}
		//					}
		//					if (x - 1 >= 0)
		//					{
		//						if (z + 1 < cellsDown)
		//						{
		//							AddToOpenSet(
		//								___upperPathGrid,
		//								bt,
		//								ec,
		//								ex,
		//								ez,

		//								grid[x - 1, z + 1], bestNode, 14, teamId);
		//						}
		//						if (z - 1 >= 0)
		//						{
		//							AddToOpenSet(
		//								___upperPathGrid,
		//								bt,
		//								ec,
		//								ex,
		//								ez,

		//								grid[x - 1, z - 1], bestNode, 14, teamId);
		//						}
		//					}
		//				}

		//				// Add the node to the corresponding grid
		//				if (bestCell.isStairs && allowUpperGrid)
		//				{
		//					Pathfinder.Node[,] array = ___pathGrid;
		//					if (bestNode.grid == ___pathGrid)
		//					{
		//						array = ___upperPathGrid;
		//					}

		//					AddToOpenSet(
		//						___upperPathGrid,
		//						bt,
		//						ec,
		//						ex,
		//						ez, 

		//						array[x, z], bestNode, 10, teamId);
		//				}

		//				if (openSet.Count == 0)
		//				{
		//					break;
		//				}
		//			}

		//			// Trims somthing?? idk what this does really, but it's in the original code, it's not a neccessary step of the A* algorithm
		//			if (doTrimming)
		//				typeof(Pathfinder)
		//					.GetMethod("StringPull", BindingFlags.NonPublic | BindingFlags.Instance)
		//					.Invoke(__instance, new object[] { start, end, teamId, pullBlock });

		//			// For upper grid only
		//			bool lowerGrid = !endUseUpperGrid && !startUseUpperGrid;
		//			scratchPathNode.Clear();
		//			Pathfinder.Node currentNode = start;
		//			float minY = 999f;

		//			// Find path from retracing nodes' parents
		//			while (currentNode != start && currentNode != null)
		//			{
		//				bool stairCell = currentNode.grid == ___upperPathGrid && currentNode.cell.isStairs;
		//				Vector3 pathPoint = GetPathPoint(currentNode, ___upperPathGrid);
		//				if (!stairCell && pathPoint.y > minY && !lowerGrid)
		//				{
		//					pathPoint.y = minY;
		//				}
		//				path.Insert(0, pathPoint);
		//				scratchPathNode.Insert(0, currentNode);
		//				currentNode = currentNode.parent;
		//				minY = pathPoint.y;
		//			}

		//			// Format path into 'path' list
		//			bool valid = currentNode == start;
		//			if (valid)
		//			{
		//				if (!lowerGrid)
		//				{
		//					minY = startPos.y;
		//					for (int n = 0; n < scratchPathNode.Count; n++)
		//					{
		//						bool stairCell = scratchPathNode[n].grid == ___upperPathGrid && scratchPathNode[n].cell.isStairs;
		//						Vector3 pathPoint = GetPathPoint(scratchPathNode[n], ___upperPathGrid);
		//						if (!stairCell && pathPoint.y > minY)
		//						{
		//							pathPoint.y = minY;
		//						}
		//						if (path[n].y < pathPoint.y)
		//						{
		//							Vector3 value = path[n];
		//							value.y = pathPoint.y;
		//							path[n] = value;
		//						}
		//						minY = pathPoint.y;
		//					}
		//				}
		//				endPos.y = GetPathPoint(end, ___upperPathGrid).y;
		//				path.Add(endPos);
		//			}
		//			else
		//			{
		//				path.Clear();
		//			}
		//		}
		//	}


		//}






		////[HarmonyPatch(typeof(Pathfinder),
		////	"AddToOpenSet",
		////	new Type[] {
		////		typeof(Pathfinder.Node),
		////		typeof(Pathfinder.Node),
		////		typeof(int),
		////		typeof(int)
		////	})]
		//public class PathfindingBlockerCheckPatch
		//{
		//	//	[Profile]
		//	//	static bool Prefix(
		//	//		Pathfinder __instance,

		//	//		Pathfinder.Node[,] ___upperPathGrid,
		//	//		Pathfinder.blocksPathTest ___blocksPath,
		//	//		Pathfinder.applyExtraCost ___extraCost,
		//	//		int ___endX,
		//	//		int ___endZ,


		//	//		Pathfinder.Node node,
		//	//		Pathfinder.Node parent,
		//	//		int sG,
		//	//		int teamId
		//	//		)
		//	//	{
		//	//		Cell cell = node.cell;
		//	//		bool flag = node.grid == ___upperPathGrid;
		//	//		if (!blocked)
		//	//		{
		//	//			int num = sG;
		//	//			if (!node.isFreeCell && !flag)
		//	//			{
		//	//				num += ___extraCost(cell, teamId);
		//	//			}
		//	//			num += parent.G;
		//	//			if (!node.inOpenList || node.G > num)
		//	//			{
		//	//				int num2 = cell.x - ___endX;
		//	//				if (num2 < 0)
		//	//				{
		//	//					num2 *= -1;
		//	//				}
		//	//				int num3 = cell.z - ___endZ;
		//	//				if (num3 < 0)
		//	//				{
		//	//					num3 *= -1;
		//	//				}
		//	//				int num4 = (num2 + num3) * 10;
		//	//				int f = num + num4;
		//	//				node.G = num;
		//	//				node.F = f;
		//	//				node.parent = parent;
		//	//				if (!node.inOpenList)
		//	//				{
		//	//					typeof(Pathfinder)
		//	//						.GetMethod("AddToOpenSet", BindingFlags.Instance | BindingFlags.NonPublic)
		//	//						.Invoke(__instance, new object[] { node });
		//	//					node.inOpenList = true;
		//	//				}
		//	//			}
		//	//		}


		//	//		return false;
		//	//	}

		//	public static bool BlocksPathDirectional(Cell from, Cell to)
		//	{
		//		try
		//		{
		//			CellMark metaFrom = ElevationManager.GetCellMark(from);
		//			CellMark metaTo = ElevationManager.GetCellMark(to);

		//			Dictionary<Vector3, Direction> dirs = new Dictionary<Vector3, Direction>()
		//				{
		//					{ new Vector3(1f, 0f, 0f), Direction.East },
		//					{ new Vector3(0f, 0f, 1f), Direction.South },
		//					{ new Vector3(-1f, 0f, 0f), Direction.West },
		//					{ new Vector3(0f, 0f, -1f), Direction.North },
		//				};

		//			Dictionary<Vector3, Diagonal> diagonals = new Dictionary<Vector3, Diagonal>()
		//				{
		//					{ new Vector3(1f,0f,1f), Diagonal.SouthEast },
		//					{ new Vector3(1f,0f,-1f), Diagonal.NorthEast },
		//					{ new Vector3(-1f,0f,1f), Diagonal.SouthWest },
		//					{ new Vector3(-1f,0f,-1f), Diagonal.NorthWest },
		//				};


		//			if (metaFrom != null && metaTo != null)
		//			{
		//				if (metaFrom.elevationTier > 0 || metaTo.elevationTier > 0)
		//				{
		//					Vector3 diff = from.Center - to.Center;
		//					Vector3 diffNormalized = Vector3.ClampMagnitude(new Vector3(diff.x, 0f, diff.z), 1f);

		//					bool validCardinal = false;
		//					Direction dir = Direction.North;

		//					if (dirs.ContainsKey(diffNormalized))
		//					{
		//						validCardinal = true;
		//						dir = dirs[diffNormalized];
		//					}


		//					if (validCardinal)
		//					{
		//						if (metaFrom.blockers.Contains(dir) || metaTo.blockers.Contains(dir))
		//							return true;
		//						else
		//							return false;
		//					}
		//					else
		//						return true;
		//				}
		//			}
		//		}
		//		catch (Exception ex)
		//		{
		//			DebugExt.HandleException(ex);
		//		}
		//		return false;
		//	}
		//}

		//[HarmonyPatch(typeof(Pathfinder), "SearchForClosestUnblockedCell")]
		//public class BlockedSearchCellPatch
		//{
		//	[Profile]
		//	static void Postfix(Pathfinder __instance, ref Pathfinder.Node __result, int sx, int sz, Vector3 start, Vector3 end, int teamId)
		//	{
		//		try
		//		{
		//			Cell cell = null;
		//			float min = float.MaxValue;
		//			int num = 1;
		//			int xMin = Mathf.Clamp(sx - num, 0, World.inst.GridWidth - 1);
		//			int xMax = Mathf.Clamp(sx + num, 0, World.inst.GridWidth - 1);
		//			int zMin = Mathf.Clamp(sz - num, 0, World.inst.GridHeight - 1);
		//			int zMax = Mathf.Clamp(sz + num, 0, World.inst.GridHeight - 1);
		//			for (int i = xMin; i <= xMax; i++)
		//			{
		//				for (int j = zMin; j <= zMax; j++)
		//				{
		//					Cell current = World.inst.GetCellDataUnsafe(i, j);
		//					if (current != null)
		//					{
		//						if (!__instance.blocksPath(current, teamId) && 
		//							!PathingManager.BlockedCompletely(current) &&
		//							!PathfindingBlockerCheckPatch.BlocksPathDirectional(World.inst.GetCellDataClamped(start), current))
		//						{
		//							float dist = Mathff.DistSqrdXZ(current.Center, start);
		//							if (dist < min)
		//							{
		//								min = dist;
		//								cell = current;
		//							}
		//						}
		//					}
		//				}
		//			}
		//			if (cell != null)
		//			{
		//				__result = __instance.GetFieldValue<Pathfinder.Node[,]>("pathGrid")[cell.x, cell.z];
		//				return;
		//			}
		//			__result = null;
		//		}
		//		catch(Exception ex)
		//		{
		//			DebugExt.HandleException(ex);
		//		}
		//	}


		//	public static void DoElevationBlock(Cell cell, ref bool result)
		//	{

		//		CellMark meta = ElevationManager.GetCellMark(cell);
		//		if (meta != null)
		//		{
		//			if (PathingManager.BlockedCompletely(cell))
		//			{
		//				result = true;
		//			}
		//		}
		//	}

		//	static void Finalizer(Exception __exception)
		//	{
		//		DebugExt.HandleException(__exception);
		//	}

		//}



		////[HarmonyPatch(typeof(Pathfinder), "FindPath")]
		////class FindPathRecalculatePatch
		////{
		////	[Profile]
		////	static void Prefix(Pathfinder __instance, ref Vector3 startPos, ref Vector3 endPos, int teamId)
		////	{
		////		bool flagStart = false;
		////		bool flagEnd = false;

		////		Cell start = World.inst.GetCellData(startPos);
		////		Cell end = World.inst.GetCellData(endPos);

		////		Pathfinder.Node newStart = null;
		////		Pathfinder.Node newEnd = null;

		////		if (start != null)
		////		{
		////			if (PathingManager.BlockedCompletely(start))
		////			{
		////				newStart = typeof(Pathfinder).GetMethod("SearchForClosestUnblockedCell", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[]
		////				{
		////					(int)startPos.x,
		////					(int)startPos.z,
		////					startPos,
		////					endPos,
		////					teamId
		////				}) as Pathfinder.Node;

		////				Mod.dLog("redirecting" + start.Center.ToString() + " to " + newStart.cell.Center.ToString());

		////				flagStart = true;
		////			}
		////		}
		////		if (end != null)
		////		{
		////			if (PathingManager.BlockedCompletely(end))
		////			{
		////				newEnd = typeof(Pathfinder).GetMethod("SearchForClosestUnblockedCell", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[]
		////				{
		////					(int)endPos.x,
		////					(int)endPos.z,
		////					endPos,
		////					startPos,
		////					teamId
		////				}) as Pathfinder.Node;

		////				flagEnd = true;
		////			}
		////		}

		////		if (flagStart)
		////		{
		////			startPos = newStart.cell.Center;
		////		}
		////		if (flagEnd)
		////		{
		////			endPos = newEnd.cell.Center;
		////		}
		////	}

		////	static void Postfix(Vector3 startPos, Vector3 endPos, ref List<Vector3> path)
		////	{
		////		if (path.Count == 0)
		////			Mod.dLog($"Aborted path from [{startPos}] to [{endPos}]");
		////	}

		////	static void Finalizer(Exception __exception)
		////	{
		////		DebugExt.HandleException(__exception);
		////	}


		////}


		//[HarmonyPatch(typeof(Pathfinder), "FindPathRaw")]
		//class FindPathRawRecalculatePatch
		//{
		//	[Profile]
		//	static void Prefix(Pathfinder __instance, ref Vector3 startPos, ref Vector3 endPos, int teamId)
		//	{
		//		bool flagStart = false;
		//		bool flagEnd = false;

		//		Cell start = World.inst.GetCellData(startPos);
		//		Cell end = World.inst.GetCellData(endPos);

		//		Pathfinder.Node newStart = null;
		//		Pathfinder.Node newEnd = null;

		//		if (start != null)
		//		{
		//			if (PathingManager.BlockedCompletely(start))
		//			{
		//				newStart = typeof(Pathfinder).GetMethod("SearchForClosestUnblockedCell", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[]
		//				{
		//					(int)startPos.x,
		//					(int)startPos.z,
		//					startPos,
		//					endPos,
		//					teamId
		//				}) as Pathfinder.Node;

		//				flagStart = true;
		//			}
		//		}
		//		if (end != null)
		//		{
		//			if (PathingManager.BlockedCompletely(end))
		//			{
		//				newEnd = typeof(Pathfinder).GetMethod("SearchForClosestUnblockedCell", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[]
		//				{
		//					(int)endPos.x,
		//					(int)endPos.z,
		//					endPos,
		//					startPos,
		//					teamId
		//				}) as Pathfinder.Node;

		//				flagEnd = true;
		//			}
		//		}

		//		if (flagStart)
		//		{
		//			startPos = newStart.cell.Center;
		//		}
		//		if (flagEnd)
		//		{
		//			endPos = newEnd.cell.Center;
		//		}
		//	}




		//	static void Finalizer(Exception __exception)
		//	{
		//		DebugExt.HandleException(__exception);
		//	}

		//}


		//#region Path Blockers

		//[HarmonyPatch(typeof(World), "GetBlocksFootPath")]
		//class WorldFootPathBlockerPatch
		//{
		//	static void Postfix(ref bool __result, Cell cell)
		//	{
		//		BlockedSearchCellPatch.DoElevationBlock(cell, ref __result);

		//	}
		//}

		//[HarmonyPatch(typeof(World), "GetBlocksFootPathForArmies")]
		//class WorldArmyPathBlockerPatch
		//{
		//	static void Postfix(ref bool __result, Cell c)
		//	{
		//		BlockedSearchCellPatch.DoElevationBlock(c, ref __result);
		//	}
		//}

		//[HarmonyPatch(typeof(World), "GetBlocksFootPathForOgres")]
		//class WorldOgreFootPathBlockerPatch
		//{
		//	static void Postfix(ref bool __result, Cell cell)
		//	{

		//		BlockedSearchCellPatch.DoElevationBlock(cell, ref __result);
		//	}
		//}

		//[HarmonyPatch(typeof(Pathfinder), "GetPathBlocked")]
		//class PathfinderPathBlockerPatch
		//{
		//	static void Postfix(ref bool __result, Pathfinder.Node n)
		//	{
		//		__result &= !PathfindingBlockerCheckPatch.BlocksPathDirectional(n.parent.cell, n.cell);
		//	}
		//}

		//[HarmonyPatch(typeof(PlacementMode), "blocksPath")]
		//class PlacementModePathBlockerPatch
		//{
		//	static void Postfix(ref bool __result, Cell c)
		//	{
		//		BlockedSearchCellPatch.DoElevationBlock(c, ref __result);
		//	}
		//}

		//#endregion
	}


	[HarmonyPatch(typeof(Pathfinder), "FindPath")]
	public class PathfindingFindPathRedirect
	{
		public static long msThreshold { get; private set; } = 30;

		private static Stopwatch timer = new Stopwatch();

		static bool Prefix(
			Pathfinder __instance,

			ref Vector3 startPos,
			bool startUseUpperGrid,
			ref Vector3 endPos,
			bool endUseUpperGrid,

			ref List<Vector3> path,

			Pathfinder.blocksPathTest bt,
			Pathfinder.blocksPathTest pullBlock,
			Pathfinder.applyExtraCost ec,

			int teamId,

			bool doDiagonal,
			bool doTrimming,
			bool allowUpperGrid)
		{
			Cell endCell = World.inst.GetCellData(endPos);
			if (WorldRegions.Unreachable.Contains(endCell))
			{
				Cell newCell = Pathing.FindNearUnblockedFast(endCell, 2);
				endPos = newCell.Center;
			}

			Cell startCell = World.inst.GetCellData(startPos);
			if (WorldRegions.Unreachable.Contains(startCell))
			{
				Cell newCell = Pathing.FindNearUnblockedFast(startCell, 2);
				startPos = newCell.Center;
			}

			//if (Settings.debug)
			//	timer.Start();

			//if (ElevationPathfinder.main != null)
			//{
			//	try
			//	{
			//		ElevationPathfinder.Path(
			//			startPos,
			//			startUseUpperGrid,
			//			endPos,
			//			endUseUpperGrid,

			//			ref path,

			//			bt,
			//			pullBlock,
			//			ec,

			//			teamId,

			//			doDiagonal,
			//			doTrimming,
			//			allowUpperGrid,
			//			__instance.GetHashCode());

			//	}
			//	catch(Exception ex)
			//             {
			//		DebugExt.HandleException(ex);
			//             }

			//	if (path.Count == 0)
			//		Mod.dLog($"Failed to path from {startPos}{(startUseUpperGrid ? "u" : "l")} to {endPos}{(endUseUpperGrid ? "u" : "l")} in {timer.ElapsedMilliseconds} ms");

			//	if (Settings.debug)
			//	{
			//		timer.Stop();
			//		if (timer.ElapsedMilliseconds > msThreshold)
			//			Mod.dLog($"p {startPos}{(startUseUpperGrid ? "u" : "l")} to {endPos}{(endUseUpperGrid ? "u" : "l")}: {path.Count} node path created in {timer.ElapsedMilliseconds} ms");
			//	}


			//	return false;
			//}
			//else
			return true;
		}

		static void Postfix(
			Vector3 startPos,
			bool startUseUpperGrid,
			Vector3 endPos,
			bool endUseUpperGrid,

			ref List<Vector3> path)
        {
			//if (Settings.debug && ElevationPathfinder.main == null)
			//{
			//	timer.Stop();
			//	if (timer.ElapsedMilliseconds > msThreshold)
			//		Mod.dLog($"p {startPos}{(startUseUpperGrid ? "u" : "l")} to {endPos}{(endUseUpperGrid ? "u" : "l")}: {path.Count} node path created in {timer.ElapsedMilliseconds} ms");
			//}
		}
	}

	[HarmonyPatch(typeof(Pathfinder), "FindPathRaw")]
	public class PathfindingFindPathRawRedirect
	{
		static bool Prefix(
			ref Vector3 startPos, 
			ref Vector3 endPos,
			
			ref List<Vector3> path,
			
			Pathfinder.blocksPathTest bt, 
			Pathfinder.applyExtraCost ec,
			int teamId)
		{
            Cell endCell = World.inst.GetCellData(endPos);
            if (WorldRegions.Unreachable.Contains(endCell))
            {
                Cell newCell = Pathing.FindNearUnblockedFast(endCell, 2);
                endPos = newCell.Center;

                Mod.dLog($"redirected; {endCell.x}_{endCell.z} changed to {newCell.x}_{newCell.z}");
            }

            Cell startCell = World.inst.GetCellData(startPos);
            if (WorldRegions.Unreachable.Contains(startCell))
            {
                Cell newCell = Pathing.FindNearUnblockedFast(startCell, 2);
                startPos = newCell.Center;

                Mod.dLog($"redirected; {startCell.x}_{startCell.z} changed to {newCell.x}_{newCell.z}");
            }

            //if (ElevationPathfinder.main != null)
            //{
            //	ElevationPathfinder.main.Path(
            //		startPos, false,
            //		endPos, false,

            //		ref path,

            //		bt,
            //		bt,
            //		ec,

            //		teamId,

            //		false, false, false);


            //	return false;
            //}
            //else
            return true;
		}
	}

	//[HarmonyPatch(typeof(Villager), "MoveTo")]
	public class RedirectToThreadedPatch
	{
		static bool Prefix(Villager __instance, Vector3 newPos)
		{
			__instance.MoveToDeferred(newPos);
			return false;
		}
	}

}


namespace Fox.Debugging
{
	//[HarmonyPatch(typeof(PersonUI), "UpdateInternal")]
	public class PeasantPathDebug
	{
		private static LineRenderer _line;
		private static Guid? lastSelected = null;

		private static float width = 0.1f;
		private static Color color = Color.green;

		static void Postfix(PersonUI __instance)
		{
			if (!_line)
			{
				_line = new GameObject("PathDrawer").AddComponent<LineRenderer>();

				_line.startWidth = width;
				_line.endWidth = width;

				_line.startColor = color;
				_line.endColor = color;

				_line.material = new Material(Shader.Find("Standard"));
				_line.alignment = LineAlignment.Local;
			}

			//if (DebugLines.active)
			//{
			Guid? selected = __instance.villager != null ? (Guid?)__instance.villager.guid : null;

			if (selected != lastSelected || Input.GetKeyDown(Settings.keycode_updatePathView))
			{
				Log(__instance.villager);
				if (__instance.villager != null)
				{
					_line.numPositions = __instance.villager.travelPath.Count;
					_line.SetPositions(__instance.villager.travelPath.data);
				}
				else
					_line.SetPositions(new Vector3[0]);

				lastSelected = selected;
			}
			//}
		}

		public static void Log(Villager villager)
        {
			GamePath path = (typeof(Villager).GetField("deferredPath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(villager) as GamePath);

			string text = "";

			text += $"Paralyzed: {villager.paralyzed}";
			//text += $"Debug: {path.debug}";
			text += $"\nGamePath: \n\tStatus: {path.status}\n\tStart: {path.start}\n\tEnd: {path.end}\n\tResult: size {path.result.Count}";
			text += $"\nTravelPath: size {villager.travelPath.Count}";

			DebugExt.dLog(text);
			DebugExt.dLog("Start", false, path.start);
			DebugExt.dLog("End", false, path.end);
			if (villager.travelPath.Count > 0)
				DebugExt.dLog("Current", false, villager.travelPath.data[0]);
        }
	}

}