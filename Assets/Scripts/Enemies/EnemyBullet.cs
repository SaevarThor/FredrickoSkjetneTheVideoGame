using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to a bullet prefab (small capsule or sphere GameObject).
/// EnemyShooter spawns this at the muzzle and it travels forward,
/// drawing a trail behind it and dealing damage on impact.
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed        = 25f;      // Units per second — slow enough to dodge
    [SerializeField] private float maxLifetime  = 3f;       // Destroy if it never hits anything
    [SerializeField] private float bulletRadius = 0.06f;    // SphereCast radius for reliable hit detection

    [Header("Trail")]
    [SerializeField] private float trailStartWidth = 0.04f;
    [SerializeField] private float trailEndWidth   = 0f;
    [SerializeField] private float trailTime       = 0.12f; // How long the trail lingers behind the bullet
    [SerializeField] private Color trailColor      = new Color(1f, 0.85f, 0.3f, 1f); // Hot yellow-white

    // Set by EnemyShooter on spawn
    [HideInInspector] public float   damage;
    [HideInInspector] public LayerMask hitLayers;
    [HideInInspector] public GameObject bulletHolePrefab;
    public GameObject BloodHitPrefab; 
    [HideInInspector] public GameObject impactFlashPrefab;

    private Vector3      _direction;
    private bool         _hasHit = false;
    private TrailRenderer _trail;

    private void Awake()
    {
        SetupTrail();
    }

    /// <summary>Called by EnemyShooter immediately after Instantiate.</summary>
    public void Init(Vector3 direction)
    {
        _direction = direction.normalized;
    }

    private void Start()
    {
        Destroy(gameObject, maxLifetime);
    }

    private void Update()
    {
        if (_hasHit) return;

        float stepDist = speed * Time.deltaTime;
        Vector3 move   = _direction * stepDist;

        // SphereCast so thin bullets don't clip through geometry at high speeds
        if (Physics.SphereCast(transform.position, bulletRadius, _direction,
            out RaycastHit hit, stepDist + bulletRadius, hitLayers))
        {
            OnHit(hit);
            return;
        }

        transform.position += move;

        // Orient bullet along travel direction
        if (_direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(_direction);
    }

    private void OnHit(RaycastHit hit)
    {
        if (_hasHit) return;
        _hasHit = true;

        // Damage
        IDamageable damageable = hit.collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);

            if (BloodHitPrefab != null)
            {
                GameObject blood = Instantiate(BloodHitPrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
                Destroy(blood, 1f);
            }
        }else if (bulletHolePrefab != null)
        {
            GameObject hole = Instantiate(bulletHolePrefab,
                hit.point + hit.normal * 0.01f,
                Quaternion.LookRotation(-hit.normal));
            Destroy(hole, 10f);

            // Impact flash
            if (impactFlashPrefab != null)
            {
                GameObject flash = Instantiate(impactFlashPrefab, hit.point, Quaternion.identity);
                Destroy(flash, 0.2f);
            }
        }

        transform.position = hit.point;
        StartCoroutine(DestroyAfterTrail());
    }

    private IEnumerator DestroyAfterTrail()
    {
        // Detach trail so it lingers in world space while bullet GameObject is gone
        if (_trail != null)
        {
            _trail.transform.SetParent(null);
            Destroy(_trail.gameObject, _trail.time + 0.1f);
        }
        Destroy(gameObject);
        yield break;
    }

    // -------------------------------------------------------------------------
    // Trail setup — built procedurally, no prefab needed
    // -------------------------------------------------------------------------
    private void SetupTrail()
    {
        _trail = gameObject.AddComponent<TrailRenderer>();

        _trail.time       = trailTime;
        _trail.startWidth = trailStartWidth;
        _trail.endWidth   = trailEndWidth;
        _trail.minVertexDistance = 0.02f;
        _trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _trail.receiveShadows    = false;
        _trail.generateLightingData = false;

        // Auto-generate additive unlit material — same approach as pellet trail
        Shader unlit = Shader.Find("Sprites/Default");
        if (unlit == null) unlit = Shader.Find("Unlit/Color");
        Material mat = new Material(unlit);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        _trail.material = mat;

        // Gradient: full color at bullet → transparent at tail
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(trailColor, 0f),
                new GradientColorKey(trailColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        _trail.colorGradient = gradient;
    }

    private void OnDestroy()
    {
        // Safety — destroy lifetime coroutine cleanup
        if (_hasHit) return;
        if (_trail != null && _trail.transform.parent == transform)
            Destroy(_trail.gameObject);
    }
}
