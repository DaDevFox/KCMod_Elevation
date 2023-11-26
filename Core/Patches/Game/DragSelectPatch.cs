using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;
using System.Reflection;
using System.Threading;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(GameUI),"UpdateCellSelector")]
    class DragSelectPatch
    {
        static float yOffset = 0.1f;
        static float xzMargin = 0.1f;


        static void Postfix(GameUI __instance)
        {
            try
            {
                bool dragSelecting = (bool)typeof(GameUI)
                    .GetField("dragSelecting", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(__instance);

                if (!dragSelecting)
                    return;

                if (GameUI.inst.DockCursorModeActive())
                    return;

                Vector3 _dragStart = (Vector3)typeof(GameUI)
                    .GetField("dragStart", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(__instance);
                Vector3 _dragEnd = (Vector3)typeof(GameUI)
                    .GetField("dragEnd", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(__instance);


                Vector3 dragStart = new Vector3((float)((int)_dragStart.x) - xzMargin/2f, _dragStart.y, (float)((int)_dragStart.z) - xzMargin/2f);
                Vector3 dragEnd = new Vector3((float)((int)_dragEnd.x) + xzMargin/2f, _dragEnd.y, (float)((int)_dragEnd.z) + xzMargin/2f);

                Vector3 diff = dragStart - dragEnd;
                Vector3 newSize = new Vector3(Mathf.Abs(diff.x) + 1f, 0f, Mathf.Abs(diff.z) + 1f);

                Vector3 center = dragStart - (diff * 0.5f) + new Vector3(0.5f, 0f, 0.5f);

                __instance.CellHighlighter.SetTargetWorldBounds(center, newSize);
                __instance.CellHighlighter.SetFillY(dragEnd.y - dragStart.y + yOffset);
            }
            catch(Exception ex)
            {
                DebugExt.HandleException(ex);
            }
        }


    }

    #region Incorporable to GridPointerIntersection Changes

    [HarmonyPatch(typeof(DockRouteCursorMode), "UpdateRouteHighlightTest")]
    class LogisticsUIPointerIntersectionHighlightPatch
    {
        static void Prefix(Ray ray, ref Vector3 mousePos)
        {
            Cell cell = World.inst.GetCellDataClamped(mousePos);
            if (cell == null)
                return;

            CellMeta meta = Grid.Cells.Get(cell);
            if (meta == null)
                return;

            mousePos.y = meta.Elevation;
        }
    }

    [HarmonyPatch(typeof(DockRouteCursorMode), "UpdateSpecialEndpointInsertion")]
    class LogisticsUIPointerIntersectionSpecialHighlightPatch
    {
        static void Prefix(ref Vector3 mousePos, Cell cell)
        {
            Cell selectedCell = World.inst.GetCellDataClamped(mousePos);
            if (selectedCell == null)
                return;

            CellMeta meta = Grid.Cells.Get(selectedCell);
            if (meta == null)
                return;

            mousePos.y = meta.Elevation;
        }
    }
    
    [HarmonyPatch(typeof(DockRouteCursorMode), "UpdateSelectedEndpoint")]
    class LogisticsUIPointerIntersectionSelectedEndpointPatch
    {
        static void Prefix(ref Vector3 mousePos, Cell cell)
        {
            Cell selectedCell = World.inst.GetCellDataClamped(mousePos);
            if (selectedCell == null)
                return;

            CellMeta meta = Grid.Cells.Get(selectedCell);
            if (meta == null)
                return;

            mousePos.y = meta.Elevation;
        }
    }


    [HarmonyPatch(typeof(DockRouteCursorMode), "MtxForBuilding")]
    class LogisticBuildingMatrixPatch
    {
        static void Postfix(Building b, int overlapped, ref DockRouteCursorMode.Cylinder c, float ___buildingExpand, Vector3 ___endPointScale, ref Matrix4x4 __result)
        {
            Cell selectedCell = World.inst.GetCellDataClamped(b.transform.position);
            if (selectedCell == null)
                return;
            CellMeta meta = Grid.Cells.Get(selectedCell);
            if (meta == null)
                return;


            Vector3 vector = b.Center();
            vector = new Vector3(vector.x, -0.05f, vector.z);
            float num = (float)overlapped * 0.5f;
            Vector3 a = new Vector3(b.size.x + ___buildingExpand, ___endPointScale.y, b.size.x + ___buildingExpand);
            Vector3 pos = vector - new Vector3(0f, (float)overlapped * 0.05f, 0f);

            pos += new Vector3(0f, meta.Elevation, 0f);

            Vector3 vector2 = a + new Vector3(num, 0f, num);
            c.axis = new Vector3(0f, vector2.y, 0f);
            c.pos = pos;
            c.radius = vector2.x * 0.5f;
            __result = Matrix4x4.TRS(pos, Quaternion.identity, vector2);
        }
    }

    [HarmonyPatch(typeof(DockRouteCursorMode), "ValidCell")]
    class LogisticsUIValidCellPatch
    {
        static void Postfix(Cell cell, ref bool __result)
        {
            if (cell == null)
                return;

            __result &= !WorldRegions.Unreachable.Contains(cell);
        }
    }

    #endregion
}
