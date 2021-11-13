﻿using BepInEx;

using HarmonyLib;

using System.Collections.Generic;
using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

namespace CriticalDice {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class CriticalDice : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.criticaldice";
    public const string PluginName = "CriticalDice";
    public const string PluginVersion = "1.0.0";

    static readonly int _sayHashCode = "Say".GetStableHashCode();
    static readonly Regex _htmlTagsRegex = new("<.*?>");

    static readonly System.Random _random = new();

    static MonoBehaviour _plugin;
    Harmony _harmony;

    public void Awake() {
      _plugin = this;
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(ZRoutedRpc))]
    class ZRoutedRpcPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(ZRoutedRpc.RPC_RoutedRPC))]
      static IEnumerable<CodeInstruction> RPC_RoutedRPCTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(
                    OpCodes.Callvirt,
                    AccessTools.Method(
                        typeof(ZRoutedRpc.RoutedRPCData), nameof(ZRoutedRpc.RoutedRPCData.Deserialize))))
            .Advance(offset: 1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
            .InsertAndAdvance(Transpilers.EmitDelegate<Action<ZRoutedRpc.RoutedRPCData>>(ProcessRoutedRpcData))
            .InstructionEnumeration();
      }
    }

    static void ProcessRoutedRpcData(ZRoutedRpc.RoutedRPCData routedRpcData) {
      if (routedRpcData.m_methodHash == _sayHashCode) {
        ProcessRpcSayData(routedRpcData);
      }
    }

    static void ProcessRpcSayData(ZRoutedRpc.RoutedRPCData routedRpcData) {
      routedRpcData.m_parameters.ReadInt();
      string playerName = _htmlTagsRegex.Replace(routedRpcData.m_parameters.ReadString(), string.Empty);
      string messageText = routedRpcData.m_parameters.ReadString();
      routedRpcData.m_parameters.SetPos(0);

      if (messageText.StartsWith("!roll")) {
        ProcessDiceRollRpcData(playerName, messageText, routedRpcData);
      }
    }

    static void ProcessDiceRollRpcData(string playerName, string messageText, ZRoutedRpc.RoutedRPCData routedRpcData) {
      if (!ParseDiceRoll(messageText, out long result)) {
        return;
      }

      MatchCollection matches = _diceRollRegex.Matches(messageText);

      if (matches.Count <= 0) {
        return;
      }

      ZRoutedRpc routedRpc = ZRoutedRpc.m_instance;
      routedRpc.m_rpcMsgID++;

      ZPackage parameters = new();
      parameters.Write((int) Talker.Type.Normal);
      parameters.Write("<color=#AEC6D3><b>Server</b></color>");
      parameters.Write($"{playerName} rolled... {result}");

      ZRoutedRpc.RoutedRPCData response = new() {
        m_msgID = routedRpc.m_id + routedRpc.m_rpcMsgID,
        m_senderPeerID = routedRpc.m_id,
        m_targetPeerID = ZRoutedRpc.Everybody,
        m_targetZDO = routedRpcData.m_targetZDO,
        m_methodHash = _sayHashCode,
        m_parameters = parameters,
      };

      _plugin.StartCoroutine(SendDiceRollResponse(response));
    }

    static readonly Regex _diceRollRegex =
        new(@"^!roll\s+(?:(?<simple>\d+)\s*$|(?<count>\d*)d(?<faces>\d+)\s*(?<modifier>[\+-]\d+)?.*$)");

    static bool ParseDiceRoll(string input, out long result) {
      result = 0;

      MatchCollection matches = _diceRollRegex.Matches(input);

      if (matches.Count <= 0) {
        return false;
      }

      Match match = matches[0];

      if (match.Groups["simple"].Length > 0) {
        if (!int.TryParse(match.Groups["simple"].Value, out int simple) || simple < 2) {
          return false;
        }

        result += _random.Next(simple) + 1;
        return true;
      }

      if (!int.TryParse(match.Groups["count"].Value, out int diceCount)) {
        diceCount = 1;
      }

      if (!int.TryParse(match.Groups["faces"].Value, out int diceFaces) || diceFaces < 2) {
        return false;
      }

      if (int.TryParse(match.Groups["modifier"].Value, out int modifier)) {
        result += modifier;
      }

      diceCount = Math.Min(diceCount, 20);
      diceFaces = Math.Min(diceFaces, 1000);

      for (int i = 0; i < diceCount; i++) {
        result += _random.Next(diceFaces) + 1;
      }

      return true;
    }

    static readonly WaitForSeconds _waitInterval = new(seconds: 0.25f);

    static IEnumerator SendDiceRollResponse(ZRoutedRpc.RoutedRPCData response) {
      // Rewrite this so that only those players in range will actually be routed the RPC. Let's us position the text
      // to be in front of the player at the cost of having to calculate all peers within range.
      // ZNetPeer does have m_refPos so we can just do a simple sqrMagnitude calculation (as usual).
      // Will need to modifiy the transpiler delegate to also include the ZRpc parameter for the client invoking it.
      yield return _waitInterval;
      ZRoutedRpc.m_instance.RouteRPC(response);
    }
  }
}