using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;
using Fox.Rendering;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.ChatMapper;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(World), "Unplace")]
    public class UnplacePatch
    {
        static void Postfix(Building building)
        {
            if (!building)
                return;

            if (building.GetComponent<Road>())
            {
                RoadStairs.Update(World.inst.GetCellDataClamped(building.transform.position), true);
            }
        }
    }


    [HarmonyPatch(typeof(Road), "UpdateRotationForRoad")]
    public class RoadStairs
    {
        internal static Dictionary<Cell, RoadData> _data { get; set; } = new Dictionary<Cell, RoadData>();

        private static InstanceMeshSystem normalStairsSystem;
        private static InstanceMeshSystem stoneStairsSystem;

        private static Cell[] scratchNeighbors = new Cell[4];


        public enum RoadType
        {
            normal,
            stone
        }

        public struct RoadData
        {
            public bool valid;

            public Cell cell;

            public RoadType type;
            public Tuple<Vector3, Vector3>[] stairs;
        }

        public static void Init()
        {
            RoadAssets.stairs_normalMaterial.enableInstancing = true;
            RoadAssets.stairs_stoneMaterial.enableInstancing = true;


            normalStairsSystem = new InstanceMeshSystem(RoadAssets.stairs_normal, RoadAssets.stairs_normalMaterial);
            stoneStairsSystem = new InstanceMeshSystem(RoadAssets.stairs_stone, RoadAssets.stairs_stoneMaterial);
        }

        public static void Tick()
        {
            normalStairsSystem.Update();
            stoneStairsSystem.Update();

            normalStairsSystem.material.SetFloat("_Snow", TerrainGen.inst.GetSnowFade());
            normalStairsSystem.material.SetVector("_SnowColor", TerrainGen.inst.snowColor);
            normalStairsSystem.material.SetFloat("_MaxHeight", (float)(ElevationManager.maxElevation + 1) * ElevationManager.elevationInterval);

            stoneStairsSystem.material.SetFloat("_Snow", TerrainGen.inst.GetSnowFade());
            stoneStairsSystem.material.SetVector("_SnowColor", TerrainGen.inst.snowColor);
            stoneStairsSystem.material.SetFloat("_MaxHeight", (float)(ElevationManager.maxElevation + 1) * ElevationManager.elevationInterval);
        }

        public static void Reset()
        {
            foreach (RoadData data in _data.Values)
                RemoveInstances(data);

            _data.Clear();

            normalStairsSystem.Clear();
            stoneStairsSystem.Clear();
        }

        public static void HandleRoadChange(OnBuildingAddRemove obj)
        {
            Road road = obj.targetBuilding.GetComponent<Road>();
            if (!road && !CanConnect(obj.targetBuilding))
                return;
                
            Cell cell = World.inst.GetCellDataClamped(obj.targetBuilding.transform.position);
            

            World.inst.GetNeighborCells(obj.targetBuilding.GetCell(), ref scratchNeighbors);
            foreach (Cell neighbor in scratchNeighbors)
                Update(neighbor);

            if (road)
                Update(cell, !obj.added);
        }

        static void RemoveInstances(RoadData data)
        {
            for (int i = 0; i < 4; i++)
            {
                string tag = $"{data.cell.x}_{data.cell.z}s{i}";
                
                if (normalStairsSystem.Has(tag))
                    normalStairsSystem.Remove(tag);
                if (stoneStairsSystem.Has(tag))
                    stoneStairsSystem.Remove(tag);
            }
        }

        static void UpdateInstances(RoadData data)
        {
            InstanceMeshSystem system = data.type == RoadType.stone ? stoneStairsSystem : normalStairsSystem;

            for(int i = 0; i < 4; i++)
            {
                string tag = $"{data.cell.x}_{data.cell.z}s{i}";
                //DebugExt.dLog($"tag {tag}; {((i < data.stairs.Length) ? "stair" : "slot")}", true);
                if (i < data.stairs.Length)
                {
                    Tuple<Vector3, Vector3> stair = data.stairs[i];
                    system.Set(tag, Vector3.Lerp(stair.Item1, stair.Item2, 0.5f - ElevationManager.roadSlopingRadius/2f), Quaternion.LookRotation(stair.Item1 - stair.Item2, new Vector3(0f, 1f, 0f)), new Vector3(1f, 1f, ElevationManager.roadSlopingRadius * 2f));
                }
                else if (system.Has(tag))
                    system.Remove(tag);
            }
        }

        public static void Update(Cell cell, bool remove = false)
        {
            if(remove && _data.ContainsKey(cell))
            {
                RemoveInstances(_data[cell]);
                _data.Remove(cell);
            }
            else
            if (cell.StructureCompareUniqueNameAll(World.roadHash) || cell.StructureCompareUniqueNameAll(World.stoneRoadHash))
            {
                Road road = cell.OccupyingStructure.Where((b) => b.GetComponent<Road>() != null).First().GetComponent<Road>();
                Update(road, road.transform.position);
            }
        }

        public static void Update(Road road, Vector3? pos = null)
        {
            Building building = road.GetComponent<Building>();
            Cell cell = World.inst.GetCellDataClamped(road.transform.position);

            if (!Player.inst.Buildings.Contains(building))
                return;

            if (!_data.ContainsKey(cell))
            {
                RoadData roadData = CreateRoadDataFor(road);
                if (roadData.valid)
                    _data.Add(cell, roadData);
                else
                    return;
            }
            else
                _data[cell] = UpdateData(_data[cell], pos);

            UpdateInstances(_data[cell]);
        }

        public static void UpdateAll()
        {
            Reset();
            foreach (CellMeta meta in Grid.Cells)
                Update(meta.cell);
        }

        static void Postfix(Road __instance)
        {
            Update(__instance);
        }

        private static RoadData CreateRoadDataFor(Road road)
        {
            RoadData data = new RoadData()
            {
                valid = false,
                stairs = new Tuple<Vector3, Vector3>[0],
                type = road.GetComponent<Building>().UniqueName == "road" ? RoadType.normal : RoadType.stone, // default stone type for gardens
            };

            Cell cell = World.inst.GetCellDataClamped(road.transform.position);
            if (cell == null)
                return data;

            data.cell = cell;

            CellMeta meta = Grid.Cells.Get(cell);
            if (meta == null)
                return data;

            data.valid = true;

            return UpdateData(data);
        }

        private static RoadData UpdateData(RoadData data, Vector3? pos = null)
        {
            if (!data.valid)
                return data;

            if (pos.HasValue)
            {
                data.cell = World.inst.GetCellDataClamped(pos.Value);
                Road road = data.cell.OccupyingStructure.Find((b) => b.GetComponent<Road>() != null).GetComponent<Road>();
                data.type = road.GetComponent<Building>().UniqueName == "road" ? RoadType.normal : RoadType.stone;
                if (!road.GetComponent<Building>().IsBuilt())
                {
                    RemoveInstances(data);

                    return data;
                }
            }

            CellMeta originMeta = Grid.Cells.Get(data.cell);
            if (originMeta == null)
                return data;

            List<Tuple<Vector3, Vector3>> stairs = new List<Tuple<Vector3, Vector3>>();

            Cell[] scratchNeighbors = new Cell[4];
            World.inst.GetNeighborCells(data.cell, ref scratchNeighbors);
            foreach(Cell cell in scratchNeighbors)
            {
                CellMeta meta = Grid.Cells.Get(cell);
                if (meta == null)
                    continue;

                if (meta.elevationTier - originMeta.elevationTier != 1)
                    continue;

                Vector3 location = (new Vector3(cell.Center.x, data.cell.Center.y, cell.Center.z));
                for (int k = 0; k < cell.OccupyingStructure.Count; k++)
                {
                    if (CanConnect(cell.OccupyingStructure[k]))
                        stairs.Add(new Tuple<Vector3, Vector3>(originMeta.Center, location));

                    break;
                }
            }

            data.stairs = new Tuple<Vector3, Vector3>[stairs.Count];
            for (int i = 0; i < stairs.Count; i++)
                data.stairs[i] = stairs[i];

            return data;
        }

        public static bool CanConnect(Building building)
        {
            return Road.ConnectsToRoad(building) && building.IsBuilt();
        }

        public static bool CanConnect(Cell cell)
        {
            return cell.BottomStructureIs(World.townsquareHash) ||
                cell.StructureCompareCategoryNameAll("park".GetHashCode()) ||
                cell.StructureCompareCategoryNameAll(World.pathHash) ||
                cell.StructureCompareCategoryNameAll(World.gateHash);
        }

        public static bool Stair(Cell from, Cell to)
        {
            Building structure = from.BottomStructure ?? from.TopSubStructure;

            if (!structure)
                return false;

            Road road = structure.GetComponent<Road>();
            if (!road)
                return false;

            //Building other = to.BottomStructure ?? to.TopSubStructure;
            //if (!other || !other.GetComponent<Road>())
            //    return false;

            //return true;

            if (!_data.ContainsKey(from))
                return false;

            return _data[from].stairs
                    .Contains(new Tuple<Vector3, Vector3>(from.Center, new Vector3(to.Center.x, from.Center.y, to.Center.z)));
        }
    }

    
}
