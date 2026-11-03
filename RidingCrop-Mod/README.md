# Riding Crop

Adds a family of **seven craftable one-handed riding crops** to Valheim, from the plain leather crop up to the finest crop of
the family. Every hit deals blunt damage **and makes the target bleed**. Each tier hits harder and lasts longer than the one
before. The crops use the vanilla Club's attack animations, have no secondary attack, and train their own skill, **RidingCrop**.

Author: **kydldev**

## Requirements

- BepInEx, [Jotunn](https://valheim.thunderstore.io/package/ValheimModding/Jotunn/) and [KydlCore](https://valheim.thunderstore.io/package/KydlDev/KydlCore/) 0.1.0.
- Every player on a server needs the mod installed, at the **same version** (see Multiplayer).

## Features

The mod has two functionalities. Each has its own switch, and `[General] EnableAll` switches both.

- **Crops**: the seven riding crops and the RidingCrop skill.
- **Sound**: the crops' own swing (whoosh) and hit (slap) sounds.

## Usage

Craft a crop at the workbench, forge or black forge (see the tables below), equip it like a club and swing. The skill trains while you
hit things. Press **F6** to open the KY hub (KydlCore) and pick **Riding Crop** to change the settings with live checking.

The items, the skill and the sounds are added to the game when it starts, so every setting marked "needs a restart" below takes effect
after the next game start. The detailed log switch works at any time.

## Configuration

Config file: `BepInEx/config/com.kydldev.RidingCrop.cfg` (created on first start). All settings are also on the **Settings** tab of the
Riding Crop page in the KY hub (F6).

| Section | Setting | Default | Description |
|---------|---------|---------|-------------|
| General | `EnableAll` | `true` | Switches the whole mod on or off. Needs a game restart. While it is off, Riding Crop items are unknown to the game, so keep it on for any world or character that uses them, and use the same value as the server. |
| Crops | `Enable` | `true` | Adds the seven crops and their RidingCrop skill. Needs a restart. Use the same value as the server. |
| Sound | `Enable` | `true` | The crops use their own swing and hit sounds. Off keeps the Club's sounds. Needs a restart. |
| Sound | `WhooshVolume` | `1` | Volume of the swing sound (0 to 1). Needs a restart. |
| Sound | `SlapVolume` | `1` | Volume of the hit sound (0 to 1). Needs a restart. |
| Sound | `ReplaceVanillaSounds` | `true` | `true` = the crops only make their own sounds. `false` = the Club's sounds play as well. Needs a restart. |
| Sound | `SlapNormalize` | `true` | `true` = the quiet build-up (about 47 ms) is cut from the start of the hit sound, so the impact is heard the moment you hit. `false` = use the sound as it is in the asset bundle. Needs a restart. |
| DebugLog | `Enable` | `false` | Writes extra detail to the BepInEx log: asset bundle contents, every crop's stats, recipes and stations, sound setup, and every whoosh, slap and bleed event. Can be changed while the game runs. |

All item stats are set by the mod and are not configurable, so everyone on a server has identical values.

### Coming from 1.0.0

The 1.0.0 keys were renamed. On the first start of 2.0.0 the old values are carried over: `[General] Enabled` becomes `[General] EnableAll`
and `[General] DebugLogging` becomes `[DebugLog] Enable`. The Sound settings kept their names.

## Multiplayer

The crops are items that exist in the world data, so every player and the server must run the same version of the mod. The mod uses
the network check `EveryoneMustHaveMod` with an exact version match (major.minor.patch): a client with another version is refused.
Keep `[General] EnableAll` and `[Crops] Enable` the same on the server and all clients. The sound settings only affect your own game.
## Crops and recipes (weakest to strongest)

| # | Item | Item id | Ingredients |
|---|------|---------|-------------|
| 1 | Do I boar you? | `RidingCrop` | 10 Leather Scraps, 2 Resin, 5 Wood |
| 2 | Dear, oh deer! | `RidingCrop_V1` | 10 Deer Hide, 2 Bronze, 5 Fine Wood |
| 3 | Bear with me! | `RidingCrop_V3` | 10 Bear Hide, 2 Surtling Core, 5 Core Wood |
| 4 | Howl yeah! | `RidingCrop_V4` | 10 Wolf Pelt, 2 Obsidian, 5 Fine Wood |
| 5 | Loxy Poxy | `RidingCrop_V6` | 10 Lox Pelt, 2 Tar, 5 Ancient Bark |
| 6 | Askvin worthy | `RidingCrop_V2` | 10 Askvin Hide, 2 Charred Bone, 5 Ashwood |
| 7 | Seal the deal! | `RidingCrop_V5` | 10 Moose Hide, 2 Seal Pelt, 5 Timberwood |

Item ids also work with the console `spawn` command (devcommands).

## Crafting stations

Each upgrade needs one station level more than the previous quality (same station).

| # | Item | Q1 | Q2 | Q3 | Q4 |
|---|------|----|----|----|----|
| 1 | Do I boar you? | Workbench 1 | Workbench 2 | Workbench 3 | Workbench 4 |
| 2 | Dear, oh deer! | Workbench 2 | Workbench 3 | Workbench 4 | Workbench 5 |
| 3 | Bear with me! | Forge 1 | Forge 2 | Forge 3 | Forge 4 |
| 4 | Howl yeah! | Forge 2 | Forge 3 | Forge 4 | Forge 5 |
| 5 | Loxy Poxy | Forge 3 | Forge 4 | Forge 5 | Forge 6 |
| 6 | Askvin worthy | Black Forge 1 | Black Forge 2 | Black Forge 3 | Black Forge 4 |
| 7 | Seal the deal! | Black Forge 2 | Black Forge 3 | Black Forge 4 | Black Forge 5 |

## Stats

Blunt damage grows by 40% per tier and the bleed by 25% per tier, each on top of the previous tier. The bleed deals its damage
once per second for 5 seconds and ignores armor and resistances. A target that is already bleeding cannot be made to bleed again
(not by the same crop and not by another one) until the running bleed has finished. Values are at skill level 0, quality 1.

| # | Item | Blunt | Bleed per second | Bleed in total (5 s) | Durability |
|---|------|------:|-----------------:|---------------------:|-----------:|
| 1 | Do I boar you? | 20 | 10 | 50 | 200 |
| 2 | Dear, oh deer! | 28 | 12.5 | 62.5 | 250 |
| 3 | Bear with me! | 39.2 | 15.6 | 78 | 300 |
| 4 | Howl yeah! | 54.9 | 19.5 | 98 | 350 |
| 5 | Loxy Poxy | 76.8 | 24.4 | 122 | 400 |
| 6 | Askvin worthy | 107.6 | 30.5 | 153 | 450 |
| 7 | Seal the deal! | 150.6 | 38.1 | 191 | 500 |

### Combat stats

No secondary attack. The last hit of the attack chain deals 2x damage. Block armor and parry block armor are shown at
skill 0 / 100. Parry block armor is block armor x 2 (parry bonus 2x).

| # | Item | Stagger | Knockback | Stamina | Block armor | Parry block armor | Block force |
|---|------|--------:|----------:|--------:|------------:|------------------:|------------:|
| 1 | Do I boar you? | 12 | 30 | 6 | 3 / 4.5 | 6 / 9 | 20 |
| 2 | Dear, oh deer! | 25 | 60 | 8 | 12 / 18 | 24 / 36 | 30 |
| 3 | Bear with me! | 35 | 80 | 10 | 21 / 31.5 | 42 / 63 | 30 |
| 4 | Howl yeah! | 55 | 100 | 12 | 30 / 45 | 60 / 90 | 30 |
| 5 | Loxy Poxy | 95 | 100 | 14 | 39 / 58.5 | 78 / 117 | 30 |
| 6 | Askvin worthy | 125 | 100 | 16 | 57 / 85.5 | 114 / 171 | 30 |
| 7 | Seal the deal! | 150 | 100 | 18 | 66 / 99 | 132 / 198 | 30 |

## Upgrading

Every crop can be upgraded at its crafting station (see above) up to **quality 4** (three upgrades). Each upgrade costs half of
each recipe amount (rounded up).

Every upgrade adds a flat **+6 blunt damage** and a flat **+40 durability** on every crop. The bleed does not change with upgrades.

| # | Item | Blunt Q1 | Q2 | Q3 | Q4 | Durability Q1 | Q2 | Q3 | Q4 |
|---|------|---------:|---:|---:|---:|--------------:|---:|---:|---:|
| 1 | Do I boar you? | 20 | 26 | 32 | 38 | 200 | 240 | 280 | 320 |
| 2 | Dear, oh deer! | 28 | 34 | 40 | 46 | 250 | 290 | 330 | 370 |
| 3 | Bear with me! | 39.2 | 45.2 | 51.2 | 57.2 | 300 | 340 | 380 | 420 |
| 4 | Howl yeah! | 54.9 | 60.9 | 66.9 | 72.9 | 350 | 390 | 430 | 470 |
| 5 | Loxy Poxy | 76.8 | 82.8 | 88.8 | 94.8 | 400 | 440 | 480 | 520 |
| 6 | Askvin worthy | 107.6 | 113.6 | 119.6 | 125.6 | 450 | 490 | 530 | 570 |
| 7 | Seal the deal! | 150.6 | 156.6 | 162.6 | 168.6 | 500 | 540 | 580 | 620 |

(Blunt damage at skill level 0.)

## Skill

All crops train the **RidingCrop** skill (shown in the Skills window). Dev-command test:

```
devcommands
raiseskill ridingcrop 50
```

## Sounds

- The slap sound starts immediately and is not affected by distance, so it is not drowned out by loud sounds such as a tree breaking.
- The swing (whoosh) and slap sounds are boosted by 50% on the sound data itself, because Unity cannot play a sound above full
  volume; peaks that would go past full scale are softly limited instead of clipping.
- A swing that hits several targets plays the slap sound only once.

## Troubleshooting

Set `[DebugLog] Enable` to `true` (it can be changed while the game runs) and check the BepInEx log
(`BepInEx/LogOutput.log`). It then lists the asset bundle contents, each crop's stats, recipe and station, the sound setup, and
every whoosh, slap and bleed event.
