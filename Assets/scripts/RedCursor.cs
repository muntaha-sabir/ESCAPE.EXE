using UnityEngine;

// Goes on: Sam's red cursor in Level 4. Also add a trigger collider + the Hazard script,
// so touching Pip restarts the level.
public class RedCursor : MonoBehaviour
{
    public Transform target;          // Pip (found automatically if left empty)
    public float speed = 2.5f;        // keep this SLOWER than Pip's moveSpeed (6)
    public float startDelay = 1.5f;   // gives the player a moment before the chase starts

    void Start()
    {
        if (target == null)
        {
            GameObject pip = GameObject.FindWithTag("Player");
            if (pip != null) target = pip.transform;
        }
    }

    void Update()
    {
        if (startDelay > 0) { startDelay -= Time.deltaTime; return; }
        if (target == null) return;
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }
}
