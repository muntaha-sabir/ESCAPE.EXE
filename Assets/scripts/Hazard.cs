using UnityEngine;
using UnityEngine.SceneManagement;

// Goes on: spikes, the red cursor, and a big invisible "KillZone" below each level.
// Touching it restarts the current level.
public class Hazard : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) Restart();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player")) Restart();
    }

    public static void Restart()
    {
        GameAudio.Play("loose", 0.9f, 0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
