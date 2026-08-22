# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

*Vikings vs Everyone* is a Unity 2D board-defense prototype (PvZ-style): the player spends
diamonds to place defenders on a 6-lane board, defenders shoot/melee/mine/brew potions, and
Viking walkers spawn down lanes toward a chest. Runtime gameplay classes use a `VVE` prefix.

- Unity version: `6000.5.0f1` (Unity 6), render stack: URP `17.5.0`
- UI text: TextMeshPro. Tweening: PrimeTween (UPM package, see `Packages/manifest.json`).
- Input: legacy Input Manager is active (`Input.GetKeyDown`/`GetMouseButtonDown` throughout);
  `activeInputHandler: 2` in `ProjectSettings/ProjectSettings.asset` means "Both" — the new Input
  System package is present but not what gameplay code actually uses.

## Working In This Repo

This is a pure Unity project — there is no CLI build/lint/test pipeline and no automated test
suite yet (see `docs/DEVELOPMENT.md`'s "Known Cleanup Candidates" — tests are still a TODO).
To build or run anything, open the project in the Unity Editor:

1. Open the repo folder as a Unity project (Unity `6000.5.0f1`); let it restore packages from
   `Packages/manifest.json`.
2. Build Settings scene order: `Assets/Scenes/MainMenu.unity` (index 0, entry point) →
   `Assets/Scenes/Level 1.unity` (index 1, the only gameplay scene — all levels run inside it).
3. Press Play on either scene to test. There is no separate build/compile step to run outside
   the Editor — Unity compiles scripts automatically on domain reload.

Because there's no Editor access from a coding-agent session, any hand-authored `.unity`/`.meta`
changes should be flagged as unverified (not compiled/opened by Unity) until a human confirms
them in the Editor — see the pattern used in `docs/implemention_plans/main_menu.md`'s Progress
Log for how that's tracked.

## Repo Documentation

Read these before making non-trivial changes — they cover ground this file intentionally doesn't
repeat:

- `README.md` — controls, game loop, folder map, "Current Notes" gotchas.
- `docs/GAMEPLAY.md` — mechanics, characters, wave tuning, win/loss state.
- `docs/ARCHITECTURE.md` — per-system breakdown of the board/placement/combat/enemy/pickup layers.
- `docs/DEVELOPMENT.md` — step-by-step checklists for adding a character, enemy, or tuning
  waves/economy/board life.
- `docs/TODO.md` — known upcoming work.
- `docs/implemention_plans/` — implementation plans for larger features (e.g. the Main Menu),
  each tracking a "Progress Log" checklist of what's actually been done vs. deferred.

**These docs have some drift from current code** — written before the Levels/Stage-Select/Main
Menu system existed, and before a class rename:
- `VVEBoardCharacter` (named in `ARCHITECTURE.md`/`DEVELOPMENT.md`) is now `VVEDefender`
  (`Assets/_Scripts/Board/VVEDefender.cs`).
- `PlantPlacementManager` (in `Assets/_Scripts/Placement/CharPlacementManagement.cs` — yes, the
  file/class names still don't match) is unchanged and as described.
- Per-level starting currency/available units/unlocks now come from YAML level files (see
  below), not fixed scene values — `docs/DEVELOPMENT.md`'s "14 diamonds" is only `Level 1`'s
  old default, not a hard rule.

## Scene & Menu Flow

- **`MainMenu.unity`** (build index 0): a single `VVEMainMenuController` builds its entire
  Canvas UI procedurally in code at `Awake`/`Start` (Main / Stage Select / Settings panels) —
  there is no hand-placed Canvas hierarchy in the scene file. Stage Select lists levels grouped
  by `VVELevelDefinition.Stage`, from `VVELevelLoader.DiscoverLevels()`. Picking a level stores
  its id in the static `VVEPendingLevelSelection` holder and loads `Level 1`.
- **`Level 1.unity`** (build index 1): the only gameplay scene — every level runs inside it, no
  per-level scenes. `VVELevelSelectUI.Start()` consumes the pending level id (if set by the Main
  Menu) and shows the defender-loadout screen (`VVEDefenderLoadoutUI`) with a single "Continue"
  prompt for that level, rather than starting it immediately. Pressing Continue calls
  `VVEWaveDirector.StartLevel(level)`.
- **Continuous run flow**: on `VVEWaveDirector.LevelCompleted`, `VVELevelSelectUI` resets the
  board (`PlantPlacementManager.ResetBoard()` — destroys placed defenders *and* any uncollected
  `VVEBoardPickup`s), unlocks that level's rewards (`VVEDefenderUnlocks.UnlockAll`), and reopens
  the loadout + Continue prompt for the *next* level in discovery order — not the full
  stage-select grid. The full grid (`VVELevelSelectUI.OpenMenu()` with no argument) is only used
  standalone (opening `Level 1` directly without going through the Main Menu) or after the last
  level (no "run complete" screen exists yet).
- UI throughout this project (level-select cards, defender loadout grid/tray, progress bar, main
  menu) is built as plain GameObjects/`SpriteRenderer`/`TextMeshPro` (or, for the Main Menu,
  `UnityEngine.UI`) constructed in code at runtime — not authored by hand in the Editor. Follow
  that convention rather than introducing Editor-authored prefabs for new menu-style UI.

## Level Data

Levels are YAML files under `Assets/Levels/`, named `NN-NN.yml` or `NN-NN_slug.yml` (enforced by
a regex in `VVELevelLoader`). Parsed by a small hand-rolled parser (`VVEMiniYaml`) into
`VVELevelDefinition`. Key fields: `id`, `name`, `stage`/`level` (ints — stage grouping in the Main
Menu uses this int, not the filename), `settings.lanes`/`starting_currency`, `available_units`,
`waves` (each wave has `time` or chained `time_offset`, and `spawns` with `unit`/`count`/`lane`/
`interval` or `time_offset`+`duration`+`spacing`), and `unlocks` (defender ids granted on
completion, applied via `VVEDefenderUnlocks.UnlockAll`).

## Defender Unlocks & Loadout

`VVEDefenderUnlocks` (static, `PlayerPrefs`-backed) tracks which defender ids are unlocked and
which are in the current 6-slot loadout, independent of any single level. `VVEDefenderCatalog`
(a scene component, not an asset) is the id → prefab/cost/display-name source of truth that
`VVEDefenderUnlocks`/`VVEDefenderLoadoutUI` look entries up in — it only exists inside `Level 1`,
so anything needing catalog data (e.g. a Defender showcase) outside that scene can't resolve it
without duplicating data.
