using System.Collections.Generic;

public enum VVEWaveType
{
    Normal,
    Flag,
    Final
}

public enum VVESpawnSpacing
{
    Even,
    Random
}

public class VVESpawnDefinition
{
    public string Unit = "";
    public int Count = 1;
    public string Lane = "random";

    // Legacy field: fixed seconds between consecutive spawns, used only when Duration <= 0.
    public float Interval = 1f;

    // New scheduling model: spawn starts TimeOffset seconds after its wave starts, and its
    // Count units are distributed across Duration seconds according to Spacing.
    public float TimeOffset = 0f;
    public float Duration = 0f;
    public VVESpawnSpacing Spacing = VVESpawnSpacing.Random;

    public float ImpliedDuration()
    {
        if (Duration > 0f)
        {
            return Duration;
        }

        return Count > 1 ? (Count - 1) * Interval : 0f;
    }

    public float EndOffset()
    {
        return TimeOffset + ImpliedDuration();
    }
}

public class VVEWaveDefinition
{
    // Absolute seconds from level start. Computed by VVELevelLoader from either an explicit
    // legacy "time" value, or by chaining TimeOffset onto the previous wave's end time.
    public float Time;

    // Relative seconds after the previous wave ends (or after level start for the first wave).
    // Only used when the wave YAML specifies "time_offset" instead of an absolute "time".
    public float TimeOffset;
    public bool UsesTimeOffset;

    // Seconds this wave spans, implied by its latest spawn's end offset. Computed by the loader.
    public float Duration;

    public VVEWaveType Type = VVEWaveType.Normal;
    public List<VVESpawnDefinition> Spawns = new List<VVESpawnDefinition>();
}

public class VVELevelDefinition
{
    public string Id = "";
    public string Name = "";
    public int Stage = 1;
    public int Level = 1;
    public int Lanes = 6;
    public int StartingCurrency = 100;
    public List<string> AvailableUnits = new List<string>();
    public List<VVEWaveDefinition> Waves = new List<VVEWaveDefinition>();
    public string SourcePath = "";
}
