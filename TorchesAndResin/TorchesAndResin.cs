﻿using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace TorchesAndResin {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class TorchesAndResin : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.torchesandresin";
    public const string PluginName = "TorchesAndResin";
    public const string PluginVersion = "1.3.0";

    const float TorchStartingFuel = 10000f;

    static readonly int FuelHashCode = "fuel".GetStableHashCode();
    static readonly string[] EligibleTorchItemNames = {
      "piece_groundtorch_wood", // standing wood torch
      "piece_groundtorch", // standing iron torch
      "piece_walltorch", // sconce torch
      "piece_brazierceiling01", // hanging brazier
      "piece_brazierfloor01" // floor brazier
    };

    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    Harmony _harmony;

    public void Awake() {
      IsModEnabled = Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginVersion);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(Fireplace))]
    class FireplacePatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(Fireplace.Awake))]
      static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Ldstr, "UpdateFireplace"),
                new CodeMatch(OpCodes.Ldc_R4, 0f),
                new CodeMatch(OpCodes.Ldc_R4, 2f))
            .Advance(offset: 1)
            .SetOperandAndAdvance(0.5f)
            .InstructionEnumeration();
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(Fireplace.Awake))]
      static void AwakePrefix(ref Fireplace __instance) {
        if (IsModEnabled.Value
            && Array.IndexOf(EligibleTorchItemNames, Utils.GetPrefabName(__instance.gameObject)) >= 0) {
          __instance.m_startFuel = TorchStartingFuel;
        }
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Fireplace.Awake))]
      static void AwakePostfix(ref Fireplace __instance) {
        if (!IsModEnabled.Value
            || !__instance.m_nview
            || !__instance.m_nview.IsValid()
            || !__instance.m_nview.IsOwner()
            || Array.IndexOf(EligibleTorchItemNames, __instance.m_nview.GetPrefabName()) < 0) {
          return;
        }

        __instance.m_startFuel = TorchStartingFuel;
        __instance.m_nview.GetZDO().Set(FuelHashCode, TorchStartingFuel);
      }
    }
  }
}
