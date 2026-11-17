using System.Collections;
using UnityEngine;

// Goes on: a desktop file icon the player can drag (spring.png, dragon_final.png...)
// Needs: a BoxCollider2D on the "Files" layer.
public class DroppableFile : MonoBehaviour
{
    [Tooltip("The thing inside the game that appears when this file is dropped in (switched OFF at start). Leave EMPTY for a joke file.")]
    public GameObject spawnInGame;

    [Tooltip("A trigger collider covering the game window's play area (on the DropZone layer)")]
    public Collider2D dropZone;

    [Tooltip("Joke files only: pop-up like 'file too big lol' (switched OFF at start)")]
    public GameObject jokePopup;

    [Tooltip("Optional sound. Put the AudioSource on a DIFFERENT object (e.g. Managers), because this file disappears.")]
    public AudioSource dropSound;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    // Called by DragManager when the player lets go of this file
    public void OnDropped()
    {
        bool inZone = dropZone != null && dropZone.OverlapPoint(transform.position);

        if (inZone && spawnInGame != null)
        {
            spawnInGame.SetActive(true);
            if (dropSound != null) dropSound.Play();
            gameObject.SetActive(false);
            return;
        }

        if (inZone && jokePopup != null) StartCoroutine(ShowJoke());
        transform.position = startPos;   // snap back
    }

    IEnumerator ShowJoke()
    {
        jokePopup.SetActive(true);
        yield return new WaitForSeconds(2f);
        jokePopup.SetActive(false);
    }
}
