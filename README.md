# Advanced Accessories Menu

A plugin that sorts accessory slots in the character maker into named, collapsible groups.

Supported games: Honey Select 2, Koikatsu Sunshine, Koikatsu.
Also built for AI-Shoujo, but not tested there yet.

| Game's slot list | | Accessories menu |
|---|:---:|---|
| ![Koikatsu Sunshine slot list](AdvancedAccessoriesMenu/screenshots/2_KKS_mess.png) | ➜ | ![Koikatsu Sunshine Accessories menu](AdvancedAccessoriesMenu/screenshots/2_KKS_neat.png) |

## Features

- Create groups for accessory slots.
- Toggle between advanced menu and old style.
- Empty slots can be hidden.
- KKS / KK outfits have their own groups.

## Requirements

- [BepInEx](https://github.com/BepInEx/BepInEx) 5.4
- [HS2API / KKSAPI / AIAPI](https://github.com/IllusionMods/IllusionModdingAPI) 1.48 or newer, KKAPI 1.42.2 or newer

## Installation

Copy the file for your game into `BepInEx\plugins`:

- Honey Select 2: `HS2_AdvancedAccessoriesMenu.dll`
- Koikatsu Sunshine: `KKS_AdvancedAccessoriesMenu.dll`
- AI-Shoujo (not tested): `AI_AdvancedAccessoriesMenu.dll`
- Koikatsu: `KK_AdvancedAccessoriesMenu.dll`

## Usage

1. Press **Advanced** button to toggle between new menu and old.
2. Create groups by giving them a name and set range.

## Compatibility

| Plugin | HS2 / AI | KKS / KK |
|---|---|---|
| MoreAccessories | [1.2.2+](https://bitbucket.org/Joan6694/hsplugins) | [2.0.21+](https://github.com/jalil49/MoreAccessories) |
| SliderHighlight (accessory highlight on hover) | — | 2.2+ |

## Changelog

- 0.1.0 — first version.

## License

GPL-3.0, see [LICENSE](LICENSE).
