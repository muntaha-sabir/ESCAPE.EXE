using System.Collections;
using TMPro;
using UnityEngine;

// Goes on: the Managers object in Level 5 (and in each ending scene, for the typed text).
// Types the message letter by letter, then switches on the choices.
public class NotepadEnding : MonoBehaviour
{
    public TMP_Text notepadText;
    [TextArea(3, 10)]
    public string message = "...wait.\nyou're not a bug.\nyou're the only part of this game that ever worked.\n\nI'm so tired, Pip. I don't know how to finish this.";
    public float typeSpeed = 0.05f;       // seconds per letter
    public float pauseOnNewLine = 0.4f;   // little pause at each new line

    public GameObject showWhenDone;       // Level 5: the two exits. Endings: the "Ending X/3" card + buttons
    public GameObject secretChoice;       // Level 5 only: the gold ending.png (appears only if all 3 secrets found)
    public AudioSource keyClick;          // optional typing sound

    IEnumerator Start()
    {
        if (showWhenDone != null) showWhenDone.SetActive(false);
        if (secretChoice != null) secretChoice.SetActive(false);
        notepadText.text = "";

        yield return new WaitForSeconds(1f);

        foreach (char letter in message)
        {
            notepadText.text += letter;
            if (keyClick != null && letter != ' ') keyClick.Play();
            yield return new WaitForSeconds(letter == '\n' ? pauseOnNewLine : typeSpeed);
        }

        if (showWhenDone != null) showWhenDone.SetActive(true);
        if (secretChoice != null) secretChoice.SetActive(GameState.AllSecretsFound);
    }
}
