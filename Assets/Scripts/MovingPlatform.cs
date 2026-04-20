using UnityEngine;

/// <summary>
/// Attach to any ground/platform GameObject.
/// Moves it back and forth along a chosen axis using a sine wave,
/// and correctly carries the player (and any Rigidbody) standing on it.
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    public enum MoveAxis
    {
        LeftRight,  // Moves along local X
        UpDown,     // Moves along local Y
        ForwardBack // Moves along local Z
    }

    [Header("Movement")]
    [SerializeField] private MoveAxis axis       = MoveAxis.LeftRight;
    [SerializeField] private float    distance   = 3f;    // Units each side from origin
    [SerializeField] private float    speed      = 1f;    // Full cycles per second
    [SerializeField] private float    phaseOffset = 0f;   // Start offset in radians — stagger multiple platforms

    [Header("Easing")]
    [Tooltip("Smoothstep makes the platform ease in/out at each end rather than moving at constant speed")]
    [SerializeField] private bool smoothstep = true;

    // Internal
    private Vector3 _origin;         // World position at Start
    private Vector3 _moveDirection;  // Unit vector for the chosen axis

    // Carry — we move the player by tracking how far the platform moved this frame
    private Vector3 _lastPosition;
    private CharacterController _playerOnPlatform;   // Non-null when player is riding

    private void Start()
    {
        _origin       = transform.position;
        _lastPosition = transform.position;

        _moveDirection = axis switch
        {
            MoveAxis.LeftRight   => transform.right,
            MoveAxis.UpDown      => transform.up,
            MoveAxis.ForwardBack => transform.forward,
            _                    => transform.right
        };
    }

    private void FixedUpdate()
    {
        MovePlatform();
        CarryPlayer();
    }

    // -------------------------------------------------------------------------
    // Move the platform along its sine wave
    // -------------------------------------------------------------------------
    private void MovePlatform()
    {
        float t = Mathf.Sin(Time.time * speed * Mathf.PI * 2f + phaseOffset); // -1..1

        if (smoothstep)
        {
            // Remap -1..1 → 0..1, apply smoothstep, remap back
            float t01 = (t + 1f) * 0.5f;
            t01 = t01 * t01 * (3f - 2f * t01); // smoothstep formula
            t = t01 * 2f - 1f;
        }

        transform.position = _origin + _moveDirection * (t * distance);
    }

    // -------------------------------------------------------------------------
    // Carry — push the player by however far the platform moved this frame
    // so they don't slide off or rubber-band
    // -------------------------------------------------------------------------
    private void CarryPlayer()
    {
        Vector3 delta = transform.position - _lastPosition;
        _lastPosition = transform.position;

        if (_playerOnPlatform != null)
        {
            print ("moving player"); 
            _playerOnPlatform.Move(delta);
        }
    }

    // -------------------------------------------------------------------------
    // Detect player landing and leaving
    // -------------------------------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (_playerOnPlatform != null) return;

        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null)
            _playerOnPlatform = cc;
    }

    private void OnTriggerExit(Collider other)
    {
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null && cc == _playerOnPlatform)
            _playerOnPlatform = null;
    }

    // -------------------------------------------------------------------------
    // Gizmos — preview the travel path in the editor
    // -------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
        Vector3 origin = Application.isPlaying ? _origin : transform.position;

        Vector3 dir = axis switch
        {
            MoveAxis.LeftRight   => transform.right,
            MoveAxis.UpDown      => transform.up,
            MoveAxis.ForwardBack => transform.forward,
            _                    => transform.right
        };

        Vector3 pointA = origin + dir *  distance;
        Vector3 pointB = origin + dir * -distance;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawWireSphere(pointA, 0.15f);
        Gizmos.DrawWireSphere(pointB, 0.15f);
        Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
        Gizmos.DrawWireSphere(origin, 0.1f);
    }
}