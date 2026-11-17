using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Goes on: the "EvilEndingDirector" object in Ending_Creator (ENDING: UNEXPECTED - RESTORE EVERYTHING).
// Everything here is automatic - the player only watches.
//  A. Maximized NOTES. Pip starts to reply... P!P.exe drops in and cracks open. P!P deletes Pip
//     (ITEM DELETED pops up), then types as Pip: "Yes Sam, it's me." Sam: "Okay, I'll give you a happy ending."
//  B. Sam opens SJQ3. The level is purple and wrong. P!P walks to Pip's home, stares at you, goes inside.
//  C. A huge glitch. Sam and P!P swap places: tiny Sam is now trapped in the game.
//     The game shrinks back to a window. The cursor is purple now (P!P is outside) and deletes Sam's game.
//  D. Flashback: a tape rewinds to the Recycle Bin... your cursor clicks RESTORE EVERYTHING.
//     "Not everything was meant to be restored..."
//  E. Black screen, final words, THE END?  "Ending: Unexpected."  Click to play again.
public class EvilEndingDirector : MonoBehaviour
{
    [Header("Sounds")]
    public AudioClip music;                     // tense loop
    [Range(0f, 1f)] public float musicVolume = 0.55f;
    public AudioClip glitchBurst, glitchBlip, crack, deleteZap, sting, rewind, click;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    [Header("A. The NOTES")]
    public GameObject notesRoot;
    public NotesTyper samLine1, pipLabel, pipText, samLabel, samText;
    public string samSaid = "Pip..? Is that you?";
    public string pipCut = "Ye~_/!";
    public string pipFake = "Yes Sam, it's me.";
    public string samReply = "Okay, I'll give you a happy ending.";
    public SpriteRenderer pip;
    public Sprite[] pipIdle, pipGlitch, pipDelete;
    public SpriteRenderer pipexe;               // P!P (starts hidden)
    public Sprite[] pxIdle, pxWalk, pxGlitch;
    public Sprite pxStare;
    public SpriteRenderer exeIcon;              // the P!P.exe file (starts hidden)
    public Sprite[] crackFrames, poofFrames;
    public SpriteRenderer bubble;               // ? / ! over Pip (starts hidden)
    public Sprite[] confusedFrames, exclaimFrames;
    public GameObject itemDeletedPip;           // "ITEM DELETED pip.png" (starts hidden)
    public SpriteRenderer glitchOverlay;        // full-screen glitch
    public Sprite[] screenGlitch;
    public string crackLine = "[That's not supposed to happen...]";
    public string fakeLine = "[...]";

    [Header("B. The game, purple and wrong")]
    public GameObject desktopRoot;
    public SamCursor cursor;
    public float cursorSpeed = 9f;
    public SpriteRenderer gameIcon;
    public GameObject gameWindow;
    public GameObject gameRoot;
    public Transform world, parallax;
    public float maxScroll = 44f, scrollSpeed = 6f;
    public SpriteRenderer home;
    public Sprite[] homeFrames;                 // evil: 0 closed (wilted) / 1 open / 2 red window
    public Vector3 doorOffset = new Vector3(-0.0625f, -2f, 0f);
    public SpriteRenderer walker;               // P!P inside the game, later tiny Sam
    public Color tint = new Color(0.78f, 0.63f, 1f, 1f);
    public string levelLine = "[...Something doesn't feel right..]";

    [Header("C. The swap")]
    public string glitchLine = "[Wait- What's going on??!-]";
    public Sprite[] samIdle, samWalk;
    public GameObject trappedWindow;
    public Vector2 windowCloseLocal = new Vector2(6.1875f, 4.0625f);
    public Sprite[] purpleCursor;               // 0 normal, 1 clicking
    public SpriteRenderer recycleBin;
    public Sprite binFull;
    public GameObject itemDeletedGame;          // "ITEM DELETED SJQ3.exe" (starts hidden)

