using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Elevation.Patches;
using UnityEngine;
using Harmony;
using Fox.Profiling;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using JetBrains.Annotations;

namespace Elevation
{
    public static class WorldRegions
    {
        public static event Action onMarked;

        #region Async Settings

        public static bool async { get; set; } = true;
        public static bool secondsDistribution { get; set; } = false;

        public static float updateInterval { get; set; } = 0.5f;
        public static float timePerFrame { get; set; } = 0.0666667f;

        public static float minProcessingTime = 0.2f;
        public static float minMarkingTime = 0.2f;
        public static float minFinalizingTime = 0.2f;



        private static Stopwatch timer;
        private static float elapsed = 0f;

        private static int cache = 0;

        #endregion

        /// <summary>
        /// Returns whether the world has had its regions marked already
        /// </summary>
        public static bool Marked { get; set; } = false;

        /// <summary>
        /// Returns whether the region categorization algorithm is currently busy with an operation
        /// </summary>
        public static bool Busy { get; private set; } = false;

        public static List<Cell> Dirty { get; private set; } = new List<Cell>();

        public static HashSet<Cell> Unreachable { get; private set; } = new HashSet<Cell>();
        public static List<Cell> BeginSearchPositions { get; private set; } = new List<Cell>();

        private static Dictionary<string, CellData> cellsData = new Dictionary<string, CellData>();
        private static Dictionary<int, List<CellData>> regionData = new Dictionary<int, List<CellData>>();

        private static List<CellMeta> openSet = new List<CellMeta>();

        public static void Tick()
        {
            if (Marked)
            {
                //UI.loadingDialog.desiredProgress = Mathf.Clamp01(Dirty.Count / 10f);

                if (elapsed > updateInterval)
                {
                    if (Dirty.Count > 0)
                        UpdateDirty();
                    elapsed %= updateInterval;
                }

                elapsed += Time.unscaledDeltaTime;
            }
        }

        public static int GetTileRegion(Cell cell)
        {
            if(cell != null)
                if (cellsData.ContainsKey(CellMetadata.GetPositionalID(cell)))
                    return cellsData[CellMetadata.GetPositionalID(cell)].region;
            return -1;
        }


        public static void MarkDirty(Cell cell)
        {
            Dirty.Add(cell);
        }

        public static void AddNeighbors(CellMeta meta)
        {
            Cell[] neighbors = new Cell[4];
            World.inst.GetNeighborCells(meta.cell, ref neighbors);
            foreach (Cell neighbor in neighbors)
            {
                CellMeta neighborMeta = Grid.Cells.Get(neighbor);
                if (neighborMeta && (Unreachable.Contains(neighbor) 
                    //|| !PathableNeighbor(neighborMeta)
                    ))
                    Dirty.Add(neighbor);
            }
        }

        // TODO: critical elevation change tracking; do not allow dugouts to undo critical elevation pieces

        private static bool PathableNeighbor(CellMeta meta) 
        {
            if (meta == null)
                return false;

            bool result = false;
            Cell[] neighbors = new Cell[4];
            World.inst.GetNeighborCells(meta.cell, ref neighbors);

            foreach (Cell neighbor in neighbors)
            {
                if (neighbor == null || Unreachable.Contains(neighbor))
                    continue;

                CellMeta neighborMeta = Grid.Cells.Get(neighbor);
                if (neighborMeta && Mathf.Abs(neighborMeta.elevationTier - meta.elevationTier) <= 1)
                {
                    result = true;
                }
            }

            return result;
        }

        public static void UpdateDirty()
        {
            Cell[] scratchDirty = new Cell[Dirty.Count];
            Dirty.CopyTo(scratchDirty);

            for (int i = 0; i < scratchDirty.Length; i++) 
            {
                // if, after some change, cell now has a pathable (not in unreachable list) neighbor within 1 elevation tier, cell is no longer unreachable.
                Cell cell = scratchDirty[i];
                CellMeta meta = Grid.Cells.Get(cell);
                if (meta != null && cell != null)
                {
                    if (PathableNeighbor(meta))
                    {
                        if (Unreachable.Contains(cell))
                        {
                            Unreachable.Remove(cell);
                            ClearBlockedCellColor(cell, true); 
                        }
                    }
                    else if (!Unreachable.Contains(cell))
                    {
                        Unreachable.Add(cell);
                        SetBlockedCellColor(cell, true);
                    }

                    AddNeighbors(meta);
                    Dirty.Remove(cell);
                }
            }
        }

