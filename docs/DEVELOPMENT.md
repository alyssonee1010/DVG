# Development Guide

This guide covers common changes in the current Dev vs Gamers prototype.

## Project Setup

Use Unity `6000.5.0f1` when possible. Package versions are locked through `Packages/manifest.json` and `Packages/packages-lock.json`.

Recommended local flow:

1. Open the repo folder in Unity.
2. Let Unity import packages and assets.
3. Open `Assets/Scenes/Recovered_PlayMode_20260705_1956.unity`.
4. Press Play and test the board-defense flow.

## Main Scene To Edit

For current gameplay work, edit:

`Assets/Scenes/Recovered_PlayMode_20260705_1956.unity`

The project also has `Assets/Scenes/Level 1.unity`, but most current DVG mechanics are wired in the recovered scene.

## Adding A Placeable Character

1. Create or duplicate a prefab under `Assets/Prefabs/Placement Characters`.
2. Add `DVGBoardCharacter`.
3. Add `DVGHealth` or let `DVGBoardCharacter` require/find it.
4. Add one or more role components:
   - `DVGRowProjectileShooter` for ranged lane attacks.
   - `DVGBoardMeleeAttacker` for melee lane attacks.
   - `DVGMinerMiningReward` for diamond generation.
   - `DVGWizardPotionReward` for healing potion generation.
5. Add an `Animator` if attacks/rewards depend on animation events.
6. Add colliders so clicks, targeting, and enemy attack ranges can find the character.
7. Add or update a `DVGPlacementCharacterSlot` in the scene.
8. Assign the prefab and set its diamond cost on the slot.
9. Test placement, row sorting, enemy targeting, damage, and removal.

## Adding A Ranged Character

Use `DVGRowProjectileShooter`.

Checklist:

- Assign `projectilePrefab`.
- Assign `firePoint` or tune `firePointOffset`.
- Set `projectileDirection`, usually `(1, 0)` for shooting right.
- Tune `fireInterval`, `sightRange`, and `minimumFireDistance`.
- If animation should control the shot timing, enable `waitForShootAnimationEvent` and call `ShootProjectileAnimationEvent` from the attack animation.
- Confirm the projectile prefab has or receives `DVGDamageProjectile`.

## Adding A Melee Character

Use `DVGBoardMeleeAttacker`.

Checklist:

- Tune `attackRange`, `attackCooldown`, and `attackDamage`.
- Set `attackDirection` to match the side enemies approach from.
- Add/verify attack animation events if damage should land on a specific frame.
- Test against a Viking walker in the same lane.

## Adding A Resource Character

For diamonds, use `DVGMinerMiningReward`.

For healing potions, use `DVGWizardPotionReward`.

Checklist:

- Assign reward sprites.
- Tune event count per pickup.
- Tune start offset, landing offset, arc height, and collider radius.
- Confirm spawned pickups include `DVGBoardPickup`.
- Confirm the pickup resource is correct: `Diamonds` or `HealingPotions`.
- Test that pickups can be clicked and expire after their lifetime.

## Adding An Enemy

1. Create a prefab under `Assets/Prefabs/Vikings` or another enemy folder.
2. Add `DVGHealth`.
3. Add a script that implements `IDVGEnemyLaneWalker`, or reuse `DVGEnemyVikingWalker`.
4. Add a collider for projectile/melee targeting.
5. Add an animator with compatible triggers if using the existing walker behavior:
   - `Attack`
   - `AfterKill`
6. Add the enemy prefab to the `DVGEnemyLaneSpawner` enemy list.
7. Set max health, move speed, and weight on the spawner option.
8. Test that the enemy spawns, walks, attacks placed characters, dies cleanly, and damages board life on exit.

## Tuning Enemy Waves

Wave tuning lives on `DVGEnemyLaneSpawner`.

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
| enemy option `weight` | Relative spawn chance for that option |
| enemy option `maxHealth` | Health assigned when spawned |
| enemy option `moveSpeed` | Lane movement speed assigned when spawned |

The recovered scene currently ramps from an 11 second interval and 4 alive enemies toward a 3.5 second interval and 14 alive enemies over about 330 seconds.

## Tuning Board Life

Board life lives on `DVGBoardLife`.

Current behavior:

- starts at 5 life
- leaked enemies usually deal 1 life
- reaches `GAME OVER` at 0 life
- pauses time when game over is enabled

Tune `startingLife` for overall forgiveness, and tune `DVGEnemyVikingWalker.boardDamageOnExit` for enemy leak severity.

## Tuning Economy

Economy has three main knobs:

- `DVGUsableWallet.startingDiamonds`
- `DVGPlacementCharacterSlot.cost`
- miner/potion reward rates and pickup values

The recovered scene currently starts with 4 diamonds. Current observed slot costs are:

| Character | Cost |
| --- | --- |
| Miner | 2 |
| Potion maker | 3 |
| Wizard | 4 |
| Archer | 5 |
| Cave man | 8 |

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

- Rename `PlantPlacementManager` to a DVG-specific placement name.
- Rename `Recovered_PlayMode_20260705_1956.unity` once it is accepted as the main scene.
- Separate legacy platformer content from board-defense content.
- Move scene-only balance values into ScriptableObjects once tuning stabilizes.
- Add automated edit-mode tests for wallet, health, lane spawning, and projectile target filtering.
