using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;
using Elevation;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Elevation.Utils;

namespace Elevation
{
    public abstract class ElevationPathfinder
    {
		/// <summary>
		/// Setting to true will recognize when a path is taking too long on a worker thread and retry it with modified coordinates. 
		/// <para>Setting to false will leave the thread blocked</para>
		/// </summary>
		public static bool retryFailedPaths = false;
		/// <summary>
		/// Setting to true will mark when a thread gets blocked and reroute paths meant to be pathed on it to other threads.
		/// <para>This will reduce the symptoms of an underlying pathfinding problem but ultimately solve nothing, it essentially gives you six extra lives (for six worker threads) before all threaded paths stop working because all of the threads got blocked</para>
		/// </summary>
		public static bool markBlockedThreads = false;

		public struct PathData
        {
			int fromX;
			int fromZ;
			int toX;
			int toZ;

			public PathData(int fromX, int fromZ, int toX, int toZ)
            {
				this.fromX = fromX;
				this.fromZ = fromZ;
				this.toX = toX;
				this.toZ = toZ;
            }

			public PathData(GamePath path)
            {
				this.fromX = (int)path.start.x;
				this.fromZ = (int)path.start.z;
				this.toX = (int)path.end.x;
				this.toZ = (int)path.end.z;
            }
        }

		public static Vector3 failedPathSignal = new Vector3(-1f, -1f, -1f);

        public static Type current { get; } = typeof(ExternalPathfinderModified);

        public static ElevationPathfinder main { get; private set; }
        public static Dictionary<int, ElevationPathfinder> pathfinders { get; private set; } = new Dictionary<int, ElevationPathfinder>();
		/// <summary>
		/// Stores the number of times a path from a specific location to another location has been retried
		/// </summary>
		public static Dictionary<PathData, int> gamePathRetries { get; } = new Dictionary<PathData, int>();

        public static int backedUpThreshold { get; } = 10000;

		public int aliasHashCode { get; internal set; }