        public static void ClearBlockedCellColor(Cell cell, bool update = false)
        {
            ColorManager.GetTileColor(cell.fertile, cell.IrrigationCoverage > 0, out Color normalColor, out Color winterColor);
            TerrainGen.inst.SetTerrainPixelColor(cell.x, cell.z, normalColor, winterColor);
            if (update)
                TerrainGen.inst.UpdateTextures();
        }

        public static void SetBlockedCellColor(Cell cell, bool update = false)
        {
            float bias = ColorManager.coloringBias;
            Color unreachableColor = ColorManager.unreachableColor;

            ColorManager.GetTileColor(cell.fertile, cell.IrrigationCoverage > 0, out Color basegameNormalColor, out Color basegameWinterColor);
            TerrainGen.inst.SetTerrainPixelColor(cell.x, cell.z, Color.Lerp(basegameNormalColor, unreachableColor, bias), Color.Lerp(basegameWinterColor, unreachableColor, bias));
            if (update)
                TerrainGen.inst.UpdateTextures();
        }

        public static void Search()
        {
            if (async)
                GameObject.FindObjectOfType<Mod>().StartCoroutine(RegionSearchAsync());
            else
                RegionSearch();
        }

        #region Single Frame

        // TODO: Async region search; pause game at beginning, show loading dialog, and asynchronously execute the region search
        [Profile]
        public static void RegionSearch()
        {
            Marked = false;

            regionData.Clear();
            cellsData.Clear();

            List<CellData> remaining = new List<CellData>();

            // Preperation
            // Mark all cells that support elevation as nodes
            foreach (CellMeta meta in Grid.Cells)
            {
                CellData nodeData = new CellData()
                {
                    cell = meta.cell,
                    meta = meta,
                    region = -1
                };
                    
                cellsData.Add(CellMetadata.GetPositionalID(meta.cell), nodeData);
                remaining.Add(nodeData);
            }

            int region = 1;

            // Iterate on all nodes, each node will be given a region in which it resides. 
            // Any node can reach another node in the same region, but anywhere else is unreachable 
            //while(remaining.Count > 0)
            //{
            //    CellData node = remaining[0];

            //    node.region = region;
            //    //IterateNode(node);
            //    region++;
            //}

            ReformatRegions();

            Busy = false;
            MarkComplete();
            
            Mod.Log("Blocked Regions Pruned");
        }

        private static void IterateNode(CellMeta meta)
        {
            if (meta == null)
                return;
            openSet.Remove(meta);

            if (meta.cell != null)
            {
                Cell[] neighbors = new Cell[4];
                World.inst.GetNeighborCells(meta.cell, ref neighbors);

                foreach (Cell cell in neighbors)
                {
                    if (cell == null)
                        continue;

                    CellMeta other = Grid.Cells.Get(cell);
                    if (other == null)
                        continue;

                    if (openSet.Contains(other) && Pathing.Connected(meta.cell, other.cell))
                    {
                        IterateNode(other);
                    }
                }
            }

            //List<Direction> dirs = new List<Direction>() { Direction.East, Direction.North, Direction.West, Direction.South };
            //foreach (Direction dir in dirs)
            //{

            //    CellData other = node.GetCardinal(dir);
            //    if (openSet.Contains(node))
            //        continue;

            //    if (CheckSameRegion(node, other))
            //        continue;

            //    if (!Pathing.Connected(node.cell, other.cell) && !Pathing.BlocksForBuilding(other.cell))
            //    {
            //        TagSameRegion(node, other);
            //        IterateNode(other);
            //    }
            //}
        }

        #endregion

        #region Async

        private static int openSetCount = 0;

