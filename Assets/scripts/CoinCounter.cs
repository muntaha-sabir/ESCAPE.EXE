using TMPro;
using UnityEngine;

// Goes on: the coin text in the top-left corner (a TextMeshPro UI text).
// Coins reset when the level restarts. That's fine for a jam.
public class CoinCounter : MonoBehaviour
{
    public TMP_Text label;

    static CoinCounter instance;
    int coins;

    void Awake()
    {
        instance = this;
        if (label == null) label = GetComponent<TMP_Text>();
        UpdateLabel();
    }

    public static void Add(int amount)
    {
        if (instance == null) return;
        instance.coins += amount;
        instance.UpdateLabel();
    }

    void UpdateLabel()
    {
        if (label != null) label.text = "Coins: " + coins;
    }
}
