using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Goes on: the "Level4Director" object. Runs the Recycle Bin level:
//  1. The bin opens. Pip wakes up and blinks twice.  "[Sam...deleted Pip..?]"
//  2. ALARM: Sam is emptying the bin. Red lights, the camera zooms in, a 15 second timer in the toolbar.
//     "[Don't give up. Help Pip escape! (Dont forget to get the last letter tho)]"  - old files vanish as time runs.
//  3. Player drags jump_fix.zip, Pip jumps onto Pip_sketch.old and gets the P. The timer stops.
//  4. P!P.exe glitches into view and the restore pop-up appears with a 3 second clock:
//       RESTORE EVERYTHING  -> P!P.exe comes back too (secretly)  -> Level 5
//       RESTORE PIP ONLY    -> only Pip                           -> Level 5
//       time runs out       -> everything is deleted, Pip too     -> sad ending
public class Level4Director : MonoBehaviour
{
    static Level4Director I;

    [Header("Pip waking up")]
    public Transform pip;
    public Sprite[] blinkFrames;              // 0 open, 1 half, 2 closed
    public float wakeDelay = 1.2f;

    [Header("Narration (exact text)")]
    public string line1 = "[Sam...deleted Pip..?]";
    public float line1Hold = 3f;
    public float gapBetweenLines = 1f;
    public string line2 = "[Don't give up. Help Pip escape! (Dont forget to get the last letter tho)]";

    [Header("Alarm")]
    public SpriteRenderer window;
    public Sprite alarmWindow;
    public SpriteRenderer redLights;
    public float lightsPulseSpeed = 1.3f;
    public float zoomSize = 7.8125f;          // 8.4375 = normal view, smaller = closer
    public Vector2 zoomCenter = new Vector2(0f, -0.5625f);
    public float zoomTime = 0.8f;

    [Header("Timer")]
    public float mainTime = 15f;              // to get the P
    public float popupTime = 3f;              // to choose in the pop-up
    public Sprite[] digitSprites;             // 0-9 and ':'
    public Vector2 digitsPos = new Vector2(9.0625f, 5.8125f);
    public float barLeft = -5.0625f, barWidth = 13.625f, barY = 5.78125f;
    public SpriteRenderer[] oldFiles;         // vanish one by one as the bar fills
    public Sprite[] poofFrames;

    [Header("Pip_sketch.old")]
    public SpriteRenderer sketch;
    public FrameLoop sketchGlow;
    public Sprite sketchTaken;
    public SpriteRenderer jumpFix;

    [Header("P!P.exe + restore pop-up")]
    public GameObject pipExe;                 // hidden until Pip has the P
    public GameObject popup;
    public SpriteRenderer btnEverything, btnPipOnly, popupClock;
    public Sprite[] everythingStates, pipOnlyStates, clockFrames;
    public Vector2 popupBarLocal = new Vector2(-3.375f, -1.90625f);
    public float popupBarWidth = 7.6875f;

    [Header("If time runs out: Pip is deleted piece by piece")]
    public Sprite[] confusedBubble;           // "?" over his head
    public Sprite[] exclaimBubble;            // "!" over his head
    public Sprite[] pipGlitchFrames;
    public Sprite[] pipDeleteFrames;          // pieces falling apart, top to bottom

    [Header("Restore / delete")]
    public Sprite[] pipRestoreFrames;         // Pip glitching (also used when he is deleted)
    public Sprite[] sparkleFrames;
    public SpriteRenderer[] deskLetters;
    public Transform dust;
    public float fadeTime = 1.2f;
    public string nextScene = "Level5_Notepad";
    public string sadScene = "Ending_Freedom"; // placeholder scene for the sad ending (we'll build it next)

    PlayerController pc; Rigidbody2D rb; SpriteRenderer pipSR; PipAnimator anim;
    Camera cam; Behaviour pixelPerfect; float camSize0; Vector3 camPos0;
    SpriteRenderer barFill, popupBar; SpriteRenderer[] digits = new SpriteRenderer[4];
    float timeLeft, popupLeft; bool timerOn, popupOn, finished, lightsOn;
    int lastTick = 99, lastPopTick = 99;
    Vector3 dustBase;

