# Levels

## Format

Each yaml file in this folder is a level that can be played in game.
In each yaml file the waves are specified.
Waves have the following parameters:

## Parameters

- **time_offset:** the pause between the last waves end and the start of this one.
- **tier:** options: flag/final, this indicates that the wave is special and should be shown in the level progress bar.
- **spawns:** each item in this list is a spawn group.
    - time_offset: the delayed after a waves start, before this group is spawned
    - spacing: this determines the spacing algorith (the timing the units are spawned), (options: random/even)
    - unit: the type of enemy unit
    - count: the number of units that are spawned
    - duration: the duration during which this spawn groups unit should be spawned.