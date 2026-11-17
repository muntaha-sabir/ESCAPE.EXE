using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Goes on: the "Level3Director" object (Sam's desktop right after Pip was deleted).
//  1. The screen blinks back on. Pip is gone. Sam types "Whew...Deleted it." in his NOTES.
//  2. Narration: "[Where's Pip..?]". The Recycle Bin glows and blinks.
//  3. The player clicks the Recycle Bin -> the screen goes black -> the Recycle Bin opens (next scene).
public class Level3Director : MonoBehaviour
{
    [Header("Sam's NOTES")]
    public NotesTyper samNotes;
    public string samLine = "Whew...Deleted it.";
    public float samDelay = 1.2f;            // after the screen turns on

    [Header("Narration")]
    public string storyLine = "[Where's Pip..?]";
    public float storyDelay = 2.6f;
    public string clickHint = "click the recycle bin";
    public float clickHintAfter = 7f;        // only if the player hasn't clicked yet

    [Header("Recycle Bin")]
    public SpriteRenderer recycleBin;
    public SpriteRenderer binGlow;           // glowing outline (child)
    public SpriteRenderer binHighlight;      // blue box when the mouse is over it
    public float blinkSpeed = 2.5f;

    [Header("Going inside")]
    public float fadeTime = 1.1f;
    public string nextScene = "Level4_Delete";

    bool canClick, clicked;

    IEnumerator Start()
    {
        if (binHighlight != null) binHighlight.enabled = false;
        yield return new WaitForSeconds(samDelay);
        if (samNotes != null) StartCoroutine(samNotes.Type(samLine));
        yield return new WaitForSeconds(Mathf.Max(0f, storyDelay - samDelay));
        Narrator.Story(storyLine, 600f);
        canClick = true;
        yield return new WaitForSeconds(clickHintAfter);
        if (!clicked) Narrator.Hint(clickHint, Narrator.Icon.Mouse, 600f);
    }

    void Update()
    {
        // glow blinks on and off softly so the player notices the bin
        if (binGlow != null && !clicked)
            binGlow.color = new Color(1f, 1f, 1f, 0.25f + 0.75f * (0.5f + 0.5f * Mathf.Sin(Time.time * blinkSpeed * Mathf.PI)));

        if (!canClick || clicked || recycleBin == null || Camera.main == null || Mouse.current == null) return;
        Vector2 sp = Mouse.current.position.ReadValue();
        Vector3 mp = Camera.main.ScreenToWorldPoint(new Vector3(sp.x, sp.y, 10f));
        Bounds b = recycleBin.bounds;
        bool over = mp.x >= b.min.x && mp.x <= b.max.x && mp.y >= b.min.y && mp.y <= b.max.y;
        if (binHighlight != null) binHighlight.enabled = over;
        if (over && Mouse.current.leftButton.wasPressedThisFrame) StartCoroutine(OpenBin());
    }

    IEnumerator OpenBin()
    {
        clicked = true;
        Narrator.ClearHint();
        if (binGlow != null) binGlow.color = Color.white;
        GameAudio.Play("whoosh", 0.7f, 0f, 0.7f); GameAudio.Ambience("room_night", 0.1f);

        // little "click" squash on the bin
        Transform t = recycleBin.transform;
        Vector3 s = t.localScale;
        t.localScale = new Vector3(s.x * 1.1f, s.y * 0.9f, 1f);
        yield return new WaitForSeconds(0.08f);
        t.localScale = s;
        yield return new WaitForSeconds(0.25f);
        Narrator.ClearStory();

        // the screen goes black...
        var black = new GameObject("ScreenOff");
        black.transform.position = new Vector3(0f, 0.5625f, 0f);
        black.transform.localScale = new Vector3(26f, 14f, 1f);
        var bsr = black.AddComponent<SpriteRenderer>();
        bsr.sprite = FX.WhitePixel;
        bsr.sortingLayerName = "UI";
        bsr.sortingOrder = 1000;
        float k = 0f;
        while (k < fadeTime) { k += Time.deltaTime; bsr.color = new Color(0f, 0f, 0f, k / fadeTime); yield return null; }
        bsr.color = Color.black;
        yield return new WaitForSeconds(0.35f);
        SceneManager.LoadScene(nextScene);    // ...and blinks open as the Recycle Bin
    }
}
