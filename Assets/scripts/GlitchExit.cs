using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Goes on: the crack in Level 1 (a trigger). Simple glitch: white flash + camera shake + sound, then next level.
// Needs: a full-screen white UI Image with alpha 0 at the start, and "Raycast Target" UNTICKED.
public class GlitchExit : MonoBehaviour
{
    public string nextScene = "Level2_Desktop";
    public Image whiteFlash;
    public AudioSource glitchSound;     // put the AudioSource on THIS object, not on Pip
    public float shakeStrength = 0.3f;
    public float duration = 0.6f;

    bool used;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used || !other.CompareTag("Player")) return;
        used = true;
        StartCoroutine(Glitch(other.gameObject));
    }

    IEnumerator Glitch(GameObject pip)
    {
        pip.SetActive(false);                       // Pip vanishes into the crack
        if (glitchSound != null) glitchSound.Play();

        Transform cam = Camera.main.transform;
        Vector3 camStart = cam.position;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = 1f - (t / duration);          // goes from 1 down to 0
            Vector2 shake = Random.insideUnitCircle * shakeStrength * k;
            cam.position = camStart + new Vector3(shake.x, shake.y, 0f);
            if (whiteFlash != null) whiteFlash.color = new Color(1f, 1f, 1f, k);
            yield return null;
        }

        cam.position = camStart;
        SceneManager.LoadScene(nextScene);
    }
}
