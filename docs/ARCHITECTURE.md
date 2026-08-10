# Architecture

This document maps the current runtime systems in the Dev vs Gamers Unity project.

## High-Level Runtime Flow

1. `DVGUsableWallet` initializes diamonds and healing potions.
2. `PlantPlacementManager` listens for mouse/keyboard input.
3. Character slot clicks select a `DVGPlacementCharacterSlot`.
4. Board clicks spend diamonds and instantiate the selected character prefab.
5. Spawned defenders receive/keep `DVGBoardCharacter`, which records cell and sorting state.
6. `DVGEnemyLaneSpawner` builds lane start/end points from the board and periodically spawns enemies.
7. Enemies implement `IDVGEnemyLaneWalker`, walk their assigned lane, attack defenders, and damage board life if they exit.
8. Combat and pickups update health, wallet resources, counters, and world health bars.

## System Map

```mermaid
flowchart TD
    Wallet["DVGUsableWallet"] --> Counters["DVGUsableCounterUI / Potion Counter"]
    Slot["DVGPlacementCharacterSlot"] --> Placement["PlantPlacementManager"]
    Placement --> Tilemap["Placement Tilemap"]
    Placement --> Wallet
    Placement --> Character["DVGBoardCharacter"]
    Character --> Health["DVGHealth"]
    Character --> HealthBar["DVGWorldHealthBar"]
    Spawner["DVGEnemyLaneSpawner"] --> Board["DVGBoard / DVGTilemapBoard"]
    Spawner --> Enemy["IDVGEnemyLaneWalker"]
    Enemy --> BoardLife["DVGBoardLife"]
    Enemy --> Character
    Shooter["DVGRowProjectileShooter"] --> Projectile["DVGDamageProjectile"]
    Projectile --> EnemyHealth["Enemy DVGHealth"]
    Miner["DVGMinerMiningReward"] --> Pickup["DVGBoardPickup"]
    PotionMaker["DVGWizardPotionReward"] --> Pickup
    Pickup --> Wallet
    PotionUse["DVGHealingPotionUseController"] --> Wallet
    PotionUse --> Health
```

## Board Layer

### `DVGBoard`

Prefab-board helper with explicit rows, columns, spacing, and `DVGTile` references. It can return world positions and place occupants into tile references.

### `DVGTile`

Stores row, column, and a current occupant reference for prefab-board placement.

### `DVGTilemapBoard`

Tilemap-board helper that finds its `Grid` and `Tilemap`, checks tile occupancy, and converts cells to centered world positions. This is useful for tilemap-authored boards.

### `DVGBoardCharacter`

Runtime metadata attached to placed defenders:

- stores the assigned cell
- exposes `HasCell`
- ensures/links `DVGHealth`
- sets max health on enable
- applies row-based `SortingGroup` ordering

### `DVGBoardLife`

Represents the chest/base health. Enemies call `TryDamageActiveBoardLife` when they leak through a lane.

## Placement Layer

### `PlantPlacementManager`

The main input coordinator for board-defense mode. Despite the name, it handles character placement.

Responsibilities:

- select character slots
- validate placement tilemap cells
- spend diamonds
- instantiate selected character prefabs
- maintain an occupied-cell dictionary
- create placement previews
- collect board pickups
- route healing potion clicks
- remove placed characters

Important defaults:

- right click clears selection
- `X` toggles remove mode
- `Left Shift` temporarily activates remove mode

### `DVGPlacementCharacterSlot`

Scene component for character shop slots. It stores:

- character prefab
- diamond cost
- optional cost text
- selection indicator
- selected scale multiplier

Slot cost is scene data. If a prefab gets more powerful, remember to update its slot cost in the scene.

## Economy And UI Layer

### `DVGUsableWallet`

Singleton-style runtime wallet for:

- diamonds
- healing potions

It exposes change events used by UI counters.

### `DVGUsableCounterUI`

Updates TextMeshPro counters from wallet events. It can display diamonds or healing potions depending on `CounterResource`.

### `DVGHealingPotionUseController`

Handles potion count display, potion-counter click detection, aiming mode, target highlighting, cursor ghost rendering, spending potions, and healing damaged board characters.

## Combat Layer

### `DVGHealth`

Shared health model. Other systems should use this instead of custom health when interacting with DVG board entities.

### `DVGRowProjectileShooter`

Lane-aware ranged attacker for placed characters.

- Searches all `IDVGEnemyLaneWalker` behaviours.
- Filters by same lane.
- Chooses the closest valid forward target.
- Fires pooled or fresh `DVGDamageProjectile` instances.

### `DVGDamageProjectile`

Projectile behavior for DVG combat. It moves in a direction, damages health targets, can apply stun source metadata, and can return to a pool.

### `DVGBoardMeleeAttacker`

Lane-aware melee attacker for placed defenders.

### `DVGHitRecoil`

Visual/gameplay hit response:

- recoil movement
- optional return after recoil
- stun trigger support
- random stun chance and resistance
- hit flash
- hit sounds through `DVGAnimationSoundPlayer`

## Enemy Layer

### `IDVGEnemyLaneWalker`

Interface required by lane enemies. The spawner depends on this instead of a concrete enemy class.

### `DVGEnemyLaneSpawner`

Spawns weighted enemy options into random board lanes. It supports both prefab-board and tilemap-board lane construction.

Difficulty ramping lerps from the starting interval/alive cap toward lower interval and higher alive cap over `timeToMaxDifficulty`.

### `DVGEnemyVikingWalker`

Current lane enemy implementation. It walks toward the end of a lane, attacks defenders in the same row, plays an after-kill pause, and damages board life on exit.

## Pickup Layer

### `DVGBoardPickup`

Clickable board pickup for diamonds or healing potions. It has a lifetime timer, collect sound hook, and visual collect effect.

### `DVGThrownPickup`

Animates pickups from a start point to a landing point along a small arc.

### `DVGMinerMiningReward`

Creates diamond pickups, usually from mining animation events.

### `DVGWizardPotionReward`

Creates healing potion pickups, usually from brewing/animation events.

## Legacy Platformer Layer

The repo still has earlier platformer scripts:

- `PlayerController`
- `PhysicsObject`
- `Enemy`
- `AttackBox`
- `Collectable`
- `Door`
- `Breakable`
- `SceneLoadTrigger`
- `PlayerStats`

These are not the core DVG board-defense architecture, but they may still be useful as reference or for older scenes.

## Naming Notes

- `PlantPlacementManager` should eventually be renamed to something like `DVGPlacementManager`.
- `CharPlacementManagement.cs` contains `PlantPlacementManager`, so file/class names do not currently match.
- `Recovered_PlayMode_20260705_1956.unity` appears to be a recovered scene but currently holds the active prototype.
