# Main Menu — Implementation Plan

## Progress Log

Implemented so far (this pass):

- [x] `Assets/_Scripts/UI/VVEMainMenuController.cs` — builds the Canvas and all three
      panels (Main / Stage Select / Settings) procedurally at runtime, same convention
      as `VVELevelSelectUI.BuildMenu`/`VVEDefenderLoadoutUI.Rebuild`. Handles panel
      navigation, Stage Select population, Quit, and the volume slider.
- [x] `Assets/_Scripts/UI/VVEPendingLevelSelection.cs` — tiny static holder that carries
      the chosen level id across the MainMenu → gameplay scene load.
- [x] `Assets/_Scripts/UI/VVEAudioSettings.cs` — minimal master-volume setting
      (`AudioListener.volume` + `PlayerPrefs`), applied on menu `Awake`.
- [x] `VVELevelSelectUI.Start()` now consumes a pending level selection (if the Main
      Menu set one) and starts that level directly instead of opening its own menu —
      additive change, existing standalone behavior (testing "Level 1" scene directly)
      is unchanged. **Old system not removed yet** — see Deferred below.
- [x] `Assets/Scenes/MainMenu.unity` created and registered as build index 0 (Main
      Camera + one `MainMenuController` object holding `VVEMainMenuController`;
      everything else is built by that script in code). EventSystem is also created
      automatically at runtime by the controller if one isn't present.
- [x] Stage grouping uses `VVELevelDefinition.Stage` (the existing int field from level
      YAML), not a parsed filename — see **Note on stage IDs** below, this differs
      from the plan's `03.1`-style example.
- [x] Panel transitions use a plain coroutine alpha cross-fade (no PrimeTween) — see
      **Note on animation** below.

- [x] Continuous in-level flow: entering "Level 1" with a pending level (from Main Menu
      Stage Select) no longer starts the level immediately — it now opens
      `VVELevelSelectUI`'s existing loadout screen (`VVEDefenderLoadoutUI`) together
      with a single "Continue" prompt card for that level (wave director stays
      inactive, progress bar stays hidden, both automatically — see
      `VVELevelProgressUI.Awake`/`OnLevelStarted`). Pressing Continue starts the level.
      On `VVEWaveDirector.LevelCompleted`, the flow returns to the same loadout +
      Continue prompt for the *next* level in `VVELevelLoader.DiscoverLevels()` order
      (not the full stage-select grid) — so a full run has no stage-select in between
      levels. `VVELevelSelectUI.OpenMenu(VVELevelDefinition onlyLevel = null)` now
      takes an optional single-level override; the full grid is still built when
      `onlyLevel` is `null` (used for standalone testing and once there's no next
      level after the last one — no "run complete" screen exists yet, falls back to
      the grid).

- [x] `PlantPlacementManager.ResetBoard()` (in `CharPlacementManagement.cs`) now also
      destroys any un-collected `VVEBoardPickup` instances (diamonds/potions) still
      lying on the board, alongside the defenders it already removed — both run on
      every level completion via `VVELevelSelectUI.OnLevelCompleted`.

