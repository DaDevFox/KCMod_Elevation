using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;
using System.Diagnostics;
using System.Reflection;

namespace Elevation.Patches
{
    // TXX726

    public static class YInterpolation
    {
        /// <summary>
        /// Returns 0 in normal circumstances and special values in circumstances in which y must be adjusted (IE when in water)
        /// </summary>
        /// <param name="position"></param>
        /// <param name="slopingRadius"></param>
        /// <param name="effectiveRadius"></param>
        /// <returns></returns>
        private static float GetYBase(Vector3 position, float slopingRadius, out float effectiveRadius)
        {
            effectiveRadius = slopingRadius == -1f ? ElevationManager.slopingRadius : slopingRadius;

            Cell cell = World.inst.GetCellDataClamped(position);
            if (cell == null)
                return 0f;

            if (cell.Type == ResourceType.Water)
            {
                bool floated = false;
                if (cell.OccupyingStructure.Count > 0)
                {
                    Building building = cell.OccupyingStructure[0];
                    floated = ((building.categoryHash == World.pathHash || building.uniqueNameHash == World.dockHash || building.uniqueNameHash == World.fishinghutHash) && building.IsBuilt());
                }
                if (!floated)
                {
                    return -0.3f;
                }
            }

            return 0f;
        }

        public static float GetMidpointSlopedY(Vector3 position, float slopingRadius = -1f)
        {
            float yBase = GetYBase(position, slopingRadius, out float radius);
            if (yBase != 0f)
                return yBase;

            Cell cell = World.inst.GetCellDataClamped(position);
            if (cell == null)
                return 0f;

            Vector3 difference = (position - cell.Center).xz();

            CellMeta current = Grid.Cells.Get(position);
            CellMeta other = null;

            if (other != null || current != null)
            {
                if (difference.z >= 0.5f - radius)
                {
                    other = Grid.Cells.Get(new Vector3(position.x, position.y, position.z + 1f));
                    return Mathf.Lerp(current != null ? current.Elevation : 0f, other != null ? other.Elevation : 0f,
                        (difference.z - (0.5f - radius)) / (2f * radius));
                }
                else if (difference.z <= -0.5f + radius)
                {
                    other = Grid.Cells.Get(new Vector3(position.x, position.y, position.z - 1f));
                    return Mathf.Lerp(other != null ? other.Elevation : 0f, current != null ? current.Elevation : 0f,
                        1f - (-difference.z - (0.5f - radius)) / (2f * radius));
                }
                else if (difference.x >= 0.5f - radius)
                {
                    other = Grid.Cells.Get(new Vector3(position.x + 1f, position.y, position.z));
                    return Mathf.Lerp(current != null ? current.Elevation : 0f, other != null ? other.Elevation : 0f,
                        (difference.x - (0.5f - radius)) / (2f * radius));
                }
                else if (difference.x <= -0.5f + radius)
                {
                    other = Grid.Cells.Get(new Vector3(position.x - 1f, position.y, position.z));
                    return Mathf.Lerp(other.Elevation, current.Elevation,
                        1f - (-difference.x - (0.5f - radius)) / (2f * radius));
                }
            }


            if (current != null)
                return current.Elevation;

            return position.y;
        }
    