    void Awake() { I = this; }

    IEnumerator Start()
    {
        pc = pip.GetComponent<PlayerController>(); rb = pip.GetComponent<Rigidbody2D>();
        pipSR = pip.GetComponent<SpriteRenderer>(); anim = pip.GetComponent<PipAnimator>();
        cam = Camera.main; camSize0 = cam.orthographicSize; camPos0 = cam.transform.position;
        pixelPerfect = cam.GetComponent("PixelPerfectCamera") as Behaviour;
        if (pipExe != null) pipExe.SetActive(false);
        if (popup != null) popup.SetActive(false);
        if (redLights != null) redLights.enabled = false;
        if (dust != null) dustBase = dust.localPosition;
        timeLeft = mainTime;

        // Pip lies still with his eyes closed, then wakes up and blinks twice
        pc.enabled = false;
        if (anim != null) anim.enabled = false;
        pipSR.sprite = blinkFrames[2];
        yield return new WaitForSeconds(wakeDelay);
        yield return Eyes(2, 0.7f); yield return Eyes(1, 0.09f); yield return Eyes(0, 0.5f);
        for (int i = 0; i < 2; i++)
        {
            yield return Eyes(1, 0.07f); yield return Eyes(2, 0.11f); yield return Eyes(1, 0.07f);
            yield return Eyes(0, i == 0 ? 0.26f : 0.2f);
        }
        if (anim != null) anim.enabled = true;

        Narrator.Story(line1, line1Hold);
        yield return new WaitForSeconds(line1.Length * 0.04f + line1Hold + 0.6f + gapBetweenLines);

        // ALARM
        if (window != null && alarmWindow != null) window.sprite = alarmWindow;
        GameAudio.Play("evil_sting", 0.6f, 0f);
        StartCoroutine(TimerAlert());
        MakeTimerUI();
        lightsOn = true; if (redLights != null) redLights.enabled = true;
        StartCoroutine(Zoom(zoomSize, new Vector3(zoomCenter.x, zoomCenter.y, camPos0.z), zoomTime, false));
        pc.enabled = true;
        Narrator.Story(line2, 600f);
        yield return new WaitForSeconds(line2.Length * 0.04f + 0.3f);   // timer starts once the line is typed
        if (!finished) { timerOn = true; GameAudio.Layer("timer_recycle", 0.75f); GameAudio.MusicVolume(0.85f); }
    }

    // a quick red "! TIMER !" sign blinks twice so the player knows a timer is coming
    IEnumerator TimerAlert()
    {
        var sp = Resources.Load<Sprite>("GameArt/alert_timer");
        if (sp == null) yield break;
        var go = new GameObject("TimerAlert");
        go.transform.position = new Vector3(0f, 3.5f, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp; sr.sortingLayerName = "UI"; sr.sortingOrder = 990;
        for (int i = 0; i < 2; i++)
        {
            sr.enabled = true; go.transform.localScale = Vector3.one * 1.15f;
            GameAudio.Play("tick", 1f, 0f, 0.8f);
            yield return new WaitForSeconds(0.08f); go.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.2f);
            sr.enabled = false;
            yield return new WaitForSeconds(0.12f);
        }
        Destroy(go);
    }

    IEnumerator Eyes(int f, float t) { pipSR.sprite = blinkFrames[f]; yield return new WaitForSeconds(t); }

    void MakeTimerUI()
    {
        barFill = NewRenderer("TimerBar", new Vector3(barLeft, barY, 0f), "UI", 905, null);
        barFill.color = new Color(1f, 0.23f, 0.31f);
        for (int i = 0; i < 4; i++)
            digits[i] = NewRenderer("Digit" + i, new Vector3(digitsPos.x + new float[] { 0f, 0.375f, 0.8125f, 1.3125f }[i], digitsPos.y, 0f), "UI", 906, null);
        ShowTime(mainTime);
        SetBar(barFill, barLeft, barWidth, 0.5f, 0f);
    }

    SpriteRenderer NewRenderer(string n, Vector3 pos, string layer, int order, Transform parent)
    {
        var go = new GameObject(n);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        var r = go.AddComponent<SpriteRenderer>();
        r.sortingLayerName = layer; r.sortingOrder = order;
        return r;
    }

