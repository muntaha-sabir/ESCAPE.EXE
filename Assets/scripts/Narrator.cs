using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Goes on: a "Narrator" object in each scene. Shows small typed captions outside Sam's computer:
//   STORY = centred, white text in a soft grey box   ->  Narrator.Story("[...]");
//   PIP   = bottom-left with a tiny Pip face        ->  Narrator.Pip("...");
//   HINT  = bottom-right, yellow, key/mouse icon    ->  Narrator.Hint("click x to close", Narrator.Icon.Mouse);
public class Narrator : MonoBehaviour
{
    public enum Icon { None, Key, Mouse }

    [Header("Art")]
    public Texture2D fontSheet;              // story_font_6x9 (Read/Write ON - already set)
    public Sprite pipIcon, keyIcon, mouseIcon;
    public int cellWidth = 6, cellHeight = 9;
    public float fontPixelsPerUnit = 16f;    // 16 = same size as the game's pixels (32 = half size)

    [Header("Colours")]
    public Color storyColor = Color.white;
    public Color pipColor = new Color(1f, 0.89f, 0.925f);
    public Color hintColor = new Color(1f, 0.886f, 0.478f);
    public Color shadowColor = new Color(0.17f, 0.133f, 0.21f);
    public Color boxColor = new Color(0.431f, 0.424f, 0.478f, 0.53f);     // light grey, see-through
    public Color boxBorder = new Color(0.804f, 0.784f, 0.843f, 0.47f);

    [Header("Timing")]
    public float charTime = 0.04f;           // typing speed
    public float defaultHold = 3f;           // seconds a line stays after it finishes typing
    public float fadeTime = 0.6f;

    [Header("Where (world units)")]
    public Vector2 storyCenter = new Vector2(0f, -5.4375f);     // centre of the story box
    public Vector2 pipTopLeft = new Vector2(-14.5f, -7.5f);
    public Vector2 hintTopRight = new Vector2(14.5f, -7.5f);
    public string sortingLayer = "Frame";

    const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz.,!?':[]-~/()_+;\"\u2661";
    static Narrator I;
    readonly Dictionary<char, Sprite> glyphs = new Dictionary<char, Sprite>();
    readonly Dictionary<char, int> widths = new Dictionary<char, int>();
    Slot story = new Slot(), pip = new Slot(), hint = new Slot();
    float PX { get { return 1f / fontPixelsPerUnit; } }

    class Slot { public GameObject root; public Coroutine co; }
    enum Align { Left, Center, Right }

    void Awake()
    {
        I = this;
        if (fontSheet == null) return;
        for (int i = 0; i < CHARS.Length; i++)
        {
            int w = 0;
            for (int x = 0; x < cellWidth; x++)
                for (int y = 0; y < cellHeight; y++)
                    if (fontSheet.GetPixel(i * cellWidth + x, y).a > 0.5f) w = Mathf.Max(w, x + 1);
            widths[CHARS[i]] = Mathf.Max(1, w);
            glyphs[CHARS[i]] = Sprite.Create(fontSheet, new Rect(i * cellWidth, 0, cellWidth, cellHeight), new Vector2(0f, 1f), fontPixelsPerUnit);
        }
    }

    // ---------- call these from anywhere ----------
    public static void Story(string text, float hold = -1f) { if (I != null) I.Show(I.story, text, null, I.storyColor, Align.Center, I.storyCenter, hold); }
    public static void Pip(string text, float hold = -1f) { if (I != null) I.Show(I.pip, text, I.pipIcon, I.pipColor, Align.Left, I.pipTopLeft, hold); }
    public static void Hint(string text, Icon icon = Icon.None, float hold = -1f)
    {
        if (I == null) return;
        Sprite s = icon == Icon.Key ? I.keyIcon : icon == Icon.Mouse ? I.mouseIcon : null;
        I.Show(I.hint, text, s, I.hintColor, Align.Right, I.hintTopRight, hold);
    }
    public static void ClearStory() { if (I != null) I.Clear(I.story); }
    public static void ClearPip() { if (I != null) I.Clear(I.pip); }
    public static void ClearHint() { if (I != null) I.Clear(I.hint); }