        public static float GetSlantSlopedY(Vector3 position, float slopingRadius = -1f)
        {
            float yBase = GetYBase(position, slopingRadius, out float radius);
            if (yBase != 0f)
                return yBase;

            Cell cell = World.inst.GetCellDataClamped(position);
            if (cell == null)
                return 0f;

            Vector3 difference = (position - cell.Center).xz();

            CellMeta current = Grid.Cells.Get(position);
            CellMeta other = null;
            float distance = 0f;

            radius = ElevationManager.roadSlopingRadius;

            if (current != null)
            {
                if (difference.z >= 0.5f - radius)
                {
                    other = Grid.Cells.Get(new Vector3(position.x, position.y, position.z + 1f));
                    distance = difference.z;
                    //return Mathf.Lerp(current != null ? current.Elevation : 0f, other != null ? other.Elevation : 0f,
                    //    (difference.z - (0.5f - radius)) / (2f * radius));
                }
                else if (difference.z <= -0.5f + radius)
                {
                    other = Grid.Cells.Get(new Vector3(position.x, position.y, position.z - 1f));
                    distance = -difference.z;
                    //return Mathf.Lerp(other != null ? other.Elevation : 0f, current != null ? current.Elevation : 0f,
                    //    1f - (-difference.z - (0.5f - radius)) / (2f * radius));
                }
                else if (difference.x >= 0.5f - radius)
                {
                    other = Grid.Cells.Get(new Vector3(position.x + 1f, position.y, position.z));
                    distance = difference.x;
                    //return Mathf.Lerp(current != null ? current.Elevation : 0f, other != null ? other.Elevation : 0f,
                    //    (difference.x - (0.5f - radius)) / (2f * radius));
                }
                else if (difference.x <= -0.5f + radius)
                {
                    other = Grid.Cells.Get(new Vector3(position.x - 1f, position.y, position.z));
                    distance = -difference.x;
                    //return Mathf.Lerp(other.Elevation, current.Elevation,
                    //    1f - (-difference.x - (0.5f - radius)) / (2f * radius));
                }
            }
            else
                return 0f;

            if (!other)
                return current.Elevation;

            if (Math.Abs(other.elevationTier - current.elevationTier) > 1)
                return current.Elevation;

            // Horrible code ikik
            if (!RoadStairs.Stair(current.cell, other.cell))
                radius = ElevationManager.slopingRadius;
            

            bool @short = other.elevationTier > current.elevationTier;

            float start = 0.5f - radius;
            float end   = 0.5f;

            if(@short && distance > start)
                return Mathf.Lerp(current.Elevation, other.Elevation,
                        (distance - start) / (end - start));

            if (current != null)
                return current.Elevation;

            return position.y;
        }
    
    }

    [HarmonyPatch(typeof(Villager), "GetPosWithOffset")]
    public class VillagerYCorrectionPatch
    {
        static void Postfix(Villager __instance, ref Vector3 __result)
        {
            Cell cell = __instance.cell;
            if (cell == null)
                return;
            
            CellMeta meta = cell.GetMeta();
            if (meta != null && cell.Type != ResourceType.Water)
            {
                //if (__instance.travelPath.Count > 0 && Pathing.IsDiagonalXZ(__result, __instance.travelPath.data[0]))
                //    __result.y = GetYSloped(__result, __result, __instance.travelPath.data[0]);
                //else

                if (__instance.job != null
                    && __instance.job.employer != null
                    && (__result.xz() - __instance.job.employer.GetPositionForPerson(__instance).xz()).sqrMagnitude <= 1f)
                    __result.y = Mathf.Max(YInterpolation.GetSlantSlopedY(__result), __instance.Pos.y);
                else
                    __result.y = YInterpolation.GetSlantSlopedY(__result);
            }
        }

        //// TODO: Test optimization of this
        //public static float GetYSloped(Vector3 position, Vector3? diagonalFrom = null, Vector3? diagonalTo = null)
        //{
            
        //}
    }

    //[HarmonyPatch(typeof(Villager))]
    //public class VillagerYAdjustReturnPatch
    //{
    //    static MethodBase TargetMethod()
    //    {
    //        return typeof(Villager).GetMethod("GetYAdjust", BindingFlags.Static | BindingFlags.Public);
    //    }

    //    static bool Prefix(Cell cell, ref float yAdjust, ref float magnitudeMul, ref float bounceSpeedMul)
    //    {
    //        CellMeta meta = Grid.Cells.Get(cell);
    //        if (meta == null)
    //            return true;

    //        if (meta.Elevation > 0f)
    //        {
    //            yAdjust = meta.Elevation;
    //            return false;
    //        }

    //        return true;
    //    }
    //}
}
