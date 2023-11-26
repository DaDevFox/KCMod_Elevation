using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Elevation.AssetManagement;
using Elevation.Patches;

namespace Elevation
{
    public class Roads
    {

        public static int pathHash { get; } = "path".GetHashCode();

        #region Road Resizing

        public static Dictionary<string, float> axisScaleLookup { get; } = new Dictionary<string, float>()
        {
            { "road", 0.8f },
            { "stoneroad", 0.98f }
        };

        #endregion

        public static void HandleBuildingChange(OnBuildingAddRemove @event)
        {
            Road road = @event.targetBuilding.GetComponent<Road>();
            if (road == null)
                return;

            RoadStairs.HandleRoadChange(@event);
        }

    }

    public class RoadAssets
    {
        #region Stairs

        public static Mesh stairs_normal { get; private set; }
        public static Mesh stairs_stone { get; private set; }

        public static Material stairs_normalMaterial { get; private set; }
        public static Material stairs_stoneMaterial { get; private set; }

        #endregion

        static RoadAssets()
        {
            ModAssets.OnLoad += LoadAll;
        }

        public static void LoadAll()
        {
            Mod.dLog("loading road assets");
            LoadAssets();
            RoadStairs.Init();
        }

        private static void LoadAssets()
        {
            stairs_normal = ModAssets.DB.GetByName<GameObject>("roadStairs_normal").GetComponent<MeshFilter>().mesh;
            stairs_stone = ModAssets.DB.GetByName<GameObject>("roadStair_stone").GetComponent<MeshFilter>().mesh;            
        
            stairs_normalMaterial = ModAssets.DB.GetByName<GameObject>("roadStairs_normal").GetComponent<MeshRenderer>().material;
            stairs_stoneMaterial = ModAssets.DB.GetByName<GameObject>("roadStair_stone").GetComponent<MeshRenderer>().material;                
        }



        //public static void LoadAssets()
        //{
        //    assets = new Dictionary<string, GameObject>();

        //    #region Stone

        //    //Straight
        //    assets.Add("stone/straight/s1", new GameObject());
        //    assets.Add("stone/straight/s2", new GameObject());

        //    assets.Add("stone/straight/s1_2", new GameObject());

        //    //Elbow
        //    assets.Add("stone/elbow/s1", new GameObject());
        //    assets.Add("stone/elbow/s2", new GameObject());

        //    assets.Add("stone/elbow/s1_2", new GameObject());

        //    //Threeway

        //    assets.Add("stone/threeway/s1", new GameObject());
        //    assets.Add("stone/threeway/s2", new GameObject());
        //    assets.Add("stone/threeway/s3", new GameObject());

        //    //assets["stone/threeway/s1_2"] = new GameObject();
        //    //assets["stone/threeway/s1_3"] = new GameObject();
        //    //assets["stone/threeway/s2_3"] = new GameObject();

        //    //assets["stone/threeway/s1_2_3"] = new GameObject();

        //    //Fourway
        //    //assets["stone/fourway/s1"] = new GameObject();
        //    //assets["stone/fourway/s2"] = new GameObject();
        //    //assets["stone/fourway/s3"] = new GameObject();
        //    //assets["stone/fourway/s4"] = new GameObject();

        //    //assets["stone/fourway/s1_2"] = new GameObject();
        //    //assets["stone/fourway/s1_3"] = new GameObject();
        //    //assets["stone/fourway/s1_4"] = new GameObject();
        //    //assets["stone/fourway/s2_3"] = new GameObject();
        //    //assets["stone/fourway/s2_4"] = new GameObject();
        //    //assets["stone/fourway/s3_4"] = new GameObject();

        //    //assets["stone/fourway/s1_2_3"] = new GameObject();
        //    //assets["stone/fourway/s1_2_4"] = new GameObject();
        //    //assets["stone/fourway/s1_3_4"] = new GameObject();
        //    //assets["stone/fourway/s2_3_4"] = new GameObject();

        //    #endregion

        //    #region Normal

        //    //Straight
        //    //assets["normal/straight/s1"] = new GameObject();
        //    //assets["normal/straight/s2"] = new GameObject();

        //    //assets["normal/straight/s1_2"] = new GameObject();


        //    //Elbow
        //    //assets["normal/elbow/s1"] = new GameObject();
        //    //assets["normal/elbow/s2"] = new GameObject();
        //    //assets["normal/elbow/s1_2"] = new GameObject();

        //    //Threeway
        //    //assets["normal/threeway/s1"] = new GameObject();
        //    //assets["normal/threeway/s2"] = new GameObject();
        //    //assets["normal/threeway/s3"] = new GameObject();

        //    //assets["normal/threeway/s1_2"] = new GameObject();
        //    //assets["normal/threeway/s1_3"] = new GameObject();
        //    //assets["normal/threeway/s2_3"] = new GameObject();

        //    //assets["normal/threeway/s1_2_3"] = new GameObject();

        //    //Fourway
        //    //assets["normal/fourway/s1"] = new GameObject();
        //    //assets["normal/fourway/s2"] = new GameObject();
        //    //assets["normal/fourway/s3"] = new GameObject();
        //    //assets["normal/fourway/s4"] = new GameObject();

        //    //assets["normal/fourway/s1_2"] = new GameObject();
        //    //assets["normal/fourway/s1_3"] = new GameObject();
        //    //assets["normal/fourway/s1_4"] = new GameObject();
        //    //assets["normal/fourway/s2_3"] = new GameObject();
        //    //assets["normal/fourway/s2_4"] = new GameObject();
        //    //assets["normal/fourway/s3_4"] = new GameObject();

        //    //assets["normal/fourway/s1_2_3"] = new GameObject();
        //    //assets["normal/fourway/s1_2_4"] = new GameObject();
        //    //assets["normal/fourway/s1_3_4"] = new GameObject();
        //    //assets["normal/fourway/s2_3_4"] = new GameObject();

        //    #endregion

        //}


        //public static GameObject Get(string assetName)
        //{
        //    return assets.ContainsKey(assetName) ? assets[assetName] : null;
        //}
    }
}
