using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Goes on: the "Level2Director" object. Runs Level 2 (Sam's desktop) from start to end:
//  1. Pip falls out of Sam's game (SJQ3), lands on the taskbar, sits there dizzy, blinks awake and stands up.
//     Narration on landing, then when he can move, then when all 3 files are in place (see "Narration" below).
//  2. Player drags the glowing files into the "drag here" spots. Pip climbs and collects H, E, L.
//     On the first step Sam types in his NOTES: "Wait...there's something moving in my desktop!"
//     ...then deletes it and types "Is that a bug?? Im deleting it." (stays till the end)
//  3. Top step: reminder notification flashes in and out, the reminder pop-up appears where the notes were,
//     the notes jump aside. The pop-up stays until the player closes it.
//  4. Sam's red cursor races to Pip ("!!!"), right-click > delete. Pip is deleted. "ITEM DELETED" pop-up.
//  5. Player clicks dismiss -> screen slowly goes black -> next scene.
public class Level2Director : MonoBehaviour
{
    static Level2Director I;

    [Header("Pip")]
    public Transform pip;
    public Vector3 pipSpawn = new Vector3(-11.125f, 5f, 0f);    // inside the SJQ3 desktop icon
    public float startDelay = 1.8f;                             // waits for the screen to blink on
    public Vector2 popVelocity = new Vector2(-1f, 4f);          // how Pip pops out: slightly LEFT, so he lands clear of the narration box
    public Sprite[] poofFrames;
    public Sprite[] sparkleFrames;

    [Header("Sam's game glitching")]
    public SpriteRenderer sjqTaskbarSlot;
    public Sprite[] sjqGlitchFrames;
    public SpriteRenderer sjqDesktopIcon;

    [Header("Pip lands dizzy")]
    public Sprite[] sitFrames;                                  // 0 eyes closed, 1 half, 2 open
    public SpriteRenderer dizzySwirl;                           // spinning stars over his head
    public Vector3 swirlOffset = new Vector3(0f, 1.55f, 0f);
    public float dizzyTime = 2.4f;

    [Header("Narration (bottom box) and hints (bottom right)")]
    public string landLine = "[Pip fell out? The game doesn't have an ending for him... That's not fair.]";
    public float landHold = 2.5f;
    public string moveLine = "[Get Sam's attention. Drag the files in place to make a pathway for him.]";
    public string allPlacedLine = "[Get to the NOTES. Write a small message, get Sam's attention.]";
    public string lettersLine = "";                             // optional line after the first letter (empty = none)
    public string dragHint = "drag the glowing files";
    public float dragHintAfter = 9f;
    public string closeHint = "click dismiss";
    public float closeHintAfter = 3f;

    [Header("Sam's NOTES")]
    public NotesTyper samNotes;
    public string samLine1 = "Wait...there's something moving in my desktop!";
    public float samReadTime = 2.5f;                            // how long line 1 stays before Sam deletes it
    public string samLine2 = "Is that a bug?? Im deleting it.";
    public Transform notesWindow;
    public Vector3 notesMovedTo = new Vector3(9.75f, 3.9375f, 0f);

    [Header("Reminder")]
    public SpriteRenderer toast;
    public Vector3 toastShown = new Vector3(8.875f, -4.25f, 0f);
    public Vector3 toastHidden = new Vector3(18f, -4.25f, 0f);
    public float toastStay = 1.6f;
    public ErrorPopup reminderWindow;
    public AudioSource dingSound;            // optional

    [Header("Pip gets a fright")]
    public SpriteRenderer exclaimBubble;                        // single "!" when the reminder pops up
    public Vector3 bubbleOffset = new Vector3(0.25f, 2.2f, 0f);

    [Header("Sam deletes Pip")]
    public SamCursor samCursor;
    public float cursorDashSpeed = 30f;
    public SpriteRenderer alarmBubble;       // the "!!!" over Pip
    public SpriteRenderer deleteOption;      // the small right-click "delete"
    public Sprite optionNormal, optionPressed;
    public Sprite[] pipDeletedFrames;
    public SpriteRenderer recycleBin;
    public Sprite recycleBinFull;
    public ErrorPopup itemDeletedWindow;
    public AudioSource deleteSound;          // optional

    [Header("Ending")]
    public float fadeToBlackTime = 2.5f;
    public string nextScene = "Level3_Through";

    bool topReached, anyGrab, anyLetter, samDone;
    int placedCount;
    PipAnimator anim;
    PlayerController pc;
    Rigidbody2D rb;
    SpriteRenderer pipSR;

    void Awake()
    {
        I = this;
        if (toast != null) { toast.enabled = false; toast.transform.position = toastHidden; }
        if (deleteOption != null) deleteOption.gameObject.SetActive(false);
        if (alarmBubble != null) alarmBubble.gameObject.SetActive(false);
        if (dizzySwirl != null) dizzySwirl.gameObject.SetActive(false);
        if (exclaimBubble != null) exclaimBubble.gameObject.SetActive(false);
    }

    void Update()
    {
        if (dizzySwirl != null && dizzySwirl.gameObject.activeSelf) dizzySwirl.transform.position = pip.position + swirlOffset;
        if (exclaimBubble != null && exclaimBubble.gameObject.activeSelf) exclaimBubble.transform.position = pip.position + bubbleOffset;
    }

    IEnumerator Start()
    {
        pc = pip.GetComponent<PlayerController>();
        rb = pip.GetComponent<Rigidbody2D>();
        pipSR = pip.GetComponent<SpriteRenderer>();
        anim = pip.GetComponent<PipAnimator>();
        pc.enabled = false;
        pipSR.enabled = false;
        rb.simulated = false;

        yield return new WaitForSeconds(startDelay);

        // Sam's game glitches...
        GameAudio.Play("glitch_burst", 0.5f);
        for (int i = 0; i < 8; i++)
        {
            if (sjqTaskbarSlot != null && sjqGlitchFrames.Length > 1) sjqTaskbarSlot.sprite = sjqGlitchFrames[i % 2];
            if (sjqDesktopIcon != null) sjqDesktopIcon.transform.localPosition += new Vector3((i % 2 == 0 ? 1 : -1) / 16f, 0f, 0f);
            yield return new WaitForSeconds(0.07f);
        }
        if (sjqTaskbarSlot != null && sjqGlitchFrames.Length > 0) sjqTaskbarSlot.sprite = sjqGlitchFrames[0];

        // ...and Pip pops out of it
        pip.position = pipSpawn;
        FX.PlayOnce(poofFrames, pipSpawn + new Vector3(0f, 0.8f, 0f));
        GameAudio.Play("poof", 0.7f); GameAudio.Play("fall_whistle", 0.45f, 0f, 1.1f);
        pipSR.enabled = true;
        rb.simulated = true;
        rb.linearVelocity = popVelocity;

        // ...falls and lands on the taskbar
        float t = 0f;
        while (t < 4f && !(t > 0.3f && Mathf.Abs(rb.linearVelocity.y) < 0.05f)) { t += Time.deltaTime; yield return null; }
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.enabled = false;
        pipSR.flipX = false;
        pipSR.sprite = sitFrames[0];                        // sitting, eyes closed
        GameAudio.Play("land", 0.8f); GameAudio.PlayDelayed("dizzy", 0.25f, 0.5f);
        FX.PlayOnce(poofFrames, pip.position + new Vector3(0f, 0.3f, 0f));
        Narrator.Story(landLine, landHold);
        float landedAt = Time.time;
        if (dizzySwirl != null) { dizzySwirl.gameObject.SetActive(true); dizzySwirl.transform.position = pip.position + swirlOffset; }
        yield return new WaitForSeconds(dizzyTime);

        // blinks awake...
        yield return Sit(1, 0.08f); yield return Sit(2, 0.35f);
        yield return Sit(1, 0.07f); yield return Sit(0, 0.1f); yield return Sit(1, 0.07f); yield return Sit(2, 0.3f);
        // ...and stands up, no longer dizzy
        if (dizzySwirl != null) dizzySwirl.gameObject.SetActive(false);
        if (anim != null) anim.enabled = true;

        // control returns once the first narration line has finished
        float lineTime = landLine.Length * 0.04f + landHold + 0.6f;
        float left = lineTime - (Time.time - landedAt);
        if (left > 0f) yield return new WaitForSeconds(left);
        pc.enabled = true;
        Narrator.Story(moveLine, 600f);
        yield return new WaitForSeconds(dragHintAfter);
        if (!anyGrab) Narrator.Hint(dragHint, Narrator.Icon.Mouse, 8f);
    }

    // ---------- called by the other Level 2 scripts ----------
    public static void FileGrabbed() { if (I != null && !I.anyGrab) { I.anyGrab = true; Narrator.ClearHint(); } }
    public static void FilePlaced()
    {
        if (I == null) return;
        I.placedCount++;
        if (I.placedCount == 3) Narrator.Story(I.allPlacedLine, 600f);
    }
    public static void LetterCollected(char c)
    {
        if (I == null || I.anyLetter) return;
        I.anyLetter = true;                                     // Pip is on the first step
        if (!string.IsNullOrEmpty(I.lettersLine)) Narrator.Story(I.lettersLine, 5f);
        I.StartCoroutine(I.SamWrites());
    }
    public static void TopReached()
    {
        if (I == null || I.topReached) return;
        I.topReached = true;
        I.StartCoroutine(I.Finale());
    }

    // ---------- Sam typing in his NOTES ----------
    IEnumerator SamWrites()
    {
        if (samNotes == null) { samDone = true; yield break; }
        yield return samNotes.Type(samLine1);
        yield return new WaitForSeconds(samReadTime);
        yield return samNotes.Backspace();
        yield return new WaitForSeconds(0.3f);
        yield return samNotes.Type(samLine2);
        samDone = true;
    }

    // ---------- the reminder + Sam deleting Pip ----------
    IEnumerator Finale()
    {
        yield return new WaitForSeconds(0.4f);              // let the last letter fly first
        pc.enabled = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // pop-up appears where the notes were, notes jump aside, the notification flashes in and out
        if (notesWindow != null) StartCoroutine(Slide(notesWindow, notesWindow.position, notesMovedTo, 0.15f));
        Narrator.ClearStory();
        reminderWindow.Open(200);
        if (exclaimBubble != null) exclaimBubble.gameObject.SetActive(true);      // a single "!" - Pip gets a fright
        if (dingSound != null) dingSound.Play();
        StartCoroutine(ToastFlash());

        yield return WaitClosed(reminderWindow);
        yield return new WaitForSeconds(0.25f);
        while (!samDone) yield return null;                 // Sam has said "Im deleting it."

        // Sam's red cursor suddenly races to Pip
        Vector3 onPip = pip.position + new Vector3(0.4f, 0.9f, 0f);
        GameAudio.Play("evil_sting", 0.45f, 0f); GameAudio.Play("whoosh", 0.7f);
        yield return samCursor.MoveTipTo(onPip, cursorDashSpeed);
        if (exclaimBubble != null) exclaimBubble.gameObject.SetActive(false);
        if (alarmBubble != null)
        {
            alarmBubble.gameObject.SetActive(true);
            alarmBubble.transform.position = pip.position + new Vector3(0.25f, 2.2f, 0f);
        }
        yield return new WaitForSeconds(0.2f);
        yield return samCursor.Click();                     // right-click
        deleteOption.sprite = optionNormal;
        deleteOption.transform.position = samCursor.Tip + new Vector3(27f / 16f, -10f / 16f, 0f);
        deleteOption.gameObject.SetActive(true);
        yield return Pop(deleteOption.transform);
        yield return new WaitForSeconds(0.25f);
        yield return samCursor.MoveTipTo(deleteOption.transform.position + new Vector3(-0.3f, -0.05f, 0f), 12f);
        deleteOption.sprite = optionPressed;
        yield return new WaitForSeconds(0.15f);
        yield return samCursor.Click();
        yield return new WaitForSeconds(0.08f);
        deleteOption.gameObject.SetActive(false);

        // Pip glitches and gets deleted into the bin
        if (deleteSound != null) deleteSound.Play();
        GameAudio.Play("delete_zap", 0.8f, 0f);
        var anim = pip.GetComponent<PipAnimator>();
        if (anim != null) anim.enabled = false;
        rb.simulated = false;
        Vector3 p0 = pip.position;
        for (int i = 0; i < pipDeletedFrames.Length; i++)
        {
            if (i == 1 && alarmBubble != null) alarmBubble.gameObject.SetActive(false);
            pipSR.sprite = pipDeletedFrames[i];
            pip.position = p0 + new Vector3(Random.Range(-1, 2) / 16f, 0.875f, 0f);   // these frames are centred, Pip's normal ones sit on his feet
            yield return new WaitForSeconds(0.09f);
        }
        if (alarmBubble != null) alarmBubble.gameObject.SetActive(false);
        pipSR.enabled = false;
        FX.PlayOnce(poofFrames, p0 + new Vector3(0f, 0.6f, 0f));
        yield return new WaitForSeconds(0.25f);
        if (recycleBin != null)
        {
            recycleBin.sprite = recycleBinFull;
            GameAudio.Play("poof", 0.6f, 0f, 0.8f);
            FX.PlayOnce(sparkleFrames, recycleBin.transform.position);
            yield return Pop(recycleBin.transform);
        }
        yield return new WaitForSeconds(0.4f);

        // "ITEM DELETED" - stays until the player clicks dismiss (or X)
        itemDeletedWindow.Open(210);
        yield return WaitClosed(itemDeletedWindow);
        yield return new WaitForSeconds(0.3f);

        // Sam's screen slowly goes black, then the next level
        GameAudio.Play("monitor_off", 0.6f, 0f); GameAudio.Ambience("room_night", 0.15f);
        var black = new GameObject("ScreenOff");
        black.transform.position = new Vector3(0f, 0.5625f, 0f);
        black.transform.localScale = new Vector3(26f, 14f, 1f);
        var bsr = black.AddComponent<SpriteRenderer>();
        bsr.sprite = FX.WhitePixel;
        bsr.sortingLayerName = "UI";
        bsr.sortingOrder = 1000;
        float t = 0f;
        while (t < fadeToBlackTime) { t += Time.deltaTime; bsr.color = new Color(0f, 0f, 0f, t / fadeToBlackTime); yield return null; }
        bsr.color = Color.black;
        yield return new WaitForSeconds(0.8f);
        SceneManager.LoadScene(nextScene);
    }

    IEnumerator Sit(int f, float time) { pipSR.sprite = sitFrames[f]; yield return new WaitForSeconds(time); }

    IEnumerator WaitClosed(ErrorPopup w)
    {
        float t = 0f; bool hinted = false;
        while (w.IsOpen)
        {
            t += Time.deltaTime;
            if (!hinted && t > closeHintAfter) { hinted = true; Narrator.Hint(closeHint, Narrator.Icon.Mouse, 30f); }
            yield return null;
        }
        Narrator.ClearHint();
    }

    IEnumerator ToastFlash()
    {
        if (toast == null) yield break;
        toast.enabled = true;
        GameAudio.PlayDelayed("pop_up", 0.2f, 0.5f, 1.25f);
        yield return Slide(toast.transform, toastHidden, toastShown, 0.3f);
        yield return new WaitForSeconds(toastStay);
        yield return Slide(toast.transform, toastShown, toastHidden, 0.3f);
        toast.enabled = false;
    }

    IEnumerator Slide(Transform tr, Vector3 from, Vector3 to, float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            tr.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / time));
            yield return null;
        }
        tr.position = to;
    }

    IEnumerator Pop(Transform tr)
    {
        Vector3 s = tr.localScale;
        float t = 0f;
        while (t < 0.18f)
        {
            t += Time.deltaTime;
            float k = t / 0.18f;
            tr.localScale = s * (k < 0.7f ? Mathf.Lerp(0.3f, 1.1f, k / 0.7f) : Mathf.Lerp(1.1f, 1f, (k - 0.7f) / 0.3f));
            yield return null;
        }
        tr.localScale = s;
    }
}
