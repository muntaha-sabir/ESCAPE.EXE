using UnityEngine;

// Goes on: a file that holds a letter Pip can collect by landing on it (Pip_sketch.old holds the "P").
// Needs a Box Collider 2D with "Is Trigger" ticked just ABOVE the file (where Pip stands).
public class LetterGiver : MonoBehaviour
{
    public char letter = 'P';
    public Sprite letterSprite;
    bool given;

    void OnTriggerStay2D(Collider2D other)
    {
        if (given || !other.CompareTag("Player")) return;
        var rb = other.attachedRigidbody;
        if (rb != null && rb.linearVelocity.y > 0.5f) return;      // only when Pip lands on it
        given = true;
        LetterHUD.Collect(letter, letterSprite, transform.position + new Vector3(0f, 2.2f, 0f));
        Level4Director.LetterTaken();
    }
}
