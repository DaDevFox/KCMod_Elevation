using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace Elevation.Patches
{

    [HarmonyPatch(typeof(FogOfWar), "Start")]
    public class FOWCubeReplacementPatch
    {
        static void Postfix(FogOfWar __instance)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            Mesh cubeMesh = cube.GetComponent<MeshFilter>().mesh;

            __instance.mesh = cubeMesh;

            GameObject.Destroy(cube);
        }
    }

    [HarmonyPatch(typeof(FogOfWar), "UpdateHeightFor")]
    public class FOWCubeHeightPatch
    {
        static void Postfix(FogOfWar.FOWCube newCube)
        {
            if (newCube == null)
                return;

            Cell cell = World.inst.GetCellDataClamped(newCube.pos);
            if (cell == null)
                return;

            CellMeta meta = Grid.Cells.Get(cell);
            if (!meta)
                return;

            newCube.pos.y = meta.Elevation / 2f + 0.0249f;
            newCube.origScale.y = meta.Elevation + 0.1f;
            newCube.origScale.x = 1.001f;
            newCube.origScale.z = 1.001f;
        }
    }
}