        public static void InitAll(int width, int height)
        {
            if (main == null)
            {
                main = Activator.CreateInstance(current) as ElevationPathfinder;
				main.aliasHashCode = World.inst.pather.GetHashCode();

				pathfinders.Add(World.inst.pather.GetHashCode(), main);
			}
            main.Init(width, height);

            int threadCount = ((Thread[])typeof(ThreadedPathing).GetField("workerThreads", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(World.inst.threadedPather)).Length;
            Pathfinder[] pathers = (Pathfinder[])typeof(ThreadedPathing).GetField("pather", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(World.inst.threadedPather);
            
            for (int i = 0; i < threadCount; i++)
            {
                int hash = pathers[i].GetHashCode();

                if (!pathfinders.ContainsKey(hash))
                {
                    ElevationPathfinder created = Activator.CreateInstance(current) as ElevationPathfinder;
					created.aliasHashCode = hash;

					created.Init(width, height);
                    pathfinders.Add(hash, created);
                }

                pathfinders[hash].Init(width, height);

			}

			if (Settings.debug)
				DebugPathVisualizer.Init();
        }

        public static void Path(
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
            bool allowIntergridTravel,
            int hashCode = -1)
        {
            if (hashCode == -1)
                main.Path(
                    startPos, upperGridStart, endPos, upperGridEnd,

                    ref path,

                    blocksPath, pull, extraCost,

                    team,

                    doDiagonal, doTrimming, allowIntergridTravel);

            if (!pathfinders.ContainsKey(hashCode))
            {
                ElevationPathfinder created = Activator.CreateInstance(current) as ElevationPathfinder;
                created.Init(World.inst.GridWidth, World.inst.GridHeight);
				created.aliasHashCode = hashCode;
                pathfinders.Add(hashCode, created);
            }

            // TEMP
            if(current.IsSubclassOf(typeof(ExternalPathfinder)) && (pathfinders[hashCode] as ExternalPathfinder).pathing.ElapsedMilliseconds > backedUpThreshold)
            {
                Mod.dLog($"Potential backed up pathfinder: p{hashCode} pathing for {(pathfinders[hashCode] as ExternalPathfinder).pathing.ElapsedMilliseconds} ms");
            }

            pathfinders[hashCode].Path(startPos, upperGridStart, endPos, upperGridEnd,

                    ref path,

                    blocksPath, pull, extraCost,

                    team,

                    doDiagonal, doTrimming, allowIntergridTravel);



        }

        public abstract void Init(int width, int height);

        public abstract void Path(
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
            bool allowIntergridTravel);
    }


	[HarmonyPatch(typeof(Pathfinder), "AddToOpenSet")]
	static class NodeAddingPatch
	{
		static bool Prefix(Pathfinder.Node node, Pathfinder.Node parent)
		{
			CellMeta result = Grid.Cells.Get(node.cell.x, node.cell.z);
			CellMeta origin = Grid.Cells.Get(parent.cell.x, parent.cell.z);

			if (result != null && origin != null)
			{
				int resultTier = result.elevationTier;
				int originTier = origin.elevationTier;
				if (Math.Abs(resultTier - originTier) > 1)
					return false;

				if (resultTier > 0 && Pathing.IsDiagonalXZ(origin.cell, result.cell))
					return false;
			}
			
			return true;
		}
	}

  //  [HarmonyPatch(typeof(World), "FindFootPath")]
  //  static class TestPatch
  //  {
		//static MethodBase TargetMethod()
		//{
		//	return AccessTools.Method(
		//		typeof(World),
		//		"FindFootPath",
		//		new Type[]
		//		{
		//			typeof(Vector3),
		//			typeof(Vector3),
		//			typeof(ArrayExt<Vector3>).MakeByRefType(),
		//			typeof(Pathfinder.blocksPathTest),
		//			typeof(Pathfinder.blocksPathTest),
		//			typeof(Pathfinder.applyExtraCost),
		//			typeof(int),
		//			typeof(bool)
		//		});
		//}

  //      static void Prefix(
		//	Vector3 start, 
		//	Vector3 end, 
			
		//	ref ArrayExt<Vector3> path, 

		//	Pathfinder.blocksPathTest blockTest, 
		//	Pathfinder.blocksPathTest stringPullBlockTest, 
		//	Pathfinder.applyExtraCost costFunc, 
			
		//	int teamId, 
		//	ref bool doDiagonal)
  //      {
		//	doDiagonal = false;
  //      }
  //  }


    // Base pathfinding inits before any terrain is generated; this is not possible with Elevation due to the fact that the shape of the terrain affects how the pathfinding needs to be setup


  //  [HarmonyPatch(typeof(ThreadedPathing), "CalculatePaths")]
  //  static class RecalculatePatch
  //  {
		//static bool Prefix()
  //      {
		//	return true;
		//	//return false;
  //      }

  //      static void Postfix(ThreadedPathing __instance, object data)
  //      {
		//	//bool shutdown = __instance.GetPFieldValue<bool>("shutdown");
		//	//AutoResetEvent[] workerWaitHandle = __instance.GetPFieldValue<AutoResetEvent[]>("workerWaitHandle");
		//	//ArrayExt<GamePath>[] pathsToCalculate = __instance.GetPFieldValue<ArrayExt<GamePath>[]>("pathsToCalculate");
		//	//Pathfinder[] pather = __instance.GetPFieldValue<Pathfinder[]>("pather");
		//	//Countdown mainWaitHandle = __instance.GetPFieldValue<Countdown>("mainWaitHandle");
		//	//ArrayExt<GamePath> requestedPaths = __instance.GetPFieldValue<ArrayExt<GamePath>>("requestedPaths");

		//	//int num = (int)data;
		//	//workerWaitHandle[num].Set();
		//	//while (!shutdown)
		//	//{
		//	//	workerWaitHandle[num].WaitOne();
		//	//	for (int i = 0; i < pathsToCalculate[num].Count; i++)
		//	//	{
		//	//		try
		//	//		{
		//	//			GamePath gamePath = pathsToCalculate[num].data[i];
		//	//			Vector3 start = gamePath.start;
		//	//			Vector3 end = gamePath.end;
		//	//			//gamePath.debug = 1;
		//	//			do
		//	//			{
		//	//				start = gamePath.start;
		//	//				end = gamePath.end;
		//	//				switch (gamePath.pathType)
		//	//				{
		//	//					case GamePath.PathType.VillagerPath:
		//	//						{
		//	//							Pathfinder pathfinder = pather[num];
		//	//							Vector3 startPos = start;
		//	//							bool startUseUpperGrid = false;
		//	//							Vector3 endPos = end;
		//	//							bool endUseUpperGrid = false;
		//	//							GamePath gamePath2 = gamePath;
		//	//							Pathfinder.blocksPathTest bt = new Pathfinder.blocksPathTest(World.GetBlocksFootPath);
		//	//							Pathfinder.blocksPathTest pullBlock = new Pathfinder.blocksPathTest(World.GetBlocksFootPath);
		//	//							pathfinder.FindPath(startPos, startUseUpperGrid, endPos, endUseUpperGrid, ref gamePath2.result, bt, pullBlock, new Pathfinder.applyExtraCost(World.GetFootPathCost), gamePath.teamId, true, false, false);
		//	//							//gamePath.debug = 2;
		//	//							break;
		//	//						}
		//	//					case GamePath.PathType.MilitaryPathPostBlock:
		//	//						{
		//	//							bool flag = false;
		//	//							Cell cellDataClamped = World.inst.GetCellDataClamped(start);
		//	//							if (!cellDataClamped.isUpperGridBlocked && start.y > 0.1f && World.GetCellHeightPos(cellDataClamped).y > 0f)
		//	//							{
		//	//								flag = true;
		//	//							}
		//	//							bool flag2 = false;
		//	//							Cell cellDataClamped2 = World.inst.GetCellDataClamped(end);
		//	//							if (!cellDataClamped2.isUpperGridBlocked && end.y > 0.1f && World.GetCellHeightPos(cellDataClamped2).y > 0f)
		//	//							{
		//	//								flag2 = true;
		//	//							}
		//	//							Pathfinder pathfinder2 = pather[num];
		//	//							Vector3 startPos2 = start;
		//	//							bool startUseUpperGrid2 = flag;
		//	//							Vector3 endPos2 = end;
		//	//							bool endUseUpperGrid2 = flag2;
		//	//							GamePath gamePath3 = gamePath;
		//	//							Pathfinder.blocksPathTest bt2 = new Pathfinder.blocksPathTest(World.NoBlock);
		//	//							Pathfinder.blocksPathTest pullBlock2 = new Pathfinder.blocksPathTest(World.GetBlocksFootPathForArmies);
		//	//							pathfinder2.FindPath(startPos2, startUseUpperGrid2, endPos2, endUseUpperGrid2, ref gamePath3.result, bt2, pullBlock2, new Pathfinder.applyExtraCost(World.GetMilitaryFootPathCost), gamePath.teamId, false, true, true);
		//	//							if (gamePath.result.Count <= 0)
		//	//							{
		//	//								Pathfinder pathfinder3 = pather[num];
		//	//								Vector3 startPos3 = start;
		//	//								bool startUseUpperGrid3 = flag;
		//	//								Vector3 endPos3 = end;
		//	//								bool endUseUpperGrid3 = !flag2;
		//	//								GamePath gamePath4 = gamePath;
		//	//								Pathfinder.blocksPathTest bt3 = new Pathfinder.blocksPathTest(World.NoBlock);
		//	//								Pathfinder.blocksPathTest pullBlock3 = new Pathfinder.blocksPathTest(World.GetBlocksFootPathForArmies);
		//	//								pathfinder3.FindPath(startPos3, startUseUpperGrid3, endPos3, endUseUpperGrid3, ref gamePath4.result, bt3, pullBlock3, new Pathfinder.applyExtraCost(World.GetMilitaryFootPathCost), gamePath.teamId, false, true, true);
		//	//							}
		//	//							break;
		//	//						}
		//	//					case GamePath.PathType.OgrePathPostBlock:
		//	//						{
		//	//							Pathfinder pathfinder4 = pather[num];
		//	//							Vector3 startPos4 = start;
		//	//							bool startUseUpperGrid4 = false;
		//	//							Vector3 endPos4 = end;
		//	//							bool endUseUpperGrid4 = false;
		//	//							GamePath gamePath5 = gamePath;
		//	//							Pathfinder.blocksPathTest bt4 = new Pathfinder.blocksPathTest(World.NoBlock);
		//	//							Pathfinder.blocksPathTest pullBlock4 = new Pathfinder.blocksPathTest(World.GetBlocksFootPathForOgres);
		//	//							pathfinder4.FindPath(startPos4, startUseUpperGrid4, endPos4, endUseUpperGrid4, ref gamePath5.result, bt4, pullBlock4, new Pathfinder.applyExtraCost(World.GetOgreFootPathCost), gamePath.teamId, false, true, false);
		//	//							break;
		//	//						}
		//	//					case GamePath.PathType.BoatPathPostBlock:
		//	//						{
		//	//							Pathfinder pathfinder5 = pather[num];
		//	//							Vector3 startPos5 = start;
		//	//							bool startUseUpperGrid5 = false;
		//	//							Vector3 endPos5 = end;
		//	//							bool endUseUpperGrid5 = false;
		//	//							GamePath gamePath6 = gamePath;
		//	//							Pathfinder.blocksPathTest bt5 = new Pathfinder.blocksPathTest(World.NoBlock);
		//	//							Pathfinder.blocksPathTest pullBlock5 = new Pathfinder.blocksPathTest(World.GetBlocksWaterPath);
		//	//							pathfinder5.FindPath(startPos5, startUseUpperGrid5, endPos5, endUseUpperGrid5, ref gamePath6.result, bt5, pullBlock5, new Pathfinder.applyExtraCost(World.GetBoatPathCost), gamePath.teamId, false, true, false);
		//	//							break;
		//	//						}
		//	//				}
		//	//			}
		//	//			while (start != gamePath.start || end != gamePath.end);

		//	//			if (ElevationPathfinder.retryFailedPaths && gamePath.result.Count == 1 && gamePath.result[0] == ElevationPathfinder.failedPathSignal)
		//	//			{
		//	//				if (!ElevationPathfinder.gamePathRetries.ContainsKey(new ElevationPathfinder.PathData(gamePath)))
		//	//					ElevationPathfinder.gamePathRetries.Add(new ElevationPathfinder.PathData(gamePath), 0);

		//	//				ElevationPathfinder.gamePathRetries[new ElevationPathfinder.PathData(gamePath)] += 1;

		//	//				requestedPaths.Add(gamePath);
		//	//				Mod.dLog("Retrying threaded path after initial failed attempt");
		//	//			}
		//	//			else
		//	//			{
		//	//				pathsToCalculate[num].data[i].status = GamePath.Status.Complete;

		//	//				if (ElevationPathfinder.gamePathRetries.ContainsKey(new ElevationPathfinder.PathData(gamePath)))
		//	//					ElevationPathfinder.gamePathRetries.Remove(new ElevationPathfinder.PathData(gamePath));
		//	//			}
		//	//			//gamePath.debug = 3;
		//	//		}
		//	//		catch (Exception)
		//	//		{
		//	//		}
		//	//	}
		//	//	mainWaitHandle.Signal();
		//	//}
  //      }
  //  }



}
