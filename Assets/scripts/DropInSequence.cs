using System.Collections;
using UnityEngine;

// Goes on: the LevelFX object in Level 1.
// At the start, every object in the list is hidden. Then, one by one (after its Delay):
//   Pop OFF -> a cloud poof appears in the sky and the object falls down into place
//   Pop ON  -> a sparkle, and the object pops into view (stars, the "?")
// Place each object in the scene exactly where it should END UP.
// The ground is NOT in this list - it is there from the very start.
[System.Serializable]
public class DropEntry
{
    public GameObject target;
    public float delay = 1f;
    public bool pop;
}

public class DropInSequence : MonoBehaviour
{
    public DropEntry[] entries;

    [Header("Player can't move until everything has landed")]
    public Behaviour lockPlayer;          // drag Pip's PlayerController here
    public float unlockExtraDelay = 0.15f;
    public float dropFromY = 6f;      // where in the sky things poof in
    public float fallTime = 0.35f;
    public Sprite[] poofFrames;
    public Sprite[] sparkleFrames;

    void Awake()
    {
        foreach (var e in entries)
            if (e.target != null) FX.SetVisible(e.target, false);
        if (lockPlayer != null) lockPlayer.enabled = false;
    }

    void Start()
    {
        float last = 0f;
        foreach (var e in entries)
        {
            if (e.target == null) continue;
            StartCoroutine(Run(e));
            last = Mathf.Max(last, e.delay + (e.pop ? 0.25f : fallTime + 0.15f));
        }
        StartCoroutine(Unlock(last + unlockExtraDelay));
    }

    IEnumerator Unlock(float after)
    {
        yield return new WaitForSeconds(after);
        if (lockPlayer != null) lockPlayer.enabled = true;
    }

    IEnumerator Run(DropEntry e)
    {
        yield return new WaitForSeconds(e.delay);
        if (e.pop) yield return FX.PopIn(e.target, sparkleFrames);
        else yield return FX.DropIn(e.target, dropFromY, fallTime, poofFrames);
    }
}
