using UnityEngine;

// Goes on: any decoration that just loops frames forever (e.g. Pip hopping on the Super Jump Quest start screen).
// Drag the sliced frames into "Frames" in order. Each frame can have its own time (seconds).
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteLoop : MonoBehaviour
{
    public Sprite[] frames;
    [Tooltip("Seconds per frame. Leave empty to use Default Time for every frame.")]
    public float[] times = { 0.20f, 0.11f, 0.07f, 0.13f, 0.07f, 0.12f };   // matches the 6-frame hop
    public float defaultTime = 0.12f;

    SpriteRenderer sr;
    int index;
    float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (frames != null && frames.Length > 0) sr.sprite = frames[0];
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;
        timer += Time.deltaTime;
        float t = (times != null && index < times.Length) ? times[index] : defaultTime;
        if (timer >= t)
        {
            timer -= t;
            index = (index + 1) % frames.Length;
            sr.sprite = frames[index];
        }
    }
}
