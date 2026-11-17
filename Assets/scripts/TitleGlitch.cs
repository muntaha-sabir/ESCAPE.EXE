using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Goes on: the "LogoGlitch" object in the Title scene (it sits exactly on top of the ESCAPE.EXE logo).
// Most of the time it is invisible, so you see the normal logo.
// Every few seconds it flickers through 2-4 glitch frames for a split second, then hides again.
// It stops as soon as the player clicks or presses a key to start.
[RequireComponent(typeof(SpriteRenderer))]
public class TitleGlitch : MonoBehaviour
{
    public Sprite[] glitchFrames;
    public float minWait = 1.2f;       // shortest time between glitches
    public float maxWait = 3f;         // longest time between glitches
    public float frameTime = 0.05f;    // how long each glitch frame shows

    SpriteRenderer sr;
    Vector3 basePos;
    bool stopped;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        basePos = transform.localPosition;
        sr.enabled = false;
        StartCoroutine(Loop());
    }

    void Update()
    {
        if (stopped) return;
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if ((kb != null && kb.anyKey.wasPressedThisFrame) || (mouse != null && mouse.leftButton.wasPressedThisFrame))
        {
            stopped = true;
            sr.enabled = false;
            transform.localPosition = basePos;
        }
    }

    IEnumerator Loop()
    {
        if (glitchFrames == null || glitchFrames.Length == 0) yield break;
        yield return new WaitForSeconds(0.6f);
        while (!stopped)
        {
            int count = Random.Range(2, 5);
            GameAudio.Play("glitch_blip", 0.22f, 0.2f);
            for (int i = 0; i < count && !stopped; i++)
            {
                sr.sprite = glitchFrames[Random.Range(0, glitchFrames.Length)];
                sr.enabled = true;
                transform.localPosition = basePos + new Vector3(Random.Range(-1, 2) / 16f, 0f, 0f);
                yield return new WaitForSeconds(frameTime);
            }
            sr.enabled = false;
            transform.localPosition = basePos;
            yield return new WaitForSeconds(Random.Range(minWait, maxWait));
        }
    }
}
