using System.Collections;
using UnityEngine;

// Goes on: each hidden file (pip_sketch_2014.png, old_game.zip, dont_open.txt).
// Needs: a collider with "Is Trigger" ticked.
public class SecretFile : MonoBehaviour
{
    public string secretId = "secret1";     // secret1, secret2, secret3 (all different!)
    public GameObject showOnPickup;         // pop-up with the picture / message (switched OFF at start)
    public float showFor = 3f;

    void Start()
    {
        // Already found earlier? Don't show it again.
        if (GameState.secretsFound.Contains(secretId)) gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        GameState.secretsFound.Add(secretId);

        // Hide the file but keep this object alive so it can close the pop-up later
        GetComponent<Collider2D>().enabled = false;
        foreach (SpriteRenderer r in GetComponentsInChildren<SpriteRenderer>()) r.enabled = false;

        if (showOnPickup != null) StartCoroutine(ShowThenHide());
    }

    IEnumerator ShowThenHide()
    {
        showOnPickup.SetActive(true);
        yield return new WaitForSeconds(showFor);
        showOnPickup.SetActive(false);
    }
}
