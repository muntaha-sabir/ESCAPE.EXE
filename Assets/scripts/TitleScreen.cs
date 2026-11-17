using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Goes on: the Main Camera in the Title scene (the very first scene of ESCAPE.EXE).
// "CLICK TO START" blinks. Any click or key -> quick fade to black -> loads the Boot scene
// (Sam's desk with Super Jump Quest on the monitor).
// It waits for the click forever, so it can stay on screen while the team gives the intro.
public class TitleScreen : MonoBehaviour
{
    public string nextScene = "Boot";
    public SpriteRenderer clickToStart;   // drag the ClickToStart object here
    public float blinkSpeed = 1.6f;       // blinks per second
    public float fadeTime = 0.6f;
    public AudioSource startSound;        // optional

    bool started;
    SpriteRenderer fade;

    void Start()
    {
        fade = MakeFade();
    }

    void Update()
    {
        if (started) return;

        if (clickToStart != null)
            clickToStart.enabled = Mathf.Repeat(Time.time * blinkSpeed, 1f) < 0.65f;

        var kb = Keyboard.current;
        var mouse = Mouse.current;
        bool go = (kb != null && kb.anyKey.wasPressedThisFrame)
               || (mouse != null && mouse.leftButton.wasPressedThisFrame);

        if (go) { started = true; StartCoroutine(FadeOut()); }
    }

    IEnumerator FadeOut()
    {
        if (clickToStart != null) clickToStart.enabled = true;
        if (startSound != null) startSound.Play();
        GameAudio.Play("whoosh", 0.6f);

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            fade.color = new Color(0f, 0f, 0f, t / fadeTime);
            yield return null;
        }
        SceneManager.LoadScene(nextScene);
    }

    SpriteRenderer MakeFade()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        var go = new GameObject("Fade");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0f, 10f);
        go.transform.localScale = new Vector3(200f, 200f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.sortingLayerName = "UI";
        sr.sortingOrder = 100;
        sr.color = new Color(0f, 0f, 0f, 0f);
        return sr;
    }
}