    void SetBar(SpriteRenderer r, float left, float width, float height, float k)
    {
        if (r.sprite == null) r.sprite = FX.WhitePixel;
        float w = Mathf.Max(0f, width * Mathf.Clamp01(k));
        r.enabled = w > 0.01f;
        r.transform.localScale = new Vector3(w, height, 1f);
        var p = r.transform.localPosition; p.x = left + w * 0.5f; r.transform.localPosition = p;
    }

    void ShowTime(float t)
    {
        int s = Mathf.Clamp(Mathf.CeilToInt(t), 0, 59);
        int[] idx = { s / 60, 10, (s % 60) / 10, s % 10 };
        for (int i = 0; i < 4; i++) digits[i].sprite = digitSprites[idx[i]];
    }

    void Update()
    {
        if (dust != null) dust.localPosition = dustBase + new Vector3(Mathf.Sin(Time.time * 0.3f) * 0.25f, Mathf.Sin(Time.time * 0.21f) * 0.15f, 0f);
        if (redLights != null && lightsOn) redLights.color = new Color(1f, 1f, 1f, 0.35f + 0.65f * (0.5f + 0.5f * Mathf.Sin(Time.time * lightsPulseSpeed * Mathf.PI * 2f)));

        if (timerOn && !finished)
        {
            timeLeft -= Time.deltaTime;
            int sec = Mathf.CeilToInt(timeLeft);
            if (sec != lastTick && sec >= 1 && sec <= 5) { lastTick = sec; GameAudio.Play("tick", 0.9f, 0f); }
            float k = 1f - Mathf.Clamp01(timeLeft / mainTime);
            SetBar(barFill, barLeft, barWidth, 0.5f, k);
            ShowTime(timeLeft);
            for (int i = 0; i < oldFiles.Length; i++)
                if (oldFiles[i] != null && oldFiles[i].enabled && k >= (i + 1f) / (oldFiles.Length + 1f)) Vanish(oldFiles[i]);
            if (timeLeft <= 0f) StartCoroutine(TimeUp());
        }

        if (popupOn && !finished)
        {
            popupLeft -= Time.deltaTime;
            int psec = Mathf.CeilToInt(popupLeft);
            if (psec != lastPopTick && psec >= 1) { lastPopTick = psec; GameAudio.Play("tick", 1f, 0f, 1.25f); }
            float k = 1f - Mathf.Clamp01(popupLeft / popupTime);
            SetBar(popupBar, popupBarLocal.x, popupBarWidth, 0.25f, k);
            popupClock.sprite = clockFrames[Mathf.Clamp(Mathf.RoundToInt(k * (clockFrames.Length - 1)), 0, clockFrames.Length - 1)];
            bool e = Button(btnEverything, everythingStates), p = Button(btnPipOnly, pipOnlyStates);
            if (e) StartCoroutine(Restore(true));
            else if (p) StartCoroutine(Restore(false));
            else if (popupLeft <= 0f) StartCoroutine(TimeUp());
        }
    }

    bool Button(SpriteRenderer b, Sprite[] states)
    {
        if (Mouse.current == null || cam == null) return false;
        Vector2 sp = Mouse.current.position.ReadValue();
        Vector3 mp = cam.ScreenToWorldPoint(new Vector3(sp.x, sp.y, 10f));
        Bounds r = b.bounds;
        bool over = mp.x >= r.min.x && mp.x <= r.max.x && mp.y >= r.min.y && mp.y <= r.max.y;
        b.sprite = states[over ? (Mouse.current.leftButton.isPressed ? 2 : 1) : 0];
        if (over && Mouse.current.leftButton.wasPressedThisFrame) { b.sprite = states[2]; return true; }
        return false;
    }

    void Vanish(SpriteRenderer r)
    {
        if (r == null || !r.enabled) return;
        GameAudio.Play("poof", 0.35f, 0.12f);
        FX.PlayOnce(poofFrames, r.transform.position + new Vector3(0f, 0.3f, 0f));
        r.enabled = false;
        foreach (var c in r.GetComponentsInChildren<SpriteRenderer>()) c.enabled = false;
    }

