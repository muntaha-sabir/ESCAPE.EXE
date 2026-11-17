using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Goes on: the Managers object in the Boot scene. Press Space to start a fresh game.
public class BootScreen : MonoBehaviour
{
    public string firstLevel = "Level1_Game";
    public float waitBeforeInput = 2f;   // lets the intro play before Space works

    void Update()
    {
        if (waitBeforeInput > 0) { waitBeforeInput -= Time.deltaTime; return; }

        var kb = Keyboard.current;
        if (kb != null && kb.spaceKey.wasPressedThisFrame)
        {
            GameState.ResetAll();
            SceneManager.LoadScene(firstLevel);
        }
    }
}
