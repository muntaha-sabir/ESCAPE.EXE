using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Goes on: the "LetterHUD" object (the H E L P slots on the desk under the monitor).
// When Pip collects a letter it pops up, then flies down into its slot.
public class LetterHUD : MonoBehaviour
{
    public string order = "HELP";
    public Transform[] slots;               // one per letter, same order as above
    public Sprite[] sparkleFrames;

    static LetterHUD I;
    readonly HashSet<char> got = new HashSet<char>();

    void Awake() { I = this; }

    public static bool Has(char c) { return I != null && I.got.Contains(c); }

    public static void Collect(char c, Sprite s, Vector3 from)
    {
        if (I == null || I.got.Contains(c)) return;
        I.got.Add(c);
        GameAudio.Play("letter_collect", 0.7f, 0f);
        I.StartCoroutine(I.Fly(c, s, from));
        Level2Director.LetterCollected(c);
    }

    IEnumerator Fly(char c, Sprite s, Vector3 from)
    {
        int idx = order.IndexOf(c);
        if (idx < 0 || idx >= slots.Length) yield break;

        var go = new GameObject("Letter_" + c);
        go.transform.position = from;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.sortingLayerName = "Frame";
        sr.sortingOrder = 30;
        FX.PlayOnce(sparkleFrames, from, 18f, "Frame", 31);

        float t = 0f;                                       // pop
        while (t < 0.18f)
        {
            t += Time.deltaTime;
            float k = t / 0.18f;
            go.transform.localScale = Vector3.one * (k < 0.7f ? Mathf.Lerp(0f, 1.3f, k / 0.7f) : Mathf.Lerp(1.3f, 1f, (k - 0.7f) / 0.3f));
            yield return null;
        }
        go.transform.localScale = Vector3.one;
        yield return new WaitForSeconds(0.3f);

        Vector3 a = from, b = slots[idx].position;           // fly in an arc
        t = 0f;
        while (t < 0.55f)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / 0.55f);
            Vector3 p = Vector3.Lerp(a, b, k);
            p.y += Mathf.Sin(k * Mathf.PI) * 1.5f;
            go.transform.position = p;
            yield return null;
        }
        go.transform.position = b;
        go.transform.SetParent(slots[idx], true);
        FX.PlayOnce(sparkleFrames, b, 18f, "Frame", 31);
    }
}
