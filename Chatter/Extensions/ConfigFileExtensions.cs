﻿using System;
using System.Collections.Generic;

using BepInEx.Configuration;

using HarmonyLib;

namespace ComfyLib {
  public static class ConfigFileExtensions {
    static readonly Dictionary<string, int> SectionToOrderCache = new();

    static int GetSettingOrder(string section) {
      if (!SectionToOrderCache.TryGetValue(section, out int order)) {
        order = 0;
      }

      SectionToOrderCache[section] = order - 1;
      return order;
    }

    public static ConfigEntry<T> BindInOrder<T>(
        this ConfigFile config,
        string section,
        string key,
        T defaultValue,
        string description,
        AcceptableValueBase acceptableValues) {
      return config.Bind(
          section,
          key,
          defaultValue,
          new ConfigDescription(
              description,
              acceptableValues,
              new ConfigurationManagerAttributes {
                Order = GetSettingOrder(section)
              }));
    }

    public static ConfigEntry<T> BindInOrder<T>(
        this ConfigFile config,
        string section,
        string key,
        T defaultValue,
        string description,
        System.Action<ConfigEntryBase> customDrawer = null,
        bool browsable = true,
        bool hideDefaultButton = false) {
      return config.Bind(
          section,
          key,
          defaultValue,
          new ConfigDescription(
              description,
              null,
              new ConfigurationManagerAttributes {
                Browsable = true,
                CustomDrawer = customDrawer,
                HideDefaultButton = hideDefaultButton,
                Order = GetSettingOrder(section)
              }));
    }

    public static StringListConfigEntry BindInOrder(
        this ConfigFile config, string section, string key, string description, string valuesSeparator) {
      return new StringListConfigEntry(config, section, key, description, valuesSeparator, GetSettingOrder(section));
    }

    internal sealed class ConfigurationManagerAttributes {
      public System.Action<ConfigEntryBase> CustomDrawer;
      public bool? Browsable;
      public bool? HideDefaultButton;
      public int? Order;
    }

    public static void LateBindInOrder(this ConfigFile config, Action<ConfigFile> bindFunc) {
      _lateBindEntries.Enqueue(() => bindFunc.Invoke(config));
    }

    static readonly Queue<Action> _lateBindEntries = new();

    [HarmonyPatch(typeof(FejdStartup))]
    static class FejdStartupPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(FejdStartup.Awake))]
      static void AwakePostfix() {
        while (_lateBindEntries.Count > 0) {
          _lateBindEntries.Dequeue()?.Invoke();
        }
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(FejdStartup.OnDestroy))]
      static void OnDestroyPostfix() {
        _lateBindEntries.Clear();
      }
    }
  }
}
