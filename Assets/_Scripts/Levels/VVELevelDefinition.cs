using System.Collections.Generic;

public enum VVEWaveType
{
    Normal,
    Flag,
    Final
}

public class VVESpawnDefinition
{
    public string Unit = "";
    public int Count = 1;
    public float Interval = 1f;
    public string Lane = "random";
}

public class VVEWaveDefinition
{
    public float Time;
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
