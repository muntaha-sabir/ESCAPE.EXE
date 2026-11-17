using UnityEngine;

// Goes on: Pip (next to PlayerController). The simplest possible animation:
// no Animator window needed. Drag the sliced frames into the three lists.
//   Idle = standing still, Walk = moving left/right on the ground, Jump = in the air.
[RequireComponent(typeof(SpriteRenderer))]
public class PipAnimator : MonoBehaviour
{
    public Sprite[] idle;    // e.g. 2 frames
    public Sprite[] walk;    // e.g. 4 frames
    public Sprite[] jump;    // e.g. 1 frame

    public float idleFps = 3f;
    public float walkFps = 10f;

    SpriteRenderer sr;
    Rigidbody2D rb;
    float timer;
    Sprite[] current;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (rb == null) return;
        Vector2 v = rb.linearVelocity;

        Sprite[] want;
        float fps;
        if (Mathf.Abs(v.y) > 0.1f && jump != null && jump.Length > 0) { want = jump; fps = 1f; }
        else if (Mathf.Abs(v.x) > 0.1f && walk != null && walk.Length > 0) { want = walk; fps = walkFps; }
        else { want = idle; fps = idleFps; }

        if (want == null || want.Length == 0) return;
        if (want != current) { current = want; timer = 0f; }   // restart the new animation from frame 0

        timer += Time.deltaTime;
        int frame = (int)(timer * fps) % current.Length;
        sr.sprite = current[frame];
    }
}
