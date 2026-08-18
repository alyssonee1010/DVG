using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class VVELevelLoader
{
    static readonly Regex FileNamePattern = new Regex(@"^\d{2}-\d{2}(_.*)?\.yml$", RegexOptions.IgnoreCase);

    public static string LevelsDirectory
    {
        get { return Path.Combine(Application.dataPath, "Levels"); }
    }

    public static List<VVELevelDefinition> DiscoverLevels()
    {
        List<VVELevelDefinition> levels = new List<VVELevelDefinition>();
        string directory = LevelsDirectory;
        if (!Directory.Exists(directory))
        {
            Debug.LogWarning($"Levels folder not found: {directory}");
            return levels;
        }

        string[] filePaths = Directory.GetFiles(directory, "*.yml");
        foreach (string filePath in filePaths)
        {
            string fileName = Path.GetFileName(filePath);
            if (!FileNamePattern.IsMatch(fileName))
            {
                Debug.LogWarning($"Skipping level file '{fileName}': name must look like 'NN-NN.yml' or 'NN-NN_name.yml'.");
                continue;
            }

            VVELevelDefinition level = LoadFromFile(filePath);
            if (level != null)
            {
                levels.Add(level);
            }
        }

        levels.Sort(CompareLevels);
        return levels;
    }

    static int CompareLevels(VVELevelDefinition a, VVELevelDefinition b)
    {
        int stageCompare = a.Stage.CompareTo(b.Stage);
        if (stageCompare != 0)
        {
            return stageCompare;
        }

        return a.Level.CompareTo(b.Level);
    }

    public static VVELevelDefinition LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Level file not found: {path}");
            return null;
        }

        try
        {
            string text = File.ReadAllText(path);
            VVELevelDefinition level = LoadFromText(text);
            level.SourcePath = path;
            return level;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to load level '{path}': {exception.Message}");
            return null;
        }
    }

    public static VVELevelDefinition LoadFromText(string yamlText)
    {
        object parsed = VVEMiniYaml.Parse(yamlText);
        Dictionary<string, object> root = parsed as Dictionary<string, object>;
        if (root == null)
        {
            throw new FormatException("Level YAML root must be a mapping.");
        }

        VVELevelDefinition level = new VVELevelDefinition();
        level.Id = GetString(root, "id", "");
        level.Name = GetString(root, "name", "");
        level.Stage = GetInt(root, "stage", 1);
        level.Level = GetInt(root, "level", 1);

        object settingsObject;
        if (root.TryGetValue("settings", out settingsObject))
        {
            Dictionary<string, object> settings = settingsObject as Dictionary<string, object>;
            if (settings != null)
            {
                level.Lanes = GetInt(settings, "lanes", level.Lanes);
                level.StartingCurrency = GetInt(settings, "starting_currency", level.StartingCurrency);
            }
        }

        object unitsObject;
        if (root.TryGetValue("available_units", out unitsObject))
        {
            List<object> units = unitsObject as List<object>;
            if (units != null)
            {
                foreach (object unit in units)
                {
                    if (unit != null)
                    {
                        level.AvailableUnits.Add(Convert.ToString(unit, CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        object wavesObject;
        if (root.TryGetValue("waves", out wavesObject))
        {
            List<object> waves = wavesObject as List<object>;
            if (waves != null)
            {
                foreach (object waveObject in waves)
                {
                    Dictionary<string, object> waveMap = waveObject as Dictionary<string, object>;
                    if (waveMap != null)
                    {
                        level.Waves.Add(ParseWave(waveMap));
                    }
                }
            }
        }

        return level;
    }

    static VVEWaveDefinition ParseWave(Dictionary<string, object> waveMap)
    {
        VVEWaveDefinition wave = new VVEWaveDefinition();
        wave.Time = GetFloat(waveMap, "time", 0f);
        wave.Type = ParseWaveType(GetString(waveMap, "type", "normal"));

        object spawnsObject;
        if (waveMap.TryGetValue("spawns", out spawnsObject))
        {
            List<object> spawns = spawnsObject as List<object>;
            if (spawns != null)
            {
                foreach (object spawnObject in spawns)
                {
                    Dictionary<string, object> spawnMap = spawnObject as Dictionary<string, object>;
                    if (spawnMap != null)
                    {
                        VVESpawnDefinition spawn = new VVESpawnDefinition();
                        spawn.Unit = GetString(spawnMap, "unit", "");
                        spawn.Count = GetInt(spawnMap, "count", 1);
                        spawn.Interval = GetFloat(spawnMap, "interval", 1f);
                        spawn.Lane = GetString(spawnMap, "lane", "random");
                        wave.Spawns.Add(spawn);
                    }
                }
            }
        }

        return wave;
    }

    static VVEWaveType ParseWaveType(string text)
    {
        string normalized = text.Trim().ToLowerInvariant();
        if (normalized == "flag")
        {
            return VVEWaveType.Flag;
        }

        if (normalized == "final")
        {
            return VVEWaveType.Final;
        }

        return VVEWaveType.Normal;
    }

    static string GetString(Dictionary<string, object> map, string key, string defaultValue)
    {
        object value;
        if (map.TryGetValue(key, out value) && value != null)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        return defaultValue;
    }

    static int GetInt(Dictionary<string, object> map, string key, int defaultValue)
    {
        object value;
        if (!map.TryGetValue(key, out value) || value == null)
        {
            return defaultValue;
        }

        if (value is int)
        {
            return (int)value;
        }

        if (value is float)
        {
            return Mathf.RoundToInt((float)value);
        }

        int parsed;
        if (value is string && int.TryParse((string)value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
        {
            return parsed;
        }

        return defaultValue;
    }

    static float GetFloat(Dictionary<string, object> map, string key, float defaultValue)
    {
        object value;
        if (!map.TryGetValue(key, out value) || value == null)
        {
            return defaultValue;
        }

        if (value is float)
        {
            return (float)value;
        }

        if (value is int)
        {
            return (int)value;
        }

        float parsed;
        if (value is string && float.TryParse((string)value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
        {
            return parsed;
        }

        return defaultValue;
    }
}
