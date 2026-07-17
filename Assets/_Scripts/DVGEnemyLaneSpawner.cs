using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DVGEnemyLaneSpawner : MonoBehaviour
{
    enum WalkDirection
    {
        RightToLeft,
        LeftToRight
    }

    [System.Serializable]
    class EnemySpawnOption
    {
        public MonoBehaviour prefab = null;
        [Min(1)] public int maxHealth = 100;
        [Min(0f)] public float moveSpeed = 0.75f;
        [Min(0f)] public float weight = 1f;
    }

    struct Lane
    {
        public int Index;
        public Vector3 Start;
        public Vector3 End;
    }

    [Header("Board")]
    [SerializeField] DVGBoard board;
    [SerializeField] DVGTilemapBoard tilemapBoard;
    [SerializeField] WalkDirection walkDirection = WalkDirection.RightToLeft;
    [SerializeField] float edgePadding = 0f;
    [SerializeField] float laneYOffset = 0f;
    [SerializeField] float spawnOutsideBoardTiles = 2f;
    [SerializeField] float exitPastBoardTiles = 0f;

    [Header("Spawning")]
    [SerializeField] EnemySpawnOption[] enemies;
    [SerializeField] bool spawnOnStart = true;
    [SerializeField] float initialDelay = 0.5f;
    [SerializeField] float spawnInterval = 3f;
    [SerializeField] int maxAliveEnemies = 12;

    readonly List<Lane> lanes = new List<Lane>();
    readonly List<MonoBehaviour> aliveEnemies = new List<MonoBehaviour>();
    float spawnTimer;

    void Awake()
    {
        if (board == null)
        {
            board = FindAnyObjectByType<DVGBoard>();
        }

        if (tilemapBoard == null)
        {
            tilemapBoard = FindAnyObjectByType<DVGTilemapBoard>();
        }

        RebuildLanes();
        spawnTimer = Mathf.Max(0f, initialDelay);
    }

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnEnemy();
        }
    }

    void Update()
    {
        CleanupAliveList();

        if (lanes.Count == 0 || enemies == null || enemies.Length == 0 || spawnInterval <= 0f)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f)
        {
            return;
        }

        SpawnEnemy();
        spawnTimer = spawnInterval;
    }

    public void RebuildLanes()
    {
        lanes.Clear();

        if (board != null)
        {
            BuildPrefabBoardLanes();
            if (lanes.Count > 0)
            {
                return;
            }
        }

        if (tilemapBoard != null)
        {
            BuildTilemapBoardLanes();
        }
    }

    public void SpawnEnemy()
    {
        CleanupAliveList();
        if (aliveEnemies.Count >= maxAliveEnemies || lanes.Count == 0)
        {
            return;
        }

        EnemySpawnOption option = PickEnemy();
        if (option == null || option.prefab == null)
        {
            return;
        }

        Lane lane = lanes[Random.Range(0, lanes.Count)];
        MonoBehaviour instance = Instantiate(option.prefab, lane.Start, Quaternion.identity, transform);
        if (!TryGetLaneWalker(instance, out IDVGEnemyLaneWalker enemy))
        {
            Debug.LogWarning($"{nameof(DVGEnemyLaneSpawner)} could not spawn '{instance.name}' because it has no lane walker script.");
            Destroy(instance.gameObject);
            return;
        }

        enemy.BeginLaneWalk(lane.Index, lane.Start, lane.End, option.moveSpeed, option.maxHealth);
        aliveEnemies.Add(instance);
    }

    void BuildPrefabBoardLanes()
    {
        for (int row = 0; row < board.Rows; row++)
        {
            Vector3 first = board.GetWorldPosition(row, 0);
            Vector3 last = board.GetWorldPosition(row, board.Columns - 1);
            AddLane(row, first, last);
        }
    }

    void BuildTilemapBoardLanes()
    {
        Grid grid = tilemapBoard.Grid;
        Tilemap tilemap = tilemapBoard.Tilemap;
        if (grid == null || tilemap == null)
        {
            return;
        }

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
            AddLane(y, first, last);
        }
    }

    void AddLane(int laneIndex, Vector3 leftTile, Vector3 rightTile)
    {
        float tileWidth = Mathf.Max(0.01f, Mathf.Abs(rightTile.x - leftTile.x) / Mathf.Max(1, GetColumnSpan()));
        float spawnOffset = edgePadding + tileWidth * spawnOutsideBoardTiles;
        float exitOffset = edgePadding + tileWidth * exitPastBoardTiles;

        Vector3 leftSpawn = new Vector3(leftTile.x - spawnOffset, leftTile.y + laneYOffset, leftTile.z);
        Vector3 rightSpawn = new Vector3(rightTile.x + spawnOffset, rightTile.y + laneYOffset, rightTile.z);
        Vector3 leftExit = new Vector3(leftTile.x - exitOffset, leftTile.y + laneYOffset, leftTile.z);
        Vector3 rightExit = new Vector3(rightTile.x + exitOffset, rightTile.y + laneYOffset, rightTile.z);

        lanes.Add(new Lane
        {
            Index = laneIndex,
            Start = walkDirection == WalkDirection.RightToLeft ? rightSpawn : leftSpawn,
            End = walkDirection == WalkDirection.RightToLeft ? leftExit : rightExit
        });
    }

    int GetColumnSpan()
    {
        if (tilemapBoard != null && tilemapBoard.Tilemap != null)
        {
            return Mathf.Max(1, tilemapBoard.Tilemap.cellBounds.size.x - 1);
        }

        if (board != null)
        {
            return Mathf.Max(1, board.Columns - 1);
        }

        return 1;
    }

    EnemySpawnOption PickEnemy()
    {
        float totalWeight = 0f;
        foreach (EnemySpawnOption option in enemies)
        {
            if (option != null && IsValidLaneWalkerPrefab(option.prefab))
            {
                totalWeight += Mathf.Max(0f, option.weight);
            }
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float pick = Random.Range(0f, totalWeight);
        foreach (EnemySpawnOption option in enemies)
        {
            if (option == null || !IsValidLaneWalkerPrefab(option.prefab))
            {
                continue;
            }

            pick -= Mathf.Max(0f, option.weight);
            if (pick <= 0f)
            {
                return option;
            }
        }

        return null;
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

    bool IsValidLaneWalkerPrefab(MonoBehaviour prefab)
    {
        return prefab != null && TryGetLaneWalker(prefab, out _);
    }

    bool TryGetLaneWalker(MonoBehaviour source, out IDVGEnemyLaneWalker laneWalker)
    {
        laneWalker = source as IDVGEnemyLaneWalker;
        if (laneWalker != null)
        {
            return true;
        }

        laneWalker = source.GetComponent<IDVGEnemyLaneWalker>();
        return laneWalker != null;
    }
}
