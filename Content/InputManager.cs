using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;
using Fox.Profiling;
using System.Reflection;

namespace Elevation
{
    public class InputManager
    {
        public static int primary { get; set; } = 0;
        public static int secondary { get; set; } = 1;

        public static bool Primary() => Input.GetMouseButton(primary);
        public static bool PrimaryDown() => Input.GetMouseButtonDown(primary);
        public static bool PrimaryUp() => Input.GetMouseButtonUp(primary);

        public static bool Secondary() => Input.GetMouseButton(primary);
        public static bool SecondaryDown() => Input.GetMouseButtonDown(primary);
        public static bool SecondaryUp() => Input.GetMouseButtonUp(primary);



        public static void Tick()
        {
            Debug.Update();
        }

        public class Debug
        {
            public static void Update()
            {
                Cell selected = GameUI.inst.GetCellSelected();

                if (Settings.debug)
                {
                    UpdatePhantomStone();
                    // Raise/Lower
                    if (Input.GetKeyDown(Settings.keycode_raise))
                    {
                        if (ElevationManager.TryProcessElevationChange(selected, 1))
                        {
                            ElevationManager.RefreshTile(selected);
                            DebugExt.dLog("Elevation raise succesful");
                        }
                    }
                    else if (Input.GetKeyDown(Settings.keycode_lower))
                    {
                        if (ElevationManager.TryProcessElevationChange(selected, -1))
                        {
                            ElevationManager.RefreshTile(selected);
                            DebugExt.dLog("Elevation lower succesful");
                        }
                    }
                        
                    


                    // Sample Cell
                    if (Input.GetKeyDown(Settings.keycode_sampleCell))
                    {
                        CellMeta meta = Grid.Cells.Get(selected);

                        string text = "";

                        text += "Cell at " + selected.Center.ToString() + ": ";
                        text += Environment.NewLine;
                        text += "has meta: " + (meta != null).ToString();
                        text += Environment.NewLine;
                        if (meta)
                        {
                            text += "Mesh Data -";
                            text += Environment.NewLine;
                            text += $"Mesh system: {meta.mesh.system}; loc:{meta.mesh.matrix}_{meta.mesh.id}\n";
                        }
                        text += (Grid.Cells.Get(selected) != null) ? GetConnectedForCell(selected) : "";
                        text += WorldRegions.GetTileRegion(selected) != -1 ? WorldRegions.GetTileRegion(selected).ToString() +
                            Environment.NewLine : "";
                        text += WorldRegions.Unreachable.Contains(selected) ? "<color=red> - Pruned from pathfinding; unreachable</color>" : "" + Environment.NewLine;

                        if (meta.cell.GetTopMostCastlePiece() != null)
                            text += $"Stack height total: {meta.cell.CurrentStackHeight()}";

                        DebugExt.dLog(text);
                    }

                    // Pathfinding
                    if (Input.GetKeyDown(Settings.keycode_pruneCells))
                        WorldRegions.Search();
                    if (Input.GetKeyDown(Settings.keycode_directionReference))
                    {
                        if (selected.TopStructure)
                            Mod.dLog(selected.TopStructure.UniqueName + ":\n" + selected.TopStructure.transform.LabelForEachChildRecursive((child) => $"{child.position}; {child.localPosition}"));

                        DebugExt.dLog("-Z");
                        DebugExt.dLog(selected.Center, false, selected.Center);
                        Cell cell = World.inst.GetCellData(selected.Center + new Vector3(0f, 0f, -1f));
                        if (cell != null)
                            DebugExt.dLog(cell.Center, false, cell.Center);

                        DebugExt.dLog("+X");
                        DebugExt.dLog(selected.Center, false, selected.Center);
                        Cell cell2 = World.inst.GetCellData(selected.Center + new Vector3(1f, 0f, 0f));
                        if (cell2 != null)
                            DebugExt.dLog(cell2.Center, false, cell2.Center);
                    }

                    // Threaded Pathing Visualizers
                    //int thread = -1;
                    //if (Input.GetKeyDown(KeyCode.V))
                    //    thread = 0;
                    //if (Input.GetKeyDown(KeyCode.B))
                    //    thread = 1;
                    //if (Input.GetKeyDown(KeyCode.N))
                    //    thread = 2;
                    //if (Input.GetKeyDown(KeyCode.M))
                    //    thread = 3;
                    //if (Input.GetKeyDown(KeyCode.Comma))
                    //    thread = 4;
                    //if (Input.GetKeyDown(KeyCode.Period))
                    //    thread = 5;
                    //if(thread != -1)
                    //{
                    //    Pathfinder[] pathers = (Pathfinder[])typeof(ThreadedPathing).GetField("pather", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(World.inst.threadedPather);
                    //    int hash = pathers[thread].GetHashCode();

                    //    foreach (DebugPathVisualizer visualizer in DebugPathVisualizer.visualizers.Values)
                    //        visualizer.Active = false;

                    //    if (!DebugPathVisualizer.visualizers.ContainsKey(hash))
                    //        DebugPathVisualizer.visualizers.Add(hash, new DebugPathVisualizer());

                    //    DebugPathVisualizer.visualizers[hash].Active = true;
                    //}


                    // Visuals
                    if (Input.GetKeyDown(Settings.keycode_refreshTerrain))
                        ElevationManager.RefreshTerrain();
                    if (Input.GetKeyDown(Settings.keycode_refreshTile))
                        ElevationManager.RefreshTile(selected, true);

                    // UI
                    if (Input.GetKeyDown(Settings.keycode_toggleLoadingDialog))
                    {
                        if (!UI.loadingDialog.gameObject.activeSelf)
                            UI.loadingDialog.Activate();
                        else
                            UI.loadingDialog.Deactivate();
                    }

                    // Profiler
                    //if (Input.GetKeyDown(KeyCode.P))
                    //{
                    //    if (!SelectiveProfiler.instance.Active)
                    //        SelectiveProfiler.instance.Activate();
                    //    else
                    //        SelectiveProfiler.instance.Deactivate();
                    //}
                }
                if (GameState.inst.AlphaNumericHotkeysEnabled && Input.GetKeyDown(Settings.inst.c_CameraControls.s_activateKey.Key))
                {
                    TopDownModeCamera.ToggleTopDownView();
                }
            }


