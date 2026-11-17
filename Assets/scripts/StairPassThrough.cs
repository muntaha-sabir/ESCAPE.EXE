using UnityEngine;
using UnityEngine.SceneManagement;

// Nothing to set up. In Level 2 and Level 5 the desktop files ("File_...") become one-way stairs:
// Pip can still jump and stand on top of them, but if he falls to the ground he can walk
// through them (in front of them) and go back to the start of the stairs instead of getting stuck.
public static class StairPassThrough
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Hook() { SceneManager.sceneLoaded += (s, m) => Apply(s.name); }

    static void Apply(string scene)
    {
        if (scene != "Level2_Desktop" && scene != "Level5_Notepad") return;
        foreach (var c in Object.FindObjectsByType<BoxCollider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c.isTrigger || !c.gameObject.name.StartsWith("File_")) continue;
            var e = c.GetComponent<PlatformEffector2D>();
            if (e == null) e = c.gameObject.AddComponent<PlatformEffector2D>();
            e.useOneWay = true;
            e.surfaceArc = 150f;          // only the top of the file is solid
            e.useSideFriction = false;
            c.usedByEffector = true;
        }
    }
}
