using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;
using System.Reflection;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(CastleBlock), "PlaceDoor")]
    class CastleBlockDoorPatch
    {
        static void Prefix(CastleBlock __instance, ref Vector3 position, ref Vector3 faceDirection)
        {
            CellMeta meta = Grid.Cells.Get(__instance.GetComponent<Building>().GetCell());
            if (meta != null)
            {
                //position.y = __instance.transform.localPosition.y - (meta.Elevation > 0 ? meta.Elevation - ElevationManager.elevationInterval : 0);
                faceDirection.y = 0f;
            }
        }
    }


    [HarmonyPatch(typeof(CastleBlock), "UpdateBlock")]
    class CastleBlockUpdateDoorPatch
    {
        static void Postfix(CastleBlock __instance)
        {
            Cell current = __instance.GetComponent<Building>().GetCell();
            CellMeta meta = Grid.Cells.Get(current);
            if (meta != null && meta.elevationTier > 0)
            {
                Cell[] neighborCells = new Cell[4];

                Building building = __instance.GetComponent<Building>();
                World.inst.GetNeighborCells(current, ref neighborCells);


                int idx = -1;
                for (int n = 0; n < current.OccupyingStructure.Count; n++)
                {
                    if (current.OccupyingStructure[n] == building)
                    {
                        idx = n;
                        break;
                    }
                }

                float selfHeight = BuildingFormatter.GetAbsoluteHeightOfBuildingAtIndex(current, idx);
                DebugExt.dLog(" -- " + idx.ToString() + " -- ", true);
                DebugExt.dLog(selfHeight.ToString(), true);

                typeof(CastleBlock).GetMethod("ClearDoors", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[] { });
                for (int m = 0; m < neighborCells.Length; m++)
                {
                    float otherHeight = neighborCells[m].GetAbsoluteHeightTotal();

                    if (otherHeight > 0f)
                        DebugExt.dLog(otherHeight.ToString(), true);
                    else
                        continue;

                    Cell neighbor = neighborCells[m];
                    if (neighbor != null)
                    {
                        if (((selfHeight - 0.5f >= otherHeight && idx == 0) || (Mathf.Approximately(selfHeight - 0.5f, otherHeight)))  && otherHeight > 0)
                        {
                            DebugExt.dLog("Connection!", true);

                            Vector3 doorPos = current.Center - ((current.Center - neighbor.Center) / 2);
                            doorPos.y = otherHeight;

                            Vector3 direction = (current.Center - neighbor.Center).normalized.xz();

                            DebugExt.dLog(doorPos, true, doorPos);

                            typeof(CastleBlock)
                                .GetMethod("PlaceDoor", BindingFlags.Instance | BindingFlags.NonPublic)
                                .Invoke(__instance, new object[] { doorPos, direction });
                            
                            typeof(CastleBlock)
                                .GetMethod("VisibleDoors", BindingFlags.Instance | BindingFlags.NonPublic)
                                .Invoke(__instance, new object[] { true });
                        }
                        if (neighbor.TopStructureCategoryIs(World.projectileTopperHash))
                        {
                            Building tower = neighbor.StructureFindByCategory(World.projectileTopperHash);
                            
                            BuildingFormatter.UpdateBuilding(tower);
                        }
                    }
                }
            }
        }
    }
}
