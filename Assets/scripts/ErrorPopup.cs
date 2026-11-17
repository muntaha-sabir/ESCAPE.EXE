using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Goes on: each ERROR pop-up window in Level 1 (the ErrorCutscene opens them).
// The player clicks the X (or OK) with the mouse to close it.
// The X gently blinks so people know they can click it.
// The big final error uses "Closable" OFF and cycles through its glitch frames.
public class ErrorPopup : MonoBehaviour
{
    public bool closable = true;

    [Header("Buttons (child objects)")]
    public SpriteRenderer closeButton;
    public SpriteRenderer okButton;
    public Sprite xNormal, xHover, xPressed;
    public Sprite okNormal, okHover, okPressed;

    [Header("Glitchy look")]
    public Sprite[] glitchFrames;        // optional - the big error flickers through these
    public float glitchFps = 10f;
    public float jitterChance = 0.03f;   // chance per frame of a little sideways glitch

    public bool IsOpen { get; private set; }

    static readonly List<ErrorPopup> openOnes = new List<ErrorPopup>();

    SpriteRenderer sr;
    Vector3 basePos, baseScale;
    bool jittering;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        basePos = transform.localPosition;
        baseScale = transform.localScale;
    }

    void OnDestroy() { openOnes.Remove(this); }

    public void Open(int sortingOrder)
    {
        gameObject.SetActive(true);
        if (sr == null) Awake();
        IsOpen = true;
        openOnes.Remove(this);
        openOnes.Add(this);                       // newest window is on top and gets the clicks
        sr.sortingOrder = sortingOrder;
        if (closeButton != null) closeButton.sortingOrder = sortingOrder + 1;
        if (okButton != null) okButton.sortingOrder = sortingOrder + 1;
        StartCoroutine(PopIn());
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        openOnes.Remove(this);
        StartCoroutine(CloseAnim());
    }

    IEnumerator PopIn()
    {
        GameAudio.Play("pop_up", 0.65f, 0.08f);
        float t = 0f, d = 0.32f;
        while (t < d)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / d);
            float s = k < 0.7f ? Mathf.Lerp(0.3f, 1.05f, Mathf.SmoothStep(0f, 1f, k / 0.7f)) : Mathf.Lerp(1.05f, 1f, Mathf.SmoothStep(0f, 1f, (k - 0.7f) / 0.3f));
            transform.localScale = baseScale * s;
            yield return null;
        }
        transform.localScale = baseScale;
    }

    IEnumerator CloseAnim()
    {
        yield return new WaitForSeconds(0.08f);              // let the "pressed" button show
        for (int i = 0; i < 3; i++)
        {
            transform.localPosition = basePos + new Vector3(Random.Range(-3, 4) / 16f, 0f, 0f);
            sr.color = (i % 2 == 0) ? new Color(1f, 0.6f, 0.7f) : Color.white;
            yield return new WaitForSeconds(0.04f);
        }
        float t = 0f, d = 0.1f;
        while (t < d)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / d);
            transform.localScale = new Vector3(baseScale.x * (1f + 0.2f * k), baseScale.y * (1f - k), 1f);
            yield return null;
        }
        transform.localScale = baseScale;
        transform.localPosition = basePos;
        sr.color = Color.white;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!IsOpen) return;

        if (glitchFrames != null && glitchFrames.Length > 0)
            sr.sprite = glitchFrames[(int)(Time.time * glitchFps) % glitchFrames.Length];

        if (!jittering && Random.value < jitterChance) StartCoroutine(Jitter());

        if (!closable) return;

        bool onTop = openOnes.Count > 0 && openOnes[openOnes.Count - 1] == this;
        Vector2 m = MouseWorld();
        bool clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool held = Mouse.current != null && Mouse.current.leftButton.isPressed;

        if (Button(closeButton, xNormal, xHover, xPressed, m, onTop, clicked, held, true)) Close();
        else if (Button(okButton, okNormal, okHover, okPressed, m, onTop, clicked, held, false)) Close();
    }

    bool Button(SpriteRenderer b, Sprite normal, Sprite hover, Sprite pressed, Vector2 m, bool onTop, bool clicked, bool held, bool blinkHint)
    {
        if (b == null) return false;
        Bounds r = b.bounds;
        bool over = onTop && m.x >= r.min.x && m.x <= r.max.x && m.y >= r.min.y && m.y <= r.max.y;

        if (over)
        {
            b.sprite = held ? pressed : hover;
            if (clicked) { b.sprite = pressed; return true; }
        }
        else
        {
            bool blink = blinkHint && onTop && Mathf.Repeat(Time.time * 2f, 1f) < 0.5f;
            b.sprite = blink ? hover : normal;
        }
        return false;
    }

    IEnumerator Jitter()
    {
        jittering = true;
        transform.localPosition = basePos + new Vector3(Random.Range(-2, 3) / 16f, 0f, 0f);
        yield return new WaitForSeconds(0.05f);
        transform.localPosition = basePos;
        jittering = false;
    }

    static Vector2 MouseWorld()
    {
        if (Mouse.current == null || Camera.main == null) return new Vector2(9999f, 9999f);
        Vector2 p = Mouse.current.position.ReadValue();
        return Camera.main.ScreenToWorldPoint(new Vector3(p.x, p.y, 10f));
    }
}
