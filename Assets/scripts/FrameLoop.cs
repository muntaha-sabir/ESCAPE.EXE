using UnityEngine;

// Goes on: any sprite that should loop through a few frames (glows, "drag here" slots...).
// Ping Pong = goes 0,1,2,1,0... instead of 0,1,2,0,1,2 (nice for soft glows).
[RequireComponent(typeof(SpriteRenderer))]
public class FrameLoop : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 4f;
    public bool pingPong = true;

    SpriteRenderer sr;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;
        int n = frames.Length;
        int i = (int)(Time.time * fps);
        if (pingPong && n > 1) { int p = i % (2 * n - 2); i = p < n ? p : 2 * n - 2 - p; }
        else i %= n;
        sr.sprite = frames[i];
    }
}
