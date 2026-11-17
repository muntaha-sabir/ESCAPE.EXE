using UnityEngine;
using UnityEngine.SceneManagement;

// Goes on: any object, then hook UI buttons (OnClick) to these functions.
// Use it for: "Play again" and "Quit" buttons on the ending cards.
public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void PlayAgain()
    {
        GameState.ResetAll();
        SceneManager.LoadScene("Boot");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
