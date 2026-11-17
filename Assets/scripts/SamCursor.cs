using System.Collections;
using UnityEngine;

// Goes on: Sam's red mouse cursor in Level 2.
// It drifts slowly around part of the desktop (Sam half-asleep), until the story takes over and it speeds to Pip.
[RequireComponent(typeof(SpriteRenderer))]
public class SamCursor : MonoBehaviour
{
    public Sprite normal, clicking;
    public Vector2 wanderMin = new Vector2(3f, -2f);
    public Vector2 wanderMax = new Vector2(11f, 4f);
    public float wanderSpeed = 0.9f;

    SpriteRenderer sr;
    bool scripted;
    Vector3 target;
    float pause;
    // the arrow's tip is the top-left of the sprite
    Vector3 TipOffset { get { return new Vector3(-6f / 16f, 9f / 16f, 0f); } }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        PickTarget();
    }

    void PickTarget()
    {
        target = new Vector3(Random.Range(wanderMin.x, wanderMax.x), Random.Range(wanderMin.y, wanderMax.y), 0f);
        pause = Random.Range(0.4f, 1.5f);
    }

    void Update()
    {
        if (scripted) return;
        if (pause > 0f) { pause -= Time.deltaTime; return; }
        transform.position = Vector3.MoveTowards(transform.position, target, wanderSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) < 0.05f) PickTarget();
    }

    // Moves so that the TIP of the arrow ends up on 'point'.
    public IEnumerator MoveTipTo(Vector3 point, float speed)
    {
        scripted = true;
        Vector3 goal = point - TipOffset;
        goal.z = 0f;
        while (Vector3.Distance(transform.position, goal) > 0.02f)
        {
            transform.position = Vector3.MoveTowards(transform.position, goal, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = goal;
    }

    public Vector3 Tip { get { return transform.position + TipOffset; } }

    public IEnumerator Click()
    {
        GameAudio.Play("click", 0.75f);
        sr.sprite = clicking;
        yield return new WaitForSeconds(0.12f);
        sr.sprite = normal;
    }
}
