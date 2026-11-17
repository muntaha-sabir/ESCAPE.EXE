using System.Collections;
using UnityEngine;

// Goes on: P!P.exe in the Recycle Bin. It is invisible most of the time and only glitches into view for a moment.
[RequireComponent(typeof(SpriteRenderer))]
public class GlitchFlicker : MonoBehaviour
{
    public Sprite[] frames;                    // 0 normal, 1 sliced, 2 colour split, 3 faint
    public Vector2 hiddenTime = new Vector2(0.4f, 1.3f);
    public Vector2 shownTime = new Vector2(0.08f, 0.35f);

    SpriteRenderer sr;
    void Awake() { sr = GetComponent<SpriteRenderer>(); sr.enabled = false; }
    void OnEnable() { StartCoroutine(Loop()); }

    IEnumerator Loop()
    {
        while (true)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(Random.Range(hiddenTime.x, hiddenTime.y));
            float t = Random.Range(shownTime.x, shownTime.y);
            while (t > 0f)
            {
                sr.enabled = true;
                sr.sprite = frames[Random.Range(0, frames.Length)];
                float step = Random.Range(0.04f, 0.09f);
                t -= step;
                yield return new WaitForSeconds(step);
            }
        }
    }
}
