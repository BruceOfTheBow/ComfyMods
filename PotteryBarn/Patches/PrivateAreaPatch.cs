using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using static PotteryBarn.PotteryBarn;
using static PotteryBarn.Requirements;

namespace PotteryBarn {
  [HarmonyPatch(typeof(PrivateArea))]
  public class PrivateAreaPatch {
    static List<Piece> _pieceCache = new();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PrivateArea.Interact))]
    static void InteractPrefix(ref PrivateArea __instance) {
      _pieceCache = Piece.m_allPieces.Where(x => hammerCreatorShopItems.Keys.Contains(x.m_description) && !x.IsCreator()).ToList<Piece>();
      Piece.m_allPieces.RemoveAll(x => hammerCreatorShopItems.Keys.Contains(x.m_description) && !x.IsCreator());
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PrivateArea.Interact))]
    static void InteractPostfix(ref PrivateArea __instance) {
      Piece.m_allPieces.AddRange(_pieceCache);
      _pieceCache = new();
    }
  }
}
