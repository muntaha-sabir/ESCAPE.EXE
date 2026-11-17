#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

// Editor helper (not part of the game build).
// When ON, pressing Play in Unity ALWAYS starts from the Title scene,
// no matter which scene you have open - just like the real game will.
// Turn it on/off from the top menu: ESCAPE.EXE > Always Play From Title
// (turn it OFF when you want to test one level on its own).
[InitializeOnLoad]
public static class PlayFromTitle
{
    const string Key = "ESCAPE_EXE_PlayFromTitle";
    const string MenuPath = "ESCAPE.EXE/Always Play From Title";

    static PlayFromTitle()
    {
        EditorApplication.delayCall += Apply;
    }

    static bool On
    {
        get { return EditorPrefs.GetBool(Key, true); }
        set { EditorPrefs.SetBool(Key, value); }
    }

    [MenuItem(MenuPath)]
    static void Toggle()
    {
        On = !On;
        Apply();
    }

    [MenuItem(MenuPath, true)]
    static bool ToggleCheck()
    {
        Menu.SetChecked(MenuPath, On);
        return true;
    }

    static void Apply()
    {
        EditorSceneManager.playModeStartScene = On
            ? AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Title.unity")
            : null;
    }
}
#endif