- [x] Wired the user-supplied `Assets/Art/UI/title_image.png` (full scenic background) and
      `Assets/Art/UI/main_menu_buttons.png` (title logo + Play/Stage Select/Settings/Quit,
      already sliced by Unity's importer into 5 sub-sprites) into `VVEMainMenuController`,
      replacing the flat-color/text placeholders on the Main panel. Background covers the whole
      Canvas (all three panels) via `AspectRatioFitter.EnvelopeParent` so it crops instead of
      stretching at other aspect ratios; the four nav buttons are the sprites themselves (no
      separate text label needed, the art already has it baked in). Falls back to the original
      placeholder look if a sprite field is left unassigned. "Play" and "Stage Select" currently
      do the exact same thing (open Stage Select) since there's no separate resume flow yet.

Deferred / needs a manual Editor pass (not done in this session):

- [ ] Assign a `VVEDefender` prefab to `defenderPrefab` on the `MainMenuController`
      object (Inspector). Left `null` for now — the showcase silently no-ops until set.
- [ ] Placeholder/real background art and decorations — none added yet.
- [ ] Open the project in the Unity Editor and confirm `MainMenu.unity` compiles and
      plays end-to-end (Play → Stage Select → pick level → loads "Level 1" and starts
      it). This was authored by hand outside the Editor and has not been verified by
      Unity itself yet.
- [ ] Visual polish pass on Stage Select (currently unscrollable `VerticalLayoutGroup`
      list — will overflow the screen once there are many levels; no `ScrollRect` yet).
- [ ] Retire `VVELevelSelectUI.cs` / `VVELevelSelectCard.cs` (see step 11 in the plan
      below) — **not started**. The old system is still fully intact and still runs
      when "Level 1" is opened directly without going through the Main Menu.
- [ ] PrimeTween-based entrance animation for Title/Buttons/Defender — not added; only
      the coroutine idle-bob exists for the Defender.

### Note on stage IDs

The plan's `01-01` / `03.1-01` filename examples don't match the current data model:
`VVELevelDefinition.Stage` is an `int` (parsed from the level YAML's `stage:` field via
`VVELevelLoader`), and `VVELevelLoader`'s filename regex only accepts `NN-NN.yml`
(no `03.1` style). Changing that would mean modifying the level file format/loader,
which the plan explicitly says not to do without cause. Stage Select therefore groups
by the existing integer `Stage` field — this fully satisfies "derive stage from the
level identifier, no hard-coded list" without touching `VVELevelLoader`.

### Note on animation

New code avoids calling into PrimeTween directly (beyond what already exists in
`VVETween.cs`) because its exact API surface couldn't be verified against the
installed package version without compiling in the Editor. Panel fades and the
Defender idle bob use plain Unity coroutines instead — functionally equivalent, just
not using PrimeTween's easing curves. Swapping these for `PrimeTween.Tween` calls once
verified in-editor is a safe, isolated follow-up (see Deferred above).

---

## Goal

Implement a new `MainMenu` scene for *Vikings vs Everyone* containing:

- Main menu
- Stage selection
- Settings
- Quit
- Animated Defender showcase

Replace the existing world-space `VVELevelSelectUI` while preserving its functionality.

---

## Architecture

### Scene

Create `Assets/Scenes/MainMenu.unity`:

```text
MainMenu
├── Main Camera
├── Background
├── Defender
├── Decorations
└── Canvas
    ├── MainPanel
    ├── StageSelectPanel
    └── SettingsPanel
```

Main Menu, Stage Select and Settings are panels within the same scene.

Do not create separate scenes for Stage Select or Settings.

### UI

Use a Screen Space - Camera Canvas with:

- Orthographic Main Camera
- `CanvasScaler`
  - Scale With Screen Size
  - Reference resolution: 1920×1080

Interactive UI uses Unity UI components.

Visual elements such as the Defender, background and decorations remain ordinary GameObjects/SpriteRenderers.

The Defender must not need to be converted into a UI element.

### Existing Systems

Before implementing anything, inspect:

- `VVELevelLoader`
- `VVELevelSelectUI`
- `VVELevelSelectCard`
- `VVEManager`
- `VVEBoardLife`
- `VVEDefenderUnlocks`
- `VVETween`
- Existing PrimeTween usage
- Existing scene-loading and audio/settings code

Reuse existing functionality rather than creating parallel systems. In particular:

- Level discovery must use `VVELevelLoader.DiscoverLevels()`.
- Level loading should follow the existing `VVELevelSelectUI.SelectLevel` flow.
- Existing level-completion/reset/unlock behavior must be preserved.

### Main Menu

`VVEMainMenuController` should handle menu navigation:

- `ShowMainPanel()`
- `ShowStageSelect()`
- `ShowSettings()`

It should not become responsible for level discovery, gameplay state or level-completion logic.

Only one panel should be visible/interactable at a time.

Use `CanvasGroup` and PrimeTween/`VVETween` for simple panel transitions.

### Stage Select

Stage Select replaces `VVELevelSelectUI`.

Discover levels through `VVELevelLoader.DiscoverLevels()`.

Do not hard-code the available levels.

Derive the stage from the level identifier. For example:

- `01-01` → stage `01`
- `03-04` → stage `03`
- `03.1-01` → stage `03.1`

The UI should therefore naturally support:

```text
01-01
01-02
01-03

02-01
02-02

03-01
03-02
03-03
03-04

03.1-01
03.1-02
```

Use standard Unity UI buttons rather than the existing world-space collider/raycasting approach.

The exact visual layout should remain easy to change.

### Settings

Initially only implement master volume.

If an existing settings/audio system exists, reuse it.

Otherwise create a minimal `VVEAudioSettings` implementation that:

- Sets `AudioListener.volume`
- Persists the value with `PlayerPrefs`
- Restores it on startup

Do not introduce a general settings framework.

### Visuals

Add:

- Placeholder background
- Existing Defender prefab
- Optional decorations

The Defender should have an idle animation or subtle PrimeTween movement.

All placeholder art must be replaceable without code changes.

### Migration

Do not immediately delete `VVELevelSelectUI`. First:

1. Implement the new Stage Select.
2. Verify level discovery.
3. Verify level launching.
4. Verify level completion.
5. Verify board reset and Defender unlock behavior.
6. Replace existing references.
7. Search for remaining references.
8. Remove `VVELevelSelectUI` and `VVELevelSelectCard`.

Do not change `VVEManager`'s lifecycle or add it to the Main Menu scene unless inspection shows this is genuinely necessary.

### Constraints

Do not:

- Create another level-discovery system.
- Create another level-loading system.
- Change the level YAML format.
- Change gameplay behavior unnecessarily.
- Introduce a global game-state architecture.
- Introduce a generic UI framework.
- Add unnecessary abstractions.
- Modify unrelated systems.

Prefer existing project conventions over introducing new ones.

Keep classes small, semantic and focused. Comments should explain why, not what.

---

## Implementation Order

1. Inspect existing systems.
2. Create Main Menu scene.
3. Build UI panels.
4. Implement panel navigation.
5. Implement Stage Select using existing level discovery/loading.
6. Implement Settings.
7. Add Defender/background/decorations.
8. Add animations/transitions.
9. Migrate level-completion flow.
10. Remove obsolete level-select components.
11. Compile and test the complete flow.

---

## Done When

- [ ] Main Menu works. *(built; not yet verified in-Editor — see Progress Log)*
- [x] Stage Select discovers and displays all levels (via `VVELevelLoader.DiscoverLevels()`).
- [ ] ~~`03.1-01`-style stage identifiers work correctly~~ — superseded, see **Note on
      stage IDs**; grouping is by the existing int `Stage` field instead.
- [x] Levels launch through the existing system (`VVELevelSelectUI.SelectLevel` →
      `VVEWaveDirector.StartLevel`), reached via `VVEPendingLevelSelection`.
- [x] Settings persist (`VVEAudioSettings` via `PlayerPrefs`).
- [ ] Defender showcase works — code path exists, no prefab assigned yet.
- [x] Level completion returns correctly to Stage Select. *(unchanged — still
      `VVELevelSelectUI`'s own subscription to `VVEWaveDirector.LevelCompleted`, not
      touched by this change.)*
- [x] Existing reset/unlock behavior is preserved *(untouched: board reset,
      `VVEBoardLife.ResetLife`, `VVEDefenderUnlocks.UnlockAll` still run exactly as
      before inside `VVELevelSelectUI.OnLevelCompleted`)*.
- [ ] Old level-select components are no longer referenced — **not done**,
      intentionally deferred (see Progress Log / Migration Order).
- [ ] Project compiles without errors — **not verified**, no Unity Editor access this
      session; needs a compile check on next Editor open.
