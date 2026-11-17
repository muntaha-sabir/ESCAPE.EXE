using System.Collections;
using UnityEngine;

// Helper used by the Level 1 effect scripts (drop-ins, poofs, sparkles, hiding things).
// You do NOT need to put this on anything - it creates itself when needed.
public class FX : MonoBehaviour
{
    static FX host;
    static Sprite whitePixel;

    public static FX Host
    {
        get
        {
            if (host == null) host = new GameObject("FX").AddComponent<FX>();
            return host;
        }
    }

    // A 1x1 white sprite: tint it any colour and scale it to make flashes / black screens.
    public static Sprite WhitePixel
    {
        get
        {
            if (whitePixel == null)
            {
                var t = new Texture2D(1, 1);
                t.SetPixel(0, 0, Color.white);
                t.filterMode = FilterMode.Point;
                t.Apply();
                whitePixel = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }
            return whitePixel;
        }
    }

    // Turns every sprite and collider on an object (and its children) on or off.
    public static void SetVisible(GameObject go, bool on)
    {
        if (go == null) return;
        foreach (var r in go.GetComponentsInChildren<Renderer>(true)) r.enabled = on;
        foreach (var c in go.GetComponentsInChildren<Collider2D>(true)) c.enabled = on;
    }

    // Plays a little frame-by-frame effect once (poof, sparkle...) and removes it.
    public static void PlayOnce(Sprite[] frames, Vector3 pos, float fps = 16f, string layer = "UI", int order = 50)
    {
        if (frames == null || frames.Length == 0) return;
        Host.StartCoroutine(Host.Anim(frames, pos, fps, layer, order));
    }

    IEnumerator Anim(Sprite[] frames, Vector3 pos, float fps, string layer, int order)
    {
        var go = new GameObject("fx");
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = layer;
        sr.sortingOrder = order;
        foreach (var f in frames)
        {
            sr.sprite = f;
            yield return new WaitForSeconds(1f / fps);
        }
        Destroy(go);
    }

    // Poof in the sky, then the object falls down into the spot where you placed it in the scene.
    public static IEnumerator DropIn(GameObject go, float fromY, float fallTime, Sprite[] poof)
    {
        if (go == null) yield break;
        Vector3 end = go.transform.position;
        Vector3 start = new Vector3(end.x, fromY, end.z);

        PlayOnce(poof, start);
        yield return new WaitForSeconds(0.08f);

        go.transform.position = start;
        foreach (var r in go.GetComponentsInChildren<Renderer>(true)) r.enabled = true;

        float t = 0f;
        while (t < fallTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fallTime);
            go.transform.position = Vector3.LerpUnclamped(start, end, k * k);   // speeds up like gravity
            yield return null;
        }
        go.transform.position = end;
        foreach (var c in go.GetComponentsInChildren<Collider2D>(true)) c.enabled = true;

        // tiny squash when it lands
        Vector3 s = go.transform.localScale;
        go.transform.localScale = new Vector3(s.x * 1.15f, s.y * 0.85f, s.z);
        yield return new WaitForSeconds(0.07f);
        go.transform.localScale = s;
    }

    // Sparkle, then the object pops into view (used for the stars and the "?").
    public static IEnumerator PopIn(GameObject go, Sprite[] sparkle)
    {
        if (go == null) yield break;
        PlayOnce(sparkle, go.transform.position);
        Vector3 s = go.transform.localScale;
        foreach (var r in go.GetComponentsInChildren<Renderer>(true)) r.enabled = true;

        float t = 0f, d = 0.25f;
        while (t < d)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / d);
            float size = k < 0.6f ? Mathf.Lerp(0f, 1.25f, k / 0.6f) : Mathf.Lerp(1.25f, 1f, (k - 0.6f) / 0.4f);
            go.transform.localScale = s * size;
            yield return null;
        }
        go.transform.localScale = s;
        foreach (var c in go.GetComponentsInChildren<Collider2D>(true)) c.enabled = true;
    }
}
