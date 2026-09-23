# Somnus Net — Dream Blobs vs Bug Swarm

A **2D grid-based tower defense** game built in Unity. Place **Dream Blobs** on the grid to stop **Glitches** from reaching the **Dream Core** on the left. Design follows a verb-first, readable-feedback philosophy: each defender has one clear job, and the player learns through play.

## Core loop

- **Dream Blobs** — imaginative defenders you place on open grid tiles.
- **Glitches** — corrupted entities from the Bug Swarm marching along fixed paths.
- **Ponders** — currency earned passively and from defeating glitches, spent to place blobs.
- **Win** — defeat all glitches in the round(s) without letting any reach the Dream Core.
- **Lose** — a glitch touches the Dream Core.

## Game modes

| Mode | Scene | Description |
|------|-------|-------------|
| **Main Menu** | `MainMenu` | Entry point — Story Mode 1 or All Features |
| **Story Mode 1** | `StoryMode1` | Tutorial story on the **Stargazer** continent. 2 rounds, Gatekeeper only, Stargazer dialog sequences |
| **Story Mode 2** | `StoryMode2` | **Phantasia / Dream Isles** — 4 rounds, Gatekeeper / Creator / 404 / Blaze, **Outer Ring** layout, no dialogs |
| **World Map** | `WorldMap` | Continent overview — unlocks after Story Mode 1, routes to Story Mode 2 |
| **All Features** | `AllFeaturesLevel` | Sandbox with full blob roster and layout picker |

## Level layouts (All Features)

| Layout | Description |
|--------|-------------|
| **Begining** | Default swerving path |
| **Double Trouble** | Dual spawn paths (top & bottom), water tiles in the center |
| **Outer Ring** | Rectangular loop — glitches circle the ring before reaching the core |
| **Notebook Test** | Pre-start notebook UI showing the full shop roster before the level begins |
| **Layout Maker** | Paint custom primary/secondary paths on the grid |

## Dream Blob roster

| Blob | Role |
|------|------|
| **Gatekeeper** | Steady single-target damage; upgrade path: Crescent Trap (slow) |
| **Creator** | Double pulse + slow |
| **404** | — |
| **Blaze** | Explosive burst |
| **Dealer** | Split-shot support |
| **Cheerful** | Support / healing |
| **Archivist** | Resume / archival effects |
| **Countdown** | Landmine placement |
| **Lantern** | Pulse / area effects |

Story modes use a limited subset; **All Features** unlocks the full shop (paginated, 4 blobs per page).

## Glitch roster

Baseline enemies include **Glitch Mite**, **Lag Beetle**, **Shell Glitch**, and **Packet Swarm**. Mini-bosses and special glitches include **Cicadian Rhythm**, **Faze**, **Overload**, and **Sink**.

## Getting started

### Requirements

- **Unity 6000.4.9f1** (or compatible Unity 6.x)

### Open the project

1. Clone the repo and open the folder in Unity Hub.
2. Open **`Assets/SomnusNet/Scenes/MainMenu.unity`** and press **Play**.

Alternatively, use the editor menu **Somnus Net → Build Level 01** to regenerate sprites, data, prefabs, and scenes if assets are missing.

### Controls

- Click a blob in the **shop bar** at the bottom, then click an open grid cell to place it.
- **Pause** — top-right `||` button (also accessible during Notebook Test pre-start).
- **World Map** — click continent buttons to zoom in, then click the level marker to enter.

## Project structure

```
Assets/SomnusNet/
  Scenes/       MainMenu, StoryMode1/2, WorldMap, AllFeaturesLevel
  Scripts/
    Core/       GameManager, grid, layouts, economy, story rules
    Gameplay/   Placement, wave spawning
    Units/      DreamBlob, Glitch, Projectile
    UI/         HUD, shop, dialogs, world map, pause menu
    Visual/     Grid renderer, effects
    Data/       ScriptableObject definitions
  Data/Level01/ Blob & glitch assets, wave tables
  Resources/    Sprites, map art, dialog portraits
  Editor/       LevelOneSetup (batch build menu)
```

## Updating GitHub after local changes

Unity edits stay local until you push:

```powershell
git add .
git commit -m "Describe your changes"
git push
```

## License

Private repository — all rights reserved unless otherwise specified by the owner.
