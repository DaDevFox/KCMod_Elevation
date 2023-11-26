using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Elevation.Utils;

namespace Elevation
{

    public class UI
    {
        private static UI instance = new UI();
        
        public static GameObject raiseLowerUIPrefab { get; private set; }
        public static GameObject loadingDialogPrefab { get; private set; }
        public static GameObject terrainVisualIndicatorPrefab { get; private set; }
        
        public static RaiseLowerUI raiseLowerUI { get; private set; }

        /// <summary>
        /// Central reference to a loading dialog that can be used to indicate a loading process, should render immediately rather than at end of frame when Activate() is called
        /// </summary>
        public static LoadingDialog loadingDialog { get; private set; }

        public static bool loaded { get; private set; } = false;
        public static bool created { get; private set; } = false;

        public UI()
        {
            ModAssets.OnLoad += LoadAll;
        }

        public static void LoadAll()
        {
            TweeningManager instance = new GameObject("TweeningManager").AddComponent<TweeningManager>();

            raiseLowerUIPrefab = ModAssets.DB.GetByName<GameObject>("ElevationRaiseLowerControls");
            loadingDialogPrefab = ModAssets.DB.GetByName<GameObject>("GeneratingOverlay");
            
            terrainVisualIndicatorPrefab = ModAssets.DB.GetByName<GameObject>("TerrainIndicator");

            
            loaded = true;
            Mod.dLog("UI Assets Preloaded");
        }

        void SceneLoaded()
        {
            

            GameObject raiseLowerUIObj = GameObject.Instantiate(
                raiseLowerUIPrefab,
                GameState.inst.mainMenuMode.mapEditUI.transform);
            raiseLowerUI = raiseLowerUIObj.AddComponent<RaiseLowerUI>();

            GameObject loadingDialogObj = GameObject.Instantiate(
                loadingDialogPrefab,
                GameState.inst.playingMode.GameUIParent.transform.GetChild(0));
            loadingDialog = loadingDialogObj.AddComponent<LoadingDialog>();

            Mod.dLog(GameState.inst.mainMenuMode.mainMenuUI.transform.GetChild(0).name);

            //GameState.inst.mainMenuMode.newMapUI.GetComponent<NewMapUI>().doneButton.onClick.AddListener(() => { GameObject.FindObjectOfType<Mod>().Invoke("PrunePathfindingDeferred", 1f); });

            created = true;
            Mod.dLog("UI Created");
        }
    }
}
