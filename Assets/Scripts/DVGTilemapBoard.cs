using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DVGTilemapBoard : MonoBehaviour
{
    [SerializeField] Grid grid;
    [SerializeField] Tilemap tilemap;
    [SerializeField] Vector3 placementOffset = new Vector3(0f, 0.18f, 0f);

    readonly Dictionary<Vector3Int, DVGDraggableCharacter> occupants = new Dictionary<Vector3Int, DVGDraggableCharacter>();

    public Grid Grid => grid;
    public Tilemap Tilemap => tilemap;

    void Awake()
    {
        if (grid == null)
        {
            grid = GetComponentInParent<Grid>();
        }

        if (tilemap == null)
        {
            tilemap = GetComponentInChildren<Tilemap>();
        }
    }

    public bool TryPlace(DVGDraggableCharacter character, Vector3 worldPosition)
    {
        if (character == null || grid == null || tilemap == null)
        {
            return false;
        }

        Vector3Int cell = grid.WorldToCell(worldPosition);
        if (!tilemap.HasTile(cell) || IsOccupied(cell, character))
        {
            return false;
        }

        Clear(character);
        occupants[cell] = character;
        character.SetCurrentCell(this, cell);
        character.transform.position = GetCellCenterWorld(cell);
        return true;
    }

    public void Clear(DVGDraggableCharacter character)
    {
        if (character == null)
        {
            return;
        }

        Vector3Int foundCell = default;
        bool found = false;
        foreach (KeyValuePair<Vector3Int, DVGDraggableCharacter> pair in occupants)
        {
            if (pair.Value == character)
            {
                foundCell = pair.Key;
                found = true;
                break;
            }
        }

        if (found)
        {
            occupants.Remove(foundCell);
        }
    }

    public Vector3 GetCellCenterWorld(Vector3Int cell)
    {
        if (tilemap != null)
        {
            return tilemap.GetCellCenterWorld(cell) + placementOffset;
        }

        return grid.GetCellCenterWorld(cell) + placementOffset;
    }

    bool IsOccupied(Vector3Int cell, DVGDraggableCharacter requester)
    {
        return occupants.TryGetValue(cell, out DVGDraggableCharacter occupant) && occupant != null && occupant != requester;
    }
}
