using UnityEngine;

// Goes on: each star. Needs a Box Collider 2D with "Is Trigger" ticked.
// Twinkles, bobs and glows. When Pip touches it: sparkle + it disappears.
public class StarCoin : MonoBehaviour
{
    public Sprite[] twinkleFrames;
    public float twinkleFps = 8f;
    public SpriteRenderer glow;          // the soft glow behind the star (child object)
    public Sprite[] sparkleFrames;
    public AudioClip pickupSound;        // optional

    SpriteRenderer sr;
    Vector3 basePos;
    float offset;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        basePos = transform.localPosition;
        offset = transform.position.x * 0.7f;
    }

    void Update()
    {
        // bob up and down by 2 pixels (snapped to the pixel grid)
        float bob = Mathf.Round(Mathf.Sin(Time.time * 2.5f + offset) * 2f) / 16f;
        transform.localPosition = basePos + new Vector3(0f, bob, 0f);

        if (twinkleFrames != null && twinkleFrames.Length > 0 && sr.enabled)
            sr.sprite = twinkleFrames[(int)Mathf.Repeat(Time.time * twinkleFps + offset * 3f, twinkleFrames.Length)];

        if (glow != null)
            glow.color = new Color(1f, 1f, 1f, 0.45f + 0.3f * Mathf.Sin(Time.time * 4f + offset));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CoinCounter.Add(1);
        GameAudio.Play("coin", 0.6f, 0.04f);
        if (pickupSound != null) AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        FX.PlayOnce(sparkleFrames, transform.position, 18f);
        gameObject.SetActive(false);
    }
}
