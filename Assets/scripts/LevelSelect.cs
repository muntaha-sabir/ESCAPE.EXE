using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Goes on: one object in the Boot scene ONLY. It stays alive through every level.
// For testing and the judges' demo:
//   0 = Boot, 1-5 = jump to that level, R = restart level, 9 = mark all 3 secrets found
public class LevelSelect : MonoBehaviour
{
    public string[] scenes =
    {
        "Boot",
        "Level1_Game",
        "Level2_Desktop",
        "Level3_Through",
        "Level4_Delete",
        "Level5_Notepad"
    };

    static LevelSelect instance;

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit0Key.wasPressedThisFrame) Load(0);
        if (kb.digit1Key.wasPressedThisFrame) Load(1);
        if (kb.digit2Key.wasPressedThisFrame) Load(2);
        if (kb.digit3Key.wasPressedThisFrame) Load(3);
        if (kb.digit4Key.wasPressedThisFrame) Load(4);
        if (kb.digit5Key.wasPressedThisFrame) Load(5);
        if (kb.rKey.wasPressedThisFrame) Hazard.Restart();

        if (kb.digit9Key.wasPressedThisFrame)
        {
            GameState.secretsFound.Add("secret1");
            GameState.secretsFound.Add("secret2");
            GameState.secretsFound.Add("secret3");
        }
    }

    void Load(int i)
    {
        if (i < scenes.Length) SceneManager.LoadScene(scenes[i]);
    }
}
