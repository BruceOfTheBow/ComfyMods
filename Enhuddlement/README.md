# Enhuddlement

*EnemyHud enhancement and customization.*

![Splash](https://i.imgur.com/BpyBnON.png)

## Features

### HealthBar customization

  * Change the width/height/colors of the Healthbar
  * Display health values as text within the HealthBar

### Floating BossHud

  * Separate name and HealthBars follow each boss
  * Add a color gradient for the name

### Enemy level display

  * Display enemy (and boss) levels as stars (with a number)
  * Position next to name or below HealthBar

### Enemy aware/alert status display

  * Color enemy name for aware/alert status

### Show local PlayerHud

  * Display name and HealthBar over your own character

### Player PvP status indicator

  * Color player names when their PvP toggle is enabled

## Configuration

Changes to most settings will only occur when the EnemyHud is rebuilt (distance > 10m enemies, 100m bosses).

Use a [ConfigurationManager](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/) to
modify settings in-game.

![Configuration](https://imgur.com/sBDKAKf.png)

## Compatability

### BetterUI / BetterUI Reforged

  * Enhuddlment will un-patch BetterUI's EnemyHud patches to avoid mod conflicts.

## Installation

### Manual

  * Un-zip `Enhuddlement.dll` to your `/Valheim/BepInEx/plugins/` folder.

### Thunderstore (manual)

  * Go to Settings > Import local mod > Select `Enhuddlement_v1.0.0.zip`.
  * Click "Import local mod" in the pop-up for information.

## Notes

  * This is the *good enough* release with more features/options to be added later.
  * See source at: [GitHub/ComfyMods](https://github.com/redseiko/ComfyMods/tree/main/Enhuddlement).
  * Looking for a chill Valheim server? [Comfy Valheim Discord](https://discord.gg/ameHJz5PFk)
  * Check out our community driven listing site at: [valheimlist.org](https://valheimlist.org/)

## Changelog

### 1.1.0

  * Updated for the `v0.214.2` patch.
  * Updated BepInEx dependency to `denikson-BepInExPack_Valheim-5.4.2100`.
  * Removed `BossHud.Name.useGradientEffect` config option for now as it is not compatible with TextMeshPro.

### 1.0.0

  * Initial release.