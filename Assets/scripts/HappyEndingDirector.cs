using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Goes on: the "HappyEndingDirector" object in Ending_Return (the HAPPY ending - RESTORE PIP ONLY).
//  1. Black... back to Sam's desktop. Narration "[Sam has finally finished the game]".
//  2. Sam's red cursor opens Super Jump Quest 3. The window maximizes to the whole screen (like Level 1).
//  3. The player walks Pip to the right; the world scrolls along. No errors this time. Stars to grab.
//  4. Pip grabs the floating key -> he walks on by himself to his little home.
//  5. The key opens the door, Pip jumps for joy, waves, "Thank you!", and goes inside. The light turns on.
//  6. "[Pip made it home.]"  THE END  "Ending: Happy."  Click to play again.
public class HappyEndingDirector : MonoBehaviour
{
    [Header("Desktop part")]
    public GameObject desktopRoot;              // wallpaper, icons, NOTES, taskbar
    public SamCursor samCursor;
    public float cursorSpeed = 9f;              // normal pace
    public SpriteRenderer gameIcon;             // SJQ3 icon
    public GameObject gameWindow;               // small SJQ3 window (starts hidden)
    public string startLine = "[Sam has finally finished the game]";
    public float startLineHold = 1.6f;
    public float startDelay = 1.2f;             // after the screen starts fading in
    public float maximizeTime = 0.6f;

    [Header("Game part")]
    public GameObject gameRoot;                 // the level (starts hidden)
    public Transform pip;
    public Vector3 pipStart = new Vector3(-10f, -4f, 0f);
    public Transform world;                     // everything that scrolls 1:1 (ground, blocks, key, home)
    public Transform parallax;                  // bushes band, scrolls slower
    public float parallaxFactor = 0.5f;
    public float scrollAnchorX = -3f;           // Pip stays about here while the world moves
    public float maxScroll = 44f;
    public string controlsHint = "a / d  move   space  jump";
    public Transform[] stars;                   // little stars to grab on the way
    public Sprite[] sparkleFrames;
    public float grabDistance = 0.95f;

    [Header("The key")]
    public Transform key;
    public Collider2D keyGate;                  // invisible wall after the key (removed once Pip has it)

    [Header("Home")]
    public SpriteRenderer home;
    public Sprite[] homeFrames;                 // 0 closed, 1 door open, 2 lights on
    public Vector3 doorOffset = new Vector3(-0.0625f, -2f, 0f);       // door centre, from the home's centre
    public Vector3 windowOffset = new Vector3(1.5625f, -1.25f, 0f);
    public float standBeforeDoor = 4.2f;        // where Pip stops (left of the house)
    public float walkSpeed = 3.2f;
    public Sprite[] hopFrames;                  // Pip's joy hop (6 frames)
    public float hopForce = 8.5f;
    public Sprite[] waveFrames;                 // Pip waving (2 frames)
    public Sprite idleFrame;
    public GameObject thankYouBubble;           // "Thank you!" (starts hidden)

    [Header("The end")]
    public string endLine = "[Pip made it home.]";
    public SpriteRenderer theEnd;               // "THE END" (starts hidden)
    public NotesTyper endTyper;                 // "Ending: Happy." under THE END
    [Header("Final black screen: Pip walks in at the bottom, hops twice and waves")]
    public SpriteRenderer endPip;               // starts hidden
    public Sprite[] walkFrames;
    public Vector3 endPipFrom = new Vector3(-12f, -5f, 0f);
    public Vector3 endPipTo = new Vector3(0f, -5f, 0f);
    public float endPipWalkTime = 3.2f;
    public NotesTyper endTyper1;
    public string endLine1 = "You helped Pip get back home, Sam completed the game.";
    public NotesTyper endTyper2;
    public string endLine2 = "Sam remembers Pip.";
    public string endingLabel = "Ending: Happy.";
    public string playAgainHint = "click to play again";
    public string titleScene = "Title";

    PlayerController pc; Rigidbody2D rb; SpriteRenderer pipSR; PipAnimator anim;
    float scrolled, keyBaseY;
    bool playing, hasKey, canRestart, restarting;

