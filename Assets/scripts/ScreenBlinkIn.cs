using System.Collections;
using UnityEngine;

// Covers the screen with a colour at the start of a scene, then reveals it.
//  Level 1:  Color = white, Only Monitor Screen = OFF, Blink = OFF  -> continues the white flash from Boot
//  Level 2:  Color = black, Only Monitor Screen = ON,  Blink = ON   -> Sam's screen slowly blinks back to life
// Goes on: any object in the scene (e.g. LevelFX).
public class ScreenBlinkIn : MonoBehaviour
{
    public Color color = Color.black;
    public bool onlyMonitorScreen = true;
    public bool blink = true;
    public float waitFirst = 0.3f;
    public float fadeTime = 0.9f;

    IEnumerator Start()
    {
        var go = new GameObject("ScreenCover");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = FX.WhitePixel;
        sr.sortingLayerName = "UI";
        sr.sortingOrder = 300;
        sr.color = color;

        if (onlyMonitorScreen)
        {
            go.transform.position = new Vector3(0f, 0.5625f, 0f);
            go.transform.localScale = new Vector3(26f, 14f, 1f);
        }
        else
        {
            go.transform.SetParent(Camera.main.transform, false);
            go.transform.localPosition = new Vector3(0f, 0f, 10f);
            go.transform.localScale = new Vector3(200f, 200f, 1f);
        }

        yield return new WaitForSeconds(waitFirst);

        if (blink)
        {
            SetA(sr, 0.0f); yield return new WaitForSeconds(0.05f);
            SetA(sr, 1.0f); yield return new WaitForSeconds(0.15f);
            SetA(sr, 0.5f); yield return new WaitForSeconds(0.05f);
            SetA(sr, 1.0f); yield return new WaitForSeconds(0.12f);
        }

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            SetA(sr, 1f - t / fadeTime);
            yield return null;
        }
        Destroy(go);
    }

    void SetA(SpriteRenderer sr, float a)
    {
        sr.color = new Color(color.r, color.g, color.b, a);
    }
}
