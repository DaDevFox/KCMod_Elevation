using Harmony;
using System;
using UnityEngine;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(Road), "TrySetMesh")]
    public static class RoadSizePatch
    {
        static void Postfix(Road __instance, Transform ___curr)
        {
            if (__instance.isBridge)
                return;

            Building building = __instance.GetComponent<Building>();
            if (!building.IsBuilt())
                return;

            Cell cell = building.GetCell();
            CellMeta meta = Grid.Cells.Get(cell);
            if (!meta)
                return;

            bool hasDifferentHeightNeighbor = false;
            Cell[] neighbors = new Cell[4];
            World.inst.GetNeighborCells(cell, ref neighbors);
            foreach(Cell neighbor in neighbors)
            {
                CellMeta neighborMeta = Grid.Cells.Get(neighbor);
                if (!neighborMeta)
                    continue;

                if (neighbor.StructureCompareCategoryNameAll(Roads.pathHash) && Math.Abs(neighborMeta.elevationTier - meta.elevationTier) <= 1)
                    hasDifferentHeightNeighbor = true;
            }

            if (!hasDifferentHeightNeighbor)
                return;

            string uniqueName = __instance.GetComponent<Building>().UniqueName;
            float scale = Roads.axisScaleLookup.ContainsKey(uniqueName) ? Roads.axisScaleLookup[uniqueName] : 1f;



            Transform[] meshRoots = new Transform[]
            {
                __instance.Straight,
                __instance.Elbow,
                __instance.Intersection3,
                __instance.Intersection4,

                __instance.Single,
                __instance.DeadEnd,
                __instance.ElbowFilled,
                __instance.ThreeLeft,

                __instance.ThreeRight,
                __instance.ThreeFilled,
                __instance.FourOne,
                __instance.FourTwo,

                __instance.FourTwoDiagonal,
                __instance.FourThree,
                __instance.Full
            };

            (bool, bool)[] scalingAxes = new (bool, bool)[]
            {
                (false, true),
                (true, true),
                (true, true),
                (true, true),

                (false, false),
                (false, true),
                (false, false),
                (true, true),

                (true, true),
                (false, false),
                (true, true),
                (true, true),

                (true, true),
                (true, true),
                (false, false),

            };

            for(int i = 0; i <  meshRoots.Length; i++)
            {
                Transform meshRoot = meshRoots[i];
                (bool, bool) scalingAxisPair = scalingAxes[i];
                
                if (meshRoot == ___curr)
                {
                    __instance.GetComponentInChildren<MeshFilter>().transform.localScale = new Vector3(scalingAxisPair.Item1 ? scale : 1f, 1f, scalingAxisPair.Item2 ? scale : 1f);
                }
            }

        }
    }
}