    // ---------- called by LetterGiver when Pip lands on Pip_sketch.old ----------
    public static void LetterTaken()
    {
        if (I == null || I.finished || I.popupOn) return;
        I.StartCoroutine(I.GotP());
    }

    IEnumerator GotP()
    {
        timerOn = false;                                     // the main timer freezes
        GameAudio.Layer("", 0f); GameAudio.MusicVolume(0.6f);
        if (sketchGlow != null) sketchGlow.enabled = false;
        if (sketch != null && sketchTaken != null) sketch.sprite = sketchTaken;
        if (pipExe != null) pipExe.SetActive(true);          // P!P.exe starts glitching in and out
        GameAudio.Play("glitch_blip", 0.5f);
        yield return new WaitForSeconds(0.6f);
        foreach (var l in deskLetters) if (l != null) l.color = new Color(1f, 0.92f, 0.55f);
        yield return new WaitForSeconds(0.12f);
        foreach (var l in deskLetters) if (l != null) l.color = Color.white;
        if (finished) yield break;

        popup.SetActive(true);
        GameAudio.Play("pop_up", 0.8f);
        popupBar = NewRenderer("PopupBar", new Vector3(popupBarLocal.x, popupBarLocal.y, 0f), "UI", 953, popup.transform);
        popupBar.color = new Color(1f, 0.23f, 0.31f);
        SetBar(popupBar, popupBarLocal.x, popupBarWidth, 0.25f, 0f);
        Vector3 s = popup.transform.localScale; float t = 0f;
        while (t < 0.28f) { t += Time.deltaTime; popup.transform.localScale = s * Mathf.Lerp(0.3f, 1f, Mathf.SmoothStep(0f, 1f, t / 0.28f)); yield return null; }
        popup.transform.localScale = s;
        popupLeft = popupTime;
        popupOn = true;
    }

    IEnumerator Restore(bool everything)
    {
        finished = true; popupOn = false;
        GameState.pipExeRestored = everything;               // decides the ending later
        GameAudio.Play("restore", 0.8f, 0f); GameAudio.Music("", 0f, 1.2f);
        yield return new WaitForSeconds(0.1f);
        popup.SetActive(false);
        Narrator.ClearStory(); Narrator.ClearHint();
        pc.enabled = false; if (anim != null) anim.enabled = false;
        rb.linearVelocity = Vector2.zero; rb.simulated = false;
        StartCoroutine(LightsOff());
        StartCoroutine(Zoom(camSize0, camPos0, zoomTime, true));

        Vector3 p0 = pip.position;
        FX.PlayOnce(sparkleFrames, p0 + new Vector3(0f, 1f, 0f), 16f, "UI", 60);
        for (int k = 0; k < 4; k++) { pipSR.color = (k % 2 == 0) ? new Color(1f, 0.95f, 0.6f) : Color.white; yield return new WaitForSeconds(0.08f); }
        pipSR.color = Color.white;
        foreach (var f in pipRestoreFrames)
        {
            pipSR.sprite = f; pip.position = p0 + new Vector3(Random.Range(-1, 2) / 16f, 0.875f, 0f);
            yield return new WaitForSeconds(0.09f);
        }
        pipSR.enabled = false;
        FX.PlayOnce(sparkleFrames, p0 + new Vector3(0f, 0.8f, 0f), 16f, "UI", 60);
        if (everything)                                       // everything goes - P!P.exe too, very quietly
        {
            foreach (var r in oldFiles) Vanish(r);
            if (pipExe != null) pipExe.SetActive(false);
        }
        yield return new WaitForSeconds(0.3f);
        yield return Flash(true, nextScene);
    }

