using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to an empty GameObject on the ShotgunShooter's same object (or any persistent GO).
/// ShotgunShooter calls SpawnTrail() for each pellet hit.
/// The trail draws a line from the muzzle to the impact point, then fades out.
/// </summary>
public class ShotgunPelletTrail : MonoBehaviour
{
    [Header("Trail Appearance")]
    [SerializeField] private Color trailStartColor = new Color(1f, 0.05f, 0.05f, 1f);   // Bright red
    [SerializeField] private Color trailEndColor   = new Color(0.6f, 0f, 0f, 1f);        // Darker red at impact end
    [SerializeField] private float startWidth      = 0.018f;
    [SerializeField] private float endWidth        = 0.006f;

    [Header("Timing")]
    [SerializeField] private float travelTime  = 0.055f;   // How long the line takes to "draw" to the hit point
    [SerializeField] private float holdTime    = 0.12f;    // How long the full trail is visible
    [SerializeField] private float fadeTime    = 0.25f;    // How long it takes to fade out

    [Header("Material")]
    [Tooltip("Leave null to use a generated unlit additive material")]
    [SerializeField] private Material trailMaterial;

    private void Awake()
    {
        // Auto-generate a simple additive unlit material if none is assigned.
        // This gives the trail a glowing, hot look without needing an asset.
        if (trailMaterial == null)
        {
            Shader unlit = Shader.Find("Sprites/Default");
            if (unlit == null) unlit = Shader.Find("Unlit/Color");
            trailMaterial = new Material(unlit);
            trailMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            trailMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive blend
            trailMaterial.SetInt("_ZWrite", 0);
            trailMaterial.renderQueue = 3000;
        }
    }

    // -------------------------------------------------------------------------
    // Public API — called by ShotgunShooter for each pellet
    // -------------------------------------------------------------------------

    /// <summary>Call this when a pellet hits something.</summary>
    public void SpawnTrail(Vector3 muzzlePosition, Vector3 hitPosition)
    {
        StartCoroutine(TrailRoutine(muzzlePosition, hitPosition));
    }

    /// <summary>Call this when a pellet misses (traces to max range).</summary>
    public void SpawnMissTrail(Vector3 muzzlePosition, Vector3 direction, float range)
    {
        Vector3 endPoint = muzzlePosition + direction * range;
        StartCoroutine(TrailRoutine(muzzlePosition, endPoint));
    }

    // -------------------------------------------------------------------------
    // Trail coroutine — draws, holds, then fades the LineRenderer
    // -------------------------------------------------------------------------
    private IEnumerator TrailRoutine(Vector3 start, Vector3 end)
    {
        // Create a fresh GameObject with a LineRenderer for this trail
        GameObject trailGO = new GameObject("PelletTrail");
        LineRenderer lr = trailGO.AddComponent<LineRenderer>();

        lr.material          = trailMaterial;
        lr.positionCount     = 2;
        lr.useWorldSpace     = true;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows    = false;

        lr.startWidth = startWidth;
        lr.endWidth   = endWidth;

        lr.SetPosition(0, start);
        lr.SetPosition(1, start); // Start collapsed at muzzle

        SetColors(lr, 1f);

        // --- Phase 1: Draw the line from muzzle to hit point ---
        float elapsed = 0f;
        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);
            lr.SetPosition(1, Vector3.Lerp(start, end, t));
            yield return null;
        }
        lr.SetPosition(1, end);

        // --- Phase 2: Hold ---
        yield return new WaitForSeconds(holdTime);

        // --- Phase 3: Fade out ---
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / fadeTime));
            SetColors(lr, alpha);
            yield return null;
        }

        Destroy(trailGO);
    }

    // -------------------------------------------------------------------------
    // Helper — sets both color keys with a given alpha
    // -------------------------------------------------------------------------
    private void SetColors(LineRenderer lr, float alpha)
    {
        Color s = trailStartColor;
        Color e = trailEndColor;
        s.a = alpha;
        e.a = alpha;

        // Use a gradient so start and end can be different colors
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(s, 0f), new GradientColorKey(e, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0f), new GradientAlphaKey(alpha, 1f) }
        );
        lr.colorGradient = gradient;
    }
}