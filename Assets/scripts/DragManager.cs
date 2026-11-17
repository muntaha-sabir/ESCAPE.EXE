using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Goes on: one empty "Managers" object in every scene that has windows or files to drag.
// Windows are grabbed by their TitleBar. Files are grabbed anywhere.
public class DragManager : MonoBehaviour
{
    public Camera cam;                  // leave empty to use the Main Camera
    public LayerMask draggableLayers;   // tick TitleBar + Files
    public LayerMask playerLayer;       // tick Player

    Transform dragging;
    Vector3 offset;
    DraggableWindow draggingWindow;
    DroppableFile draggingFile;

    void Start()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || cam == null) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouse.position.ReadValue());
        mouseWorld.z = 0f;

        // Pick something up
        if (mouse.leftButton.wasPressedThisFrame)
        {
            // Ignore clicks on UI (pop-up buttons etc.)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            TryPickUp(mouseWorld);
        }

        // Move it
        if (dragging != null && mouse.leftButton.isPressed)
        {
            Vector3 target = mouseWorld + offset;
            if (draggingWindow == null || CanMoveWindow(draggingWindow, target - dragging.position))
                dragging.position = target;
        }

        // Let go
        if (dragging != null && mouse.leftButton.wasReleasedThisFrame)
        {
            if (draggingFile != null) draggingFile.OnDropped();
            dragging = null;
            draggingWindow = null;
            draggingFile = null;
        }
    }

    void TryPickUp(Vector3 mouseWorld)
    {
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld, draggableLayers);
        if (hit == null) return;

        draggingFile = hit.GetComponent<DroppableFile>();
        if (draggingFile != null)
        {
            dragging = draggingFile.transform;
        }
        else
        {
            DraggableWindow window = hit.GetComponentInParent<DraggableWindow>();
            if (window != null && window.canDrag)
            {
                draggingWindow = window;
                dragging = window.transform;
            }
        }

        if (dragging != null) offset = dragging.position - mouseWorld;
    }

    // Stops a window being pushed INTO Pip. (Pip riding on the window is fine: he is its child.)
    bool CanMoveWindow(DraggableWindow window, Vector3 delta)
    {
        foreach (Collider2D col in window.GetComponentsInChildren<Collider2D>())
        {
            if (col.isTrigger) continue;
            Bounds b = col.bounds;
            Collider2D hit = Physics2D.OverlapBox(b.center + delta, b.size * 0.95f, 0f, playerLayer);
            if (hit != null && !hit.transform.IsChildOf(window.transform)) return false;
        }
        return true;
    }
}