    IEnumerator Start()
    {
        pc = pip.GetComponent<PlayerController>(); rb = pip.GetComponent<Rigidbody2D>();
        pipSR = pip.GetComponent<SpriteRenderer>(); anim = pip.GetComponent<PipAnimator>();
        pc.enabled = false; pipSR.enabled = false; rb.simulated = false;
        if (gameRoot != null) gameRoot.SetActive(false);
        if (gameWindow != null) gameWindow.SetActive(false);
        if (thankYouBubble != null) thankYouBubble.SetActive(false);
        if (theEnd != null) theEnd.gameObject.SetActive(false);
        if (endPip != null) endPip.gameObject.SetActive(false);
        if (key != null) keyBaseY = key.localPosition.y;

        // --- back on Sam's desktop
        yield return new WaitForSeconds(startDelay);
        Narrator.Story(startLine, startLineHold);
        yield return new WaitForSeconds(0.3f);

        // --- Sam opens his finished game
        if (samCursor != null && gameIcon != null)
        {
            Vector3 p = gameIcon.bounds.center; p.z = 0f;
            yield return StartCoroutine(samCursor.MoveTipTo(p, cursorSpeed));
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(samCursor.Click());
            yield return new WaitForSeconds(0.08f);
            yield return StartCoroutine(samCursor.Click());
        }
        yield return PopIn(gameWindow);
        yield return new WaitForSeconds(0.45f);

        // --- ...and maximizes it to the whole screen, like Level 1
        if (gameWindow != null)
        {
            Transform t = gameWindow.transform;
            Vector3 p0 = t.position, p1 = new Vector3(0f, 0.5625f, 0f);
            Vector3 s1 = new Vector3(26f / 14f, 14f / 9f, 1f);
            float k = 0f;
            while (k < maximizeTime)
            {
                k += Time.deltaTime;
                float e = Mathf.Clamp01(k / maximizeTime); e = e * e * (3f - 2f * e);
                t.position = Vector3.Lerp(p0, p1, e);
                t.localScale = Vector3.Lerp(Vector3.one, s1, e);
                yield return null;
            }
        }
        var flash = Cover(Color.white, 1f);
        GameAudio.Play("whoosh", 0.7f); GameAudio.Music("happy_game", 0.5f, 0.5f); GameAudio.Ambience("", 0f);
        if (samCursor != null) samCursor.gameObject.SetActive(false);
        if (gameWindow != null) gameWindow.SetActive(false);
        if (desktopRoot != null) desktopRoot.SetActive(false);
        if (gameRoot != null) gameRoot.SetActive(true);
        pip.position = pipStart;
        pipSR.enabled = true; rb.simulated = true; rb.linearVelocity = Vector2.zero; pc.enabled = true;
        yield return Fade(flash, Color.white, 1f, 0f, 0.7f);
        Destroy(flash.gameObject);

        // --- the player walks Pip home
        playing = true;
        Narrator.Hint(controlsHint, Narrator.Icon.Key, 5f);
    }