    [Header("D. Flashback")]
    public SpriteRenderer flashback;            // full-screen tape frames (starts hidden)
    public Sprite[] flashFrames;                // 0-5 rewind, 6 the choice, 7 the button pressed
    public SpriteRenderer playerCursor;
    public Sprite[] playerCursorFrames;
    public Vector3 restoreButton = new Vector3(0f, 0.6875f, 0f);
    public NotesTyper bigTyper;
    public string flashLine = "Not everything was meant to be restored...";

    [Header("E. The end")]
    public SpriteRenderer endSam, endBubble;
    public NotesTyper endTyper1, endTyper2, labelTyper;
    public string endLine1 = "You restored everything, including what should've been left deleted.";
    public string endLine2 = "Now Sam is in the computer, P!P.exe is free in the world";
    public string endingLabel = "Ending: Unexpected.";
    public SpriteRenderer theEnd;
    public Sprite theEndPlain, theEndQ;
    public string playAgainHint = "click to play again";
    public string titleScene = "Title";

    AudioSource sfx, mus;
    float musTarget;
    bool canRestart, restarting;
    static readonly Vector3 TipOffset = new Vector3(-6f / 16f, 9f / 16f, 0f);

    IEnumerator Start()
    {
        sfx = gameObject.AddComponent<AudioSource>(); sfx.playOnAwake = false; sfx.spatialBlend = 0f;
        GameAudio.Music("", 0f, 0.3f);
        mus = gameObject.AddComponent<AudioSource>(); mus.playOnAwake = false; mus.loop = true; mus.clip = music; mus.volume = 0f; mus.spatialBlend = 0f;
        foreach (var g in new GameObject[] { desktopRoot, gameWindow, gameRoot, trappedWindow, itemDeletedPip, itemDeletedGame })
            if (g != null) g.SetActive(false);
        foreach (var r in new SpriteRenderer[] { pipexe, exeIcon, bubble, walker, flashback, playerCursor, endSam, endBubble, theEnd })
            if (r != null) r.gameObject.SetActive(false);
        if (cursor != null) cursor.gameObject.SetActive(false);
        if (glitchOverlay != null) glitchOverlay.color = new Color(1f, 1f, 1f, 0f);

        // ================= A. the NOTES =================
        StartCoroutine(Glitch(0.4f, 0.8f, false));
        Play(glitchBlip);
        StartCoroutine(Animate(pip, pipIdle, 3f));
        yield return StartCoroutine(samLine1.Type(samSaid));
        yield return new WaitForSeconds(0.8f);
        yield return StartCoroutine(pipLabel.Type("[Pip]:")); pipLabel.HideCaret();
        yield return StartCoroutine(pipText.Type(pipCut));
        // the reply breaks
        Play(glitchBlip);
        StartCoroutine(Glitch(0.25f, 0.7f, false));
        yield return Flicker(pip, pipGlitch, pipIdle[0]);
        pipText.HideCaret();
        yield return new WaitForSeconds(0.6f);

        // P!P.exe drops onto the page and cracks open
        Vector3 land = exeIcon.transform.position, top = land + new Vector3(0f, 12f, 0f);
        exeIcon.gameObject.SetActive(true);
        for (float t = 0f; t < 0.5f; t += Time.deltaTime) { float p = t / 0.5f; exeIcon.transform.position = Vector3.Lerp(top, land, p * p); yield return null; }
        exeIcon.transform.position = land; Play(glitchBlip);
        for (float t = 0f; t < 0.5f; t += Time.deltaTime) { exeIcon.transform.position = land + new Vector3(Random.Range(-0.08f, 0.08f), 0f, 0f); yield return null; }
        exeIcon.transform.position = land;
        Narrator.Story(crackLine, 3.5f);
        Play(crack);
        GameAudio.TapeStop(0.8f);                                         // the calm desktop tune dies
        StartMusic(0.6f, 2f);
        if (crackFrames != null && crackFrames.Length > 0) FX.PlayOnce(crackFrames, land, 12f, "UI", 70);
        yield return new WaitForSeconds(0.35f);
        if (poofFrames != null && poofFrames.Length > 0) FX.PlayOnce(poofFrames, land, 16f, "UI", 71);
        exeIcon.gameObject.SetActive(false);
        Vector3 pxPos = new Vector3(land.x, pip.transform.position.y, 0f);
        pipexe.transform.position = pxPos;
        pipexe.gameObject.SetActive(true);
        anims[pipexe] = StartCoroutine(Identity(pipexe));
        yield return new WaitForSeconds(0.6f);

        // Pip: ?   ...P!P walks over...   Pip: !
        ShowBubble(confusedFrames);
        yield return new WaitForSeconds(1.1f);
        StopAnimate(pipexe);
        pipexe.flipX = true;
        ShowBubble(exclaimFrames);
        Vector3 nearPip = pip.transform.position + new Vector3(1.25f, 0f, 0f);
        yield return Walk(pipexe, pxWalk, nearPip, 2.2f);
        pipexe.sprite = pxIdle[0];

        // P!P touches Pip -> deleted, piece by piece
        yield return new WaitForSeconds(0.3f);
        bubble.gameObject.SetActive(false);
        Play(glitchBurst);
        StopAnimate(pip);
        yield return Flicker(pip, pipGlitch, pipGlitch[0]);
        Play(deleteZap);
        StartCoroutine(PopFor(itemDeletedPip, 0.8f));
        foreach (var f in pipDelete) { pip.sprite = f; yield return new WaitForSeconds(0.09f); }
        pip.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        Play(glitchBlip);
        yield return Flicker(pipexe, pxGlitch, pxIdle[0]);                // (glitch #1)

        // P!P takes Pip's place... and types as Pip
        yield return Walk(pipexe, pxWalk, pip.transform.position, 1.6f);
        pipexe.sprite = pxIdle[0]; pipexe.flipX = false;
        anims[pipexe] = StartCoroutine(Identity(pipexe));
        yield return new WaitForSeconds(0.4f);
        Narrator.Story(fakeLine, 3f);
        yield return StartCoroutine(pipText.Backspace());
        yield return StartCoroutine(pipText.Type(pipFake));
        pipText.HideCaret();
        yield return new WaitForSeconds(1.2f);
        yield return StartCoroutine(samLabel.Type("[Sam]:")); samLabel.HideCaret();
        yield return StartCoroutine(samText.Type(samReply));
        yield return new WaitForSeconds(1.6f);
        var black = Cover(Color.black, 0f);
        yield return Fade(black, Color.black, 0f, 1f, 1.2f);
        StopAnimate(pipexe);
        notesRoot.SetActive(false); pipexe.gameObject.SetActive(false);
        Narrator.ClearStory();
        yield return new WaitForSeconds(0.6f);

        // ================= B. Sam opens SJQ3 =================
        desktopRoot.SetActive(true);
        cursor.gameObject.SetActive(true);
        yield return Fade(black, Color.black, 1f, 0f, 1f);
        yield return CursorTo(gameIcon.bounds.center, cursorSpeed);
        yield return DoubleClick();
        yield return PopIn(gameWindow);
        yield return new WaitForSeconds(0.4f);
        yield return Resize(gameWindow.transform, gameWindow.transform.position, new Vector3(0f, 0.5625f, 0f), Vector3.one, new Vector3(26f / 14f, 14f / 9f, 1f), 0.6f);
        SetA(black, Color.white, 1f);
        gameWindow.SetActive(false); desktopRoot.SetActive(false); cursor.gameObject.SetActive(false);
        gameRoot.SetActive(true);
        foreach (var r in gameRoot.GetComponentsInChildren<SpriteRenderer>(true)) r.color = tint;
        GameAudio.MusicPitch(1f, 0.1f); GameAudio.Music("happy_game_evil", 0.7f, 0.3f);   // the cheery SJQ tune, but wrong
        walker.transform.position = new Vector3(-10f, -4.4375f, 0f);
        walker.gameObject.SetActive(true); walker.sprite = pxIdle[0]; walker.flipX = false;
        yield return Fade(black, Color.white, 1f, 0f, 0.7f);
        Narrator.Story(levelLine, 4f);
        musTarget = musicVolume * 0.8f;

        // P!P walks to Pip's home by itself, the world scrolls past
        yield return Walk(walker, pxWalk, new Vector3(-3f, -4.4375f, 0f), 3.2f);
        float scrolled = 0f, nextGlitch = 15f; int frame = 0; float ft = 0f;
        while (scrolled < maxScroll)
        {
            float d = Mathf.Min(scrollSpeed * Time.deltaTime, maxScroll - scrolled);
            scrolled += d;
            world.position += new Vector3(-d, 0f, 0f);
            if (parallax != null) parallax.position += new Vector3(-d * 0.5f, 0f, 0f);
            ft += Time.deltaTime; if (ft > 0.1f) { ft = 0f; frame++; walker.sprite = pxWalk[frame % pxWalk.Length]; }
            if (scrolled > nextGlitch && nextGlitch < 40f)
            {
                nextGlitch = nextGlitch > 20f ? 99f : 30f;
                Play(glitchBlip);
                StartCoroutine(Glitch(0.18f, 0.5f, false));
                if (nextGlitch > 50f) yield return Flicker(walker, pxGlitch, pxWalk[0]);   // (glitch #2)
            }
            yield return null;
        }
        Vector3 homePos = home.transform.position;
        yield return Walk(walker, pxWalk, new Vector3(homePos.x - 4.2f, -4.4375f, 0f), 3f);
        walker.sprite = pxIdle[0];
        yield return new WaitForSeconds(0.6f);

        // the stare
        walker.sprite = pxStare;
        Play(sting);
        yield return new WaitForSeconds(1.6f);
        walker.sprite = pxIdle[0];
        home.sprite = homeFrames[1];
        yield return new WaitForSeconds(0.4f);
        yield return Walk(walker, pxWalk, new Vector3(homePos.x + doorOffset.x, -4.4375f, 0f), 2.4f);
        for (float t = 0f; t < 0.5f; t += Time.deltaTime) { walker.color = new Color(1f, 1f, 1f, 1f - t / 0.5f); yield return null; }
        walker.gameObject.SetActive(false); walker.color = Color.white;
        home.sprite = homeFrames[0];
        yield return new WaitForSeconds(0.6f);
        home.sprite = homeFrames[2];
        yield return new WaitForSeconds(1f);

        // ================= C. the swap =================
        Narrator.Story(glitchLine, 3f);
        musTarget = musicVolume;
        yield return new WaitForSeconds(0.5f);
        Play(glitchBurst); Play(sting);
        GameAudio.TapeStop(1.2f);
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        Vector3 cam0 = cam != null ? cam.position : Vector3.zero;
        float gEnd = Time.time + 1.8f; float nextBurst = Time.time + 0.8f;
        while (Time.time < gEnd)
        {
            glitchOverlay.sprite = screenGlitch[Random.Range(0, screenGlitch.Length)];
            glitchOverlay.color = new Color(1f, 1f, 1f, Random.value < 0.25f ? 0.4f : 1f);
            if (cam != null) cam.position = cam0 + new Vector3(Random.Range(-0.18f, 0.18f), Random.Range(-0.12f, 0.12f), 0f);
            SetA(black, Random.value < 0.5f ? new Color(1f, 0.2f, 0.3f) : Color.white, Random.value < 0.15f ? 0.5f : 0f);
            if (Time.time > nextBurst) { Play(glitchBurst); nextBurst = 99999f; }
            yield return new WaitForSeconds(0.05f);
        }
        if (cam != null) cam.position = cam0;
        glitchOverlay.color = new Color(1f, 1f, 1f, 0f); SetA(black, Color.black, 0f);
        Narrator.ClearStory();
        home.sprite = homeFrames[0];
        yield return new WaitForSeconds(1f);

        // ...the door opens. Out comes - Sam?
        home.sprite = homeFrames[1];
        yield return new WaitForSeconds(0.4f);
        walker.transform.position = new Vector3(homePos.x + doorOffset.x, -4.4375f, 0f);
        walker.flipX = true; walker.gameObject.SetActive(true);
        yield return Walk(walker, samWalk, new Vector3(homePos.x - 4.2f, -4.4375f, 0f), 2f);
        walker.sprite = samIdle[0];
        home.sprite = homeFrames[0];
        bubble.transform.position = walker.transform.position + new Vector3(0f, 2.5f, 0f);
        ShowBubble(confusedFrames);
        yield return new WaitForSeconds(0.7f);
        walker.flipX = false; yield return new WaitForSeconds(0.5f);
        walker.flipX = true; yield return new WaitForSeconds(0.5f);
        ShowBubble(exclaimFrames);
        yield return new WaitForSeconds(1.2f);

        // the game shrinks back into a window... and the cursor is purple now
        SetA(black, Color.white, 1f);
        gameRoot.SetActive(false); walker.gameObject.SetActive(false); bubble.gameObject.SetActive(false);
        desktopRoot.SetActive(true);
        trappedWindow.SetActive(true);
        Vector3 wHome = trappedWindow.transform.position;
        trappedWindow.transform.position = new Vector3(0f, 0.5625f, 0f);
        trappedWindow.transform.localScale = new Vector3(26f / 14f, 14f / 9f, 1f);
        cursor.normal = purpleCursor[0]; cursor.clicking = purpleCursor[1];
        cursor.GetComponent<SpriteRenderer>().sprite = purpleCursor[0];
        cursor.transform.position = new Vector3(9f, -2.5f, 0f);
        cursor.gameObject.SetActive(true);
        yield return Fade(black, Color.white, 1f, 0f, 0.4f);
        GameAudio.MusicPitch(0.9f, 0.1f); GameAudio.Music("happy_game_evil", 0.35f, 1f);
        yield return Resize(trappedWindow.transform, new Vector3(0f, 0.5625f, 0f), wHome, trappedWindow.transform.localScale, Vector3.one, 0.7f);
        yield return new WaitForSeconds(1.2f);

        // P!P closes the game... and drags it into the Recycle Bin
        yield return CursorTo(trappedWindow.transform.TransformPoint(windowCloseLocal), cursorSpeed * 0.7f);
        yield return Click();
        yield return PopOut(trappedWindow);
        yield return new WaitForSeconds(0.5f);
        yield return CursorTo(gameIcon.bounds.center, cursorSpeed * 0.7f);
        var csr = cursor.GetComponent<SpriteRenderer>();
        csr.sprite = purpleCursor[1]; Play(click);
        Vector3 grab = gameIcon.transform.position - cursor.transform.position;
        Vector3 goal = recycleBin.bounds.center - TipOffset; goal.z = 0f;
        while (Vector3.Distance(cursor.transform.position, goal) > 0.02f)
        {
            cursor.transform.position = Vector3.MoveTowards(cursor.transform.position, goal, cursorSpeed * 0.6f * Time.deltaTime);
            gameIcon.transform.position = cursor.transform.position + grab;
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        csr.sprite = purpleCursor[0];
        gameIcon.gameObject.SetActive(false);
        if (binFull != null) recycleBin.sprite = binFull;
        Play(deleteZap);
        yield return PopFor(itemDeletedGame, 1.4f);
        yield return new WaitForSeconds(1.2f);

        // ================= D. flashback =================
        Play(rewind); GameAudio.Play("warp", 0.9f, 0f);
        GameAudio.Music("", 0f, 0.4f);
        musTarget = 0f;
        flashback.gameObject.SetActive(true);
        Vector3 fb0 = flashback.transform.position;
        for (int i = 0; i < 6; i++)
        {
            flashback.sprite = flashFrames[i];
            flashback.transform.position = fb0 + new Vector3(Random.Range(-0.12f, 0.12f), 0f, 0f);
            yield return new WaitForSeconds(0.16f);
        }
        flashback.transform.position = fb0;
        flashback.sprite = flashFrames[6];
        desktopRoot.SetActive(false); cursor.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.8f);
        playerCursor.gameObject.SetActive(true);
        playerCursor.sprite = playerCursorFrames[0];
        Vector3 pc0 = restoreButton + new Vector3(5f, -3.5f, 0f) - TipOffset, pc1 = restoreButton + new Vector3(0.35f, -0.05f, 0f) - TipOffset;
        for (float t = 0f; t < 2.2f; t += Time.deltaTime) { float p = t / 2.2f; p = p * p * (3f - 2f * p); playerCursor.transform.position = Vector3.Lerp(pc0, pc1, p); yield return null; }
        yield return new WaitForSeconds(0.5f);
        playerCursor.sprite = playerCursorFrames[1]; flashback.sprite = flashFrames[7]; Play(click);
        yield return new WaitForSeconds(0.15f);
        playerCursor.sprite = playerCursorFrames[0];
        yield return new WaitForSeconds(0.8f);
        var band = Cover(Color.black, 0f, 46);
        band.transform.position = new Vector3(0f, -4.25f, 0f); band.transform.localScale = new Vector3(26f, 2.4f, 1f);
        yield return Fade(band, Color.black, 0f, 0.7f, 0.4f);
        yield return StartCoroutine(bigTyper.Type(flashLine));
        yield return new WaitForSeconds(2.6f);
        bigTyper.HideCaret();

        // ================= E. the end =================
        var black2 = Cover(Color.black, 0f, 48);                         // above the flashback
        yield return Fade(black2, Color.black, 0f, 1f, 1.2f);
        flashback.gameObject.SetActive(false); playerCursor.gameObject.SetActive(false); bigTyper.Clear();
        Destroy(band.gameObject);
        SetA(black, Color.black, 1f); Destroy(black2.gameObject);
        mus.Stop();
        GameAudio.MusicPitch(1f, 0.1f); GameAudio.Music("desktop_theme", 0.45f, 1f);   // the desktop tune on the end screen
        yield return new WaitForSeconds(0.8f);
        endSam.gameObject.SetActive(true);
        StartCoroutine(Animate(endSam, samIdle, 2f));
        yield return new WaitForSeconds(0.8f);
        yield return StartCoroutine(endTyper1.Type(endLine1));
        yield return new WaitForSeconds(0.8f); endTyper1.HideCaret();
        yield return StartCoroutine(endTyper2.Type(endLine2));
        yield return new WaitForSeconds(0.8f); endTyper2.HideCaret();
        endBubble.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.2f);
        theEnd.sprite = theEndPlain; theEnd.gameObject.SetActive(true);
        for (float t = 0f; t < 1.5f; t += Time.deltaTime) { theEnd.color = new Color(1f, 1f, 1f, Mathf.Clamp01(t / 1.5f)); yield return null; }
        yield return new WaitForSeconds(0.7f);
        for (int i = 0; i < 5; i++)                                       // THE END -> THE END?
        {
            theEnd.sprite = (i % 2 == 0) ? theEndQ : theEndPlain;
            theEnd.transform.position += new Vector3(i % 2 == 0 ? 0.0625f : -0.0625f, 0f, 0f);
            if (i == 0) Play(glitchBlip);
            yield return new WaitForSeconds(0.07f);
        }
        theEnd.sprite = theEndQ;
        yield return new WaitForSeconds(0.8f);
        yield return StartCoroutine(labelTyper.Type(endingLabel));
        yield return new WaitForSeconds(0.8f); labelTyper.HideCaret();
        yield return new WaitForSeconds(2f);
        Narrator.Hint(playAgainHint, Narrator.Icon.Mouse, 600f);
        canRestart = true;
    }

    void Update()
    {
        if (mus != null && mus.clip != null)
        {
            if (mus.isPlaying) mus.volume = Mathf.MoveTowards(mus.volume, musTarget, Time.deltaTime * 0.35f);
            if (mus.isPlaying && musTarget <= 0f && mus.volume <= 0.001f) mus.Stop();
        }
        if (canRestart && !restarting && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            StartCoroutine(Restart());
    }

    IEnumerator Restart()
    {
        restarting = true;
        Narrator.ClearHint(); Narrator.ClearStory();
        yield return new WaitForSeconds(0.4f);
        GameState.ResetAll();
        SceneManager.LoadScene(titleScene);
    }

    // ---------- helpers ----------
    // only the team's own sounds: a click stays a click, everything else is silent
    void Play(AudioClip c)
    {
        if (c == null) return;
        if (c == click) { GameAudio.Play("click", 0.75f); return; }
        if (sfx != null) { sfx.pitch = 1f + Random.Range(-0.05f, 0.05f); sfx.PlayOneShot(c, sfxVolume); }
    }

    void StartMusic(float vol, float fade)
    {
        return;                                  // no music of our own in this ending
#pragma warning disable 0162
        if (mus == null || music == null) return;
        musTarget = musicVolume * vol;
        if (!mus.isPlaying) { mus.volume = 0f; mus.Play(); }
#pragma warning restore 0162
    }

    readonly System.Collections.Generic.Dictionary<SpriteRenderer, Coroutine> anims = new System.Collections.Generic.Dictionary<SpriteRenderer, Coroutine>();
    IEnumerator Animate(SpriteRenderer r, Sprite[] frames, float fps)
    {
        if (anims.ContainsKey(r) && anims[r] != null) StopCoroutine(anims[r]);
        anims[r] = StartCoroutine(Loop(r, frames, fps));
        yield break;
    }
    IEnumerator Loop(SpriteRenderer r, Sprite[] frames, float fps)
    {
        int i = 0;
        while (r != null && frames != null && frames.Length > 0) { r.sprite = frames[i % frames.Length]; i++; yield return new WaitForSeconds(1f / fps); }
    }
    // P!P is not stable: it flickers between P!P, Pip and glitch frames
    IEnumerator Identity(SpriteRenderer r)
    {
        int i = 0; Vector3 p0 = r.transform.position;
        while (r != null)
        {
            float roll = Random.value;
            if (roll < 0.16f) { r.sprite = pipIdle[0]; r.transform.position = p0; yield return new WaitForSeconds(Random.Range(0.08f, 0.18f)); }
            else if (roll < 0.32f)
            {
                r.sprite = pxGlitch[Random.Range(0, pxGlitch.Length)];
                r.transform.position = p0 + new Vector3(Random.value < 0.5f ? 0.0625f : -0.0625f, 0f, 0f);
                if (Random.value < 0.4f) Play(glitchBlip);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.12f));
                r.transform.position = p0;
            }
            else { r.sprite = pxIdle[i++ % pxIdle.Length]; yield return new WaitForSeconds(Random.Range(0.25f, 0.6f)); }
        }
    }

    void StopAnimate(SpriteRenderer r) { if (anims.ContainsKey(r) && anims[r] != null) { StopCoroutine(anims[r]); anims[r] = null; } }

    IEnumerator Walk(SpriteRenderer r, Sprite[] frames, Vector3 to, float speed)
    {
        StopAnimate(r);
        Transform t = r.transform; int f = 0; float ft = 0f; Sprite pick = null;
        if (to.x < t.position.x) r.flipX = true; else if (to.x > t.position.x) r.flipX = false;
        while (Vector3.Distance(t.position, to) > 0.02f)
        {
            t.position = Vector3.MoveTowards(t.position, to, speed * Time.deltaTime);
            ft += Time.deltaTime;
            if (ft > 0.1f) { ft = 0f; f++; pick = (r == pipexe && Random.value < 0.22f) ? (Random.value < 0.5f ? pipIdle[0] : pxGlitch[Random.Range(0, pxGlitch.Length)]) : null; }
            r.sprite = pick != null ? pick : frames[f % frames.Length];
            yield return null;
        }
        t.position = to;
    }

    IEnumerator Flicker(SpriteRenderer r, Sprite[] glitch, Sprite after)
    {
        if (r == null || glitch == null || glitch.Length == 0) yield break;
        Vector3 p0 = r.transform.position;
        float[] w = { 0.06f, 0.05f, 0.08f, 0.05f };
        for (int i = 0; i < w.Length; i++)
        {
            r.sprite = glitch[i % glitch.Length];
            r.transform.position = p0 + new Vector3(i % 2 == 0 ? 0.125f : -0.0625f, 0f, 0f);
            yield return new WaitForSeconds(w[i]);
        }
        r.transform.position = p0; r.sprite = after;
    }

    IEnumerator Glitch(float time, float alpha, bool shake)
    {
        if (glitchOverlay == null || screenGlitch == null || screenGlitch.Length == 0) yield break;
        float end = Time.time + time;
        while (Time.time < end)
        {
            glitchOverlay.sprite = screenGlitch[Random.Range(0, screenGlitch.Length)];
            glitchOverlay.color = new Color(1f, 1f, 1f, alpha);
            yield return new WaitForSeconds(0.05f);
        }
        glitchOverlay.color = new Color(1f, 1f, 1f, 0f);
    }

    void ShowBubble(Sprite[] frames)
    {
        if (bubble == null || frames == null || frames.Length == 0) return;
        bubble.gameObject.SetActive(true);
        StartCoroutine(Animate(bubble, frames, 3f));
    }

    IEnumerator PopFor(GameObject w, float time)
    {
        if (w == null) yield break;
        yield return PopIn(w);
        yield return new WaitForSeconds(time);
        yield return PopOut(w);
    }

    IEnumerator CursorTo(Vector3 point, float speed)
    {
        point.z = 0f;
        yield return StartCoroutine(cursor.MoveTipTo(point, speed));
        yield return new WaitForSeconds(0.15f);
    }

    IEnumerator Click() { yield return StartCoroutine(cursor.Click()); }

    IEnumerator DoubleClick()
    {
        yield return Click();
        yield return new WaitForSeconds(0.08f);
        yield return Click();
        yield return new WaitForSeconds(0.15f);
    }

    IEnumerator Resize(Transform t, Vector3 p0, Vector3 p1, Vector3 s0, Vector3 s1, float time)
    {
        for (float k = 0f; k < time; k += Time.deltaTime)
        {
            float e = Mathf.Clamp01(k / time); e = e * e * (3f - 2f * e);
            t.position = Vector3.Lerp(p0, p1, e); t.localScale = Vector3.Lerp(s0, s1, e);
            yield return null;
        }
        t.position = p1; t.localScale = s1;
    }

    SpriteRenderer Cover(Color c, float a, int order = 40)
    {
        var go = new GameObject("Cover");
        go.transform.position = new Vector3(0f, 0.5625f, 0f);
        go.transform.localScale = new Vector3(26f, 14f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = FX.WhitePixel;
        sr.sortingLayerName = "Frame";
        sr.sortingOrder = order;
        sr.color = new Color(c.r, c.g, c.b, a);
        return sr;
    }

    void SetA(SpriteRenderer sr, Color c, float a) { sr.color = new Color(c.r, c.g, c.b, a); }

    IEnumerator Fade(SpriteRenderer sr, Color c, float from, float to, float time)
    {
        for (float t = 0f; t < time; t += Time.deltaTime) { SetA(sr, c, Mathf.Lerp(from, to, t / time)); yield return null; }
        SetA(sr, c, to);
    }

    IEnumerator PopIn(GameObject w)
    {
        if (w == null) yield break;
        GameAudio.Play("pop_up", 0.6f);
        w.SetActive(true);
        Transform t = w.transform;
        for (float k = 0f; k < 0.22f; k += Time.deltaTime)
        {
            float p = Mathf.Clamp01(k / 0.22f); p = p * p * (3f - 2f * p);
            float s = Mathf.Lerp(0.6f, 1f, p);
            t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    IEnumerator PopOut(GameObject w)
    {
        if (w == null) yield break;
        Transform t = w.transform;
        for (float k = 0f; k < 0.16f; k += Time.deltaTime)
        {
            float s = Mathf.Lerp(1f, 0.6f, Mathf.Clamp01(k / 0.16f));
            t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        w.SetActive(false);
        t.localScale = Vector3.one;
    }
}
