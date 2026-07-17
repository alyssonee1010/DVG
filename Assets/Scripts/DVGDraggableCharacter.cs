using UnityEngine;

public class DVGDraggableCharacter : MonoBehaviour
{
    [SerializeField] float dragZ = 0f;

    Camera dragCamera;
    DVGTilemapBoard currentBoard;
    Vector3Int currentCell;
    Vector3 dragOffset;
    Vector3 returnPosition;
    bool isDragging;

    void Awake()
    {
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.8f, 0.9f);
            box.offset = new Vector2(0f, 0.05f);
        }
    }

    void OnMouseDown()
    {
        dragCamera = Camera.main;
        if (dragCamera == null)
        {
            return;
        }

        isDragging = true;
        returnPosition = transform.position;
        dragOffset = transform.position - GetMouseWorldPosition();

        if (currentBoard != null)
        {
            currentBoard.Clear(this);
        }
    }

    void OnMouseDrag()
    {
        if (!isDragging || dragCamera == null)
        {
            return;
        }

        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    void OnMouseUp()
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;
        DVGTilemapBoard board = FindAnyObjectByType<DVGTilemapBoard>();
        if (board != null && board.TryPlace(this, transform.position))
        {
            return;
        }

        transform.position = returnPosition;
    }

    public void SetCurrentCell(DVGTilemapBoard board, Vector3Int cell)
    {
        currentBoard = board;
        currentCell = cell;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = Mathf.Abs(dragCamera.transform.position.z - dragZ);
        Vector3 world = dragCamera.ScreenToWorldPoint(mouse);
        world.z = dragZ;
        return world;
    }
}
