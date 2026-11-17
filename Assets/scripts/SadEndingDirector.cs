using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Goes on: the "SadEndingDirector" object in Ending_Freedom (the SAD ending - Pip was deleted in the Recycle Bin).
//  1. Black... the screen slowly comes back to Sam's desktop. NOTES: "[Sam]: ..."
//  2. Sam's red cursor slowly opens the Recycle Bin -> "This folder is empty."
//     NOTES: "Is the bug gone...?"
//  3. He closes it. NOTES (backspaced): "Let me just review the game one last time.."
//  4. He opens Super Jump Quest 3 -> Level 1 loads, the stars still spin... "PLAYER NOT FOUND"
//     NOTES: "..."   Narration (bottom): "[...]"
//  5. He hesitates, closes the game. The monitor turns off.
//  6. In the dark, Pip's little leaf falls. The final words type out in pixels (with a click sound).
//     The leaf flickers once (...?). THE END. "Ending: Sad."
//  7. Click to play again (back to the Title screen).
public class SadEndingDirector : MonoBehaviour
{
    [Header("Mood: a cold, dim layer over the wallpaper (alpha 0 = off)")]
    public Color moodTint = new Color(0.05f, 0.07f, 0.2f, 0.28f);
    public float startDelay = 5f;               // the screen fades in slowly first (ScreenBlinkIn)

    [Header("Sam's cursor (slow and tired)")]
    public SamCursor samCursor;
    public float cursorSpeed = 3.2f;
    public float hesitate = 0.6f;               // stops halfway, as if Sam isn't sure

    [Header("Sam's NOTES")]
    public NotesTyper samNotes;
    public string samIdle = "...";
    public string samBinLine = "Is the bug gone...?";
    public string samReviewLine = "Let me just review the game one last time..";
    public string samGameLine = "...";

    [Header("Recycle Bin")]
    public SpriteRenderer recycleBin;           // the desktop icon
    public GameObject binWindow;                // "This folder is empty." (starts hidden)
    public Vector2 binCloseLocal = new Vector2(4.1875f, 1.9375f);

    [Header("Super Jump Quest 3")]
    public SpriteRenderer gameIcon;             // the SJQ3 desktop icon
    public GameObject gameWindow;               // Level 1 with "PLAYER NOT FOUND" (starts hidden)
    public Vector2 gameCloseLocal = new Vector2(6.1875f, 4.0625f);
    public string gameStoryLine = "[...]";
    public float lookAtGameTime = 4.5f;

    [Header("The end")]
    public float monitorOffTime = 2f;
    public SpriteRenderer leaf;                 // Pip's leaf (starts hidden)
    public Vector3 leafStart = new Vector3(0f, 7.5f, 0f);
    public Vector3 leafEnd = new Vector3(0f, 0.25f, 0f);
    public float leafFallTime = 5f;
    [Header("Final words: typed in pixels on the black screen (no narration box)")]
    public NotesTyper endTyper1;                // first line
    public string endLine1 = "... You let Pip get deleted in the recycle bin...permanently.";
    public NotesTyper endTyper2;                // the line under it
    public string endLine2 = "Pip is gone.";
    public NotesTyper endTyper3;                // under THE END
    public string endLine3 = "Ending: Sad.";
    public Color leafFlickerColor = new Color(0.62f, 0.55f, 1f, 1f);
    public SpriteRenderer theEnd;               // "THE END" (starts hidden)
    public string playAgainHint = "click to play again";
    public string titleScene = "Title";

    bool canRestart, restarting;

