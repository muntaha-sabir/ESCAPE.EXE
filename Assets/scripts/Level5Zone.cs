using UnityEngine;

// Goes on: trigger boxes in Level 5. Tells the level when Pip lands there.
// Zone 0 = on top of the NOTES.  Zone 1 = on the second stair file.
public class Level5Zone : MonoBehaviour
{
    public int zone = 0;

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var rb = other.attachedRigidbody;
        if (rb != null && rb.linearVelocity.y > 0.5f) return;      // only when he lands
        if (zone == 0) Level5Director.PipOnNotes(); else Level5Director.PipOnSecondStair();
    }
}
