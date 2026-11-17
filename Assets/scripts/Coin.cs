using UnityEngine;

// Goes on: each coin. Needs a CircleCollider2D with "Is Trigger" ticked.
public class Coin : MonoBehaviour
{
    public AudioClip pickupSound;   // optional

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CoinCounter.Add(1);
        if (pickupSound != null) AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        gameObject.SetActive(false);
    }
}
