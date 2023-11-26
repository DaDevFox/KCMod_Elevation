using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zat.Shared.InterModComm;
using UnityEngine;

namespace Elevation.API
{
    public static class API
    {
        public static string PortObjectName { get; } = "ElevationAPI";

        private static IMCPort port;

        /*
         * Data Structures:
         * 
             * ElevationTileData (struct):
             *   int x
             *   int z
             *   int elevationTier
         *
             * RefrehTileData (struct) 
             * int x
             * int z
             * bool force
         *
            * SubmodSetData (struct):
            * string submodID
            * bool active
         *  
         * Commands:
         * 
         * -- Elevation
         * void Elevation:Refresh(bool forceUpdate)
         * void Elevation:RefreshTile(RefreshTileData position)
         * 
         * void Elevation:Set(ElevationTileData newData)
         * int Elevation:Get(Vector2 position)
         * 
         * -- Submod
         * void Submod:Set(SubmodSetData data)
         */

        public static void Init()
        {
            port = new GameObject(PortObjectName).AddComponent<IMCPort>();

            // Elevation
            port.RegisterReceiveListener<bool>("Elevation:Refresh", h_ElevationRefresh);
            port.RegisterReceiveListener<RefreshTileData>("Elevation:RefreshTile", h_ElevationRefreshTile);

            port.RegisterReceiveListener<ElevationTileData>("Elevation:Set", h_ElevationSet);
            port.RegisterReceiveListener<Vector2>("Elevation:Get", h_ElevationGet);

            // Submods
            port.RegisterReceiveListener<SubmodSetData>("Submod:Set", h_SubmodSet);
        }

        #region Elevation

        private static void h_ElevationRefresh(IRequestHandler handler, string source, bool forced)
        {
            ElevationManager.RefreshTerrain(forced);
        }

        private static void h_ElevationRefreshTile(IRequestHandler handler, string source, RefreshTileData data)
        {
            CellMeta meta = Grid.Cells.Get(data.x, data.z);
            if (meta)
                ElevationManager.RefreshTile(meta.cell, data.force);
        }

        private static void h_ElevationGet(IRequestHandler handler, string source, Vector2 position)
        {
            CellMeta meta = Grid.Cells.Get((int)position.x, (int)position.y);
            if (meta)
                handler.SendResponse<int>(PortObjectName, meta.elevationTier);
        }

        private static void h_ElevationSet(IRequestHandler handler, string source, ElevationTileData data)
        {
            CellMeta meta = Grid.Cells.Get((int)data.x, (int)data.z);
            if (meta)
                ElevationManager.SetElevation(meta.cell, data.elevationTier);
        }


        public struct ElevationTileData
        {
            public int x;
            public int z;
            public int elevationTier;
        }

        public struct RefreshTileData
        {
            public int x;
            public int z;
            public bool force;
        }

        #endregion

        #region Submods

        private static void h_SubmodSet(IRequestHandler handler, string source, SubmodSetData data)
        {
            Submods.submodActivators[data.submodID].Invoke(data.active);
        }

        public struct SubmodSetData
        {
            public string submodID;
            public bool active;
        }


        #endregion

    }
}
