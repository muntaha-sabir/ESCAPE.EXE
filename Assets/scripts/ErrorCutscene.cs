using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Goes on: the LevelFX object in Level 1. Started when Pip touches the red "?".
// What happens (all automatic, except clicking the X's):
//  1. Pip stops (the player can't move him any more) and a little "?" bubble pops over his head.
//  2. ERROR pop-up #1 appears. Player clicks X -> ERROR #2 appears. Click X -> lots more pop up at once.
//  3. In the background the "?" glitches away, a zig-zag crack opens under Pip, and he falls through.
//  4. A giant SYSTEM ERROR covers the screen, then Sam's screen goes black with a glitch -> Level 2.
// If nobody clicks, each pop-up closes itself after "Auto Close After" seconds, so it never gets stuck.
public class ErrorCutscene : MonoBehaviour
{
    [Header("Pip and the ?")]
    public Transform pip;
    public Sprite[] confusedBubbleFrames;
    public Sprite[] alarmedBubbleFrames;      // "!!!" bubble while Pip falls
    public Sprite[] questionGlitchFrames;

    [Header("Pop-ups")]
    public ErrorPopup firstError;
    public ErrorPopup secondError;
    public ErrorPopup[] moreErrors;
    public ErrorPopup bigError;
    public float autoCloseAfter = 6f;
    public float moreErrorsGap = 0.22f;

    [Header("Crack")]
    public Sprite[] crackFrames;
    public float groundTop = -4.4375f;

    [Header("Look")]
    public SpriteRenderer glitchOverlay;
    public Sprite[] glitchFrames;

    [Header("Sounds (all optional)")]
    public AudioSource popupSound;
    public AudioSource glitchSound;

    [Header("Captions (outside the computer)")]
    public string touchStory = "[...]";               // story line when Pip touches the "?"
    public string firstErrorHint = "click x to close";
    public float hintIfStuckAfter = 1.5f;

    [Header("Next level")]
    public string nextScene = "Level2_Desktop";
    public float blackoutTime = 0.5f;
    public string[] blackScreenLines = { "[it's still full of bugs.]", "[fix it... or delete it all.]" };
    public float blackLineHold = 1.6f;

    MysteryBox mystery;
    bool running;
    float glitchChance, shake;
    Transform cam;
    Vector3 camBase;
    int order = 10;
    Sprite[] bubbleFrames;
    SpriteRenderer bubbleSR;

    void Awake()
    {
        Hide(firstError); Hide(secondError); Hide(bigError);
        foreach (var e in moreErrors) Hide(e);
        if (glitchOverlay != null) glitchOverlay.enabled = false;
    }

    static void Hide(ErrorPopup e) { if (e != null) e.gameObject.SetActive(false); }

    public void Begin(MysteryBox fromBox)
    {
        if (running) return;
        running = true;
        mystery = fromBox;
        cam = Camera.main.transform;
        camBase = cam.position;
        StartCoroutine(Run());
    }

    void LateUpdate()
    {
        if (!running || cam == null) return;
        if (shake > 0f)
        {
            Vector2 r = Random.insideUnitCircle * shake;
            cam.position = camBase + new Vector3(r.x, r.y, 0f);
            shake = Mathf.Max(0f, shake - Time.deltaTime * 0.8f);
        }
        else cam.position = camBase;
    }

    IEnumerator Run()
    {
        // 1. Pip stops and looks confused
        var pc = pip.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;
        var rb = pip.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        var bubble = new GameObject("ConfusedBubble");
        bubble.transform.position = pip.position + new Vector3(0f, 2.2f, 0f);
        bubbleSR = bubble.AddComponent<SpriteRenderer>();
        bubbleSR.sortingLayerName = "Pip";
        bubbleSR.sortingOrder = 5;
        bubbleFrames = confusedBubbleFrames;
        StartCoroutine(Loop(bubbleSR, 3f));
        Narrator.Story(touchStory, 3f);
        GameAudio.MusicPitch(0.86f, 0.8f);                 // the happy music goes a bit wrong

        glitchChance = 0.08f;
        StartCoroutine(GlitchLoop());
        yield return new WaitForSeconds(0.7f);

        // 2. pop-up 1, wait for the X
        yield return ShowAndWait(firstError, firstErrorHint);
        glitchChance = 0.2f; shake = 0.12f;
        yield return new WaitForSeconds(0.3f);

        // pop-up 2, wait for the X
        yield return ShowAndWait(secondError);
        glitchChance = 0.4f; shake = 0.2f;
        if (glitchSound != null) glitchSound.Play();
        GameAudio.Play("glitch_burst", 0.45f);
        yield return new WaitForSeconds(0.25f);

        // ...then lots more
        foreach (var e in moreErrors)
        {
            Show(e);
            yield return new WaitForSeconds(moreErrorsGap);
        }
        yield return new WaitForSeconds(0.3f);

        // 3. the "?" glitches away
        float crackX = pip.position.x;
        if (mystery != null)
        {
            crackX = (pip.position.x + mystery.transform.position.x) * 0.5f;
            yield return mystery.GlitchOut(questionGlitchFrames);
        }
        yield return new WaitForSeconds(0.25f);

        // the crack opens by itself - Pip's bubble turns into "!!!" and falls with him
        if (alarmedBubbleFrames != null && alarmedBubbleFrames.Length > 0) bubbleFrames = alarmedBubbleFrames;
        bubble.transform.SetParent(pip, true);
        var crack = new GameObject("Crack");
        crack.transform.position = new Vector3(crackX, groundTop - 1f, 0f);
        var csr = crack.AddComponent<SpriteRenderer>();
        csr.sortingLayerName = "Default";
        csr.sortingOrder = 5;
        shake = 0.35f; glitchChance = 0.6f;
        for (int i = 0; i < crackFrames.Length; i++)
        {
            csr.sprite = crackFrames[i];
            if (i == 0) GameAudio.Play("pipexe_crack", 0.8f);
            if (i == 2) DropPip(rb);                // Pip falls once the crack is open
            yield return new WaitForSeconds(0.08f);
        }
        if (crackFrames.Length < 3) DropPip(rb);
        yield return new WaitForSeconds(0.35f);

        // 4. giant error, then black
        Show(bigError, 300);
        GameAudio.Play("error_beep", 0.7f, 0f); GameAudio.Play("glitch_burst", 0.7f); GameAudio.TapeStop(1.3f);
        glitchChance = 0.85f; shake = 0.25f;
        yield return new WaitForSeconds(1.1f);
        yield return Blackout();
        SceneManager.LoadScene(nextScene);
    }

