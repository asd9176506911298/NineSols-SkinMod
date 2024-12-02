using HarmonyLib;
using NineSolsAPI;
using System;

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
}
