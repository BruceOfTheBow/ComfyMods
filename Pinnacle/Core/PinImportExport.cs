﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;

namespace Pinnacle {
  public static class PinImportExport {
    public enum PinFileFormat {
      Binary,
      PlainText
    }

    static void LogInfo(string message) {
      Chat.m_instance.Ref()?.AddString(message);
      ZLog.Log(message);
    }

    public static void ExportPinsToFile(Terminal.ConsoleEventArgs args, PinFileFormat exportFormat) {
      if (!Minimap.m_instance) {
        return;
      }

      string filename =
          string.Format(
              "Pinnacle/{0}{1}",
              args.Length >= 2 ? args[1] : DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
              exportFormat switch {
                PinFileFormat.Binary => ".pins",
                PinFileFormat.PlainText => ".pins.txt",
                _ => string.Empty,
              });

      Directory.CreateDirectory(Path.GetDirectoryName(filename));

      IReadOnlyCollection<Minimap.PinData> pinsToExport =
          FilterPins(Minimap.m_instance.m_pins, args.Length >= 3 ? args[2] : string.Empty);

      LogInfo($"Exporting {pinsToExport.Count} pins to file: {filename}");

      int pinsExported =
          exportFormat switch {
            PinFileFormat.Binary => ExportPinsToBinaryFile(pinsToExport, filename),
            PinFileFormat.PlainText => ExportPinsToTextFile(pinsToExport, filename),
            _ => 0
          };

      LogInfo($"Exported {pinsExported} pins to file: {filename} ");
    }

    static int ExportPinsToBinaryFile(IReadOnlyCollection<Minimap.PinData> pins, string filename) {
      using FileStream stream = new(filename, FileMode.CreateNew);
      using BinaryWriter writer = new(stream);

      int pinsExported = 0;
      writer.Write(pins.Count);

      foreach (Minimap.PinData pin in pins) {
        if (pin.m_save) {
          writer.Write(pin.m_name);
          writer.Write(pin.m_pos.x);
          writer.Write(pin.m_pos.y);
          writer.Write(pin.m_pos.z);
          writer.Write((int) pin.m_type);
          writer.Write(pin.m_checked);
          writer.Write(pin.m_ownerID);

          pinsExported++;
        }
      }

      return pinsExported;
    }

    static int ExportPinsToTextFile(IReadOnlyCollection<Minimap.PinData> pins, string filename) {
      using StreamWriter writer = File.CreateText(filename);
      int pinsExported = 0;

      foreach (Minimap.PinData pin in pins) {
        if (pin.m_save) {
          writer.Write($"\"{pin.m_name}\",");
          writer.Write($"{pin.m_pos.x},{pin.m_pos.y},{pin.m_pos.z},");
          writer.Write($"{pin.m_type},{pin.m_checked},{pin.m_ownerID}");
          writer.WriteLine();

          pinsExported++;
        }
      }

      return pinsExported;
    }

    public static void ImportPinsFromBinaryFile(Terminal.ConsoleEventArgs args) {
      if (!Minimap.m_instance || args.Length < 2) {
        return;
      }

      string filename = $"Pinnacle/{args[1]}";

      if (!File.Exists(filename)) {
        args.Context.AddString($"Could not find file: {filename}");
        return;
      }

      Dictionary<Minimap.PinType, Sprite> pinTypeToSprite =
          Minimap.m_instance.m_icons.ToDictionary(data => data.m_name, data => data.m_icon);

      using FileStream stream = new(filename, FileMode.Open);
      using BinaryReader reader = new(stream);

      int count = reader.ReadInt32();
      List<Minimap.PinData> pins = new(capacity: count);

      LogInfo($"Reading {count} pins from file: {filename}");

      for (int i = 0; i < count; i++) {
        Minimap.PinData pin = new();
        pin.m_name = reader.ReadString();

        pin.m_pos = new() {
          x = reader.ReadSingle(),
          y = reader.ReadSingle(),
          z = reader.ReadSingle()
        };

        pin.m_type = (Minimap.PinType) reader.ReadInt32();
        pinTypeToSprite.TryGetValue(pin.m_type, out pin.m_icon);
        pin.m_checked = reader.ReadBoolean();
        pin.m_ownerID = reader.ReadInt64();
        pin.m_save = true;

        pins.Add(pin);
      }

      if (args.Length >= 3) {
        pins = FilterPins(pins, args[2]);
      }

      LogInfo($"Imported {pins.Count} pins from file: {filename}");

      Minimap.m_instance.m_pins.AddRange(pins);
    }

    static List<Minimap.PinData> FilterPins(IReadOnlyCollection<Minimap.PinData> pins, string nameRegexPattern) {
      if (nameRegexPattern.Length > 0) {
        LogInfo($"Filtering {pins.Count} pins by pin.name with regex: {nameRegexPattern}");
        Regex regex = new(nameRegexPattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

        return pins.Where(pin => pin.m_save && regex.Match(pin.m_name).Success).ToList();
      } else {
        return pins.Where(pin => pin.m_save).ToList();
      }
    }
  }
}
