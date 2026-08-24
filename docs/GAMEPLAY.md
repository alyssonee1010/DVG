# Gameplay

## Session Flow

1. MainMenu discovers level files through VVELevelLoader.
2. Selecting a level loads the shared gameplay scene and carries the selected level id through VVEPendingLevelSelection.
3. The gameplay scene opens the defender-loadout screen before the wave begins.
4. Starting the level gives VVEWaveDirector the selected level definition.
5. The player places defenders, collects pickups, and uses consumables while scheduled waves spawn.
6. A level completes after every scheduled wave has spawned and all tracked enemies have left the board or been defeated.
7. Completion resets the board, applies defender unlocks, and opens the loadout for the next discovered level.

## Defenders

Defenders are VVEDefender components backed by VVEHealth. Their behavior comes from focused role components:

- VVERowProjectileShooter attacks enemies in the same lane.
- VVEBoardMeleeAttacker handles close-range lane attacks.
- VVEMinerMiningReward creates diamond pickups.
- VVEWizardPotionReward creates potion pickups.

VVEWorldHealthBar is created for defenders at runtime. It appears after damage and hides again when health returns to full. Vikings do not receive this defender health bar.

The defender catalog maps stable ids to prefabs, display names, costs, and default unlock state. The loadout system stores unlocked ids and allows a limited set of defenders to be selected for play.

## Board Interaction

PlantPlacementManager coordinates board input:

- placement maps the shared pointer position to a tilemap cell
- removal finds a placed defender under the pointer
- pickup collection finds a VVEBoardPickup under the pointer
- potion clicks are routed to the active potion controller

VVEWorldPointer owns the shared mouse-to-world conversion and reusable visual hit-testing. Feature rules remain outside the utility; for example, healing accepts only living defenders below full health.

## Resources And Potions

VVEUsableWallet stores diamonds and healing potions.

- Placing defenders spends diamonds.
- Miners generate collectible diamond pickups.
- Potion makers generate collectible healing-potion pickups.
- Healing potion aiming highlights valid damaged defenders.
- Using a potion spends one potion and heals the selected defender.

Balance values belong to level data, catalog entries, prefabs, or serialized scene fields and are intentionally not duplicated in documentation.

## Enemies And Waves

VVEWaveDirector reads the selected level definition, resolves board lanes from the gameplay tilemap, and schedules enemy groups. Unit ids in level data are resolved through the director's configured unit options.

VVEEnemyVikingWalker is the current lane-walker implementation. It moves along its assigned lane, attacks defenders that block it, and damages VVEBoardLife if it reaches the exit.

Flag and final waves can display banners. The level progress UI reads the active wave schedule from VVEWaveDirector.

## Failure And Completion

VVEBoardLife represents the chest. Enemy leaks reduce it; reaching zero shows the game-over state and can pause gameplay.

Level completion is separate from board life: it occurs only after the wave schedule finishes and the director's tracked enemies are cleared.
