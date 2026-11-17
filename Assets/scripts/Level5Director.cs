using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Goes on: the "Level5Director" object. Runs Level 5 (back on Sam's desktop after the Recycle Bin):
//  1. The desktop comes back. Pip is out. (Is it really Pip..? Only if the player chose RESTORE EVERYTHING:
//     Pip_sketch.old is also back, and Pip / the screen glitch 3 times with vague narration.)
//  2. The player drags the NOTES down next to the file stairs. Pip climbs up onto the NOTES and can't leave it.
//  3. The H E L P letters on the desk glow. The player clicks them (any order) - each flies into the NOTES.
//     After 2 letters Sam types "What's that...?", after all 4 he goes back to "...".
//  4. Sam's cursor stops, slowly goes to the NOTES and maximizes it. Pip falls in, gets up, blinks.
//     Inside the big NOTES Pip can only jump.   (What happens next comes later.)
public class Level5Director : MonoBehaviour
{
    static Level5Director I;

    [Header("Testing")]
    public bool testAsRestoreEverything = false;   // tick to test the glitchy version without playing the Recycle Bin

    [Header("Pip")]
    public Transform pip;
    public Vector3 pipSpawn = new Vector3(-9.5f, -2.5f, 0f);
    public float startDelay = 1.4f;
    public Sprite[] poofFrames, sparkleFrames;
    public Sprite[] sitFrames;                     // 0 eyes closed, 1 half, 2 open
    public Sprite[] pipGlitchFrames;

    [Header("Only after RESTORE EVERYTHING")]
    public GameObject sketchFile;                  // Pip_sketch.old on the desktop
    public SpriteRenderer whatsThisLabel;
    public GameObject sketchViewer;
    public Vector2 viewerCloseLocal = new Vector2(3.9375f, 3.21875f);
    public string viewerLine = "[Aww...\u2661]";
    public Sprite[] screenGlitchFrames;
    public string glitchLine1 = "[...]";
    public string glitchLine2 = "[Is that a glitch..?]";
    public string glitchLine3 = "[???]";
    public float glitchLineHold = 1.5f;
    public float glitch3AfterGettingUp = 1f;

    [Header("The NOTES")]
    public SpriteRenderer notes;
    public Collider2D notesSolid;                  // switched on once the NOTES is in place
    public Collider2D[] lockWalls;                 // keep Pip on top of the NOTES
    public SpriteRenderer notesTarget;             // dotted "drag here"
    public Vector3 notesPlace = new Vector3(3.5f, -0.9375f, 0f);
    public float snapDistance = 6f;
    public NotesTyper samText;
    public string samIdle = "...";
    public string samNotices = "What's that...?";
    public string dragHint = "drag the notes down";
    public float dragHintAfter = 6f;

    [Header("H E L P")]
    public SpriteRenderer[] letters;               // on the desk, in order H E L P
    public SpriteRenderer[] letterGlows;
    public Vector2 lettersLocalStart = new Vector2(-1.71875f, -2.1875f);   // where H lands on the NOTES
    public float lettersSpacing = 1.3125f;
    public string lettersHint = "click the glowing letters";

    [Header("Sam maximizes the NOTES")]
    public SamCursor samCursor;
    public Vector2 maximizeButtonLocal = new Vector2(2.1875f, 2.78125f);
    public GameObject notesMax;
    public NotesTyper samTextMax;
    public float maximizeTime = 0.9f;
    public GameObject hideWhenMaximized;           // the desktop icons and stair files
    public string samRecognizes = "Pip..? Is that you?";

    [Header("Endings")]
    public string samHappyLine = "Okay, I'll give you a happy ending.";
    public float endingLineDelay = 1.8f;
    public string happyScene = "Ending_Return";     // RESTORE PIP ONLY
    public string evilScene = "Ending_Creator";     // RESTORE EVERYTHING

    [Header("Narration (leave empty for none)")]
    public string startLine = "";
    public string samNoticedLine = "";

    PlayerController pc; Rigidbody2D rb; SpriteRenderer pipSR; PipAnimator anim; Camera cam;
    bool evil, placed, dragging, onNotes, lettersOn, viewerOpen, cutscene, samBusy;
    bool[] used = new bool[4]; int count; bool labelDone, glitch1Done, maximized;
    Vector3 notesHome, grab;

    void Awake() { I = this; }

    IEnumerator Start()
    {
        evil = GameState.pipExeRestored || testAsRestoreEverything;
        pc = pip.GetComponent<PlayerController>(); rb = pip.GetComponent<Rigidbody2D>();
        pipSR = pip.GetComponent<SpriteRenderer>(); anim = pip.GetComponent<PipAnimator>(); cam = Camera.main;
        notesHome = notes.transform.position;
        if (sketchFile != null) sketchFile.SetActive(evil);
        if (sketchViewer != null) sketchViewer.SetActive(false);
        if (notesMax != null) notesMax.SetActive(false);
        if (notesSolid != null) notesSolid.enabled = false;
        foreach (var w in lockWalls) w.enabled = false;
        foreach (var g in letterGlows) g.enabled = false;
        pc.lockHorizontal = false; pc.enabled = false; pipSR.enabled = false; rb.simulated = false;
        StartCoroutine(SamSay(samText, samIdle));

        yield return new WaitForSeconds(startDelay);
        pip.position = pipSpawn;
        FX.PlayOnce(poofFrames, pipSpawn + new Vector3(0f, 0.8f, 0f));
        GameAudio.Play("poof", 0.7f); GameAudio.Play("land", 0.5f);
        pipSR.enabled = true; rb.simulated = true; rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.8f);
        pc.enabled = true;
        if (!string.IsNullOrEmpty(startLine)) Narrator.Story(startLine, 4f);

        yield return new WaitForSeconds(dragHintAfter);
        if (!placed && !dragging) Narrator.Hint(dragHint, Narrator.Icon.Mouse, 600f);
    }

    // ---------- the three small glitches (only after RESTORE EVERYTHING) ----------
    // 1: when Pip lands on the second stair file   2: while HELP is being placed   3: just after he gets up inside the big NOTES
    public static void PipOnSecondStair() { if (I != null) I.FirstGlitch(); }
    void FirstGlitch()
    {
        if (!evil || glitch1Done || cutscene) return;
        glitch1Done = true;
        Narrator.Story(glitchLine1, glitchLineHold);
        StartCoroutine(PipGlitch());
    }

    IEnumerator PipGlitch()
    {
        GameAudio.Play("glitch_blip", 0.6f);
        if (anim != null) anim.enabled = false;
        for (int i = 0; i < 5; i++) { pipSR.sprite = pipGlitchFrames[i % pipGlitchFrames.Length]; yield return new WaitForSeconds(0.07f); }
        if (anim != null && !cutscene) anim.enabled = true;
        else if (anim != null && maximized && pc.enabled) anim.enabled = true;
    }

    IEnumerator ScreenGlitch()
    {
        var go = new GameObject("ScreenGlitch");
        go.transform.position = new Vector3(0f, 0.5625f, 0f);
        var r = go.AddComponent<SpriteRenderer>();
        r.sortingLayerName = "UI"; r.sortingOrder = 990; r.color = new Color(1f, 1f, 1f, 0.45f);
        for (int i = 0; i < 4; i++) { r.sprite = screenGlitchFrames[i % screenGlitchFrames.Length]; yield return new WaitForSeconds(0.06f); }
        Destroy(go);
    }

    // ---------- Sam's line in the NOTES ----------
    IEnumerator SamSay(NotesTyper typer, string s)
    {
        while (samBusy) yield return null;
        samBusy = true;
        yield return typer.Backspace();
        yield return typer.Type(s);
        samBusy = false;
    }

    static bool Inside(SpriteRenderer r, Vector3 p)
    {
        Bounds b = r.bounds;
        return p.x >= b.min.x && p.x <= b.max.x && p.y >= b.min.y && p.y <= b.max.y;
    }

    void Update()
    {
        if (whatsThisLabel != null) whatsThisLabel.enabled = evil && !labelDone && Mathf.Repeat(Time.time, 1f) < 0.65f;
        if (lettersOn)
            for (int i = 0; i < 4; i++)
                if (!used[i]) letterGlows[i].color = new Color(1f, 1f, 1f, 0.25f + 0.75f * (0.5f + 0.5f * Mathf.Sin(Time.time * 5f + i)));

        var m = Mouse.current;
        if (m == null || cam == null || cutscene || maximized) return;
        Vector2 sp = m.position.ReadValue();
        Vector3 mp = cam.ScreenToWorldPoint(new Vector3(sp.x, sp.y, 10f)); mp.z = 0f;
        bool down = m.leftButton.wasPressedThisFrame;

        // the sketch viewer (close with X)
        if (viewerOpen)
        {
            Vector3 x = sketchViewer.transform.position + new Vector3(viewerCloseLocal.x, viewerCloseLocal.y, 0f);
            if (down && Mathf.Abs(mp.x - x.x) < 0.7f && Mathf.Abs(mp.y - x.y) < 0.6f) { sketchViewer.SetActive(false); viewerOpen = false; Narrator.ClearHint(); Narrator.ClearStory(); }
            return;
        }

        // drag the NOTES (grab it anywhere)
        if (!placed)
        {
            if (!dragging && down && Inside(notes, mp)) { dragging = true; grab = notes.transform.position - mp; Narrator.ClearHint(); return; }
            if (dragging)
            {
                Vector3 p = mp + grab;
                notes.transform.position = new Vector3(Mathf.Round(p.x * 16f) / 16f, Mathf.Round(p.y * 16f) / 16f, 0f);
                if (m.leftButton.wasReleasedThisFrame)
                {
                    dragging = false;
                    if (Vector2.Distance(notes.transform.position, notesPlace) <= snapDistance) StartCoroutine(PlaceNotes());
                    else StartCoroutine(Slide(notes.transform, notesHome, 0.2f));
                }
                return;
            }
        }

        // open Pip_sketch.old
        if (evil && down && sketchFile != null && Inside(sketchFile.GetComponent<SpriteRenderer>(), mp))
        {
            sketchViewer.SetActive(true); viewerOpen = true; labelDone = true;
            GameAudio.Play("pop_up", 0.7f); GameAudio.PlayDelayed("joy", 0.5f, 0.4f);
            StartCoroutine(PopViewer());
            Narrator.Story(viewerLine, 600f);                 // stays while the picture is open
            Narrator.Hint("click x to close", Narrator.Icon.Mouse, 600f);
            return;
        }

        // click the glowing letters
        if (lettersOn && down)
            for (int i = 0; i < 4; i++)
                if (!used[i] && (Inside(letters[i], mp) || Inside(letterGlows[i], mp))) { StartCoroutine(FlyLetter(i)); break; }
    }

    IEnumerator PopViewer()
    {
        Transform v = sketchViewer.transform; float t = 0f;
        while (t < 0.3f) { t += Time.deltaTime; v.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, Mathf.SmoothStep(0f, 1f, t / 0.3f)); yield return null; }
        v.localScale = Vector3.one;
    }

    IEnumerator Slide(Transform tr, Vector3 to, float time)
    {
        Vector3 from = tr.position; float t = 0f;
        while (t < time) { t += Time.deltaTime; tr.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / time)); yield return null; }
        tr.position = to;
    }

    IEnumerator PlaceNotes()
    {
        placed = true;
        yield return Slide(notes.transform, notesPlace, 0.15f);
        if (notesTarget != null) notesTarget.gameObject.SetActive(false);
        notesSolid.enabled = true;
        GameAudio.Play("snap", 0.8f);
        FX.PlayOnce(poofFrames, notesPlace + new Vector3(0f, -2.5f, 0f));
    }

    // ---------- called by Level5Zone when Pip lands on top of the NOTES ----------
    public static void PipOnNotes()
    {
        if (I == null || !I.placed || I.onNotes) return;
        I.onNotes = true;
        I.FirstGlitch();                                      // in case Pip skipped the second stair
        foreach (var w in I.lockWalls) w.enabled = true;      // Pip stays on the NOTES from now on
        I.lettersOn = true;
        foreach (var g in I.letterGlows) g.enabled = true;
        Narrator.Hint(I.lettersHint, Narrator.Icon.Mouse, 600f);
    }

    IEnumerator FlyLetter(int i)
    {
        GameAudio.Play("letter_collect", 0.6f, 0f);
        used[i] = true;
        letterGlows[i].enabled = false;
        Narrator.ClearHint();
        Transform l = letters[i].transform;
        l.SetParent(null, true);
        Vector3 a = l.position;
        Vector3 b = notes.transform.position + new Vector3(lettersLocalStart.x + lettersSpacing * i, lettersLocalStart.y, 0f);
        float t = 0f;
        while (t < 0.6f)
        {
            t += Time.deltaTime; float k = Mathf.SmoothStep(0f, 1f, t / 0.6f);
            Vector3 p = Vector3.Lerp(a, b, k); p.y += Mathf.Sin(k * Mathf.PI) * 1.5f;
            l.position = p;
            yield return null;
        }
        l.position = b;
        l.SetParent(notes.transform, true);
        letters[i].sortingLayerName = "Default"; letters[i].sortingOrder = notes.sortingOrder + 2;
        FX.PlayOnce(sparkleFrames, b, 18f, "UI", 60);
        // little key-click shake of the NOTES
        Vector3 n0 = notesPlace;
        notes.transform.position = n0 + new Vector3(0f, -1f / 16f, 0f);
        yield return new WaitForSeconds(0.06f);
        notes.transform.position = n0;

        count++;
        if (count == 1) StartCoroutine(CursorTwitch());
        else if (count == 2) StartCoroutine(SamSay(samText, samNotices));
        else if (count == 3 && evil) { Narrator.Story(glitchLine2, glitchLineHold); StartCoroutine(ScreenGlitch()); }
        if (count == 3) StartCoroutine(samCursor.MoveTipTo(Vector3.Lerp(samCursor.Tip, notes.transform.position + new Vector3(0f, 4.5f, 0f), 0.6f), 3f));
        else if (count == 4) StartCoroutine(Maximize());
    }

    IEnumerator CursorTwitch()
    {
        Transform c = samCursor.transform; Vector3 p = c.position;
        for (int i = 0; i < 4; i++) { c.position = p + new Vector3((i % 2 == 0 ? 2f : -2f) / 16f, 0f, 0f); yield return new WaitForSeconds(0.05f); }
        c.position = p;
    }

    // ---------- Sam notices, maximizes the NOTES, Pip falls in ----------
    IEnumerator Maximize()
    {
        yield return SamSay(samText, samIdle);              // back to "[Sam]: ..."
        if (!string.IsNullOrEmpty(samNoticedLine)) Narrator.Story(samNoticedLine, 3f);
        yield return new WaitForSeconds(0.9f);              // the cursor has stopped dead
        Vector3 btn = notes.transform.position + new Vector3(maximizeButtonLocal.x, maximizeButtonLocal.y, 0f);
        yield return samCursor.MoveTipTo(btn, 3.5f);        // ...slowly goes to the maximize button
        yield return new WaitForSeconds(0.4f);
        yield return samCursor.Click();

        cutscene = true; maximized = true;
        Narrator.ClearHint(); Narrator.ClearStory();
        pc.enabled = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (sketchViewer != null) sketchViewer.SetActive(false);

        // the NOTES grows until it fills the screen
        Vector3 from = notes.transform.position, to = notesMax.transform.position;
        notes.gameObject.SetActive(false);                  // its top is gone - Pip starts to fall
        notesMax.SetActive(true);
        GameAudio.Play("whoosh", 0.7f);
        float t = 0f;
        while (t < maximizeTime)
        {
            t += Time.deltaTime; float k = Mathf.SmoothStep(0f, 1f, t / maximizeTime);
            notesMax.transform.position = Vector3.Lerp(from, to, k);
            notesMax.transform.localScale = Vector3.one * Mathf.Lerp(0.25f, 1f, k);
            yield return null;
        }
        notesMax.transform.position = to; notesMax.transform.localScale = Vector3.one;
        if (hideWhenMaximized != null) hideWhenMaximized.SetActive(false);   // the files under the big NOTES are no longer solid

        // Pip falls to the bottom of the big NOTES, sits for a moment, blinks, gets up (no dizzy stars this time)
        t = 0f;
        while (t < 4f && !(t > 0.3f && Mathf.Abs(rb.linearVelocity.y) < 0.05f)) { t += Time.deltaTime; yield return null; }
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.enabled = false;
        pipSR.flipX = false;
        FX.PlayOnce(poofFrames, pip.position + new Vector3(0f, 0.3f, 0f));
        yield return Sit(0, 0.9f); yield return Sit(1, 0.08f); yield return Sit(2, 0.35f);
        yield return Sit(1, 0.07f); yield return Sit(0, 0.1f); yield return Sit(1, 0.07f); yield return Sit(2, 0.3f);
        if (anim != null) anim.enabled = true;

        cutscene = false;                                   // Pip is free to walk and jump inside the big NOTES
        pc.lockHorizontal = false;
        pc.enabled = true;
        if (evil) StartCoroutine(ThirdGlitch());

        // Sam's cursor slowly comes closer, as if recognizing him
        yield return new WaitForSeconds(0.4f);
        yield return samCursor.MoveTipTo(pip.position + new Vector3(1.6f, 1.4f, 0f), 2.2f);
        yield return SamSay(samTextMax, samRecognizes);

        // --- the endings split here
        yield return new WaitForSeconds(endingLineDelay);
        if (!evil)
        {
            yield return SamSay(samTextMax, samHappyLine);
            yield return new WaitForSeconds(2.2f);
            yield return FadeOutTo(happyScene);
        }
        else
        {
            yield return new WaitForSeconds(0.6f);
            yield return GlitchOutTo(evilScene);
        }
    }

    IEnumerator FadeOutTo(string scene)
    {
        GameAudio.Music("", 0f, 1.2f);
        Narrator.ClearStory(); Narrator.ClearHint();
        var black = new GameObject("ScreenOff");
        black.transform.position = new Vector3(0f, 0.5625f, 0f);
        black.transform.localScale = new Vector3(26f, 14f, 1f);
        var bsr = black.AddComponent<SpriteRenderer>();
        bsr.sprite = FX.WhitePixel;
        bsr.sortingLayerName = "UI";
        bsr.sortingOrder = 1000;
        for (float k = 0f; k < 1.4f; k += Time.deltaTime) { bsr.color = new Color(0f, 0f, 0f, k / 1.4f); yield return null; }
        bsr.color = Color.black;
        yield return new WaitForSeconds(0.6f);
        SceneManager.LoadScene(scene);
    }

    IEnumerator GlitchOutTo(string scene)
    {
        GameAudio.Play("glitch_burst", 0.8f); GameAudio.Music("", 0f, 0.3f);
        Narrator.ClearStory(); Narrator.ClearHint();
        var g = new GameObject("GlitchOut");
        g.transform.position = new Vector3(0f, 0.5625f, 0f);
        var gsr = g.AddComponent<SpriteRenderer>();
        gsr.sortingLayerName = "UI"; gsr.sortingOrder = 1000;
        for (int i = 0; i < 8; i++)
        {
            if (screenGlitchFrames != null && screenGlitchFrames.Length > 0) gsr.sprite = screenGlitchFrames[i % screenGlitchFrames.Length];
            yield return new WaitForSeconds(0.05f);
        }
        SceneManager.LoadScene(scene);
    }

    IEnumerator ThirdGlitch()
    {
        yield return new WaitForSeconds(glitch3AfterGettingUp);
        Narrator.Story(glitchLine3, glitchLineHold);
        yield return PipGlitch();
    }

    IEnumerator Sit(int f, float time) { pipSR.sprite = sitFrames[f]; yield return new WaitForSeconds(time); }
}
