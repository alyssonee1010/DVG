using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VVEWaveDirector : MonoBehaviour
{
    enum WalkDirection
    {
        RightToLeft,
        LeftToRight
    }

    [System.Serializable]
    class UnitOption
    {
        public string unitId = "";
        public MonoBehaviour prefab;
        public int maxHealth = 100;
        public float moveSpeed = 0.9f;
    }

    struct Lane
    {
        public int Index;
        public Vector3 Start;
        public Vector3 End;
    }

    [Header("Board")]
    [SerializeField] VVETilemapBoard tilemapBoard;
    [SerializeField] WalkDirection walkDirection = WalkDirection.RightToLeft;
    [SerializeField] float edgePadding = 0f;
    [SerializeField] float laneYOffset = -0.7f;
    [SerializeField] float spawnOutsideBoardTiles = 2f;
    [SerializeField] float exitPastBoardTiles = 0f;

    [Header("Units")]
    [SerializeField] UnitOption[] units;

    [Header("Wave Banner")]
    [SerializeField] string flagWaveMessage = "ELITE WAVE INCOMING";
    [SerializeField] string finalWaveMessage = "FINAL WAVE";
    [SerializeField] string levelCompleteMessage = "LEVEL COMPLETE";
    [SerializeField] Vector3 bannerWorldOffset = new Vector3(0f, 4.2f, 0f);
    [SerializeField] float bannerDuration = 3f;

    [Header("Level Completion")]
    [SerializeField] float boardClearCheckInterval = 1f;

    readonly List<Lane> lanes = new List<Lane>();
    readonly List<MonoBehaviour> aliveEnemies = new List<MonoBehaviour>();
    Coroutine runningLevelRoutine;
    TextMeshPro bannerText;

    public VVELevelDefinition CurrentLevel { get; private set; }
    public float LevelStartTime { get; private set; }
    public bool IsRunning
    {
        get { return runningLevelRoutine != null; }
    }

    public event System.Action<VVELevelDefinition> LevelStarted;
    public event System.Action<VVELevelDefinition> LevelCompleted;

    void Awake()
    {
        if (tilemapBoard == null)
        {
            tilemapBoard = FindAnyObjectByType<VVETilemapBoard>();
        }

        RebuildLanes();
    }

    public void RebuildLanes()
    {
        lanes.Clear();

        if (tilemapBoard == null || tilemapBoard.Grid == null || tilemapBoard.Tilemap == null)
        {
            return;
        }

        Tilemap tilemap = tilemapBoard.Tilemap;
        BoundsInt bounds = tilemap.cellBounds;

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            bool foundTile = false;
            int minX = int.MaxValue;
            int maxX = int.MinValue;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (!tilemap.HasTile(cell))
                {
                    continue;
                }

                foundTile = true;
                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
            }

            if (!foundTile)
            {
                continue;
            }

            Vector3 first = tilemap.GetCellCenterWorld(new Vector3Int(minX, y, 0));
            Vector3 last = tilemap.GetCellCenterWorld(new Vector3Int(maxX, y, 0));
            AddLane(y, first, last, Mathf.Max(1, maxX - minX));
        }
    }

    void AddLane(int laneIndex, Vector3 leftTile, Vector3 rightTile, int columnSpan)
    {
        float tileWidth = Mathf.Max(0.01f, Mathf.Abs(rightTile.x - leftTile.x) / columnSpan);
        float spawnOffset = edgePadding + tileWidth * spawnOutsideBoardTiles;
        float exitOffset = edgePadding + tileWidth * exitPastBoardTiles;

        Vector3 leftSpawn = VVELaneDepth.WithLaneZ(new Vector3(leftTile.x - spawnOffset, leftTile.y + laneYOffset, leftTile.z), laneIndex);
        Vector3 rightSpawn = VVELaneDepth.WithLaneZ(new Vector3(rightTile.x + spawnOffset, rightTile.y + laneYOffset, rightTile.z), laneIndex);
        Vector3 leftExit = VVELaneDepth.WithLaneZ(new Vector3(leftTile.x - exitOffset, leftTile.y + laneYOffset, leftTile.z), laneIndex);
        Vector3 rightExit = VVELaneDepth.WithLaneZ(new Vector3(rightTile.x + exitOffset, rightTile.y + laneYOffset, rightTile.z), laneIndex);

        Lane lane;
        lane.Index = laneIndex;
        lane.Start = walkDirection == WalkDirection.RightToLeft ? rightSpawn : leftSpawn;
        lane.End = walkDirection == WalkDirection.RightToLeft ? leftExit : rightExit;
        lanes.Add(lane);
    }

    public void StartLevel(VVELevelDefinition level)
    {
        if (level == null)
        {
            return;
        }

        StopLevel();
        CurrentLevel = level;
        RebuildLanes();
        aliveEnemies.Clear();

        VVEUsableWallet wallet = VVEUsableWallet.Instance != null ? VVEUsableWallet.Instance : FindAnyObjectByType<VVEUsableWallet>();
        if (wallet != null)
        {
            wallet.SetDiamonds(level.StartingCurrency);
        }

        LevelStartTime = Time.time;
        LevelStarted?.Invoke(level);
        runningLevelRoutine = StartCoroutine(RunLevel(level));
    }

    public void StopLevel()
    {
        if (runningLevelRoutine != null)
        {
            StopCoroutine(runningLevelRoutine);
            runningLevelRoutine = null;
        }
    }

    IEnumerator RunLevel(VVELevelDefinition level)
    {
        foreach (VVEWaveDefinition wave in level.Waves)
        {
            float waitUntil = LevelStartTime + wave.Time;
            while (Time.time < waitUntil)
            {
                yield return null;
            }

            if (wave.Type == VVEWaveType.Flag)
            {
                ShowBanner(flagWaveMessage);
            }
            else if (wave.Type == VVEWaveType.Final)
            {
                ShowBanner(finalWaveMessage);
            }

            yield return StartCoroutine(RunWave(wave));
        }

        yield return StartCoroutine(WaitForBoardClear());

        ShowBanner(levelCompleteMessage);
        runningLevelRoutine = null;
        LevelCompleted?.Invoke(level);
    }

    IEnumerator WaitForBoardClear()
    {
        while (true)
        {
            CleanupAliveList();
            if (aliveEnemies.Count == 0)
            {
                yield break;
            }

            yield return new WaitForSeconds(boardClearCheckInterval);
        }
    }

    void CleanupAliveList()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }

    IEnumerator RunWave(VVEWaveDefinition wave)
    {
        List<Coroutine> spawnRoutines = new List<Coroutine>();
        foreach (VVESpawnDefinition spawn in wave.Spawns)
        {
            spawnRoutines.Add(StartCoroutine(RunSpawn(spawn)));
        }

        foreach (Coroutine routine in spawnRoutines)
        {
            yield return routine;
        }
    }

    IEnumerator RunSpawn(VVESpawnDefinition spawn)
    {
        for (int i = 0; i < spawn.Count; i++)
        {
            SpawnUnit(spawn.Unit, spawn.Lane);
            if (i < spawn.Count - 1)
            {
                yield return new WaitForSeconds(Mathf.Max(0.01f, spawn.Interval));
            }
        }
    }

    void SpawnUnit(string unitId, string laneSpec)
    {
        if (lanes.Count == 0)
        {
            return;
        }

        UnitOption option = FindUnit(unitId);
        if (option == null || option.prefab == null)
        {
            Debug.LogWarning($"{nameof(VVEWaveDirector)}: no prefab configured for unit '{unitId}'.");
            return;
        }

        Lane lane = PickLane(laneSpec);
        MonoBehaviour instance = Instantiate(option.prefab, lane.Start, Quaternion.identity, transform);

        IVVEEnemyLaneWalker enemy;
        if (!TryGetLaneWalker(instance, out enemy))
        {
            Debug.LogWarning($"{nameof(VVEWaveDirector)}: unit '{unitId}' prefab has no lane walker script.");
            Destroy(instance.gameObject);
            return;
        }

        enemy.BeginLaneWalk(lane.Index, lane.Start, lane.End, option.moveSpeed, option.maxHealth);
        aliveEnemies.Add(instance);
    }

    bool TryGetLaneWalker(MonoBehaviour source, out IVVEEnemyLaneWalker laneWalker)
    {
        laneWalker = source as IVVEEnemyLaneWalker;
        if (laneWalker != null)
        {
            return true;
        }

        laneWalker = source.GetComponent<IVVEEnemyLaneWalker>();
        return laneWalker != null;
    }

    Lane PickLane(string laneSpec)
    {
        int laneIndex;
        bool isRandom = string.IsNullOrEmpty(laneSpec) || laneSpec.Equals("random", System.StringComparison.OrdinalIgnoreCase);
        if (!isRandom && int.TryParse(laneSpec, out laneIndex))
        {
            for (int i = 0; i < lanes.Count; i++)
            {
                if (lanes[i].Index == laneIndex)
                {
                    return lanes[i];
                }
            }
        }

        return lanes[Random.Range(0, lanes.Count)];
    }

    UnitOption FindUnit(string unitId)
    {
        if (units == null)
        {
            return null;
        }

        foreach (UnitOption option in units)
        {
            if (option != null && option.unitId == unitId)
            {
                return option;
            }
        }

        return null;
    }

    void ShowBanner(string message)
    {
        EnsureBannerText();
        bannerText.text = message;
        bannerText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideBanner));
        Invoke(nameof(HideBanner), bannerDuration);
    }

    void HideBanner()
    {
        if (bannerText != null)
        {
            bannerText.gameObject.SetActive(false);
        }
    }

    void EnsureBannerText()
    {
        if (bannerText != null)
        {
            return;
        }

        GameObject textObject = new GameObject("Wave Banner Text");
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = bannerWorldOffset;

        bannerText = textObject.AddComponent<TextMeshPro>();
        bannerText.alignment = TextAlignmentOptions.Center;
        bannerText.fontSize = 3f;
        bannerText.color = new Color(1f, 0.85f, 0.2f, 1f);
        bannerText.sortingOrder = 20000;

        Renderer bannerRenderer = textObject.GetComponent<Renderer>();
        if (bannerRenderer != null)
        {
            bannerRenderer.sortingLayerName = "UI";
        }

        bannerText.gameObject.SetActive(false);
    }
}
