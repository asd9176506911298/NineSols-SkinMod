using HarmonyLib;
using MonsterLove.StateMachine;
using NineSolsAPI;
using RCGFSM.Projectiles;
using RCGMaker.Core;
using System;
using UnityEngine;
using static Linefy.PolygonalMesh;

namespace SkinMod;

[HarmonyPatch]
public class Patches {

    // Patches are powerful. They can hook into other methods, prevent them from runnning,
    // change parameters and inject custom code.
    // Make sure to use them only when necessary and keep compatibility with other mods in mind.
    // Documentation on how to patch can be found in the harmony docs: https://harmony.pardeike.net/articles/patching.html
    //[HarmonyPatch(typeof(PoolManager), "BorrowOrInstantiate",
    //new Type[] { typeof(GameObject), typeof(Vector3), typeof(Quaternion), typeof(Transform), typeof(Action<PoolObject>) })]
    //[HarmonyPrefix]
    //public static bool Patch(ref GameObject __result, GameObject obj, Vector3 position, Quaternion rotation, Transform parent = null, Action<PoolObject> handler = null) {

    //    //ToastManager.Toast(obj.name);

    //    return true; // the original method should be executed
    //}

    [HarmonyPatch(typeof(PoolManager), "Borrow",
    new Type[] { typeof(PoolObject), typeof(Vector3), typeof(Quaternion), typeof(Transform), typeof(Action<PoolObject>) })]
    [HarmonyPrefix]
    public static bool Prefix(ref PoolObject __result, PoolObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, Action<PoolObject> handler = null) {

        if (prefab.name == "MultiSpriteEffect_Prefab 燃燒Variant")
            return false;

        return true; // the original method should be executed
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Actor), "PlayAnimation", new Type[] {typeof(string),typeof(bool),typeof(float)})]
    private static bool PatcUpdate(ref Actor __instance, string stateName) {
        if (__instance != Player.i)
            return true;

        //ToastManager.Toast(stateName);

        GameObject jie = GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/JieChuan(Clone)/Animator");

        if (jie == null)
            return true;

        Animator anim = jie.GetComponent<Animator>();

        if (stateName == "Idle") {
            anim.SetInteger("Status", 0);
        }
        else if (stateName.Contains("Run")) {
            anim.SetInteger("Status", 1);
        }
        if (stateName.Contains("Parry")) {
            anim.SetInteger("Status", 2);
        }

        return true; // the original method should be executed
    }
}