        public static IEnumerator RegionSearchAsync()
        {
            UI.loadingDialog.Activate();
            UI.loadingDialog.title = "pruning_title";

            Marked = false;
            Busy = true;

            regionData.Clear();
            cellsData.Clear();

            openSet.Clear();

            float totalElapsed = 0f;

            float timeBreak = 5f;
            float elapsed = 0f;
            int count = 0;

            UI.loadingDialog.description = "pruning_preprocessing";
            UI.loadingDialog.UpdateText();
            UI.loadingDialog.desiredProgress = 0.5f;

            Dictionary<int, List<CellMeta>> landmasses = new Dictionary<int, List<CellMeta>>();

            for(int landmass = 0; landmass < World.inst.cellsToLandmass.Length; landmass++)
            {
                ArrayExt<Cell> cells = World.inst.cellsToLandmass[landmass];
                for (int cellIdx = 0; cellIdx < World.inst.cellsToLandmass[landmass].Count; cellIdx++)
                {
                    Cell cell = cells.data[cellIdx];
                    if (cell.deepWater)
                        continue;

                    CellMeta meta = Grid.Cells.Get(cell);
                    if (!meta)
                        continue;

                    if (!landmasses.ContainsKey(cell.landMassIdx))
                        landmasses.Add(cell.landMassIdx, new List<CellMeta>());

                    landmasses[cell.landMassIdx].Add(meta);
                    openSet.Add(meta);

                    elapsed += Time.unscaledDeltaTime;
                    totalElapsed += Time.unscaledDeltaTime;
                    count++;

                    if (elapsed > timeBreak)
                    {
                        elapsed = 0;
                        UI.loadingDialog.desiredProgress = ((float)count) / Grid.Cells.Count;
                        yield return new WaitForEndOfFrame();
                    }
                }
            }

            // Preperation
            // Mark all cells that support elevation as nodes
            //foreach (CellMeta meta in Grid.Cells)
            //{
            //    if (meta.cell.deepWater)
            //        continue;

            //    if (!landmasses.ContainsKey(meta.cell.landMassIdx))
            //        landmasses.Add(meta.cell.landMassIdx, new List<CellMeta>());
                
            //    landmasses[meta.cell.landMassIdx].Add(meta);
            //    openSet.Add(meta);

            //    elapsed += Time.unscaledDeltaTime;
            //    totalElapsed += Time.unscaledDeltaTime;
            //    count++;

            //    if(elapsed > timeBreak)
            //    {
            //        elapsed = 0;
            //        UI.loadingDialog.desiredProgress = ((float)count) / Grid.Cells.Count;
            //        yield return new WaitForEndOfFrame();
            //    }
            //}

            // wait for the min time to complete if not complete already 
            while (totalElapsed < minProcessingTime)
            {
                yield return new WaitForEndOfFrame();
                totalElapsed += Time.unscaledDeltaTime;
            }


            UI.loadingDialog.description = "pruning_floodfill";
            UI.loadingDialog.desiredProgress = 0f;
            UI.loadingDialog.UpdateText();
            totalElapsed = 0f;
            elapsed = 0f;
            yield return new WaitForEndOfFrame();
            
            DebugExt.dLog($"Pruning cells for {landmasses.Count} landmasses", true);

            openSetCount = openSet.Count;

            TerrainGen.inst.ClearOverlay(true);

            foreach(KeyValuePair<int, List<CellMeta>> pair in landmasses)
            {
                foreach (CellMeta meta in pair.Value)
                {
                    if (!meta.cell.deepWater && meta.elevationTier == 0)
                    {
                        if (Settings.showMapProcessing)
                        {
                            TerrainGen.inst.SetOverlayPixelColor(meta.cell.x, meta.cell.z, Color.black);
                            TerrainGen.inst.UpdateOverlayTextures(8f, 0.65f, 0.9f);
                        }

                        Mod.dLog("Landmass Prune Started");
                        yield return IterateNodeAsync(meta);
                        totalElapsed += Time.unscaledDeltaTime;
                        break;
                    }
                }

                Mod.dLog("Landmass pruned");   
            }

            for(int i = 0; i < openSet.Count; i++)
                Unreachable.Add(openSet[i].cell);

            DebugExt.dLog($"Pruned; {openSet.Count} unreachable cells flagged");
            Mod.dLog($"Pruned; {openSet.Count} unreachable cells flagged");

            // wait for the min time to complete if not complete already 
            while (totalElapsed < minMarkingTime)
            {
                yield return new WaitForEndOfFrame();
                totalElapsed += Time.unscaledDeltaTime;
            }

            UI.loadingDialog.description = "pruning_reformat";
            UI.loadingDialog.desiredProgress = 1f;
            UI.loadingDialog.UpdateText();
            yield return new WaitForEndOfFrame();

            TerrainGen.inst.ClearOverlay(true);

            UI.loadingDialog.Deactivate();
            Busy = false;
            MarkComplete();
            Mod.Log("Blocked Regions Pruned [async]");
        }


