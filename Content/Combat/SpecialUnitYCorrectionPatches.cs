using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(SiegeCatapult), "Tick")]
    public class CatapultYCorrectionPatch
    {
        static void Postfix(SiegeCatapult __instance, ref float ___speedMul)
        {
            Vector3 pos = __instance.transform.position;

            CellMeta meta = Grid.Cells.Get(pos);
            if (!meta)
                return;

            if(___speedMul == 1f)
                ___speedMul = 1f - (float)meta.elevationTier * Combat.catapultSpeedDecrement;

            pos.y = YInterpolation.GetSlantSlopedY(pos);
            __instance.transform.position = pos;
            __instance.transform.rotation = Quaternion.Euler(0f, __instance.transform.rotation.eulerAngles.y, 0f);


            foreach(Transform soldier in __instance.footSoldiers)
            {
                soldier.position += new Vector3(0f, YInterpolation.GetSlantSlopedY(soldier.position), 0f);
            }
        }
    }

    [HarmonyPatch(typeof(SiegeMonster), "Update")]
    public class OgreYCorrectionPatch
    {
        static void Postfix(SiegeMonster __instance, ref float ___speedMul)
        {
            Vector3 pos = __instance.transform.position;
            CellMeta meta = Grid.Cells.Get(pos);
            if (!meta)
                return;

            if (___speedMul == 1f)
                ___speedMul = 1f - (float)meta.elevationTier * Combat.ogreSpeedDecrement;

            pos.y = YInterpolation.GetSlantSlopedY(pos);
            __instance.transform.position = pos;
        }
    }
}
