using UnityEngine;

public class EyeTracker : MonoBehaviour
{
    public Transform target;       // The FingerTip
    public float eyeRadius = 0.3f; // How far the pupil can move from center

    private Vector3 initialPosition;

    void Start()
    {
        // Remember where the pupil sits relative to the socket
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        if (target == null || transform.parent == null) return;

        // 1. Get direction from Eye to Finger
        Vector3 direction = (target.position - transform.parent.position).normalized;

        // 2. Calculate distance, but clamp it so it doesn't leave the eye
        float distance = Vector2.Distance(target.position, transform.parent.position);
        float moveAmount = Mathf.Min(distance, eyeRadius);

        // 3. Move the pupil relative to its socket
        transform.localPosition = initialPosition + (direction * moveAmount);
    }
}