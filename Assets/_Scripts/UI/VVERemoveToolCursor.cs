using UnityEngine;

// World-space icon that sits at a fixed "home" position (its scene placement) and, while the
// remove tool is active, detaches and follows the mouse instead - replacing the system cursor
// with its own sprite. Driven by PlantPlacementManager via FollowCursor(bool); ContainsPoint lets
// PlantPlacementManager detect a click on the icon itself while it's still at its home position.
[RequireComponent(typeof(SpriteRenderer))]
public class VVERemoveToolCursor : MonoBehaviour
{
    bool removeToolIsActive;
    Vector3 originalPosition;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Used to detect a click on the icon while it's sitting at its home position (i.e. not
    // already following the mouse as the active remove-tool cursor).
    public bool ContainsPoint(Vector3 worldPosition)
    {
        return spriteRenderer != null && spriteRenderer.bounds.Contains(worldPosition);
    }

    void Update()
    {
        if (!removeToolIsActive)
        {
            return;
        }

        MoveToMouse();
    }

    void MoveToMouse()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z);
    }

    public void FollowCursor(bool isOn)
    {
        if (isOn)
            MoveToMouse();
        else
            transform.position = originalPosition;
        removeToolIsActive = isOn;
        Cursor.visible = !isOn;
    }

    void OnDisable()
    {
        if (removeToolIsActive)
            Cursor.visible = true;
    }

    void OnEnable()
    {
        if (removeToolIsActive)
            Cursor.visible = false;
    }

}