            private static string GetConnectedForCell(Cell cell)
            {
                CellMeta meta = Grid.Cells.Get(cell);
                string text = "\n";

                if(ElevationPathfinder.main is ExternalPathfinder)
                {
                    ExternalPathfinder pathfinder = ElevationPathfinder.main as ExternalPathfinder;
                    ExternalPathfinder.Node node = pathfinder.aStar.SearchSpace[cell.x, cell.z];

                    text += $"Path Grid Size: {pathfinder.aStar.SearchSpace.GetLength(0)}x{pathfinder.aStar.SearchSpace.GetLength(1)}";

                    if(node != null)
                    {
                        text += " - Cell has PF Node";
                    }
                }

                if (ElevationPathfinder.main is PrebakedPathfinder)
                {
                    PrebakedPathfinder.Node node = PrebakedPathfinder.GetAt(meta.cell);

                    if (meta == null || node == null)
                        return "";


                    text = node.ToString() + Environment.NewLine;

                    text += "Connected: " + node.connected.Count.ToString();

                    foreach (KeyValuePair<PrebakedPathfinder.Node, float> pair in node.connected)
                    {
                        text += Environment.NewLine;
                        text += $"{pair.Key} with weight {pair.Value}";
                    }
                    text += Environment.NewLine;


                    return text;
                }

                text += "\nConnected: \n";
                foreach (CellMeta neighbor in meta.neighborsPlusFast)
                    if (neighbor != null)
                        text += $"neighbor: {neighbor.ToString()}{Environment.NewLine}";
                return text;
            }

