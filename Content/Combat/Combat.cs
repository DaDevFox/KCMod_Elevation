using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Elevation
{
    public static class Combat
    {
        public static bool smartUnitPathing = true;

        public static float towerRangeIncrement = 1f;

        public static float elevationDamageIncrement { get; set; } = 0.25f;
        public static float catapultSpeedDecrement { get; set; } = 0.05f;
        public static float ogreSpeedDecrement { get; set; } = 0.05f;

        public static float GetDamageMultiplier(IMoveableUnit target, Vector3 sourcePos)
        {
            Cell source = World.inst.GetCellDataClamped(sourcePos);
            if (source == null)
                return -1f;
            CellMeta sourceMeta = Grid.Cells.Get(source);
            if (!sourceMeta)
                return -1f;

            Cell current = World.inst.GetCellData(target.GetPos());
            if (current == null)
                return -1f;
            CellMeta currentMeta = Grid.Cells.Get(current);
            if (currentMeta == null)
                return -1f;

            IMoveableUnit attacker = OrdersManager.inst.FindUnitAt(source.x, source.z, MoveType.Land);
            if (attacker == null) return -1f;

            float difference = (float)(currentMeta.elevationTier - sourceMeta.elevationTier);
            float multiplier = difference > 0f ? 1f / (difference * elevationDamageIncrement + 1f) : (-difference * elevationDamageIncrement + 1f);

            return multiplier;
        }
    }
}
