using UnityEngine;
using Harmony;
using System.Reflection;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(UnitSystem.Army))]
    public class ArmyDamagePatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(UnitSystem.Army).GetMethod("IProjectileHitable.TakeProjectileDamage", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        static void Prefix(UnitSystem.Army __instance, ref float dmg, DamageType dmgType, DamageSource dmgSource, Vector3 sourcePos, Vector3 incomingVel, int teamID)
        {
            float multiplier = Combat.GetDamageMultiplier(__instance, sourcePos);
            if (multiplier == -1f)
                return;

            dmg *= multiplier;
        }
    }

    [HarmonyPatch(typeof(SiegeCatapult))]
    public class CatapultDamagePatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(SiegeCatapult).GetMethod("IProjectileHitable.TakeProjectileDamage", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        static void Prefix(SiegeCatapult __instance, ref float dmg, DamageType dmgType, DamageSource dmgSource, Vector3 sourcePos, Vector3 incomingVel, int attackerTeamID)
        {
            float multiplier = Combat.GetDamageMultiplier(__instance, sourcePos);
            if (multiplier == -1f)
                return;

            dmg *= multiplier;
        }
    }

    [HarmonyPatch(typeof(SiegeMonster))]
    public class OgreDamagePatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(SiegeMonster).GetMethod("IProjectileHitable.TakeProjectileDamage", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        static void Prefix(SiegeMonster __instance, ref float dmg, DamageType dmgType, DamageSource dmgSource, Vector3 sourcePos, Vector3 incomingVel, int attackerTeamID)
        {
            float multiplier = Combat.GetDamageMultiplier(__instance, sourcePos);
            if (multiplier == -1f)
                return;

            dmg *= multiplier;
        }
    }
}