            // Courtesy of Phedg1
            #region Phantom Stone

            private static bool f2Down = false;
            private static bool mousePointerDown = false;

            private static void UpdatePhantomStone()
            {
                if (Input.GetKeyDown(KeyCode.F2))
                {
                    f2Down = true;
                }
                if (f2Down)
                {
                    if (PointingSystem.GetPointer().GetPrimaryDown())
                    {
                        mousePointerDown = true;
                    }
                    if (PointingSystem.GetPointer().GetPrimaryUp())
                    {
                        if (mousePointerDown)
                        {
                            mousePointerDown = false;
                            Cell cell = World.inst.GetCellData(GameUI.inst.GridPointerIntersection());
                            int modelsLength = 0;
                            if (cell.Models != null)
                            {
                                modelsLength = cell.Models.Count;
                            }
                            Log("CELL MODELS: " + modelsLength.ToString());
                            Log("CELL TYPE: " + cell.Type.ToString());
                            Vector2 cellCenter = new Vector2(cell.x + 0.5f, cell.z + 0.5f);

                            int modelsOutsideTheirTile = 0;
                            List<Vector2> allCellModelPositions = new List<Vector2>();
                            for (int x = 0; x < World.inst.GridWidth; x++)
                            {
                                for (int z = 0; z < World.inst.GridHeight; z++)
                                {
                                    Cell worldCell = World.inst.GetCellData(x, z);
                                    if (worldCell.Models != null)
                                    {
                                        Vector2 worldCellCenter = new Vector2(worldCell.x + 0.5f, worldCell.z + 0.5f);
                                        foreach (GameObject model in worldCell.Models)
                                        {
                                            if (DistanceBetweenTwoPoints(worldCellCenter, model.transform.position) > 0.75f)
                                            {
                                                modelsOutsideTheirTile += 1;
                                            }
                                            allCellModelPositions.Add(RoundToThreePlaces(model.transform.position));
                                        }
                                    }
                                }
                            }
                            Log("MODELS OUTSIDE THEIR TILE: " + modelsOutsideTheirTile.ToString());
                            int modelsNotAttachedToACell = 0;
                            int ghostModels = 0;
                            for (int childIndex = 0; childIndex < World.inst.resourceContainer.transform.childCount; childIndex++)
                            {
                                if (!allCellModelPositions.Contains(RoundToThreePlaces(World.inst.resourceContainer.transform.GetChild(childIndex).position)))
                                {
                                    modelsNotAttachedToACell += 1;
                                }
                                if (DistanceBetweenTwoPoints(cellCenter, World.inst.resourceContainer.transform.GetChild(childIndex).position) < 0.71f)
                                {
                                    ghostModels += 1;
                                }
                            }
                            Log("MODELS NOT ATTACHED TO A CELL: " + modelsNotAttachedToACell.ToString());
                            Log("GHOST MODELS: " + ghostModels.ToString());
                        }
                    }
                }
                if (Input.GetKeyUp(KeyCode.F2))
                {
                    f2Down = false;
                }
            }

            private static Vector2 RoundToThreePlaces(Vector3 givenPosition)
            {
                return new Vector2(Mathf.RoundToInt(givenPosition.x * 1000), Mathf.RoundToInt(givenPosition.z * 1000));
            }

            private static float DistanceBetweenTwoPoints(Vector2 pointA, Vector3 pointB)
            {
                return Mathf.Sqrt(Mathf.Pow(pointA.x - pointB.x, 2) + Mathf.Pow(pointA.y - pointB.z, 2));
            }

            public static void Log(object givenObject, bool traceBack = false)
            {
                if (givenObject == null)
                {
                    DebugExt.Log("null");
                }
                else
                {
                    DebugExt.dLog(givenObject);
                }
                if (traceBack)
                {
                    DebugExt.dLog(StackTraceUtility.ExtractStackTrace());
                }
            }

            #endregion
        }

    }
}
