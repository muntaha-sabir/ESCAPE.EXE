using System.Collections.Generic;

// Not attached to anything. It just remembers the hidden files between levels.
public static class GameState
{
    // Uses names, so a secret can't be counted twice if Pip dies and the level reloads
    public static HashSet<string> secretsFound = new HashSet<string>();
    public const int TotalSecrets = 3;

    public static bool AllSecretsFound => secretsFound.Count >= TotalSecrets;

    // Recycle Bin choice: true = the player clicked RESTORE EVERYTHING (P!P.exe came back too)
    public static bool pipExeRestored;

    public static void ResetAll()
    {
        secretsFound.Clear();
        pipExeRestored = false;
    }
}
