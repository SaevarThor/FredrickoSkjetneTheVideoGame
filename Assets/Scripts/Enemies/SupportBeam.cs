using UnityEngine;

/// <summary>
/// Attach to the EnemyAIFlyingSupport GameObject.
/// Draws a pulsing, dancing blue line between this unit and its protected ally.
/// Call SetTarget() when protecting starts, ClearTarget() when it stops.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class SupportBeam : MonoBehaviour
{
    [Header("Beam Appearance")]
    [SerializeField] private Color beamColorCore  = new Color(0.3f, 0.7f, 1f, 1f);    // Bright cyan-blue center
    [SerializeField] private Color beamColorEdge  = new Color(0.1f, 0.3f, 1f, 0.6f);  // Deeper blue at tail
    [SerializeField] private float beamWidth      = 0.06f;

    [Header("Segments")]
    [SerializeField] private int segmentCount     = 24;     // More = smoother wave

    [Header("Wave / Dance")]
    [SerializeField] private float waveAmplitude  = 0.25f;  // How far the beam sways side to side
    [SerializeField] private float waveFrequency  = 3f;     // Wave cycles along the beam length
    [SerializeField] private float waveSpeed      = 4f;     // How fast the wave travels along the beam

    [Header("Pulse")]
    [SerializeField] private float pulseSpeed     = 3f;     // Brightness pulse rate
    [SerializeField] private float pulseMin       = 0.5f;   // Min alpha multiplier
    [SerializeField] private float pulseMax       = 1f;     // Max alpha multiplier

    [Header("Offsets")]
    [SerializeField] private Vector3 sourceOffset = new Vector3(0f, 0.5f, 0f);  // Lift off unit center
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.2f, 0f);  // Aim at ally chest

    private LineRenderer _lr;
    private Transform    _target;
    private bool         _active = false;

    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        ConfigureLineRenderer();
        _lr.enabled = false;
    }

    private void LateUpdate()
    {
        if (!_active || _target == null)
        {
            _lr.enabled = false;
            return;
        }

        _lr.enabled = true;
        UpdateBeam();
    }

    // -------------------------------------------------------------------------
    // Public API — called by EnemyAIFlyingSupport
    // -------------------------------------------------------------------------
    public void SetTarget(Transform ally)
    {
        _target = ally;
        _active = true;
        _lr.enabled = true;
    }

    public void ClearTarget()
    {
        _active = false;
        _target = null;
        _lr.enabled = false;
    }

    // -------------------------------------------------------------------------
    // Beam update — recalculate all segment positions each frame
    // -------------------------------------------------------------------------
    private void UpdateBeam()
    {
        Vector3 start = transform.position + sourceOffset;
        Vector3 end   = _target.position   + targetOffset;

        // Two perpendicular axes for the wave to sway in world space
        Vector3 along = (end - start).normalized;
        Vector3 perp  = Vector3.Cross(along, Vector3.up).normalized;
        if (perp == Vector3.zero)
            perp = Vector3.Cross(along, Vector3.forward).normalized;
        Vector3 perp2 = Vector3.Cross(along, perp).normalized;

        // Pulse — modulate alpha over time
        float pulse = Mathf.Lerp(pulseMin, pulseMax,
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);

        // Build gradient with pulsed alpha
        Gradient gradient = new Gradient();
        Color c0 = beamColorCore; c0.a *= pulse;
        Color c1 = beamColorEdge; c1.a *= pulse * 0.5f;
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(c0, 0f),
                new GradientColorKey(c0, 0.5f),
                new GradientColorKey(c1, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(c0.a, 0f),
                new GradientAlphaKey(c0.a, 0.5f),
                new GradientAlphaKey(c1.a, 1f)
            }
        );
        _lr.colorGradient = gradient;

        // Place each segment point along a dancing sine wave
        for (int i = 0; i <= segmentCount; i++)
        {
            float t        = (float)i / segmentCount;
            Vector3 linear = Vector3.Lerp(start, end, t);

            // Sine wave on two axes with a time offset so it looks like it's flowing
            float wave     = Mathf.Sin(t * waveFrequency * Mathf.PI * 2f - Time.time * waveSpeed);
            float wave2    = Mathf.Sin(t * waveFrequency * Mathf.PI * 2f - Time.time * waveSpeed + 1.57f);

            // Envelope — amplitude is zero at both ends so beam always meets source/target cleanly
            float envelope = Mathf.Sin(t * Mathf.PI);

            Vector3 point = linear
                + perp  * wave  * waveAmplitude * envelope
                + perp2 * wave2 * waveAmplitude * 0.5f * envelope;

            _lr.SetPosition(i, point);
        }
    }

    // -------------------------------------------------------------------------
    // LineRenderer initial config
    // -------------------------------------------------------------------------
    private void ConfigureLineRenderer()
    {
        _lr.positionCount    = segmentCount + 1;
        _lr.useWorldSpace    = true;
        _lr.startWidth       = beamWidth;
        _lr.endWidth         = beamWidth * 0.5f;
        _lr.numCapVertices   = 4;
        _lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _lr.receiveShadows   = false;

        // Auto-generate additive unlit material so the beam glows on dark surfaces
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        Material mat = new Material(shader);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        _lr.material = mat;
    }
}