    IEnumerator Start()
    {
        if (binWindow != null) binWindow.SetActive(false);
        if (gameWindow != null) gameWindow.SetActive(false);
        if (leaf != null) leaf.gameObject.SetActive(false);
        if (theEnd != null) theEnd.gameObject.SetActive(false);
        MakeMoodLayer();

        yield return new WaitForSeconds(startDelay);

        // --- [Sam]: ...
        yield return Type(samIdle);
        yield return new WaitForSeconds(2f);

        // --- Sam checks the Recycle Bin... it's empty
        if (recycleBin != null)
        {
            yield return CursorTo(recycleBin.bounds.center, true);
            yield return DoubleClick();
            yield return PopIn(binWindow);
            yield return new WaitForSeconds(0.9f);
            yield return Retype(samBinLine);
            yield return new WaitForSeconds(2.6f);
            yield return CursorTo(binWindow.transform.TransformPoint(binCloseLocal), false);
            yield return Click();
            yield return PopOut(binWindow);
            yield return new WaitForSeconds(1.2f);
        }

        // --- "Let me just review the game one last time.."
        yield return Retype(samReviewLine);
        yield return new WaitForSeconds(1.8f);

        // --- Sam opens his game... no hero
        if (gameIcon != null)
        {
            yield return CursorTo(gameIcon.bounds.center, true);
            yield return DoubleClick();
            yield return PopIn(gameWindow);
            GameAudio.Play("error_beep", 0.5f, 0f);
            yield return new WaitForSeconds(1.2f);
        }
        yield return Retype(samGameLine);
        Narrator.Story(gameStoryLine, 600f);
        yield return new WaitForSeconds(lookAtGameTime);

        // --- he hovers over the X for a moment... and closes it
        if (gameWindow != null)
        {
            yield return CursorTo(gameWindow.transform.TransformPoint(gameCloseLocal), false);
            yield return new WaitForSeconds(1.2f);
            yield return Click();
            yield return PopOut(gameWindow);
        }
        Narrator.ClearStory();
        yield return new WaitForSeconds(2f);

        // --- the monitor slowly turns off
        GameAudio.Play("monitor_off", 0.6f, 0f); GameAudio.Ambience("", 0f, 2f); GameAudio.Music("", 0f, 1.5f);
        var black = new GameObject("MonitorOff");
        black.transform.position = new Vector3(0f, 0.5625f, 0f);
        black.transform.localScale = new Vector3(26f, 14f, 1f);
        var bsr = black.AddComponent<SpriteRenderer>();
        bsr.sprite = FX.WhitePixel;
        bsr.sortingLayerName = "Frame";
        bsr.sortingOrder = 40;                  // under the narration box (49+)
        float k = 0f;
        while (k < monitorOffTime) { k += Time.deltaTime; bsr.color = new Color(0f, 0f, 0f, Mathf.Clamp01(k / monitorOffTime)); yield return null; }
        bsr.color = Color.black;
        yield return new WaitForSeconds(1.5f);

        // --- Pip's leaf drifts down in the dark
        GameAudio.Music("sad_violin", 0.6f, 2f);
        if (leaf != null)
        {
            leaf.gameObject.SetActive(true);
            float t = 0f;
            while (t < leafFallTime)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / leafFallTime);
                float sway = Mathf.Sin(t * 2.2f) * 0.7f * (1f - p);
                Vector3 pos = Vector3.Lerp(leafStart, leafEnd, 1f - (1f - p) * (1f - p));
                pos.x += sway;
                leaf.transform.position = pos;
                leaf.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 2.2f + 0.8f) * 25f * (1f - p));
                yield return null;
            }
            leaf.transform.rotation = Quaternion.identity;
        }
        yield return new WaitForSeconds(1f);

        // --- the final words type out under the leaf
        yield return TypeEnd(endTyper1, endLine1);
        yield return new WaitForSeconds(1.2f);
        if (endTyper1 != null) endTyper1.HideCaret();
        yield return TypeEnd(endTyper2, endLine2);
        yield return new WaitForSeconds(1.8f);
        if (endTyper2 != null) endTyper2.HideCaret();

        // --- ...did something just move? (the leaf glitches once, then is still)
        if (leaf != null) yield return LeafFlicker();
        yield return new WaitForSeconds(1.2f);

        // --- THE END
        if (theEnd != null)
        {
            theEnd.gameObject.SetActive(true);
            float t = 0f;
            while (t < 1.5f) { t += Time.deltaTime; theEnd.color = new Color(1f, 1f, 1f, Mathf.Clamp01(t / 1.5f)); yield return null; }
        }
        yield return new WaitForSeconds(1f);
        yield return TypeEnd(endTyper3, endLine3);
        yield return new WaitForSeconds(0.8f);
        if (endTyper3 != null) endTyper3.HideCaret();
        yield return new WaitForSeconds(2.5f);
        Narrator.Hint(playAgainHint, Narrator.Icon.Mouse, 600f);
        canRestart = true;
    }

    void Update()
    {
        if (!canRestart || restarting || Mouse.current == null) return;
        if (Mouse.current.leftButton.wasPressedThisFrame) StartCoroutine(Restart());
    }

    IEnumerator Restart()
    {
        restarting = true;
        Narrator.ClearHint();
        Narrator.ClearStory();
        yield return new WaitForSeconds(0.4f);
        GameState.ResetAll();
        GameAudio.Music("", 0f, 0.5f);
        SceneManager.LoadScene(titleScene);
    }

    // ---------- helpers ----------
    IEnumerator Type(string s)
    {
        if (samNotes != null && !string.IsNullOrEmpty(s)) yield return StartCoroutine(samNotes.Type(s));
    }

    IEnumerator Retype(string s)              // backspace what's there, then type the new line
    {
        if (samNotes == null) yield break;
        yield return StartCoroutine(samNotes.Backspace());
        yield return new WaitForSeconds(0.35f);
        yield return Type(s);
    }

    IEnumerator TypeEnd(NotesTyper typer, string line)
    {
        if (typer != null && !string.IsNullOrEmpty(line)) yield return StartCoroutine(typer.Type(line));
    }

    IEnumerator LeafFlicker()
    {
        Vector3 p0 = leaf.transform.position;
        float[] waits = { 0.06f, 0.05f, 0.09f, 0.04f };
        for (int i = 0; i < waits.Length; i++)
        {
            bool odd = (i % 2) == 0;
            leaf.color = odd ? leafFlickerColor : Color.white;
            leaf.transform.position = p0 + new Vector3(odd ? 0.0625f : 0f, 0f, 0f);
            leaf.enabled = i != 2;
            yield return new WaitForSeconds(waits[i]);
        }
        leaf.enabled = true;
        leaf.color = Color.white;
        leaf.transform.position = p0;
    }

    void MakeMoodLayer()
    {
        if (moodTint.a <= 0f) return;
        var go = new GameObject("MoodLayer");
        go.transform.position = new Vector3(0f, 0.5625f, 0f);
        go.transform.localScale = new Vector3(26f, 14f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = FX.WhitePixel;
        sr.color = moodTint;
        sr.sortingLayerName = "Background";
        sr.sortingOrder = 1;                    // just above the wallpaper, under the icons
    }

    IEnumerator CursorTo(Vector3 point, bool unsure)
    {
        if (samCursor == null) yield break;
        point.z = 0f;
        if (unsure)
        {
            Vector3 half = Vector3.Lerp(samCursor.Tip, point, 0.55f);
            yield return StartCoroutine(samCursor.MoveTipTo(half, cursorSpeed));
            yield return new WaitForSeconds(hesitate);
        }
        yield return StartCoroutine(samCursor.MoveTipTo(point, cursorSpeed));
        yield return new WaitForSeconds(0.25f);
    }

    IEnumerator Click()
    {
        if (samCursor != null) yield return StartCoroutine(samCursor.Click());
    }

    IEnumerator DoubleClick()
    {
        yield return Click();
        yield return new WaitForSeconds(0.08f);
        yield return Click();
        yield return new WaitForSeconds(0.2f);
    }

    // windows pop open with a small bounce, like the error pop-ups
    IEnumerator PopIn(GameObject w)
    {
        if (w == null) yield break;
        GameAudio.Play("pop_up", 0.7f);
        w.SetActive(true);
        Transform t = w.transform;
        float d = 0.32f, k = 0f;
        while (k < d)
        {
            k += Time.deltaTime;
            float p = Mathf.Clamp01(k / d); p = p * p * (3f - 2f * p);
            float s = Mathf.Lerp(0.6f, 1f, p);
            t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    IEnumerator PopOut(GameObject w)
    {
        if (w == null) yield break;
        Transform t = w.transform;
        float d = 0.18f, k = 0f;
        while (k < d)
        {
            k += Time.deltaTime;
            float s = Mathf.Lerp(1f, 0.6f, Mathf.Clamp01(k / d));
            t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        w.SetActive(false);
        t.localScale = Vector3.one;
    }
}