    void Update()
    {
        // the key floats up and down
        if (key != null && !hasKey)
            key.localPosition = new Vector3(key.localPosition.x, keyBaseY + Mathf.Sin(Time.time * 3f) * 0.15f, 0f);

        if (playing)
        {
            Vector3 pc0 = pip.position + new Vector3(0f, 0.85f, 0f);
            if (stars != null)
                foreach (var s in stars)
                    if (s != null && s.gameObject.activeSelf && Vector2.Distance(pc0, s.position) < grabDistance)
                    {
                        if (sparkleFrames != null && sparkleFrames.Length > 0) FX.PlayOnce(sparkleFrames, s.position);
                        s.gameObject.SetActive(false);
                        GameAudio.Play("coin", 0.6f, 0.04f);
                    }
            if (!hasKey && key != null && Vector2.Distance(pc0, key.position) < grabDistance)
                StartCoroutine(GoHome());
        }

        if (canRestart && !restarting && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            StartCoroutine(Restart());
    }

    // The world moves left instead of the camera moving right (the monitor frame and narration stay put).
    void FixedUpdate()
    {
        if (rb == null || !rb.simulated || world == null) return;
        float over = rb.position.x - scrollAnchorX;
        if (over <= 0f || scrolled >= maxScroll) return;
        float d = Mathf.Min(over, maxScroll - scrolled);
        scrolled += d;
        rb.position = new Vector2(rb.position.x - d, rb.position.y);
        world.position += new Vector3(-d, 0f, 0f);
        if (parallax != null) parallax.position += new Vector3(-d * parallaxFactor, 0f, 0f);
    }

    IEnumerator GoHome()
    {
        hasKey = true; playing = false;
        GameAudio.Play("key_get", 0.8f, 0f);
        Narrator.ClearHint();
        pc.enabled = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (sparkleFrames != null && sparkleFrames.Length > 0) FX.PlayOnce(sparkleFrames, key.position);
        if (keyGate != null) keyGate.enabled = false;

        // the key hops above Pip's head and comes along
        key.SetParent(pip, true);
        Vector3 k0 = key.localPosition, k1 = new Vector3(0f, 2.4f, 0f);
        for (float t = 0f; t < 0.35f; t += Time.deltaTime) { key.localPosition = Vector3.Lerp(k0, k1, t / 0.35f); yield return null; }
        key.localPosition = k1;
        while (Mathf.Abs(rb.linearVelocity.y) > 0.05f) yield return null;      // land first
        yield return new WaitForSeconds(0.5f);

        // Pip walks on by himself
        pipSR.flipX = false;
        while (pip.position.x < (home.transform.position.x - standBeforeDoor))
        {
            rb.linearVelocity = new Vector2(walkSpeed, rb.linearVelocity.y);
            yield return null;
        }
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        yield return new WaitForSeconds(0.6f);

        // the key opens the door
        key.SetParent(world, true);
        Vector3 a = key.position, b = home.transform.position + doorOffset;
        for (float t = 0f; t < 0.6f; t += Time.deltaTime) { key.position = Vector3.Lerp(a, b, t / 0.6f) + new Vector3(0f, Mathf.Sin(t / 0.6f * Mathf.PI) * 0.8f, 0f); yield return null; }
        if (sparkleFrames != null && sparkleFrames.Length > 0) FX.PlayOnce(sparkleFrames, b);
        key.gameObject.SetActive(false);
        if (homeFrames != null && homeFrames.Length > 1) home.sprite = homeFrames[1];
        GameAudio.Play("door", 0.7f, 0f);
        yield return new WaitForSeconds(0.6f);

        // jumps for joy (twice)
        if (anim != null) anim.enabled = false;
        for (int h = 0; h < 2; h++)
        {
            pipSR.sprite = hopFrames[0]; yield return new WaitForSeconds(0.08f);
            rb.linearVelocity = new Vector2(0f, hopForce);
            GameAudio.Play("jump", 0.7f);
            pipSR.sprite = hopFrames[3];
            yield return new WaitForSeconds(0.2f);
            if (sparkleFrames != null && sparkleFrames.Length > 0) FX.PlayOnce(sparkleFrames, pip.position + new Vector3(0f, 2.2f, 0f));
            while (rb.linearVelocity.y > -0.05f) yield return null;             // going up
            pipSR.sprite = hopFrames[4];
            while (Mathf.Abs(rb.linearVelocity.y) > 0.05f) yield return null;   // coming down
            pipSR.sprite = hopFrames[5]; yield return new WaitForSeconds(0.1f);
            pipSR.sprite = idleFrame; yield return new WaitForSeconds(0.15f);
        }
        yield return new WaitForSeconds(0.3f);

        // waves... "Thank you!"
        if (thankYouBubble != null)
        {
            thankYouBubble.transform.position = pip.position + new Vector3(0f, 2.75f, 0f);
            thankYouBubble.SetActive(true);
            GameAudio.Play("joy", 0.6f, 0f);
        }
        for (int w = 0; w < 8; w++) { pipSR.sprite = waveFrames[w % 2]; yield return new WaitForSeconds(0.22f); }
        pipSR.sprite = idleFrame;
        yield return new WaitForSeconds(0.5f);
        if (thankYouBubble != null) thankYouBubble.SetActive(false);

        // ...and goes home
        if (anim != null) anim.enabled = true;
        float doorX = home.transform.position.x + doorOffset.x;
        while (pip.position.x < doorX)
        {
            rb.linearVelocity = new Vector2(walkSpeed * 0.8f, rb.linearVelocity.y);
            yield return null;
        }
        rb.linearVelocity = Vector2.zero; rb.simulated = false;
        if (anim != null) anim.enabled = false;
        Color c0 = pipSR.color;
        for (float t = 0f; t < 0.5f; t += Time.deltaTime) { pipSR.color = new Color(c0.r, c0.g, c0.b, 1f - t / 0.5f); yield return null; }
        pipSR.enabled = false;
        yield return new WaitForSeconds(0.3f);
        if (homeFrames != null && homeFrames.Length > 2)
        {
            home.sprite = homeFrames[0];
            yield return new WaitForSeconds(0.6f);
            home.sprite = homeFrames[2];                                     // the light turns on
            GameAudio.Play("letter_collect", 0.5f, 0f);
            if (sparkleFrames != null && sparkleFrames.Length > 0) FX.PlayOnce(sparkleFrames, home.transform.position + windowOffset);
        }
        yield return new WaitForSeconds(0.8f);
        Narrator.Story(endLine, 600f);
        yield return new WaitForSeconds(3.2f);

        // --- THE END
        var black = Cover(Color.black, 0f);
        yield return Fade(black, Color.black, 0f, 1f, 1.5f);
        Narrator.ClearStory();
        GameAudio.Music("happy_game", 0.5f, 0.5f);         // the cheery SJQ music on the end screen
        yield return new WaitForSeconds(0.6f);

        // Pip walks in at the bottom of the black screen... the last words type out
        Coroutine dance = endPip != null ? StartCoroutine(EndPipDance()) : null;
        yield return new WaitForSeconds(endPipWalkTime * 0.7f);
        if (endTyper1 != null) { yield return StartCoroutine(endTyper1.Type(endLine1)); yield return new WaitForSeconds(0.8f); endTyper1.HideCaret(); }
        if (endTyper2 != null) { yield return StartCoroutine(endTyper2.Type(endLine2)); yield return new WaitForSeconds(0.8f); endTyper2.HideCaret(); }
        if (dance != null) yield return dance;
        yield return new WaitForSeconds(0.6f);

        if (theEnd != null)
        {
            theEnd.gameObject.SetActive(true);
            for (float t = 0f; t < 1.5f; t += Time.deltaTime) { theEnd.color = new Color(1f, 1f, 1f, Mathf.Clamp01(t / 1.5f)); yield return null; }
        }
        yield return new WaitForSeconds(0.8f);
        if (endTyper != null) { yield return StartCoroutine(endTyper.Type(endingLabel)); yield return new WaitForSeconds(0.8f); endTyper.HideCaret(); }
        yield return new WaitForSeconds(2f);
        Narrator.Hint(playAgainHint, Narrator.Icon.Mouse, 600f);
        canRestart = true;
    }

    IEnumerator EndPipDance()
    {
        endPip.gameObject.SetActive(true);
        Transform t = endPip.transform;
        // walk in from the lower left
        for (float k = 0f; k < endPipWalkTime; k += Time.deltaTime)
        {
            float p = k / endPipWalkTime;
            t.position = Vector3.Lerp(endPipFrom, endPipTo, p);
            endPip.sprite = walkFrames[(int)(k * 8f) % walkFrames.Length];
            endPip.color = new Color(1f, 1f, 1f, Mathf.Clamp01(k / 0.4f));
            yield return null;
        }
        t.position = endPipTo; endPip.sprite = idleFrame; endPip.color = Color.white;
        yield return new WaitForSeconds(0.5f);
        // two happy hops
        for (int h = 0; h < 2; h++)
        {
            endPip.sprite = hopFrames[0]; yield return new WaitForSeconds(0.08f);
            endPip.sprite = hopFrames[3];
            GameAudio.Play("jump", 0.6f);
            for (float k = 0f; k < 0.45f; k += Time.deltaTime)
            {
                float p = k / 0.45f;
                t.position = endPipTo + new Vector3(0f, 4f * p * (1f - p) * 1.1f, 0f);
                if (p > 0.5f) endPip.sprite = hopFrames[4];
                yield return null;
            }
            t.position = endPipTo;
            if (sparkleFrames != null && sparkleFrames.Length > 0) FX.PlayOnce(sparkleFrames, endPipTo + new Vector3(0f, 2f, 0f), 16f, "Frame", 47);
            endPip.sprite = hopFrames[5]; yield return new WaitForSeconds(0.1f);
            endPip.sprite = idleFrame; yield return new WaitForSeconds(0.2f);
        }
        // a little wave
        for (int w = 0; w < 10; w++) { endPip.sprite = waveFrames[w % 2]; yield return new WaitForSeconds(0.22f); }
        endPip.sprite = idleFrame;
    }

    IEnumerator Restart()
    {
        restarting = true;
        Narrator.ClearHint();
        Narrator.ClearStory();
        yield return new WaitForSeconds(0.4f);
        GameState.ResetAll();
        GameAudio.Music("", 0f, 0.5f);
        SceneManager.LoadScene(titleScene);
    }

    // ---------- helpers ----------
    SpriteRenderer Cover(Color c, float a)
    {
        var go = new GameObject("Cover");
        go.transform.position = new Vector3(0f, 0.5625f, 0f);
        go.transform.localScale = new Vector3(26f, 14f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = FX.WhitePixel;
        sr.sortingLayerName = "Frame";
        sr.sortingOrder = 40;                   // under the narration box (49+) and THE END
        sr.color = new Color(c.r, c.g, c.b, a);
        return sr;
    }

    IEnumerator Fade(SpriteRenderer sr, Color c, float from, float to, float time)
    {
        for (float t = 0f; t < time; t += Time.deltaTime) { sr.color = new Color(c.r, c.g, c.b, Mathf.Lerp(from, to, t / time)); yield return null; }
        sr.color = new Color(c.r, c.g, c.b, to);
    }

    IEnumerator PopIn(GameObject w)
    {
        if (w == null) yield break;
        GameAudio.Play("pop_up", 0.7f);
        w.SetActive(true);
        Transform t = w.transform;
        for (float k = 0f; k < 0.25f; k += Time.deltaTime)
        {
            float p = Mathf.Clamp01(k / 0.25f); p = p * p * (3f - 2f * p);
            float s = Mathf.Lerp(0.6f, 1f, p);
            t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        t.localScale = Vector3.one;
    }
}
