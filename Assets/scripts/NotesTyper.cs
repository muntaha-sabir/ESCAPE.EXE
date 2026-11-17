using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Goes on: the "SamText" object, a child of Sam's yellow NOTES.
// Types Sam's words on the lines under "[Sam]:" letter by letter, can backspace them, and shows a blinking cursor.
// Uses the same pixel font as the narration (story_font_6x9).
public class NotesTyper : MonoBehaviour
{
    public Texture2D fontSheet;
    public int cellWidth = 6, cellHeight = 9;
    public float fontPixelsPerUnit = 16f;
    public Color ink = new Color(0.231f, 0.227f, 0.42f, 1f);
    public int maxWidthPx = 80;           // wrap width of the lines on the notes
    public int lineHeightPx = 11;         // distance between the ruled lines
    public float typeDelay = 0.05f;
    public float backspaceDelay = 0.025f;
    public string sortingLayer = "Default";
    public int sortingOrder = 9;

    [Header("Optional: clicky typing sound (leave empty = silent)")]
    public AudioClip typeSound;
    [Range(0f, 1f)] public float typeVolume = 0.5f;
    public float typePitchJitter = 0.08f;     // small random pitch change so it doesn't sound robotic
    AudioSource audioSrc;
    bool typingOn;                            // Sam's keyboard sound is playing (only NOTES without their own Type Sound)

    const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz.,!?':[]-~/()_+;\"";
    readonly Dictionary<char, Sprite> glyphs = new Dictionary<char, Sprite>();
    readonly Dictionary<char, int> widths = new Dictionary<char, int>();
    readonly List<GameObject> shown = new List<GameObject>();
    readonly List<Vector2> places = new List<Vector2>();     // pixel position of every character (top-left)
    string text = "";
    SpriteRenderer caret;

    float PX { get { return 1f / fontPixelsPerUnit; } }

    void Awake()
    {
        if (fontSheet != null)
            for (int i = 0; i < CHARS.Length; i++)
            {
                int w = 0;
                for (int x = 0; x < cellWidth; x++)
                    for (int y = 0; y < cellHeight; y++)
                        if (fontSheet.GetPixel(i * cellWidth + x, y).a > 0.5f) w = Mathf.Max(w, x + 1);
                widths[CHARS[i]] = Mathf.Max(1, w);
                glyphs[CHARS[i]] = Sprite.Create(fontSheet, new Rect(i * cellWidth, 0, cellWidth, cellHeight), new Vector2(0f, 1f), fontPixelsPerUnit);
            }
        var c = new GameObject("Caret");
        c.transform.SetParent(transform, false);
        c.transform.localScale = new Vector3(1f, 8f, 1f) * PX;
        caret = c.AddComponent<SpriteRenderer>();
        caret.sprite = FX.WhitePixel;
        caret.color = ink;
        caret.sortingLayerName = sortingLayer;
        caret.sortingOrder = sortingOrder;
        caret.enabled = false;
    }

    int Adv(char ch) { return ch == ' ' ? 4 : (widths.ContainsKey(ch) ? widths[ch] + 1 : 4); }

    // Works out where every letter goes (whole words move to the next line, so nothing jumps while typing).
    void Layout(string s)
    {
        places.Clear();
        int x = 0, line = 0, i = 0;
        while (i < s.Length)
        {
            int j = i; int w = 0;
            while (j < s.Length && s[j] != ' ') { w += Adv(s[j]); j++; }
            if (x > 0 && x + w - 1 > maxWidthPx) { x = 0; line++; }
            for (int k = i; k < j; k++) { places.Add(new Vector2(x, line * lineHeightPx)); x += Adv(s[k]); }
            if (j < s.Length) { places.Add(new Vector2(x, line * lineHeightPx)); x += Adv(' '); }
            i = j + 1;
        }
    }

    void PutCaret(int count)
    {
        Vector2 p = Vector2.zero;                                  // just after the last letter typed
        if (count > 0 && count <= places.Count) p = places[count - 1] + new Vector2(Adv(text[count - 1]), 0f);
        caret.transform.localPosition = new Vector3((p.x + 0.5f) * PX, -(p.y + 4f) * PX, 0f);
    }

    public IEnumerator Type(string s)
    {
        Clear();
        text = s;
        Layout(s);
        caret.enabled = true;
        bool sam = true;                         // the keyboard typing sound plays for every typed text
        if (sam && !typingOn) { typingOn = true; GameAudio.StartTyping(); }
        for (int i = 0; i < s.Length; i++)
        {
            char ch = s[i];
            GameObject g = null;
            if (ch != ' ' && glyphs.ContainsKey(ch))
            {
                g = new GameObject("c");
                g.transform.SetParent(transform, false);
                g.transform.localPosition = new Vector3(places[i].x * PX, -places[i].y * PX, 0f);
                var sr = g.AddComponent<SpriteRenderer>();
                sr.sprite = glyphs[ch];
                sr.color = ink;
                sr.sortingLayerName = sortingLayer;
                sr.sortingOrder = sortingOrder;
            }
            shown.Add(g);
            if (g != null) Click();
            PutCaret(i + 1);
            yield return new WaitForSeconds(ch == '.' || ch == '?' || ch == '!' ? typeDelay * 3f : typeDelay);
        }
        if (typingOn) { typingOn = false; GameAudio.StopTyping(); }
    }

    public IEnumerator Backspace()
    {
        if (!typingOn && shown.Count > 0) { typingOn = true; GameAudio.StartTyping(); }
        for (int i = shown.Count - 1; i >= 0; i--)
        {
            if (shown[i] != null) Destroy(shown[i]);
            shown.RemoveAt(i);
            PutCaret(i);
            yield return new WaitForSeconds(backspaceDelay);
        }
        text = "";
        if (typingOn) { typingOn = false; GameAudio.StopTyping(); }
    }

    void OnDisable() { if (typingOn) { typingOn = false; GameAudio.StopTyping(); } }

    void Click()
    {
        return;                                  // only Sam's keyboard (the team's "typing" file) is used now
#pragma warning disable 0162
        if (typeSound == null) return;
        if (audioSrc == null)
        {
            audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
            audioSrc.spatialBlend = 0f;
        }
        audioSrc.pitch = 1f + Random.Range(-typePitchJitter, typePitchJitter);
        audioSrc.PlayOneShot(typeSound, typeVolume);
#pragma warning restore 0162
    }

    // stops the blinking cursor (the typed letters stay)
    public void HideCaret()
    {
        if (caret != null) caret.enabled = false;
    }

    public void Clear()
    {
        foreach (var g in shown) if (g != null) Destroy(g);
        shown.Clear();
    }

    void Update()
    {
        if (caret != null && caret.enabled) caret.color = new Color(ink.r, ink.g, ink.b, Mathf.Repeat(Time.time * 2f, 1f) < 0.55f ? 1f : 0f);
    }
}
