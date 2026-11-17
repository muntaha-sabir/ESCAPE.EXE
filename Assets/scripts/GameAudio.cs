using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// ESCAPE.EXE - all game audio in one place. Nothing to set up in any scene:
//  * It creates itself before the first scene loads and lives through every scene.
//  * All clips are loaded from Assets/Resources/GameAudio (file name = sound name).
//  * Each scene picks its own music + ambience automatically (see OnScene below).
//  * Any script can call:  GameAudio.Play("jump");   GameAudio.Music("thrilly", 0.5f);
// Volume note: on the Web (WebGL) the browser only allows sound after the first click -
// the Title screen waits for a click anyway, so everything works.
public class GameAudio : MonoBehaviour
{
    static GameAudio I;
    readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    readonly Dictionary<string, float> lastPlayed = new Dictionary<string, float>();
    AudioSource[] pool; int next;
    AudioSource musA, musB, amb, layer, typing;
    bool musOnA = true;
    string musName = "", ambName = "", layerName = "";
    float musVol, ambVol, layerVol, fadeSpeed = 1f;
    int typers;
    AudioSource stopping;
    public static float Master = 1f;
    // ONLY the team's own audio files are allowed to play (anything else is ignored)
    static readonly HashSet<string> Allowed = new HashSet<string> {
        "jump", "click", "loose", "pop_up", "tick", "typing",            // effects
        "happy_game", "thrilly", "timer_recycle", "sad_violin",          // music
        "glitch_blip", "glitch_burst", "pipexe_crack", "fall_whistle",   // glitches + drama
        "evil_sting", "delete_zap", "monitor_off", "warp",
        "title_drone", "desktop_theme", "happy_game_evil" };             // backgrounds

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        if (I != null) return;
        var go = new GameObject("GameAudio");
        DontDestroyOnLoad(go);
        I = go.AddComponent<GameAudio>();
    }

    void Awake()
    {
        foreach (var c in Resources.LoadAll<AudioClip>("GameAudio")) clips[c.name] = c;
        pool = new AudioSource[12];
        for (int i = 0; i < pool.Length; i++) pool[i] = NewSource(false);
        musA = NewSource(true); musB = NewSource(true); amb = NewSource(true); layer = NewSource(true); typing = NewSource(true);
        SceneManager.sceneLoaded += (s, m) => OnScene(s.name);
    }

    AudioSource NewSource(bool loop)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.playOnAwake = false; s.loop = loop; s.spatialBlend = 0f; s.volume = 0f;
        return s;
    }

    // ---------- what each scene sounds like ----------
    void OnScene(string scene)
    {
        StopTyping(true);
        switch (scene)
        {
            case "Title":          MusicPitch(1f, 0.1f); Music("title_drone", 0.6f, 1.5f); break;
            // Sam's desktop: a soft, slightly eerie background tune
            case "Level2_Desktop": case "Level3_Through": case "Level5_Notepad":
                                   MusicPitch(1f, 0.1f); Music("desktop_theme", 0.55f, 2f); break;
            case "Ending_Freedom": case "Ending_Return": case "Ending_Creator":
                                   MusicPitch(1f, 0.1f); Music("desktop_theme", 0.45f, 1.5f); break;
            // Happy game start: Boot + Level 1 (it warps and tape-stops when the game crashes)
            case "Boot":           MusicPitch(1f, 0.1f); Music("happy_game", 0.5f, 1f); break;
            case "Level1_Game":    MusicPitch(1f, 0.1f); Music("happy_game", 0.5f, 1f); break;
            // Thrilly: the Recycle Bin
            case "Level4_Delete":  MusicPitch(1f, 0.1f); Music("thrilly", 1f, 0.5f); break;
            // everything else: no background music (the endings start their own track at the right moment)
            default:               Music("", 0f, 1f); break;
        }
        Ambience("", 0f); Layer("", 0f);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        var cur = musOnA ? musA : musB; var old = musOnA ? musB : musA;
        if (cur != stopping) Fade(cur, musVol, dt);
        if (old != stopping) Fade(old, 0f, dt);
        Fade(amb, ambVol, dt);
        Fade(layer, layerVol, dt);
        Fade(typing, typers > 0 ? 0.55f : 0f, dt * 4f);
        // every click of the player's mouse makes a soft click
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) Play("click", 0.55f);
    }

    void Fade(AudioSource s, float target, float dt)
    {
        if (s == null) return;
        s.volume = Mathf.MoveTowards(s.volume, target * Master, dt * fadeSpeed);
        if (s.volume <= 0.001f && target <= 0f && s.isPlaying) s.Stop();
    }

    // ---------- the calls other scripts use ----------
    public static void Play(string name, float volume = 1f, float pitchJitter = 0.05f, float pitch = 1f)
    {
        if (I == null || !Allowed.Contains(name) || !I.clips.ContainsKey(name)) return;
        float now = Time.unscaledTime;                                  // the same sound twice in one frame = louder, not better
        if (I.lastPlayed.TryGetValue(name, out float last) && now - last < 0.03f) return;
        I.lastPlayed[name] = now;
        var s = I.pool[I.next]; I.next = (I.next + 1) % I.pool.Length;
        s.pitch = pitch + Random.Range(-pitchJitter, pitchJitter);
        s.volume = 1f;
        s.PlayOneShot(I.clips[name], volume * Master);
    }

    public static void Music(string name, float volume, float fade = 1f)
    {
        if (I == null) return;
        I.fadeSpeed = fade > 0f ? Mathf.Max(0.3f, 1f / fade) : 99f;
        if (name == I.musName && (I.musOnA ? I.musA : I.musB).isPlaying) { I.musVol = volume; return; }
        I.musName = name; I.musVol = volume;
        if (string.IsNullOrEmpty(name) || !Allowed.Contains(name) || !I.clips.ContainsKey(name)) { I.musVol = 0f; return; }
        I.musOnA = !I.musOnA;                                           // crossfade into the other source
        var s = I.musOnA ? I.musA : I.musB;
        s.clip = I.clips[name]; s.pitch = 1f; s.volume = 0f; s.Play();
    }

    public static void Ambience(string name, float volume, float fade = 1f)
    {
        if (I == null) return;
        I.ambVol = volume;
        if (string.IsNullOrEmpty(name) || !Allowed.Contains(name) || !I.clips.ContainsKey(name)) { I.ambVol = 0f; return; }
        if (name == I.ambName && I.amb.isPlaying) return;
        I.ambName = name; I.amb.clip = I.clips[name]; I.amb.volume = 0f; I.amb.Play();
    }

    // a second loop on top of the music (the Recycle Bin timer)
    public static void Layer(string name, float volume, float fade = 1f)
    {
        if (I == null) return;
        I.layerVol = volume;
        if (string.IsNullOrEmpty(name) || !Allowed.Contains(name) || !I.clips.ContainsKey(name)) { I.layerVol = 0f; return; }
        if (name == I.layerName && I.layer.isPlaying) return;
        I.layerName = name; I.layer.clip = I.clips[name]; I.layer.volume = 0f; I.layer.Play();
    }

    public static void PlayDelayed(string name, float delay, float volume = 1f, float pitch = 1f)
    {
        if (I != null) I.StartCoroutine(I.Later(name, delay, volume, pitch));
    }
    IEnumerator Later(string name, float delay, float volume, float pitch) { yield return new WaitForSeconds(delay); Play(name, volume, 0.03f, pitch); }

    public static void MusicVolume(float volume) { if (I != null) I.musVol = volume; }

    public static void MusicPitch(float pitch, float time)
    {
        if (I == null) return;
        I.StartCoroutine(I.PitchTo(I.musOnA ? I.musA : I.musB, pitch, time));
    }

    // the music slows down and dies, like a tape being stopped (when a game crashes)
    public static void TapeStop(float time = 1.2f)
    {
        if (I == null) return;
        var s = I.musOnA ? I.musA : I.musB;
        I.StartCoroutine(I.TapeStopRoutine(s, time));
    }

    IEnumerator PitchTo(AudioSource s, float to, float time)
    {
        float from = s.pitch;
        for (float t = 0f; t < time; t += Time.unscaledDeltaTime) { s.pitch = Mathf.Lerp(from, to, t / time); yield return null; }
        s.pitch = to;
    }

    IEnumerator TapeStopRoutine(AudioSource s, float time)
    {
        stopping = s; musVol = 0f; musName = "";
        float v0 = s.volume;
        for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
        {
            float k = t / time;
            s.pitch = Mathf.Lerp(1f, 0.15f, k * k);
            s.volume = v0 * (1f - k);
            yield return null;
        }
        s.Stop(); s.pitch = 1f; s.volume = 0f; stopping = null;
    }

    // Sam typing in his NOTES (a looping keyboard sound while any NOTES is typing)
    public static void StartTyping()
    {
        if (I == null || !I.clips.ContainsKey("typing")) return;
        I.typers++;
        if (!I.typing.isPlaying) { I.typing.clip = I.clips["typing"]; I.typing.time = Random.Range(0f, Mathf.Max(0f, I.typing.clip.length - 2f)); I.typing.Play(); }
    }

    public static void StopTyping(bool all = false)
    {
        if (I == null) return;
        I.typers = all ? 0 : Mathf.Max(0, I.typers - 1);
    }
}
