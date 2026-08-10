# Gameplay

This document describes the current Dev vs Gamers prototype as it exists in the Unity project right now.

## Core Fantasy

The player defends a treasure chest from waves of gamer/Viking attackers by placing developer-side characters on a lane board. The game currently plays like a compact lane-defense prototype: build a defense, keep the economy moving, patch up damaged units, and survive the increasing spawn pressure.

## Active Scene

The main working scene is:

`Assets/Scenes/Level 1.unity`

It includes the board, water/boat themed background, character UI, diamond counter, healing potion counter, board life/chest, and active `DVGEnemyLaneSpawner`. The old recovered scene is kept only as a backup.

## Player Resources

### Diamonds

Diamonds are the placement currency.

- Stored by `DVGUsableWallet`.
- `Level 1` currently starts with `4` diamonds.
- Character slots spend diamonds through `PlantPlacementManager`.
- Diamond pickups are collected by left clicking them.
- Miner characters can generate thrown diamond pickups.

### Healing Potions

Healing potions are a usable resource.

- Stored by `DVGUsableWallet`.
- `Level 1` currently starts with `0` potions.
- Potion pickups are collected by left clicking them.
- The potion-maker character can generate healing potion pickups.
- Clicking the potion counter enters potion aiming mode.
- A potion heals a damaged placed character by the configured `healAmount`, currently `100`.

## Controls

| Action | Input |
| --- | --- |
| Select a character slot | Left click a character icon/slot |
| Place selected character | Left click a valid board tile |
| Collect a pickup | Left click the pickup |
| Cancel current selection | Right click |
| Toggle remove tool | `X` |
| Remove placed character | Select remove tool, then left click character |
| Temporary remove mode | Hold `Left Shift`, then left click character |
| Start healing potion aiming | Left click healing potion counter/icon |
| Heal a damaged character | While aiming, left click the target |
| Cancel potion aiming | Right click or `Esc` |

## Board And Lanes

The current board-defense flow is built around a 6-lane, 20-column board:

- `DVGBoard` supports a prefab-board representation with row and column tiles.
- `DVGTilemapBoard` supports a tilemap-backed board and placement offset.
- `PlantPlacementManager` checks whether the clicked tile exists in the placement tilemap before allowing placement.
- Placed characters receive a `DVGBoardCharacter`, which stores their board cell and applies row-based sorting.
- Enemies use lane indices to decide movement, targeting, and projectile hits.

## Character Slots And Costs

`Level 1` contains five placement slots wired to these prefabs:

| Prefab | Current role | Observed scene cost |
| --- | --- | --- |
| `Miner_Character` | Generates diamond pickups over time or animation events | `2` |
| `Wizard_potion` | Generates healing potion pickups | `3` |
| `Archer_Character_1` | Shoots row projectiles | `5` |
| `Wizard_Character_9` | Ranged/magic attacker, depending on prefab components | `4` |
| `Cave_Man_Character_2` | Higher-health melee/blocking character | `8` |

Costs live on `DVGPlacementCharacterSlot` components in the scene, not on the character prefabs themselves.

## Combat

### Health

`DVGHealth` is the shared health component for DVG board entities. It supports:

- max/current health
- damage with recoil multiplier metadata
- healing
- death events
- optional destruction on death

`DVGWorldHealthBar` displays world-space health over damaged entities.

### Ranged Defenders

`DVGRowProjectileShooter`:

- requires `DVGBoardCharacter`
- scans for enemies in the same lane
- respects `sightRange` and `minimumFireDistance`
- uses a projectile pool by default
- can fire immediately or wait for an animation event
- applies lane-based sorting to projectiles

`DVGDamageProjectile`:

- moves in a launch direction
- can check hits by trigger or by lane-based overlap logic
- damages `DVGHealth`
- can return to a pool instead of being destroyed

### Melee Defenders

`DVGBoardMeleeAttacker`:

- looks forward along a lane
- attacks enemies within range
- deals damage from animation event methods or direct method calls
- can apply recoil/stun through `DVGHitRecoil`

### Enemies

`DVGEnemyLaneSpawner`:

- builds lanes from `DVGBoard` or `DVGTilemapBoard`
- waits `initialDelay`
- spawns enemies at `spawnInterval`
- caps alive enemies
- optionally ramps difficulty over time
- chooses enemy options by weight

The active `Level 1` spawner currently uses:

- `initialDelay`: `22`
- `spawnInterval`: `11`
- `maxAliveEnemies`: `4`
- `timeToMaxDifficulty`: `330`
- `minimumSpawnInterval`: `3.5`
- `maxAliveEnemiesAtFullDifficulty`: `14`
- Viking walker option: about `140` health and `0.8` speed in the scene

`DVGEnemyVikingWalker`:

- walks from the spawn edge toward the chest edge
- finds a placed character in the same lane
- attacks the character until it dies or the enemy is interrupted
- damages board life if it reaches the end
- destroys itself after leaking through

## Loss State

`DVGBoardLife` represents the chest/base life.

- Current starting life is `5`.
- A leaked enemy deals `boardDamageOnExit`, usually `1`.
- When board life reaches zero, the game creates/shows `GAME OVER`.
- The current setup pauses time on game over.

There is no explicit victory state yet. Survival duration and tuning are the current progression target.

## Current Rough Edges

- The scene being used is named like a play-mode recovery scene, but it is currently the richest playable scene.
- Some older platformer scripts and prefabs still live next to the DVG board-defense systems.
- Some gameplay state is scene-authored rather than prefab-authored, especially slot costs and active spawner tuning.
- The placement manager class is named `PlantPlacementManager` even though this is no longer a plant game.
