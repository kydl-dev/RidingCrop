# Changelog

## 2.0.0
- Ported to the KydlDev layout (KydlCore feature switches, new source layout, TestLab suites). No gameplay change. Not tested in game yet.
- Network check is now `EveryoneMustHaveMod` with exact version match (Patch level; it was Minor).
- Breaking: RidingCrop now needs the KydlCore mod (hard dependency). Install KydlCore as well.
- The settings are a tab in the KY hub window of KydlCore (F6).
- Config changes: `[General] Enabled` is now `[General] EnableAll` and `[General] DebugLogging` is now `[DebugLog] Enable` (your old values are carried over on the first start). New switches `[Crops] Enable` and `[Sound] Enable`. The Sound volume settings are unchanged.
- The items, the skill and the sounds are still added when the game starts: restart the game after changing a switch.

## 1.0.0

Initial release.

- Seven craftable one-handed Riding Crops, from the plain leather crop to the finest of the family, using the Club's attack animations.
- Every hit deals blunt damage and makes the target bleed (5 seconds, ignores armor; bleeds do not stack).
- Per-crop damage, bleed, durability, stagger, knockback, stamina, block armor, parry and block force; no secondary attack; last hit of the chain deals 2x damage.
- Upgradeable to quality 4 at the crafting station of each crop (Workbench, Forge or Black Forge, one station level higher per upgrade).
- Own skill: RidingCrop.
- Own swing (whoosh) and hit (slap) sounds.
- Inventory icons for every crop.
- Configuration: `General > Enabled` (master switch), `General > DebugLogging` (extra detail in the BepInEx log), and sound options under `Sound`.