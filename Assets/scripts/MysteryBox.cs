using System.Collections;
using UnityEngine;

// Goes on: the red "?" in Level 1. Needs a Box Collider 2D with "Is Trigger" ticked.
// Spins, bobs and glows eerily. When Pip touches it, the error cutscene starts.
// Later the cutscene makes it glitch away (GlitchOut).
public class MysteryBox : MonoBehaviour
{
    public ErrorCutscene cutscene;
    public Transform spinPart;          // the child with the "?" sprite
    public SpriteRenderer glow;         // the red glow child
    public float spinSpeed = 3f;
    public float bobPixels = 2f;

    Vector3 basePos;
    bool used, frozen;

    void Start()
    {
        basePos = transform.localPosition;
    }

    void Update()
    {
        if (frozen) return;

        if (spinPart != null)
        {
            float s = Mathf.Cos(Time.time * spinSpeed);
            float x = Mathf.Max(Mathf.Abs(s), 0.15f) * (s < 0 ? -1f : 1f);
            spinPart.localScale = new Vector3(x, 1f, 1f);
        }

        float bob = Mathf.Round(Mathf.Sin(Time.time * 2f) * bobPixels) / 16f;
        transform.localPosition = basePos + new Vector3(0f, bob, 0f);

        if (glow != null)
        {
            float a = 0.55f + 0.35f * Mathf.Sin(Time.time * 5f);
            if (Random.value < 0.02f) a = 0.1f;            // creepy flicker now and then
            glow.color = new Color(1f, 1f, 1f, a);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used || !other.CompareTag("Player")) return;
        used = true;
        GameAudio.Play("glitch_blip", 0.7f);
        if (cutscene != null) cutscene.Begin(this);
    }

    // Called by the cutscene: the "?" glitches out and disappears.
    public IEnumerator GlitchOut(Sprite[] frames)
    {
        GameAudio.Play("glitch_burst", 0.5f);
        frozen = true;
        var q = spinPart != null ? spinPart.GetComponent<SpriteRenderer>() : null;
        if (spinPart != null) spinPart.localScale = Vector3.one;
        if (frames != null && q != null)
        {
            foreach (var f in frames)
            {
                q.sprite = f;
                transform.localPosition = basePos + new Vector3(Random.Range(-2, 3) / 16f, 0f, 0f);
                if (glow != null) glow.color = new Color(1f, 1f, 1f, Random.value);
                yield return new WaitForSeconds(0.08f);
            }
        }
        gameObject.SetActive(false);
    }
}
