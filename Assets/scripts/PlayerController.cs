using UnityEngine;
using UnityEngine.InputSystem;

// Goes on: Pip
// Needs: Rigidbody2D, BoxCollider2D, an empty child object called "GroundCheck" at Pip's feet.
// Pip must be on the "Player" layer and have the "Player" tag.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    [Range(0.5f, 1f)]
    public float speedMultiplier = 0.9f;   // 0.9 = walks 10% slower than Move Speed (easier to read narration)
    public bool lockHorizontal = false;    // true = Pip can only jump, not walk (used inside the maximized NOTES)

    [Header("Ground check")]
    public Transform groundCheck;          // empty child at Pip's feet
    public float groundRadius = 0.15f;
    public LayerMask groundLayer;          // tick Ground + TitleBar

    [Header("Feel")]
    public float coyoteTime = 0.1f;        // can still jump just after walking off a ledge
    public float jumpBuffer = 0.1f;        // Space pressed just before landing still counts
    [Range(0.2f, 1f)]
    public float jumpCutMultiplier = 0.5f; // letting go of Space early keeps this much of the jump (1 = no cut)

    const float ExtraSlow = 0.93f;         // a hint slower in every level (1 = off)

    Rigidbody2D rb;
    SpriteRenderer sr;
    float coyoteTimer, bufferTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || groundCheck == null) return;

        // Move left / right
        float x = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        if (lockHorizontal) x = 0f;
        rb.linearVelocity = new Vector2(x * moveSpeed * speedMultiplier * ExtraSlow, rb.linearVelocity.y);
        if (x != 0 && sr != null) sr.flipX = x < 0;

        // Are we standing on something?
        Collider2D ground = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        bool grounded = ground != null;

        // Ride windows: become a child of the window we're standing on
        DraggableWindow window = ground ? ground.GetComponentInParent<DraggableWindow>() : null;
        Transform newParent = window ? window.transform : null;
        if (transform.parent != newParent) transform.SetParent(newParent, true);

        // Jump (with coyote time + jump buffer so it feels fair)
        coyoteTimer = grounded ? coyoteTime : coyoteTimer - Time.deltaTime;
        bufferTimer = kb.spaceKey.wasPressedThisFrame ? jumpBuffer : bufferTimer - Time.deltaTime;

        if (bufferTimer > 0 && coyoteTimer > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            GameAudio.Play("jump", 0.7f, 0.06f);
            bufferTimer = 0;
            coyoteTimer = 0;
        }

        // Let go of Space early = shorter jump
        if (kb.spaceKey.wasReleasedThisFrame && rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    // Draws the ground check circle in the Scene view so you can line it up with Pip's feet
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
