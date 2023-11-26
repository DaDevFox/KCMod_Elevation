using Assets;
using I2.Loc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Elevation
{ 
    public class Dugout : MonoBehaviour
    {
        public static List<string> dugoutCells = new List<string>();

        // Partially copied from DestructionCrew.cs

        private void Awake()
        {
            this.b = base.GetComponent<Building>();
        }

        // Token: 0x06000A7B RID: 2683 RVA: 0x000092E0 File Offset: 0x000074E0
        public void OnBuilt()
        {
            this.ReduceTerrain();
        }

        // Token: 0x06000A7C RID: 2684 RVA: 0x0006A080 File Offset: 0x00068280
        private void ReduceTerrain()
        {
            Cell cell = this.b.GetCell();
            if (ElevationManager.TryProcessElevationChange(cell, -1))
            {
                ElevationManager.RefreshTile(cell);

                EffectsMan.inst.TileActionEffect.CreateAndPlay(b.GetCell().Center);
                EffectsMan.inst.GroundImpactEffect.CreateAndPlay(b.GetCell().Center);
                SfxSystem.inst.PlayFromBankIfVisible("buildingcollapse", this.b.Center(), null);
                KingdomLog.TryLog("elevationdecrease", "A crew has managed to artificially reduce terrain, highness.", KingdomLog.LogStatus.Neutral, 1f, cell.Center, false, this.b.LandMass());

                string key = $"{cell.x}_{cell.z}";
                if (Scaffolding.scaffoldedCells.Contains(key))
                    Scaffolding.scaffoldedCells.Remove(key);
                else 
                    dugoutCells.Add($"{cell.x}_{cell.z}");
            }

            World.inst.VanishBuilding(this.b);
            World.inst.rebakeCells.Add(cell);
        }

        private void Update()
        {
        }

        // Token: 0x06000A7E RID: 2686 RVA: 0x000092E8 File Offset: 0x000074E8
        public string GetTitle()
        {
            return ScriptLocalization.RockRemoval;
        }

        // Token: 0x06000A7F RID: 2687 RVA: 0x000033ED File Offset: 0x000015ED
        public string GetNum()
        {
            return "";
        }

        // Token: 0x06000A80 RID: 2688 RVA: 0x000033ED File Offset: 0x000015ED
        public string GetExplanation(OutputTileUI tileOutput)
        {
            return "";
        }

        // Token: 0x06000A81 RID: 2689 RVA: 0x000033ED File Offset: 0x000015ED
        public string GetUnit()
        {
            return "";
        }

        // Token: 0x06000A82 RID: 2690 RVA: 0x00004BB1 File Offset: 0x00002DB1
        public string GetError()
        {
            return null;
        }

        // Token: 0x06000A83 RID: 2691 RVA: 0x00003A10 File Offset: 0x00001C10
        public bool ShowStaffDeficiencyMessage()
        {
            return true;
        }

        // Token: 0x04000A26 RID: 2598
        private Building b;
    }
}
