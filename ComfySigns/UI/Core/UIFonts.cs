﻿using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;

namespace ComfyLib {
  public static class UIFonts {
    public static Font AveriaSerifLibre { get => GetFont("AveriaSerifLibre-Regular"); }

    static readonly Dictionary<string, Font> _fontCache = new();

    public static Font GetFont(string fontName) {
      if (!_fontCache.TryGetValue(fontName, out Font font)) {
        font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(f => f.name == fontName);
        _fontCache[fontName] = font;
      }

      return font;
    }

    static readonly Dictionary<string, TMP_FontAsset> _fontAssetCache = new();

    public static TMP_FontAsset GetFontAsset(string fontName) {
      if (!_fontAssetCache.TryGetValue(fontName, out TMP_FontAsset fontAsset)) {
        fontAsset = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name == fontName);

        if (!fontAsset) {
          fontAsset = TMP_FontAsset.CreateFontAsset(GetFont(fontName));
        }

        _fontAssetCache[fontName] = fontAsset;
      }

      return fontAsset;
    }
  }
}