    void DropPip(Rigidbody2D rb)
    {
        foreach (var c in pip.GetComponentsInChildren<Collider2D>()) c.enabled = false;
        var psr = pip.GetComponent<SpriteRenderer>();
        if (psr != null) { psr.sortingLayerName = "Background"; psr.sortingOrder = 5; }   // drops BEHIND the ground
        if (bubbleSR != null) { bubbleSR.sortingLayerName = "Background"; bubbleSR.sortingOrder = 6; }
        if (rb != null) rb.linearVelocity = new Vector2(0f, 3f);                          // tiny "whoa" hop
        GameAudio.Play("fall_whistle", 0.6f, 0f);
    }

    void Show(ErrorPopup e, int forcedOrder = -1)
    {
        if (e == null) return;
        e.Open(forcedOrder > 0 ? forcedOrder : order);
        order += 10;
        if (popupSound != null) popupSound.Play();
    }

    IEnumerator ShowAndWait(ErrorPopup e, string hintIfStuck = null)
    {
        if (e == null) yield break;
        Show(e);
        float t = 0f;
        bool hinted = false;
        while (e.IsOpen && t < autoCloseAfter)
        {
            t += Time.deltaTime;
            if (!hinted && !string.IsNullOrEmpty(hintIfStuck) && t > hintIfStuckAfter)
            {
                hinted = true;
                Narrator.Hint(hintIfStuck, Narrator.Icon.Mouse, 30f);
            }
            yield return null;
        }
        if (hinted) Narrator.ClearHint();
        if (e.IsOpen) e.Close();
        yield return new WaitForSeconds(0.25f);
    }

    IEnumerator Blackout()
    {
        GameAudio.Play("monitor_off", 0.7f, 0f);
        running = false;
        if (cam != null) cam.position = camBase;

        var black = new GameObject("Blackout");
        black.transform.position = new Vector3(0f, 0.5625f, 0f);     // Sam's monitor screen
        black.transform.localScale = new Vector3(26f, 14f, 1f);
        var bsr = black.AddComponent<SpriteRenderer>();
        bsr.sprite = FX.WhitePixel;
        bsr.color = Color.black;
        bsr.sortingLayerName = "UI";
        bsr.sortingOrder = 1000;

        if (glitchOverlay != null) glitchOverlay.sortingOrder = 1010;
        float t = 0f;
        while (t < blackoutTime)
        {
            t += 0.05f;
            if (glitchOverlay != null && glitchFrames.Length > 0)
            {
                glitchOverlay.enabled = Random.value < 0.35f;
                glitchOverlay.sprite = glitchFrames[Random.Range(0, glitchFrames.Length)];
                glitchOverlay.color = new Color(1f, 1f, 1f, 0.35f);
            }
            yield return new WaitForSeconds(0.05f);
        }
        if (glitchOverlay != null) glitchOverlay.enabled = false;

        // the screen stays black and the narration types out, then the next scene powers on
        yield return new WaitForSeconds(0.5f);
        foreach (var line in blackScreenLines)
        {
            Narrator.Story(line, blackLineHold);
            yield return new WaitForSeconds(line.Length * 0.04f + blackLineHold + 0.8f);
        }
        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator GlitchLoop()
    {
        if (glitchOverlay == null || glitchFrames.Length == 0) yield break;
        glitchOverlay.sortingOrder = 500;                 // glitch lines go over the pop-ups too
        Vector3 basePos = glitchOverlay.transform.localPosition;
        while (running)
        {
            glitchOverlay.enabled = Random.value < glitchChance;
            if (glitchOverlay.enabled && Random.value < 0.3f) GameAudio.Play("glitch_blip", 0.25f, 0.25f);
            glitchOverlay.sprite = glitchFrames[Random.Range(0, glitchFrames.Length)];
            glitchOverlay.transform.localPosition = basePos + new Vector3(Random.Range(-3, 4) / 16f, 0f, 0f);
            yield return new WaitForSeconds(Random.Range(0.04f, 0.12f));
        }
    }

    IEnumerator Loop(SpriteRenderer sr, float fps)
    {
        int i = 0;
        while (sr != null)
        {
            if (bubbleFrames != null && bubbleFrames.Length > 0) sr.sprite = bubbleFrames[i++ % bubbleFrames.Length];
            yield return new WaitForSeconds(1f / fps);
        }
    }
}
