using HarmonyLib;
using NineSolsAPI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace SkinMod;

[HarmonyPatch]
public class Patches {

    //[HarmonyPatch(typeof(Player), "ResetJumpAndDash")]
    //static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    //    var codes = instructions.ToList();

    //    // Iterate through the instructions
    //    for (int i = 0; i < codes.Count; i++) {
    //        // Check if we are at the specific indices that need to be replaced
    //        if (i == 35 || i == 36 || i == 37) {
    //            // Replace with NOP (No operation) to disable this instruction
    //            yield return new CodeInstruction(OpCodes.Nop);
    //        } else {
    //            // Yield the original instruction as it is
    //            yield return codes[i];
    //        }
    //    }
    //}
    [HarmonyPatch(typeof(Player), "ResetJumpAndDash")]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        var codes = instructions.ToList();

        if (codes.Count > 37) {
            codes[35].opcode = OpCodes.Nop;
            codes[36].opcode = OpCodes.Nop;
            codes[37].opcode = OpCodes.Nop;
        }

        return codes.AsEnumerable();
    }







    //[HarmonyPatch(typeof(PlayerRollState), nameof(PlayerRollState.OnStateEnter))]
    //[HarmonyPrefix]
    //private static bool PatchStoryWalk(ref PlayerRollState __instance) {
    //    // Ensure rollStat and Stat are not null
    //    if (__instance.rollStat == null || __instance.rollStat.Stat == null) {
    //        ToastManager.Toast("rollStat or Stat is null");
    //        return true; // Continue execution even if the patch is not applied
    //    }

    //    // Proceed with the patch if both are not null
    //    //Traverse.Create(__instance.rollStat.Stat).Field("_value").SetValue(SkinMod.Instance.time);
    //    ToastManager.Toast(__instance.rollStat.Value);

    //    return true; // Allow the original method to execute
    //}

    //[HarmonyPatch(typeof(CharacterStat), "Value",methodType:MethodType.Getter)]
    //[HarmonyPostfix]
    //private static void PatchStoryWalk(ref CharacterStat __instance, ref float __result) {
    //    //__result = 0.1f;
    //    //ToastManager.Toast(__result);
    //}
}
