using Elevation.Utils;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Weather;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(Garden), "GetAdjacencyInfo")]
    public class GardenConnectPatch
    {
        static bool Prefix(Garden __instance, ref bool north, ref bool south, ref bool east, ref bool west, ref int count)
        {
            try
            {
                World.inst.ToGridCoord(__instance.transform.position, out int num, out int num2);
                north = Road.ShouldConnect(World.inst.GetCellDataClamped(num + 1, num2));
                south = Road.ShouldConnect(World.inst.GetCellDataClamped(num - 1, num2));
                east = Road.ShouldConnect(World.inst.GetCellDataClamped(num, num2 + 1));
                west = Road.ShouldConnect(World.inst.GetCellDataClamped(num, num2 - 1));

                CellMeta roadMeta = Grid.Cells.Get(World.inst.GetCellDataClamped(__instance.transform.position));

                CellMeta mNorth = Grid.Cells.Get(World.inst.GetCellDataClamped(num + 1, num2));
                CellMeta mSouth = Grid.Cells.Get(World.inst.GetCellDataClamped(num - 1, num2));
                CellMeta mEast = Grid.Cells.Get(World.inst.GetCellDataClamped(num, num2 + 1));
                CellMeta mWest = Grid.Cells.Get(World.inst.GetCellDataClamped(num, num2 - 1));

                if (roadMeta != null)
                {
                    if (mNorth != null)
                        north &= Math.Abs(roadMeta.elevationTier - mNorth.elevationTier) == 0;
                    if (mSouth != null)
                        south &= Math.Abs(roadMeta.elevationTier - mSouth.elevationTier) == 0;
                    if (mEast != null)
                        east &= Math.Abs(roadMeta.elevationTier - mEast.elevationTier) == 0;
                    if (mWest != null)
                        west &= Math.Abs(roadMeta.elevationTier - mWest.elevationTier) == 0;
                }

                if (north)
                {
                    count++;
                }
                if (south)
                {
                    count++;
                }
                if (east)
                {
                    count++;
                }
                if (west)
                {
                    count++;
                }
            }
            catch (Exception ex)
            {
                DebugExt.HandleException(ex);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Cemetery), "GetAdjacencyInfo")]
    public class CemeteryConnectPatch
    {
        static bool Prefix(Cemetery __instance, ref bool north, ref bool south, ref bool east, ref bool west, ref int count)
        {
            try
            {
                World.inst.ToGridCoord(__instance.transform.position, out int num, out int num2);
                north = Cemetery.IsCemetery(num - 1, num2);
                south = Cemetery.IsCemetery(num + 1, num2);
                east = Cemetery.IsCemetery(num, num2 + 1);
                west = Cemetery.IsCemetery(num, num2 - 1);

                CellMeta baseMeta = Grid.Cells.Get(World.inst.GetCellDataClamped(__instance.transform.position));

                CellMeta mNorth = Grid.Cells.Get(World.inst.GetCellDataClamped(num - 1, num2));
                CellMeta mSouth = Grid.Cells.Get(World.inst.GetCellDataClamped(num + 1, num2));
                CellMeta mEast = Grid.Cells.Get(World.inst.GetCellDataClamped(num, num2 + 1));
                CellMeta mWest = Grid.Cells.Get(World.inst.GetCellDataClamped(num, num2 - 1));

                if (baseMeta != null)
                {
                    if (mNorth != null)
                        north &= Math.Abs(baseMeta.elevationTier - mNorth.elevationTier) == 0;
                    if (mSouth != null)
                        south &= Math.Abs(baseMeta.elevationTier - mSouth.elevationTier) == 0;
                    if (mEast != null)
                        east &= Math.Abs(baseMeta.elevationTier - mEast.elevationTier) == 0;
                    if (mWest != null)
                        west &= Math.Abs(baseMeta.elevationTier - mWest.elevationTier) == 0;
                }

                if (north)
                {
                    count++;
                }
                if (south)
                {
                    count++;
                }
                if (east)
                {
                    count++;
                }
                if (west)
                {
                    count++;
                }
            }
            catch (Exception ex)
            {
                DebugExt.HandleException(ex);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Road), "GetAdjacencyInfo")]
    public class RoadConnectPatch
    {
        static bool Prefix(Road __instance, ref bool north, ref bool south, ref bool east, ref bool west, ref bool northEast, ref bool northWest, ref bool southEast, ref bool southWest, ref int count)
        {
            try
            {
                World.inst.ToGridCoord(__instance.transform.position, out int num, out int num2);

                CellMeta origin = World.inst.GetCellDataClamped(__instance.transform.position).GetMeta();

                Cell cNorthEast = World.inst.GetCellDataClamped(num + 1, num2 + 1);
                Cell cNorthWest = World.inst.GetCellDataClamped(num + 1, num2 - 1);
                Cell cSouthEast = World.inst.GetCellDataClamped(num - 1, num2 + 1);
                Cell cSouthWest = World.inst.GetCellDataClamped(num - 1, num2 - 1);
                
                northEast = Road.ShouldConnect(cNorthEast);
                northWest = Road.ShouldConnect(cNorthWest);
                southEast = Road.ShouldConnect(cSouthEast);
                southWest = Road.ShouldConnect(cSouthWest);

                north = Road.ShouldConnect(World.inst.GetCellDataClamped(num + 1, num2));
                south = Road.ShouldConnect(World.inst.GetCellDataClamped(num - 1, num2));
                east = Road.ShouldConnect(World.inst.GetCellDataClamped(num, num2 + 1));
                west = Road.ShouldConnect(World.inst.GetCellDataClamped(num, num2 - 1));

                CellMeta mNorth = Grid.Cells.Get(World.inst.GetCellDataClamped(num + 1, num2));
                CellMeta mSouth = Grid.Cells.Get(World.inst.GetCellDataClamped(num - 1, num2));
                CellMeta mEast = Grid.Cells.Get(World.inst.GetCellDataClamped(num, num2 + 1));
                CellMeta mWest = Grid.Cells.Get(World.inst.GetCellDataClamped(num, num2 - 1));

                CellMeta mNorthEast = Grid.Cells.Get(cNorthEast);
                CellMeta mNorthWest = Grid.Cells.Get(cNorthWest);
                CellMeta mSouthEast = Grid.Cells.Get(cSouthEast);
                CellMeta mSouthWest = Grid.Cells.Get(cSouthWest);

                if (origin)
                {
                    if (mNorthEast && mNorth && mEast)
                        northEast &= mNorthEast.elevationTier == origin.elevationTier && mNorth.elevationTier == origin.elevationTier && mEast.elevationTier == origin.elevationTier;
                    if (mNorthWest && mNorth && mWest)
                        northWest &= mNorthWest.elevationTier == origin.elevationTier && mNorth.elevationTier == origin.elevationTier && mWest.elevationTier == origin.elevationTier;
                    if (mSouthEast && mSouth && mEast)
                        southEast &= mSouthEast.elevationTier == origin.elevationTier && mSouth.elevationTier == origin.elevationTier && mEast.elevationTier == origin.elevationTier;
                    if (mSouthWest && mSouth && mWest)
                        southWest &= mSouthWest.elevationTier == origin.elevationTier && mSouth.elevationTier == origin.elevationTier && mWest.elevationTier == origin.elevationTier;

                    if (mNorth != null)
                    {
                        if (mNorth.cell.TopStructure && (mNorth.cell.TopStructure.uniqueNameHash == World.gardenHash || Cemetery.IsCemetery(mNorth.cell.x, mNorth.cell.z)))
                            north &= Math.Abs(origin.elevationTier - mNorth.elevationTier) == 0;
                        else
                            north &= Math.Abs(origin.elevationTier - mNorth.elevationTier) <= 1;
                    }
                    if (mSouth != null)
                    {
                        if (mSouth.cell.TopStructure && (mSouth.cell.TopStructure.uniqueNameHash == World.gardenHash || Cemetery.IsCemetery(mSouth.cell.x, mSouth.cell.z)))
                            south &= Math.Abs(origin.elevationTier - mSouth.elevationTier) == 0;
                        else
                            south &= Math.Abs(origin.elevationTier - mSouth.elevationTier) <= 1;
                    }
                    if (mEast != null)
                    {
                        if (mEast.cell.TopStructure && (mEast.cell.TopStructure.uniqueNameHash == World.gardenHash || Cemetery.IsCemetery(mEast.cell.x, mEast.cell.z)))
                            east &= Math.Abs(origin.elevationTier - mEast.elevationTier) == 0;
                        else
                            east &= Math.Abs(origin.elevationTier - mEast.elevationTier) <= 1;
                    }
                    if (mWest != null)
                    {
                        if (mWest.cell.TopStructure && (mWest.cell.TopStructure.uniqueNameHash == World.gardenHash || Cemetery.IsCemetery(mWest.cell.x, mWest.cell.z)))
                            west &= Math.Abs(origin.elevationTier - mWest.elevationTier) == 0;
                        else
                            west &= Math.Abs(origin.elevationTier - mWest.elevationTier) <= 1;
                    }

                }

                if (north)
                {
                    count++;
                }
                if (south)
                {
                    count++;
                }
                if (east)
                {
                    count++;
                }
                if (west)
                {
                    count++;
                }
            }
            catch (Exception ex)
            {
                DebugExt.HandleException(ex);
            }
            return false;
        }
    }
}
