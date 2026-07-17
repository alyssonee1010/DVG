using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlantPlacementManager : MonoBehaviour
{
    [SerializeField] private Tilemap placementTilemap;
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] private Vector2 placementOffset = new Vector2(-0.2f, 0.2f);

    private Dictionary<Vector3Int, DVGBoardCharacter> occupiedCells = new Dictionary<Vector3Int, DVGBoardCharacter>();

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPlacePlant();
        }
    }

    private void TryPlacePlant()
    {
        CleanupOccupiedCells();

        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Mathf.Abs(Camera.main.transform.position.z);

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;

        Vector3Int cellPosition = placementTilemap.WorldToCell(mouseWorldPosition);

        Debug.Log("Clicked cell: " + cellPosition);
        Debug.Log("Has placement tile: " + placementTilemap.HasTile(cellPosition));

        if (!placementTilemap.HasTile(cellPosition))
        {
            Debug.Log("Cannot place here. This is not a valid placement tile.");
            return;
        }

        if (occupiedCells.ContainsKey(cellPosition))
        {
            Debug.Log("Cannot place here. This tile is already occupied.");
            return;
        }

        Vector3 spawnPosition = placementTilemap.GetCellCenterWorld(cellPosition);
        spawnPosition += (Vector3)placementOffset;
        GameObject spawnedPlant = Instantiate(plantPrefab, spawnPosition, Quaternion.identity);
        DVGBoardCharacter boardCharacter = spawnedPlant.GetComponent<DVGBoardCharacter>();
        if (boardCharacter == null)
        {
            boardCharacter = spawnedPlant.AddComponent<DVGBoardCharacter>();
        }

        boardCharacter.SetCell(cellPosition);

        occupiedCells[cellPosition] = boardCharacter;

        Debug.Log("Placed " + spawnedPlant.name + " at cell " + cellPosition);
    }

    private void CleanupOccupiedCells()
    {
        List<Vector3Int> clearedCells = new List<Vector3Int>();
        foreach (KeyValuePair<Vector3Int, DVGBoardCharacter> occupiedCell in occupiedCells)
        {
            DVGBoardCharacter character = occupiedCell.Value;
            if (character == null || character.Health == null || !character.Health.IsAlive)
            {
                clearedCells.Add(occupiedCell.Key);
            }
        }

        foreach (Vector3Int cell in clearedCells)
        {
            occupiedCells.Remove(cell);
        }
    }
}