    IEnumerator TimeUp()
    {
        if (finished) yield break;
        finished = true; timerOn = false; popupOn = false;
        GameAudio.Layer("", 0f); GameAudio.Play("error_beep", 0.8f, 0f); GameAudio.TapeStop(1.6f);
        if (popup != null) popup.SetActive(false);
        if (digits[0] != null) ShowTime(0f);
        if (barFill != null) SetBar(barFill, barLeft, barWidth, 0.5f, 1f);
        Narrator.ClearStory(); Narrator.ClearHint();
        pc.enabled = false; if (anim != null) anim.enabled = false;
        rb.linearVelocity = Vector2.zero;

        // Pip looks around, confused, while the files around him disappear...
        var bubble = NewRenderer("PipBubble", pip.position + new Vector3(0.25f, 2.2f, 0f), "Pip", 5, null);
        int k = 0;
        foreach (var r in oldFiles)
        {
            Vanish(r);
            for (int i = 0; i < 2; i++) { bubble.sprite = confusedBubble[k++ % confusedBubble.Length]; pipSR.flipX = !pipSR.flipX; yield return new WaitForSeconds(0.12f); }
        }
        Vanish(jumpFix); yield return new WaitForSeconds(0.25f);
        Vanish(sketch); pipSR.flipX = false; yield return new WaitForSeconds(0.35f);

        // ...gets a fright...
        rb.simulated = false;
        Vector3 p0 = pip.position;
        for (int i = 0; i < 4; i++) { bubble.sprite = exclaimBubble[i % exclaimBubble.Length]; pip.position = p0 + new Vector3(0f, i % 2 == 0 ? 0.125f : 0f, 0f); yield return new WaitForSeconds(0.12f); }
        pip.position = p0;

        // ...glitches...
        GameAudio.Play("glitch_burst", 0.8f);
        for (int i = 0; i < 7; i++)
        {
            pipSR.sprite = pipGlitchFrames[i % pipGlitchFrames.Length];
            pip.position = p0 + new Vector3(Random.Range(-1, 2) / 16f, 0f, 0f);
            yield return new WaitForSeconds(0.07f);
        }
        pip.position = p0;

        // ...and is deleted, piece by piece
        GameAudio.Play("delete_zap", 0.8f, 0f);
        for (int i = 0; i < pipDeleteFrames.Length; i++)
        {
            pipSR.sprite = pipDeleteFrames[i];
            if (i == 3) bubble.enabled = false;
            yield return new WaitForSeconds(0.12f);
        }
        pipSR.enabled = false; bubble.enabled = false;
        yield return new WaitForSeconds(1.2f);              // only the empty bin is left (and P!P.exe, if it showed up)
        StartCoroutine(LightsOff());
        yield return Flash(false, sadScene, 2f);
    }

    IEnumerator LightsOff()
    {
        lightsOn = false; if (redLights == null) yield break;
        float a = redLights.color.a, t = 0f;
        while (t < 0.6f) { t += Time.deltaTime; redLights.color = new Color(1f, 1f, 1f, Mathf.Lerp(a, 0f, t / 0.6f)); yield return null; }
        redLights.enabled = false;
    }

    IEnumerator Zoom(float size, Vector3 pos, float time, bool restorePixelPerfect)
    {
        if (pixelPerfect != null) pixelPerfect.enabled = false;
        float s0 = cam.orthographicSize; Vector3 p0 = cam.transform.position; float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime; float k = Mathf.SmoothStep(0f, 1f, t / time);
            cam.orthographicSize = Mathf.Lerp(s0, size, k);
            cam.transform.position = Vector3.Lerp(p0, pos, k);
            yield return null;
        }
        cam.orthographicSize = size; cam.transform.position = pos;
        if (restorePixelPerfect && pixelPerfect != null) pixelPerfect.enabled = true;
    }

    IEnumerator Flash(bool white, string scene, float time = -1f)
    {
        if (time < 0f) time = fadeTime;
        var cover = new GameObject("ScreenCover");
        cover.transform.position = new Vector3(0f, 0.5625f, 0f);
        cover.transform.localScale = new Vector3(26f, 14f, 1f);
        var csr = cover.AddComponent<SpriteRenderer>();
        csr.sprite = FX.WhitePixel; csr.sortingLayerName = "UI"; csr.sortingOrder = 1000;
        float t = 0f;
        float v = white ? 1f : 0f;
        while (t < 0.25f) { t += Time.deltaTime; csr.color = new Color(v, v, v, t / 0.25f); yield return null; }
        if (white)
        {
            t = 0f;
            while (t < time) { t += Time.deltaTime; float k = 1f - t / time; csr.color = new Color(k, k, k, 1f); yield return null; }
        }
        else yield return new WaitForSeconds(time);
        csr.color = Color.black;
        yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene(scene);
    }
}
