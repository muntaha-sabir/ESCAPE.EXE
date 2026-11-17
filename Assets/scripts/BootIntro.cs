using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Goes on: the Main Camera in the Boot scene.
// Space, Enter or a mouse click -> the PLAY button presses down, the camera dives into
// Sam's monitor, the screen flashes white, and Level 1 loads.
// The white flash is made by this script automatically - nothing to set up for it.
public class BootIntro : MonoBehaviour
{
    public string firstLevel = "Level1_Game";

    [Header("PLAY button (optional)")]
    public SpriteRenderer playButton;     // drag the PlayButton object here
    public Sprite playPressed;            // drag sjq_play_button_pressed here
    public AudioSource clickSound;        // optional

    [Header("Timing")]
    public float waitBeforeInput = 1f;    // lets the judges look at the room first

    [Header("Dive into the monitor")]
    public Vector2 zoomTarget = new Vector2(0.03125f, 1.40625f);  // centre of Sam's screen
    public float zoomEndSize = 5.6f;      // the screen fills the view
    public float zoomTime = 1.2f;

    bool started;
    Camera cam;
    Behaviour pixelPerfect;
    SpriteRenderer flash;

    void Start()
    {
        cam = GetComponent<Camera>();
        pixelPerfect = GetComponent("PixelPerfectCamera") as Behaviour;
        flash = MakeWhiteFlash();
    }

    void Update()
    {
        if (started) return;
        if (waitBeforeInput > 0f) { waitBeforeInput -= Time.deltaTime; return; }

        var kb = Keyboard.current;
        var mouse = Mouse.current;
        bool go = (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
               || (mouse != null && mouse.leftButton.wasPressedThisFrame);

        if (go) { started = true; StartCoroutine(Dive()); }
    }

    IEnumerator Dive()
    {
        if (playButton != null && playPressed != null) playButton.sprite = playPressed;
        if (clickSound != null) clickSound.Play();
        GameAudio.Play("coin", 0.5f, 0f);
        yield return new WaitForSeconds(0.25f);
        GameAudio.Play("whoosh", 0.8f, 0f, 0.8f);

        if (pixelPerfect != null) pixelPerfect.enabled = false;   // otherwise it blocks the zoom

        float startSize = cam.orthographicSize;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(zoomTarget.x, zoomTarget.y, startPos.z);

        float t = 0f;
        while (t < zoomTime)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / zoomTime);
            cam.orthographicSize = Mathf.Lerp(startSize, zoomEndSize, k);
            transform.position = Vector3.Lerp(startPos, endPos, k);
            flash.color = new Color(1f, 1f, 1f, Mathf.InverseLerp(0.6f, 1f, t / zoomTime));
            yield return null;
        }

        GameState.ResetAll();
        SceneManager.LoadScene(firstLevel);
    }

    SpriteRenderer MakeWhiteFlash()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        var go = new GameObject("WhiteFlash");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0f, 10f);
        go.transform.localScale = new Vector3(200f, 200f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        sr.sortingLayerName = "UI";
        sr.sortingOrder = 100;
        sr.color = new Color(1f, 1f, 1f, 0f);
        return sr;
    }
}
