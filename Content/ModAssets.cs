using Elevation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elevation.AssetManagement;

namespace Elevation
{
    public class ModAssets
    {
        public static AssetDB DB { get; private set; }

        public static event Action OnLoad;

        public static void Init()
        {
            Load();
        }

        private static void Load()
        {
            DB = AssetBundleManager.Unpack(Mod.AssetBundlePath, Mod.AssetBundleName);
            OnLoad?.Invoke();

            UI.LoadAll();
            RoadAssets.LoadAll();
            BuildingAssets.LoadAll();

            Mod.dLog("Mod Assets Loaded");
        }
    }
}
