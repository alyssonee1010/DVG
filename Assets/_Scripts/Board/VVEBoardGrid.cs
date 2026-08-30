using System.Collections.Generic;
using UnityEngine;

// Procedurally builds a rows x columns grid of VVETile cells sized by cellSize, instead of a
// hand-painted Tilemap. [ExecuteAlways] + the Update() diff check (rather than OnValidate) is
// deliberate: Tilemap/GameObject edits triggered from inside OnValidate are unreliable in the
// Unity editor, but rebuilding from a normal Update tick behaves like any other runtime code and
// reflects Inspector changes (Rows, Columns, CellSize) immediately, in both edit mode and play mode.
[ExecuteAlways]
public class VVEBoardGrid : MonoBehaviour
{
    [SerializeField] int rows = 6;
    [SerializeField] int columns = 10;
    [SerializeField] Vector2 cellSize = new Vector2(1f, 1f);
    [SerializeField] Sprite[] whiteCellSprites;
    [SerializeField] Sprite[] blackCellSprites;
    [SerializeField] int randomSeed;
    [SerializeField] Color cellColor = Color.white;
    [SerializeField] string sortingLayerName = "Default";
    [SerializeField] int sortingOrder = 0;

    readonly List<VVETile> cells = new List<VVETile>();
    int builtRows = -1;
    int builtColumns = -1;
    Vector2 builtCellSize = Vector2.zero;
    int builtContentSignature;

    public int Rows => rows;
    public int Columns => columns;
    public Vector2 CellSize => cellSize;
    public float BoardWidth => columns * cellSize.x;
    public float BoardHeight => rows * cellSize.y;

    void OnValidate()
    {
        rows = Mathf.Max(0, rows);
        columns = Mathf.Max(0, columns);
        cellSize = new Vector2(Mathf.Max(0.01f, cellSize.x), Mathf.Max(0.01f, cellSize.y));
    }

    void OnEnable()
    {
        Rebuild();
    }

    void Update()
    {
        int contentSignature = ComputeContentSignature();
        if (rows != builtRows || columns != builtColumns || cellSize != builtCellSize || contentSignature != builtContentSignature)
        {
            Rebuild();
        }
    }

    void Rebuild()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }

        cells.Clear();

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                GameObject cellObject = new GameObject($"Tile R{row:00} C{column:00}");
                cellObject.transform.SetParent(transform, false);
                cellObject.transform.localPosition = GetCellCenterLocal(row, column);
                cellObject.transform.localScale = new Vector3(cellSize.x, cellSize.y, 1f);

                bool isWhiteCell = (row + column) % 2 == 0;
                Sprite[] cellSpriteSet = isWhiteCell ? whiteCellSprites : blackCellSprites;

                SpriteRenderer spriteRenderer = cellObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = PickSprite(cellSpriteSet, row, column);
                spriteRenderer.color = cellColor;
                spriteRenderer.sortingLayerName = sortingLayerName;
                spriteRenderer.sortingOrder = sortingOrder;

                VVETile tile = cellObject.AddComponent<VVETile>();
                tile.Setup(row, column);
                cells.Add(tile);
            }
        }

        builtRows = rows;
        builtColumns = columns;
        builtCellSize = cellSize;
        builtContentSignature = ComputeContentSignature();
    }

    // Picks a sprite from the set deterministically from (randomSeed, row, column), so a given
    // cell always shows the same sprite for a given seed regardless of rebuild order or how many
    // times the board has been resized.
    Sprite PickSprite(Sprite[] spriteSet, int row, int column)
    {
        if (spriteSet == null || spriteSet.Length == 0)
        {
            return null;
        }

        int cellHash = HashCell(randomSeed, row, column);
        int index = (int)((uint)cellHash % (uint)spriteSet.Length);
        return spriteSet[index];
    }

    static int HashCell(int seed, int row, int column)
    {
        unchecked
        {
            int hash = seed;
            hash = hash * 397 ^ row;
            hash = hash * 397 ^ column;
            return hash;
        }
    }

    int ComputeContentSignature()
    {
        unchecked
        {
            int hash = randomSeed;
            hash = hash * 397 ^ SpriteArrayHash(whiteCellSprites);
            hash = hash * 397 ^ SpriteArrayHash(blackCellSprites);
            return hash;
        }
    }

    static int SpriteArrayHash(Sprite[] sprites)
    {
        unchecked
        {
            int hash = sprites?.Length ?? 0;
            if (sprites != null)
            {
                foreach (Sprite sprite in sprites)
                {
                    hash = hash * 397 ^ (sprite != null ? sprite.GetHashCode() : 0);
                }
            }

            return hash;
        }
    }

    public Vector3 GetCellCenterLocal(int row, int column)
    {
        return new Vector3((column + 0.5f) * cellSize.x, (row + 0.5f) * cellSize.y, 0f);
    }

    public Vector3 GetCellCenterWorld(int row, int column)
    {
        return transform.TransformPoint(GetCellCenterLocal(row, column));
    }

    public bool IsValidCell(int row, int column)
    {
        return row >= 0 && row < rows && column >= 0 && column < columns;
    }

    public VVETile GetTile(int row, int column)
    {
        if (!IsValidCell(row, column))
        {
            return null;
        }

        int index = row * columns + column;
        return index >= 0 && index < cells.Count ? cells[index] : null;
    }

    public bool TryGetCellFromWorldPosition(Vector3 worldPosition, out int row, out int column)
    {
        Vector3 local = transform.InverseTransformPoint(worldPosition);
        column = Mathf.FloorToInt(local.x / cellSize.x);
        row = Mathf.FloorToInt(local.y / cellSize.y);
        return IsValidCell(row, column);
    }
}
