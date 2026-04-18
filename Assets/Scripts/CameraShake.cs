using UnityEngine;

/// <summary>
/// Attach to the same GameObject as your Main Camera.
/// Applies shake as a PURE OFFSET on top of whatever the camera's transform
/// already is — it never caches or owns the base position/rotation,
/// so it won't conflict with mouse look, recoil, or any other system.
///
/// Works by storing the offset it applied last frame and removing it
/// before applying the new one, so the net effect is always just the delta.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Shotgun Shake Profile")]
    [SerializeField] private float magnitude    = 0.06f;   // Max positional displacement (world units)
    [SerializeField] private float rotationMag  = 2.0f;    // Max rotational displacement (degrees)
    [SerializeField] private float frequency    = 20f;     // Perlin noise scroll speed
    [SerializeField] private float decayRate    = 5f;      // How fast trauma falls to zero

    // Current trauma (0..1). Shake intensity = trauma^2 for a punchy feel.
    private float _trauma = 0f;

    // The offset we applied on the previous frame — subtracted before applying the new one.
    private Vector3    _lastPosOffset = Vector3.zero;
    private Quaternion _lastRotOffset = Quaternion.identity;

    // Per-axis Perlin seeds so each axis shakes independently
    private float _seedX, _seedY, _seedRX, _seedRY, _seedRZ;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        _seedX  = Random.Range(0f, 100f);
        _seedY  = Random.Range(0f, 100f);
        _seedRX = Random.Range(0f, 100f);
        _seedRY = Random.Range(0f, 100f);
        _seedRZ = Random.Range(0f, 100f);
    }

    private void LateUpdate()
    {
        // Always undo last frame's offset first, regardless of trauma level.
        // This keeps us from accumulating drift.
        transform.localPosition -= _lastPosOffset;
        transform.localRotation  = Quaternion.Inverse(_lastRotOffset) * transform.localRotation;

        if (_trauma <= 0f)
        {
            _lastPosOffset = Vector3.zero;
            _lastRotOffset = Quaternion.identity;
            return;
        }

        float shake = _trauma * _trauma; // Squared for heavy-then-easing feel
        float t     = Time.time * frequency;

        // Sample Perlin noise, remap 0..1 → -1..1
        float px  = (Mathf.PerlinNoise(_seedX  + t, 0f) * 2f - 1f) * magnitude  * shake;
        float py  = (Mathf.PerlinNoise(_seedY  + t, 1f) * 2f - 1f) * magnitude  * shake;
        float rx  = (Mathf.PerlinNoise(_seedRX + t, 2f) * 2f - 1f) * rotationMag * shake;
        float ry  = (Mathf.PerlinNoise(_seedRY + t, 3f) * 2f - 1f) * rotationMag * shake * 0.5f;
        float rz  = (Mathf.PerlinNoise(_seedRZ + t, 4f) * 2f - 1f) * rotationMag * shake * 0.4f;

        _lastPosOffset = new Vector3(px, py, 0f);
        _lastRotOffset = Quaternion.Euler(rx, ry, rz);

        // Apply offset ON TOP of current transform
        transform.localPosition += _lastPosOffset;
        transform.localRotation  = _lastRotOffset * transform.localRotation;

        // Decay trauma
        _trauma = Mathf.Max(0f, _trauma - decayRate * Time.deltaTime);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Add trauma to trigger a shake. 0.7 is a solid shotgun blast.
    /// Stacks additively — rapid shots build up to a max of 1.
    /// </summary>
    public void Shake(float traumaAmount = 0.7f)
    {
        _trauma = Mathf.Clamp01(_trauma + traumaAmount);
    }
}