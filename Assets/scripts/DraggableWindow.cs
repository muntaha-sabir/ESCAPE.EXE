using UnityEngine;

// Goes on: the ROOT object of every window (sticky note, calculator, game window...)
// IMPORTANT: keep this object's scale at (1,1,1). Size the window using its child objects,
// otherwise Pip will stretch when he rides it.
public class DraggableWindow : MonoBehaviour
{
    [Tooltip("Untick this for the frozen 'Not Responding' game window in Level 3")]
    public bool canDrag = true;
}
