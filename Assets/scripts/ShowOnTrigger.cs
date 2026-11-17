using System.Collections;
using UnityEngine;

// Goes on: an invisible trigger zone. When Pip walks in, it switches things ON (and/or OFF).
// Use it for: tooltips, the "?" bubble, the "Build failed" pop-up, opening the crack, Sam's pop-ups.
// Tip: put TWO of these on the same zone if you want some things to appear later (use "delay").
public class ShowOnTrigger : MonoBehaviour
{
    public GameObject[] showThese;
    public GameObject[] hideThese;
    public float delay = 0f;
    public bool onlyOnce = true;

    bool done;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (done || !other.CompareTag("Player")) return;
        if (onlyOnce) done = true;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        if (delay > 0) yield return new WaitForSeconds(delay);
        foreach (GameObject g in showThese) if (g != null) g.SetActive(true);
        foreach (GameObject g in hideThese) if (g != null) g.SetActive(false);
    }
}
