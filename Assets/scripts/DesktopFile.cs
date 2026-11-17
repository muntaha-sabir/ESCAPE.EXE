using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Goes on: each GLOWING desktop file in Level 2 (Homework, Emoji, Lofi).
// The player drags it with the mouse. Dropped near a "drag here" spot it snaps in and becomes a solid stair.
// Dropped anywhere else it slides back home. When Pip lands on it, it gives its letter (H, E or L).
public class DesktopFile : MonoBehaviour
{
    public char letter = 'H';
    public Sprite letterSprite;

    [Header("Glow (child)")]
    public SpriteRenderer glow;

    [Header("Physics")]
    public Collider2D solid;               // Box Collider 2D on this object - switched on once placed
    public float snapDistance = 1.8f;

    [Header("Effects")]
    public Sprite[] poofFrames;

    public bool IsPlaced { get; private set; }

    static DesktopFile held;
    SpriteRenderer sr;
    Vector3 home, grabOffset;
    bool dragging, letterGiven;
    int baseOrder;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        home = transform.position;
        baseOrder = sr.sortingOrder;
        if (solid != null) solid.enabled = false;
    }

    void Update()
    {
        var m = Mouse.current;
        if (m == null || Camera.main == null || IsPlaced) return;
        Vector2 sp = m.position.ReadValue();
        Vector3 mp = Camera.main.ScreenToWorldPoint(new Vector3(sp.x, sp.y, 10f));
        mp.z = 0f;

        if (!dragging && held == null && m.leftButton.wasPressedThisFrame && Inside(mp))
        {
            dragging = true;
            held = this;
            grabOffset = transform.position - mp;
            SetOrder(baseOrder + 50);
            Level2Director.FileGrabbed();
        }

        if (dragging)
        {
            transform.position = Snap(mp + grabOffset);
            if (m.leftButton.wasReleasedThisFrame)
            {
                dragging = false;
                held = null;
                SetOrder(baseOrder);
                var slot = DropSlot.NearestFree(transform.position, snapDistance);
                if (slot != null) StartCoroutine(PlaceInto(slot));
                else StartCoroutine(MoveTo(home, 0.15f));
            }
        }
    }

    bool Inside(Vector3 p)
    {
        Vector3 c = transform.position;
        return Mathf.Abs(p.x - c.x) <= 1.3f && p.y <= c.y + 1.4f && p.y >= c.y - 1.4f;
    }

    void SetOrder(int o)
    {
        sr.sortingOrder = o;
        if (glow != null) glow.sortingOrder = o - 1;
    }

    static Vector3 Snap(Vector3 p)
    {
        return new Vector3(Mathf.Round(p.x * 16f) / 16f, Mathf.Round(p.y * 16f) / 16f, 0f);
    }

    IEnumerator MoveTo(Vector3 target, float time)
    {
        Vector3 start = transform.position;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, Mathf.Clamp01(t / time));
            yield return null;
        }
        transform.position = target;
    }

    IEnumerator PlaceInto(DropSlot slot)
    {
        slot.Fill(this);
        IsPlaced = true;
        if (glow != null) glow.enabled = false;
        yield return MoveTo(slot.transform.position, 0.08f);
        if (solid != null) solid.enabled = true;
        FX.PlayOnce(poofFrames, transform.position + new Vector3(0f, 0.3f, 0f));
        Vector3 s = transform.localScale;                    // little squash
        transform.localScale = new Vector3(s.x * 1.12f, s.y * 0.88f, 1f);
        yield return new WaitForSeconds(0.07f);
        transform.localScale = s;
        Level2Director.FilePlaced();
    }

    public void PipLanded()
    {
        if (letterGiven) return;
        letterGiven = true;
        LetterHUD.Collect(letter, letterSprite, transform.position + new Vector3(0f, 2.2f, 0f));
    }
}
