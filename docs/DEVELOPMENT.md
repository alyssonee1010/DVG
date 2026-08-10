# Development Guide

This guide covers common changes in the current Vikings vs Everyone prototype.

## Project Setup

Use Unity `6000.5.0f1` when possible. Package versions are locked through `Packages/manifest.json` and `Packages/packages-lock.json`.

Recommended local flow:

1. Open the repo folder in Unity.
2. Let Unity import packages and assets.
3. Open `Assets/Scenes/Level 1.unity`.
4. Press Play and test the board-defense flow.

## Main Scene To Edit

For current gameplay work, edit:

`Assets/Scenes/Level 1.unity`

The recovered play-mode scene is kept as a backup, but current gameplay work should happen in `Level 1`.

## Adding A Placeable Character

1. Create or duplicate a prefab under `Assets/Prefabs/Placement Characters`.
2. Add `VVEBoardCharacter`.
3. Add `VVEHealth` or let `VVEBoardCharacter` require/find it.
4. Add one or more role components:
   - `VVERowProjectileShooter` for ranged lane attacks.
   - `VVEBoardMeleeAttacker` for melee lane attacks.
   - `VVEMinerMiningReward` for diamond generation.
   - `VVEWizardPotionReward` for healing potion generation.
5. Add an `Animator` if attacks/rewards depend on animation events.
6. Add colliders so clicks, targeting, and enemy attack ranges can find the character.
7. Add or update a `VVEPlacementCharacterSlot` in the scene.
8. Assign the prefab and set its diamond cost on the slot.
9. Test placement, row sorting, enemy targeting, damage, and removal.

## Adding A Ranged Character

Use `VVERowProjectileShooter`.

Checklist:

- Assign `projectilePrefab`.
- Assign `firePoint` or tune `firePointOffset`.
- Set `projectileDirection`, usually `(1, 0)` for shooting right.
- Set `projectileDamage`; this overrides the damage value on each projectile instance fired by this shooter.
- Tune `fireInterval`, `sightRange`, and `minimumFireDistance`.
- If animation should control the shot timing, enable `waitForShootAnimationEvent` and call `ShootProjectileAnimationEvent` from the attack animation.
- Confirm the projectile prefab has or receives `VVEDamageProjectile`.

## Adding A Melee Character

Use `VVEBoardMeleeAttacker`.

Checklist:

- Tune `attackRange`, `attackCooldown`, and `attackDamage`.
- Set `attackDirection` to match the side enemies approach from.
- Add/verify attack animation events if damage should land on a specific frame.
- Test against a Viking walker in the same lane.

## Adding A Resource Character

For diamonds, use `VVEMinerMiningReward`.

For healing potions, use `VVEWizardPotionReward`.

Checklist:

- Assign reward sprites.
- Tune event count per pickup.
- Tune start offset, landing offset, arc height, and collider radius.
- Confirm spawned pickups include `VVEBoardPickup`.
- Confirm the pickup resource is correct: `Diamonds` or `HealingPotions`.
- Test that pickups can be clicked and expire after their lifetime.

## Adding An Enemy

1. Create a prefab under `Assets/Prefabs/Vikings` or another enemy folder.
2. Add `VVEHealth`.
3. Add a script that implements `IVVEEnemyLaneWalker`, or reuse `VVEEnemyVikingWalker`.
4. Add a collider for projectile/melee targeting.
5. Add an animator with compatible triggers if using the existing walker behavior:
   - `Attack`
   - `AfterKill`
6. Add the enemy prefab to the `VVEEnemyLaneSpawner` enemy list.
7. Set max health, move speed, and weight on the spawner option.
8. Test that the enemy spawns, walks, attacks placed characters, dies cleanly, and damages board life on exit.

## Tuning Enemy Waves

Wave tuning lives on `VVEEnemyLaneSpawner`.

Important fields:

| Field | Meaning |
| --- | --- |
| `initialDelay` | Time before the first regular spawn |
| `spawnInterval` | Starting interval between spawn attempts |
| `maxAliveEnemies` | Starting alive enemy cap |
| `rampDifficultyOverTime` | Enables the time-based ramp |
| `timeToMaxDifficulty` | Time until the ramp reaches full strength |
| `minimumSpawnInterval` | Fastest interval after ramping |
| `maxAliveEnemiesAtFullDifficulty` | Alive enemy cap at full ramp |
| `difficultyCurveExponent` | Ramp shape; lower values start ramping sooner, higher values keep the early game calmer |
| enemy option `weight` | Relative spawn chance for that option |
| enemy option `maxHealth` | Health assigned when spawned |
| enemy option `moveSpeed` | Lane movement speed assigned when spawned |

`Level 1` currently ramps from an 8 second interval and 8 alive enemies toward a 2 second interval and 21 alive enemies over about 180 seconds.

To make the game harder faster without changing the opening delay, start with these two fields:

| Goal | Change |
| --- | --- |
| Reach full pressure sooner | Lower `timeToMaxDifficulty`, for example `180` to `120` |
| Make pressure climb earlier | Lower `difficultyCurveExponent`, for example `1.3` to `1.0` |

Use `minimumSpawnInterval` and `maxAliveEnemiesAtFullDifficulty` only when you also want the late game itself to become harder.

## Tuning Board Life

Board life lives on `VVEBoardLife`.

Current behavior:

- starts at 5 life
- leaked enemies usually deal 1 life
- reaches `GAME OVER` at 0 life
- pauses time when game over is enabled

Tune `startingLife` for overall forgiveness, and tune `VVEEnemyVikingWalker.boardDamageOnExit` for enemy leak severity.

## Tuning Economy

Economy has three main knobs:

- `VVEUsableWallet.startingDiamonds`
- `VVEPlacementCharacterSlot.cost`
- miner/potion reward rates and pickup values

`Level 1` currently starts with 14 diamonds. Current observed slot costs are:

| Character | Cost |
| --- | --- |
| Miner | 8 |
| Archer | 12 |
| Wizard | 12 |
| Cave man | 18 |
| Potion maker | 21 |

## Validation Checklist

Before calling a gameplay change done, quickly test:

- Can the scene enter Play Mode?
- Can the player place every visible character slot?
- Do invalid tiles reject placement?
- Does the diamond counter update after placement and pickup collection?
- Do enemies spawn in valid lanes?
- Do defenders attack only their lane?
- Do enemies damage or destroy defenders?
- Does the chest lose life when an enemy leaks through?
- Does `GAME OVER` appear when life reaches zero?
- Can damaged defenders be healed with potions?
- Does the remove tool work with `X` and `Left Shift`?

## Known Cleanup Candidates

- Rename `PlantPlacementManager` to a VVE-specific placement name.
- Keep or delete `Recovered_PlayMode_20260705_1956.unity` once `Level 1` is fully accepted as the main scene.
- Remove or restore the disabled `Assets/Scenes/Level 2.unity` build-settings reference. The file is not present in the current project tree.
- Separate legacy platformer content from board-defense content.
- Move scene-only balance values into ScriptableObjects once tuning stabilizes.
- Add automated edit-mode tests for wallet, health, lane spawning, and projectile target filtering.
