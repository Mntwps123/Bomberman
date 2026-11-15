using System.Collections;
using UnityEngine;

// A normal follow camera: always centered on the player, no map limits.
public class ArenaCamera : MonoBehaviour
{
    [SerializeField] private Transform target; // found automatically if left empty

    [Header("Follow")]
    [Tooltip("Higher = lazier camera, lower = snappier. 0 = glued to the player.")]
    [SerializeField] private float smoothTime = 0.1f;

    [Header("View")]
    [Tooltip("90 = straight down. Lower tilts the camera back, for example 60 for an angled 3D view.")]
    [Range(30f, 90f)]
    [SerializeField] private float pitch = 90f;
    [Tooltip("How far the camera sits from the player (needs to be big enough to clear the walls).")]
    [SerializeField] private float distance = 14f;
    [Tooltip("Zoom when the camera is Orthographic. Smaller = closer.")]
    [SerializeField] private float orthographicSize = 6f;

    [Header("Player position on screen")]
    [Tooltip("Shifts the view. Positive Z puts the player lower on screen, which leaves room for the HUD at the top.")]
    [SerializeField] private Vector3 targetOffset = Vector3.zero;

    private Camera cam;
    private Vector3 velocity;
    private bool snapped = false;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        if (target == null)
            StartCoroutine(FindTarget());
    }

    // The player is spawned by a script, so it may not exist on the very first frame
    IEnumerator FindTarget()
    {
        while (target == null)
        {
            var player = FindFirstObjectByType<PlayerGridMovement>();
            if (player != null)
                target = player.transform;
            else
                yield return new WaitForSeconds(0.2f);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        snapped = false; // jump straight to the new target
    }

    void LateUpdate()
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(pitch, 0f, 0f);
        transform.rotation = rotation;

        if (cam != null && cam.orthographic)
            cam.orthographicSize = orthographicSize;

        // Sit 'distance' away from the player, along the direction the camera looks
        Vector3 focus = target.position + targetOffset;
        Vector3 desired = focus - rotation * Vector3.forward * distance;

        if (!snapped)
        {
            transform.position = desired;
            velocity = Vector3.zero;
            snapped = true;
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}