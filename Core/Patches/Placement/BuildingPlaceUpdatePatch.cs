using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Newtonsoft.Json.Serialization;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(World), "PlaceInternal")]
    public class BuildingPlaceUpdatePatch
    {
        //static void Prefix(Building PendingObj)
        //{
        //    if (PendingObj.GetComponent<RadiusBonus>())
        //        HappinessBonuses.Update(PendingObj);
        //}

        static void Postfix(Building PendingObj, bool undo)
        {
            if (!undo)
            {
                Grid.Buildings.Add(PendingObj);
                BuildingFormatter.UpdateBuilding(PendingObj);
            }
            else
                Grid.Buildings.Remove(PendingObj);
        }

        
    }

    //[HarmonyPatch(typeof(Building), "GetPositionForPerson")]
    //public class HackyWorkingPositionPatch
    //{
    //    static void Postfix(Building __instance, Villager person, ref Vector3 __result)
    //    {
    //        if (__instance.cachedCell == null)
    //            return;
    //        CellMeta meta = Grid.Cells.Get(__instance.cachedCell);
    //        if (meta == null)
    //            return;
    //        if (__result.y < meta.Elevation)
    //            __result.y += meta.Elevation;
    //    }
    //}

    //[HarmonyPatch(typeof(Building), "UpdateConstruction")]
    public class BuildFXPatch
    {
        //static void Postfix(Building __instance)
        //{
        //    if (__instance.constructionProgress >= 1f)
        //    {
        //        Correct(__instance);
        //    }
        //}

        public static void Correct(Building building)
        {
            ArrayExt<PooledObject> needTick = (ArrayExt<PooledObject>)typeof(PooledObject).GetField("needTick", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            building.ForEachTileInBounds((x, z, cell) =>
            {
                PooledObject tempObj = needTick.data[needTick.Count - 1];
                tempObj.Release();
            });
            

            building.ForEachTileInBounds((x, z, cell) =>
            {
                EffectsMan.inst.BuildPuff.CreateAndPlay(cell.Center);
            });
            
        }

        //static bool Prefix(Building __instance, List<OneOffEffect> ___buildEffects)
        //{
        //    CellMeta meta = Grid.Cells.Get(__instance.GetCell());
        //    if (!meta)
        //        return true;

        //    if (__instance.IsVisibleForFog())
        //    {
        //        __instance.ForEachTileInBounds(delegate (int x, int z, Cell cell)
        //        {
        //            OneOffEffect oneOffEffect = EffectsMan.inst.BuildEffect.CreateAndPlay(new Vector3((float)x, 0f, (float)z));
        //            oneOffEffect.AllowRelease = true;
        //            ___buildEffects.Add(oneOffEffect);
        //        });

        //        return false;
        //    }

        //    return true;
        //}
    }

    //[HarmonyPatch(typeof(Building), "UpdateConstruction")]
    //public class BuildFXConstructionPatch
    //{
    //    static void Postfix(Building __instance, List<OneOffEffect> ___buildEffects)
    //    {
    //        CellMeta meta = Grid.Cells.Get(__instance.GetCell());
    //        if (!meta)
    //            return;

    //        if (__instance.constructionProgress >= 1f)
    //        {
    //            foreach (OneOffEffect effect in ___buildEffects)
    //            {
    //                effect.transform.position = new Vector3(effect.transform.position.x, meta.Elevation, effect.transform.position.z);
    //            }
    //        }
    //    }
    //}
}