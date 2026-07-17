using UnityEngine;

public class DVGBoardCharacter : MonoBehaviour
{
    public Vector3Int Cell { get; private set; }
    public bool HasCell { get; private set; }

    public void SetCell(Vector3Int cell)
    {
        Cell = cell;
        HasCell = true;
    }
}