    // ---------- inside ----------
    int Width(string t)
    {
        int w = 0;
        foreach (char c in t) w += (c == ' ' ? 3 : (widths.ContainsKey(c) ? widths[c] : 0)) + 1;
        return Mathf.Max(0, w - 1);
    }

    void Clear(Slot s)
    {
        if (s.co != null) StopCoroutine(s.co);
        if (s.root != null) Destroy(s.root);
        s.co = null; s.root = null;
    }

    void Show(Slot s, string text, Sprite icon, Color col, Align align, Vector2 anchor, float hold)
    {
        Clear(s);
        s.root = new GameObject("Caption");
        s.co = StartCoroutine(Run(s, text, icon, col, align, anchor, hold < 0 ? defaultHold : hold));
    }

    Sprite MakeBox(int w, int h)
    {
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool corner = (x == 0 || x == w - 1) && (y == 0 || y == h - 1);
                bool edge = x == 0 || y == 0 || x == w - 1 || y == h - 1;
                tex.SetPixel(x, y, corner ? Color.clear : edge ? boxBorder : boxColor);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0f, 1f), fontPixelsPerUnit);
    }

    float Snap(float v) { return Mathf.Round(v * fontPixelsPerUnit) / fontPixelsPerUnit; }

    SpriteRenderer Make(Transform parent, Sprite sp, Vector2 pos, Color c, int order)
    {
        var go = new GameObject("c");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = sp; r.color = c; r.sortingLayerName = sortingLayer; r.sortingOrder = order;
        return r;
    }

    IEnumerator Run(Slot s, string text, Sprite icon, Color col, Align align, Vector2 anchor, float hold)
    {
        int textW = Width(text);
        float iconW = 0f, gap = 0f;
        if (icon != null) { iconW = icon.rect.width / icon.pixelsPerUnit; gap = 3 * PX; }

        float left;
        if (align == Align.Left) left = anchor.x + iconW + gap;
        else if (align == Align.Right) left = anchor.x - textW * PX;
        else left = anchor.x - textW * PX * 0.5f;
        left = Snap(left);
        float top = Snap(anchor.y);

        var all = new List<SpriteRenderer>();
        if (align == Align.Center)        // story line: dark rounded box behind the text
        {
            int bw = textW + 10, bh = cellHeight + 7;
            top = Snap(anchor.y + (bh * 0.5f - 3) * PX);
            all.Add(Make(s.root.transform, MakeBox(bw, bh), new Vector2(Snap(anchor.x - bw * 0.5f * PX), Snap(anchor.y + bh * 0.5f * PX)), Color.white, 49));
        }
        if (icon != null)
        {
            float ix = align == Align.Left ? anchor.x : left - gap - iconW;
            float ih = icon.rect.height / icon.pixelsPerUnit;
            all.Add(Make(s.root.transform, icon, new Vector2(ix + iconW * 0.5f, top + PX - ih * 0.5f), Color.white, 52));
        }

        float x = left;
        var letters = new List<SpriteRenderer[]>();
        foreach (char ch in text)
        {
            if (ch != ' ' && glyphs.ContainsKey(ch))
            {
                var sh = Make(s.root.transform, glyphs[ch], new Vector2(x, top - PX), shadowColor, 50);
                var fg = Make(s.root.transform, glyphs[ch], new Vector2(x, top), col, 51);
                sh.enabled = fg.enabled = false;
                letters.Add(new[] { sh, fg });
                all.Add(sh); all.Add(fg);
                x += (widths[ch] + 1) * PX;
            }
            else x += 4 * PX;
        }

        foreach (var pair in letters)              // typewriter
        {
            pair[0].enabled = pair[1].enabled = true;
            yield return new WaitForSeconds(charTime);
        }
        yield return new WaitForSeconds(hold);

        var start = new List<Color>();
        foreach (var r in all) start.Add(r.color);
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            for (int i = 0; i < all.Count; i++) { Color c = start[i]; c.a = 1f - t / fadeTime; all[i].color = c; }
            yield return null;
        }
        Destroy(s.root);
        s.root = null; s.co = null;
    }
}