        private static IEnumerator IterateNodeAsync(CellMeta meta, int stack = 0)
        {
            int calculationsPerYield = 30;
            int safetyBuffer = 20;
            openSet.Remove(meta);

            if (meta.cell.deepWater)
                yield return null;

            if (openSet.Count < (calculationsPerYield + safetyBuffer) || stack >= calculationsPerYield)
            {
                yield return new WaitForEndOfFrame();
                stack = 0;
            }

            UI.loadingDialog.desiredProgress = 1f - ((float)openSet.Count) / (openSetCount);

            if (Settings.showMapProcessing)
            {
                TerrainGen.inst.SetOverlayPixelColor(meta.cell.x, meta.cell.z, new Color(100f, 0, 0));
                TerrainGen.inst.UpdateOverlayTextures(8f, 0.65f, 0.9f);
                TerrainGen.inst.FadeOverlay(1f);
            }

            if (meta.cell != null)
            {
                Cell[] neighbors = new Cell[4];
                World.inst.GetNeighborCells(meta.cell, ref neighbors);

                foreach(Cell cell in neighbors)
                {
                    if (cell == null)
                        continue;

                    CellMeta other = Grid.Cells.Get(cell);
                    if (other == null)
                        continue;

                    if (openSet.Contains(other) && Pathing.Connected(meta.cell, other.cell))
                    {
                        yield return IterateNodeAsync(other, stack + 1);
                    }

                }

            }
        }

        #endregion

        private static bool CheckSameRegion(CellData a, CellData b)
        {
            return a.region == b.region;
        }

        private static void TagSameRegion(CellData a, CellData b)
        {
            if (a.region != -1)
            {
                b.region = a.region;
            }
            else
            {
                a.region = regionData.Keys.Count;
                b.region = a.region;
                regionData.Add(a.region, new List<CellData>() { a, b });
            }
            
        }

        private static void ReformatRegions()
        {
            regionData.Clear();
            foreach(CellData node in cellsData.Values)
            {
                if (regionData.ContainsKey(node.region) && node.region != -1)
                {
                    regionData[node.region].Add(node);
                }
                else
                {
                    if(node.region != -1)
                        regionData.Add(node.region, new List<CellData>() { node });
                }
            }
        }

        private static void MarkComplete()
        {
            WorldRegions.Marked = true;
            //HACKY
            ElevationManager.RefreshTerrain();

            onMarked?.Invoke();

        }

        public class CellData
        {
            public static Direction[] directions = { Direction.East, Direction.North, Direction.West, Direction.South };

            public Cell cell { get; set; }
            public CellMeta meta;
            public int region;
            public bool empty;

            public bool hasCardinals
            {
                get
                {
                    foreach (Direction dir in directions)
                    {
                        Cell cardinal = Pathing.GetCardinal(cell, dir);
                        if (Pathing.Connected(cell, cardinal) 
                            && !CheckSameRegion(this, cellsData[CellMetadata.GetPositionalID(cardinal)]) 
                            && !Pathing.BlocksForBuilding(cardinal))
                            return true;
                    }
                    return false;
                }
            }

            public CellData[] GetCardinals()
            {
                List<CellData> cardinals = new List<CellData>();
                foreach (Direction dir in directions)
                {
                    CellData found =GetCardinal(dir);
                    if (found != null)
                        cardinals.Add(found);
                }
                return cardinals.ToArray();
            }

            

            public CellData GetCardinal(Direction direction)
            {
                if (cellsData == null)
                {
                    Mod.dLog("cellsData null");
                    return null;
                }
                if (cell == null)
                {
                    Mod.dLog("cell null");
                    return null;
                }

                Cell cardinal = Pathing.GetCardinal(cell, direction);

                if (cardinal != null)
                {
                    string id = CellMetadata.GetPositionalID(cardinal);
                    if (!string.IsNullOrEmpty(id))
                    {
                        if (cellsData.ContainsKey(id))
                            return cellsData[id];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Returns wehther one tile is in the same world region as another and therefore whether a path will be able to be found from one to the other
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static bool Reachable(Cell from, Cell to)
        {
            return GetTileRegion(from) == GetTileRegion(to);
        }





    }
}
