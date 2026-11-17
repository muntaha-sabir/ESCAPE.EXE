using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Goes on: the Managers object in Level 4.
// Every few seconds shows "Delete player.png? YES / NO".
// Hook the NO button's OnClick to ClickNo, and the YES button's OnClick to ClickYes.
public class DeletePopup : MonoBehaviour
{
    public GameObject popup;          // the pop-up panel (switched OFF at start)
    public Image timerBar;            // a UI Image with Image Type = Filled (shrinks as time runs out)
    public GameObject oopsMessage;    // "...oops. ctrl+z" text (switched OFF at start)
    public AudioSource deleteSound;   // optional

    public float firstDelay = 6f;     // seconds before the first pop-up
    public float every = 10f;         // seconds between pop-ups
    public float timeToAnswer = 5f;   // seconds to click NO

    float nextTime, answerLeft;
    bool open, deleting;

    void Start()
    {
        nextTime = Time.time + firstDelay;
        if (popup != null) popup.SetActive(false);
        if (oopsMessage != null) oopsMessage.SetActive(false);
    }

    void Update()
    {
        if (deleting) return;

        if (!open && Time.time >= nextTime)
        {
            open = true;
            answerLeft = timeToAnswer;
            popup.SetActive(true);
        }

        if (open)
        {
            answerLeft -= Time.deltaTime;
            if (timerBar != null) timerBar.fillAmount = answerLeft / timeToAnswer;
            if (answerLeft <= 0) ClickYes();   // too slow = deleted
        }
    }

    public void ClickNo()
    {
        open = false;
        popup.SetActive(false);
        nextTime = Time.time + every;
    }

    public void ClickYes()
    {
        if (deleting) return;
        open = false;
        popup.SetActive(false);
        StartCoroutine(Deleted());
    }

    IEnumerator Deleted()
    {
        deleting = true;
        GameObject pip = GameObject.FindWithTag("Player");
        if (pip != null) pip.SetActive(false);
        if (deleteSound != null) deleteSound.Play();
        if (oopsMessage != null) oopsMessage.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        Hazard.Restart();
    }
}
