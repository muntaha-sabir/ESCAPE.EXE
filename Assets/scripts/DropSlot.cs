using System.Collections.Generic;
using UnityEngine;

// Goes on: each dotted "drag here" spot in Level 2.
// Needs a Box Collider 2D with "Is Trigger" ticked, sitting just ABOVE the spot (where Pip stands).
// When a glowing file is dropped near it, the file snaps in and becomes a solid stair.
// When Pip lands on the file, the file gives Pip its letter. The top step starts the reminder scene.
public class DropSlot : MonoBehaviour
{
    public bool isTopStep;
    public SpriteRenderer outline;        // the dotted "drag here" picture (hidden once filled)

    [HideInInspector] public DesktopFile file;
    public bool Filled { get { return file != null; } }

    static readonly List<DropSlot> all = new List<DropSlot>();

    void OnEnable() { all.Add(this); }
    void OnDisable() { all.Remove(this); }

    public static DropSlot NearestFree(Vector3 pos, float maxDistance)
    {
        DropSlot best = null;
        float bestD = maxDistance;
        foreach (var s in all)
        {
            if (s.Filled) continue;
            float d = Vector2.Distance(pos, s.transform.position);
            if (d < bestD) { bestD = d; best = s; }
        }
        return best;
    }

    public void Fill(DesktopFile f)
    {
        file = f;
        GameAudio.Play("snap", 0.7f);
        if (outline != null) outline.enabled = false;
        var loop = outline != null ? outline.GetComponent<FrameLoop>() : null;
        if (loop != null) loop.enabled = false;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (file == null || !file.IsPlaced || !other.CompareTag("Player")) return;
        var rb = other.attachedRigidbody;
        if (rb != null && rb.linearVelocity.y > 0.5f) return;      // only when Pip lands, not while jumping up through
        file.PipLanded();
        if (isTopStep) Level2Director.TopReached();
    }
}
