# Dev vs Gamers

Dev vs Gamers is a Unity 2D board-defense prototype. The current build is centered on a 6-row lane board where the player spends diamonds to place defenders, collects resource pickups, uses healing potions, and tries to stop Viking walkers before they reach the chest.

The repo also still contains some earlier platformer-style scripts and prefabs. Those are kept in the project, but the active gameplay direction is the DVG board/lane-defense flow.

## Current Playable State

- Main working scene: `Assets/Scenes/Level 1.unity`
- Backup recovered scene: `Assets/Scenes/Recovered_PlayMode_20260705_1956.unity`
- Unity version: `6000.5.0f1`
- Render stack: Unity 2D feature set with URP `17.5.0`
- Core board: 6 lanes by 20 columns, represented by `DVG Board 20x6.prefab` and a tilemap-backed placement area
- Enemy pressure: `DVGEnemyLaneSpawner` sends Viking walkers down random lanes and ramps spawn intensity over time
- Player economy: diamonds buy placed characters; miners can generate more diamonds; potion makers can generate healing potions

## Controls

| Action | Input |
| --- | --- |
| Select a character slot | Left click a character icon/slot |
| Place selected character | Left click a valid board tile |
| Collect diamond or potion pickups | Left click the pickup |
| Cancel selected character or potion aiming | Right click |
| Toggle remove tool | `X` |
| Temporary remove mode | Hold `Left Shift` and left click a placed character |
| Start healing potion aiming | Left click the healing potion counter/icon |
| Use healing potion | While aiming, left click a damaged placed character |
| Cancel healing potion aiming | Right click or `Esc` |

## Game Loop

1. Start with a small diamond budget.
2. Buy and place defenders on valid board cells.
3. Enemies spawn from the lane edge and walk toward the chest.
4. Defenders shoot, attack, mine, or brew depending on their prefab setup.
5. Collect dropped/generated pickups before they expire.
6. Heal damaged defenders with brewed potions.
7. If enemies reach the chest enough times, board life reaches zero and the game pauses on `GAME OVER`.

## Important Folders

| Path | Purpose |
| --- | --- |
| `Assets/_Scripts/Board` | Board tiles, placed character metadata, board life, and tilemap board helpers |
| `Assets/_Scripts/Combat` | Health, melee attacks, row shooters, and projectile damage |
| `Assets/_Scripts/Enemies` | Lane enemy interface, Viking walker behavior, and lane spawning |
| `Assets/_Scripts/Pickups` | Board pickups, thrown pickup arcs, mining rewards, and potion rewards |
| `Assets/_Scripts/Placement` | Character slot selection, placement preview, spending, removal, and click handling |
| `Assets/_Scripts/UI` | Diamond/potion wallet, counters, healing potion targeting, and world health bars |
| `Assets/Prefabs/Placement Characters` | Current placeable character prefabs |
| `Assets/Prefabs/Vikings` | Current DVG lane enemy prefab |
| `Assets/Prefabs/Projectiles` | Arrow and fireball projectile prefabs |
| `Assets/Art`, `Assets/Vikings`, `Assets/Potions`, `Assets/Gems and gold`, `Assets/Sounds` | Imported art/audio packs and generated content |

## Documentation

- `docs/GAMEPLAY.md` explains the current mechanics, controls, characters, enemy wave tuning, and win/loss state.
- `docs/ARCHITECTURE.md` maps the major runtime systems and how they talk to each other.
- `docs/DEVELOPMENT.md` covers common development workflows such as adding a placeable character, adding an enemy, and tuning difficulty.
- `docs/TODO.md` tracks upcoming gameplay, enemy, UI, art, and audio work.

## Running Locally

1. Install Unity `6000.5.0f1` or a compatible Unity 6 editor.
2. Open this folder as a Unity project.
3. Let Unity restore packages from `Packages/manifest.json`.
4. Open `Assets/Scenes/Level 1.unity`.
5. Press Play.

## Current Notes

- The scene has local tuning that differs from some prefab defaults. For example, the active enemy spawner in `Level 1` uses a Viking walker option with higher health than the prefab baseline.
- `PlantPlacementManager` is the main placement class, but its file is named `CharPlacementManagement.cs`.
- There are pre-board-defense platformer scripts such as `PlayerController`, `Enemy`, `Door`, `Collectable`, and `Breakable`. Treat them as legacy/supporting material unless a scene explicitly uses them.
- Some imported asset folders are large and include unused or backup content. Prefer editing runtime prefabs/scenes in `Assets/Prefabs`, `Assets/Scenes`, and `Assets/_Scripts`.
