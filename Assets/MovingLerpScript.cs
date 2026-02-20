using UnityEngine;

public class MovingLerpScript : MonoBehaviour
{
    [SerializeField] private float distance = 3f;
    [SerializeField] private float duration = 2f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private Color pathColor = Color.cyan;
    [SerializeField] private Color pointColor = Color.red;
    [SerializeField] private float pointGizmoSize = 0.2f;

    private Vector3 _startPosition;
    private Vector3 _leftTarget;
    private Vector3 _rightTarget;
    private bool _isMoving = true;

    void Start()
    {
        _startPosition = transform.position;
        _leftTarget = _startPosition + Vector3.left * distance;
        _rightTarget = _startPosition + Vector3.right * distance;
    }

    void Update()
    {
        if (!_isMoving) return;

        float t = Mathf.PingPong(Time.time / duration, 1f);
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        transform.position = Vector3.Lerp(_leftTarget, _rightTarget, smoothT);
    }

    /// <summary>
    /// Stops the object's left/right movement, freezing it at its current position.
    /// </summary>
    public void StopMovement()
    {
        _isMoving = false;
    }

    /// <summary>
    /// Resumes the object's left/right movement from its current position.
    /// </summary>
    public void ResumeMovement()
    {
        _isMoving = true;
    }

    /// <summary>
    /// Returns whether the object is currently moving.
    /// </summary>
    public bool IsMoving() => _isMoving;

    void OnDrawGizmos()
    {
        if (!showDebug) return;

        Vector3 origin = Application.isPlaying ? _startPosition : transform.position;
        Vector3 left = origin + Vector3.left * distance;
        Vector3 right = origin + Vector3.right * distance;

        Gizmos.color = _isMoving ? pathColor : Color.gray;
        Gizmos.DrawLine(left, right);

        Gizmos.color = pointColor;
        Gizmos.DrawSphere(left, pointGizmoSize);
        Gizmos.DrawSphere(right, pointGizmoSize);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(origin, pointGizmoSize * 0.75f);

#if UNITY_EDITOR
        UnityEditor.Handles.color = _isMoving ? pathColor : Color.gray;
        UnityEditor.Handles.Label(left + Vector3.up * 0.3f, "Left Target");
        UnityEditor.Handles.Label(right + Vector3.up * 0.3f, "Right Target");
        UnityEditor.Handles.Label(origin + Vector3.up * 0.3f, "Origin");

        if (Application.isPlaying)
        {
            Gizmos.color = _isMoving ? Color.yellow : Color.gray;
            Gizmos.DrawSphere(transform.position, pointGizmoSize * 0.5f);
            UnityEditor.Handles.Label(
                transform.position + Vector3.down * 0.3f,
                _isMoving ? "Current" : "Stopped"
            );
        }
#endif
    }
}