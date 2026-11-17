using UnityEngine;
using UnityEngine.SceneManagement;

// Goes on: any trigger that sends Pip to another scene.
// Use it for: level exits, the cracks in Level 3, and the 3 ending choices in Level 5.
public class LevelExit : MonoBehaviour
{
    public string nextScene = "Level2_Desktop";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) SceneManager.LoadScene(nextScene);
    }
}
