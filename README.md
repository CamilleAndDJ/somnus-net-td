# Somnus Net — Dream Blobs vs Bug Swarm

A **Plants vs. Zombies–style** 2D lane tower defense set in the Somnus Net. Design choices follow the master context document at `C:\Users\Camille Lin\design-docs\MASTER-CONTEXT.md`.

## Theme

- **Dream Blobs** — soft, expressive defenders made of imagination (the “plants”).
- **Glitches** — corrupted entities from the Bug Swarm infecting the net (the “zombies”).
- **Ponders** — currency earned passively (+10 every 10s) and from defeating glitches.

## Level 01 roster

| Dream Blob | Verb | Cost | Role |
|------------|------|------|------|
| **Basic** | Shoot | 50 | Steady lane damage |
| **Dream** | Slow | 85 | Double pulse + slows glitches |
| **Mushroom** | Block | 35 | Soft wall (320 HP) |
| **Flame** | Explode | 100 | One-shot area burst |

| Glitch | Role |
|--------|------|
| **Glitch Mite** | Slow baseline |
| **Lag Beetle** | Fast, fragile |
| **Shell Glitch** | Armor layer |
| **Packet Swarm** | Fast, low HP, arrives in groups |

**Win:** survive all waves. **Lose:** any glitch reaches the **Dream Core** (left).

## Open in Unity

1. **Unity 6000.4.9f1** (or compatible 6.x).
2. Open project folder: `C:\Users\Camille Lin\Projects\somnus-net-td`
3. Menu: **Somnus Net → Build Level 01** (generates sprites, data, prefabs, and scene).
4. Open scene `Assets/SomnusNet/Scenes/Level01_SomnusBreach.unity` and press **Play**.

### Controls

- Click a blob button at the bottom, then click a grid cell to place.
- Survive all waves without letting glitches reach the **Dream Core** (left).

## Project layout

```
Assets/SomnusNet/
  Scripts/     Gameplay, units, UI
  Editor/      LevelOneSetup (menu + batch build)
  Data/        ScriptableObject defs (after build)
  Scenes/      Level 01 scene (after build)
```
