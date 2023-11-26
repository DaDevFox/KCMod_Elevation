using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;

namespace Elevation.Patches
{

    [HarmonyPatch(typeof(UnitSystem), "TargetGroupInRange")]
    public class MeleeRangePatch
    {
        static bool Prefix(UnitSystem.Army myGroup, IMoveableUnit targetGroup, ref bool __result)
        {
            Vector3 targetPos = targetGroup.GetPos();

            Cell target = World.inst.GetCellDataClamped(targetGroup.GetPos());
            if (target == null)
                return true;

            CellMeta targetMeta = Grid.Cells.Get(target);
            if (targetMeta == null || targetMeta.elevationTier == 0)
                return true;

            Cell current = World.inst.GetCellDataClamped(myGroup.generalPos);
            if (current == null)
                return true;

            CellMeta currentMeta = Grid.Cells.Get(current);
            if (currentMeta == null)
                return true;

            bool grounded;
            if (currentMeta != targetMeta)
                grounded = Mathf.Approximately(myGroup.generalPos.y, currentMeta.Elevation) && (Mathf.Approximately(targetPos.y, targetMeta.Elevation) || targetMeta.elevationTier == 0);
            else
                grounded = Mathf.Abs(myGroup.generalPos.y - targetPos.y) <= ElevationManager.elevationInterval && Mathf.Approximately(targetPos.y, targetMeta.Elevation);

            if ((grounded && Math.Abs(currentMeta.elevationTier - targetMeta.elevationTier) <= 1) || Mathf.Abs(myGroup.generalPos.y - targetGroup.GetPos().y) < 0.21f)
            {
                float num = myGroup.meleeRange * myGroup.meleeRange;
                float num2 = Mathff.DistSqrdXZ(targetGroup.GetPos(), myGroup.tilePos);
                if (num2 < num && num2 < 2.25f)
                {
                    __result = true;
                    return false;
                }
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(UnitSystem), "MoveTo", argumentTypes:new Type[] {typeof(UnitSystem.Army), typeof(Vector3), typeof(float)})]
    public class GeneralYCorrectionPatch
    {
        static bool Prefix(UnitSystem.Army group, Vector3 targetPos, float dt, ref bool __result)
        {
            Cell target = World.inst.GetCellDataClamped(targetPos);
            if (target == null)
                return true;

            CellMeta targetMeta = Grid.Cells.Get(target);
            if (targetMeta == null || targetMeta.elevationTier == 0)
                return true;

            Cell current = World.inst.GetCellDataClamped(group.generalPos);
            if (current == null) 
                return true;

            CellMeta currentMeta = Grid.Cells.Get(current);
            if (currentMeta == null)
                return true;

            float num = 0.0001f;
            bool grounded = false;
            
            if(currentMeta != targetMeta)
                grounded = Mathf.Approximately(group.generalPos.y, currentMeta.Elevation) && (Mathf.Approximately(targetPos.y, targetMeta.Elevation) || targetMeta.elevationTier == 0);
            else
                grounded = Mathf.Abs(group.generalPos.y - targetPos.y) <= ElevationManager.elevationInterval && Mathf.Approximately(targetPos.y, targetMeta.Elevation);


            if (grounded)
            {
                //DebugExt.dLog("Grounded movement", true);
                group.generalPos = Mathff.MoveTowards(group.generalPos.xz(), targetPos.xz(), group.speed * dt);
                group.generalPos.y = YInterpolation.GetSlantSlopedY(group.generalPos);

                Vector3 distance = group.generalPos.xz() - targetPos.xz();
                if (distance.x * distance.x + distance.z * distance.z < num)
                {
                    __result = true;
                    return false;
                }
            }
            else
            {
                if (targetPos.y > group.generalPos.y)
                {
                    Vector3 vector = targetPos;
                    vector.y = group.generalPos.y;
                    group.generalPos = Mathff.MoveTowards(group.generalPos, vector, group.speed * dt);
                    Vector3 vector2 = vector - group.generalPos;
                    if (vector2.x * vector2.x + vector2.z * vector2.z < num)
                    {
                        group.generalPos = Mathff.MoveTowards(group.generalPos, targetPos, group.speed * dt);
                        if (Mathff.Abs(group.generalPos.y - targetPos.y) < 0.01f)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
                else
                {
                    Vector3 generalPos = group.generalPos;
                    generalPos.y = targetPos.y;
                    group.generalPos = Vector3.MoveTowards(group.generalPos, generalPos, group.speed * dt);
                    if (Mathff.Abs(group.generalPos.y - targetPos.y) < 0.01f)
                    {
                        group.generalPos = Vector3.MoveTowards(group.generalPos, targetPos, group.speed * dt);
                        Vector3 vector3 = targetPos - group.generalPos;
                        if (vector3.x * vector3.x + vector3.z * vector3.z < num)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(UnitSystem), "UpdateUnitsTargetPos")]
    public class UnitYCorrectionPatch
    {
        static bool Prefix(UnitSystem.Army group)
        {
            Cell cell = World.inst.GetCellDataClamped(group.generalPos);
            if (cell == null)
                return true;

            CellMeta meta = Grid.Cells.Get(cell);
            if(meta == null || meta.elevationTier == 0) 
                return true;

            Vector3 current = group.generalPos;
            if (group.pathIdx < group.path.Count && group.path.Count > 0)
            {
                current = group.path.data[group.pathIdx];
            }
            if (group.blocking)
            {
                current = group.tilePos;
            }
            if (current.y < group.generalPos.y)
            {
                current = group.generalPos;
            }
            else
            {
                current.y = group.generalPos.y;
            }
            float num = 1f;
            if (current.y > meta.Elevation)
            {
                num = 0.65f;
            }
            int i = 0;
            int count = group.units.Count;
            while (i < count)
            {
                UnitSystem.Unit unit = group.units.data[i];
                Vector3 offset = group.tilePosOffsets[i % group.squadSize];
                unit.targetPos.x = current.x + offset.x * num;
                unit.targetPos.z = current.z + offset.z * num;
                unit.targetPos.y = group.moving ? YInterpolation.GetMidpointSlopedY(unit.pos) : current.y + offset.y * num;

                i++;
            }

            return false;
        }
    }
}
