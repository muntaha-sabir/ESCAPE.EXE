using System.Collections;
using UnityEngine;

// Goes on: any object. Shows ONE caption line (Pip / Hint / Story), either after some seconds or when Pip walks into this
// object's trigger zone (needs a Box Collider 2D with "Is Trigger" ticked for the zone version).
public class NarrationCue : MonoBehaviour
{
    public enum When { AfterSeconds, PipEntersZone }
    public enum Who { Pip, Hint, Story }

    public When when = When.AfterSeconds;
    public float delay = 0f;
    public Who who = Who.Pip;
    public Narrator.Icon hintIcon = Narrator.Icon.None;
    public string text = "...";
    public float hold = 3f;

    bool done;

    IEnumerator Start()
    {
        if (when != When.AfterSeconds) yield break;
        yield return new WaitForSeconds(delay);
        Fire();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (when != When.PipEntersZone || done || !other.CompareTag("Player")) return;
        StartCoroutine(FireLater());
    }

    IEnumerator FireLater()
    {
        done = true;
        yield return new WaitForSeconds(delay);
        Say();
    }

    void Fire()
    {
        if (done) return;
        done = true;
        Say();
    }

    void Say()
    {
        if (who == Who.Pip) Narrator.Pip(text, hold);
        else if (who == Who.Story) Narrator.Story(text, hold);
        else Narrator.Hint(text, hintIcon, hold);
    }
}
