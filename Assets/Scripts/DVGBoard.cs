using UnityEngine;

public class DVGBoard : MonoBehaviour
{
    [SerializeField] int rows = 6;
    [SerializeField] int columns = 20;
    [SerializeField] float tileSpacing = 1f;
    [SerializeField] DVGTile[] tiles;

    public int Rows => rows;
    public int Columns => columns;
    public float TileSpacing => tileSpacing;

    public DVGTile GetTile(int row, int column)
    {
        if (row < 0 || row >= rows || column < 0 || column >= columns || tiles == null)
        {
            return null;
        }

        int index = row * columns + column;
        return index >= 0 && index < tiles.Length ? tiles[index] : null;
    }

    public bool TryPlaceAt(int row, int column, GameObject occupant)
    {
        DVGTile tile = GetTile(row, column);
        return tile != null && tile.TryPlace(occupant);
    }

    public Vector3 GetWorldPosition(int row, int column)
    {
        DVGTile tile = GetTile(row, column);
        return tile != null ? tile.transform.position : transform.position;
    }
}
