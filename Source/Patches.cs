using HarmonyLib;
using MonsterLove.StateMachine;
using NineSolsAPI;
using RCGFSM.Projectiles;
using RCGMaker.Core;
using System;
using System.Linq;
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

        //if(SkinMod.Instance.isEnableSkin)
        if (prefab.name == "MultiSpriteEffect_Prefab 燃燒Variant")
            return false;

        return true; // the original method should be executed
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Actor), "PlayAnimation", new Type[] {typeof(string),typeof(bool),typeof(float)})]
    private static bool PatcUpdate(ref Actor __instance, string stateName) {
        if (__instance != Player.i)
            return true;

        //ToastManager.Toast("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/Attack(Clone)/Animator");

        

        GameObject atkObject = GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/Attack(Clone)/Animator");

        if (atkObject != null) {
            Animator atkAnim = atkObject.GetComponent<Animator>();
            //if ((stateName.Contains("Attack1") /*|| stateName.Contains("Attack2") || stateName.Contains("Attack3") || stateName.Contains("AirAttack"))*/ && !stateName.Contains("Charge"))){
            if (stateName == "Attack1" || stateName == "AirAttack" || stateName == "WallAttack" || stateName == "RopeAttack") {
                atkAnim.SetInteger("Attack", 100);
            } else if (stateName.Contains("Attack2")) {
                atkAnim.SetInteger("Attack", 101);
            } else if (stateName.Contains("Attack3")) {
                atkAnim.SetInteger("Attack", 102);
            } else if (stateName.Contains("Fall") || stateName.Contains("Jump") || stateName.Contains("ParryAirSpinNormalHit")) {
                atkAnim.SetInteger("Attack", 201);
            } else if (stateName.Contains("Foo")) {
                atkAnim.SetInteger("Attack", 2);
            } else if (stateName.Contains("Attack") && stateName.Contains("Charge")) {
                atkAnim.SetInteger("Attack", 3);
            } else if (stateName.Contains("ParryCounterDeflectAttack")) {
                atkAnim.SetInteger("Attack", 4);
            } else if (stateName.Contains("Parry")) {
                atkAnim.SetInteger("Attack", 5);
            } else if (stateName.Contains("Run") || stateName.Contains("Jump") || stateName.Contains("DashRoll")) {
                atkAnim.SetInteger("Attack", 6);
            } else {
                atkAnim.SetInteger("Attack", 0);
            }
        }

        //ToastManager.Toast(stateName);

        GameObject curObject = GameObject.Find($"GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/{SkinMod.Instance.objectName}(Clone)/Animator");

        //ToastManager.Toast(curObject);

        if (curObject == null)  
            return true;

        Animator anim = curObject.GetComponent<Animator>();

        if (stateName == "Idle") {
            anim.SetInteger("Status", 0);
        } else if (stateName.Contains("Run")) {
            anim.SetInteger("Status", 1);
        } else if (stateName.Contains("Parry")) {
            anim.SetInteger("Status", 2);
        } else if (stateName.Contains("DashRoll")) {
            anim.SetInteger("Status", 3);
        } else if (stateName.Contains("Heal")) {
            anim.SetInteger("Status", 4);
        } else if (stateName.Contains("AirAttack")) {
            anim.SetInteger("Status", 5);
        } else if (stateName.Contains("Shoot") && ( stateName.Contains("Prepare") || stateName.Contains("In Air"))) {
            anim.SetInteger("Status", 6);
        } else if (stateName.Contains("Rope")){
            ToastManager.Toast("Rope");
            if(stateName.Contains("Idle"))
                anim.SetInteger("Status", 100);
            else if (stateName.Contains("Up") || stateName.Contains("Down"))
                anim.SetInteger("Status", 101);
        } else {
            anim.SetInteger("Status", 0);
        }

        

        return true; // the original method should be executed
    }
}
