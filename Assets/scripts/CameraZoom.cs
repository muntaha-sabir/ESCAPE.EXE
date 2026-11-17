using System.Collections;
using UnityEngine;

// Goes on: the Main Camera (Orthographic, with the Pixel Perfect Camera component).
// Level 2: the "reveal" - starts zoomed in on Pip, then zooms OUT to show the whole monitor and desk.
// Level 5: the opposite - starts on the whole view, then slowly zooms IN on the Notepad.
// The Pixel Perfect Camera is switched off during the zoom (it would block it) and back on at the end.
public class CameraZoom : MonoBehaviour
{
    [Header("Start (where the camera begins)")]
    public float startSize = 3f;
    public Vector2 startPosition = new Vector2(-10f, -4f);

    [Header("End (where the camera finishes)")]
    [Tooltip("8.4375 = the full 480x270 view at 16 pixels per unit (what the Pixel Perfect Camera uses)")]
    public float endSize = 8.4375f;
    public Vector2 endPosition = Vector2.zero;

    [Header("Timing")]
    public float delay = 0.5f;
    public float duration = 2f;

    [Header("Level 5 zoom-in: leave the Pixel Perfect Camera off at the end")]
    public bool keepPixelPerfectOff = false;

    Camera cam;
    Behaviour pixelPerfect;

    IEnumerator Start()
    {
        cam = GetComponent<Camera>();
        pixelPerfect = GetComponent("PixelPerfectCamera") as Behaviour;
        if (pixelPerfect != null) pixelPerfect.enabled = false;

        float z = transform.position.z;      // keep the camera's Z (usually -10)
        cam.orthographicSize = startSize;
        transform.position = new Vector3(startPosition.x, startPosition.y, z);

        yield return new WaitForSeconds(delay);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duration);   // smooth ease in/out
            cam.orthographicSize = Mathf.Lerp(startSize, endSize, k);
            Vector2 p = Vector2.Lerp(startPosition, endPosition, k);
            transform.position = new Vector3(p.x, p.y, z);
            yield return null;
        }

        cam.orthographicSize = endSize;
        transform.position = new Vector3(endPosition.x, endPosition.y, z);
        if (pixelPerfect != null && !keepPixelPerfectOff) pixelPerfect.enabled = true;
    }
}
