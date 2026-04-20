using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to enemy alongside EnemyAI.
/// Spawns a physical bullet projectile that travels through the world
/// rather than an instant raycast, so the player can see and dodge it.
/// </summary>
public class EnemyShooter : MonoBehaviour
{
    [Header("Rifle Stats")]
    [SerializeField] private float damage         = 35f;
    [SerializeField] private float fireRate       = 2.2f;
    [SerializeField] private float windUpTime     = 0.6f;
    [SerializeField] private float accuracySpread = 0.02f;

    [Header("Burst")]
    [SerializeField] private int   burstCount = 1;
    [SerializeField] private float burstDelay = 0.18f;

    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;      // Prefab with EnemyBullet component

    [Header("References")]
    [SerializeField] private Transform    muzzlePoint;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource  audioSource;
    [SerializeField] private AudioClip    fireSound;
    [SerializeField] private AudioClip    windUpSound;
    [SerializeField] private LayerMask    hitLayers = ~0;

    [Header("Impact")]
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private GameObject impactFlashPrefab;

    // State
    private bool      _engaged     = false;
    public bool      _isWindingUp = false;
    private float     _fireTimer   = 0f;
    private Transform _player;

    private void Start()
    {
        _player = ReferenceManager.Instance.PlayerTransform;
    }

    public void SetEngaged(bool engaged)
    {
        _engaged   = engaged;
        _fireTimer = fireRate * 0.5f;
        if (!engaged)
            StopAllCoroutines();
    }

    private void Update()
    {

        if (_player == null) _player = ReferenceManager.Instance.PlayerTransform; 
        if (!_engaged || _player == null || _isWindingUp) return;

        _fireTimer -= Time.deltaTime;
        if (_fireTimer <= 0f)
            StartCoroutine(WindUpAndFire());
    }

    private IEnumerator WindUpAndFire()
    {
        _isWindingUp = true;
        _fireTimer   = fireRate;

        if (audioSource != null && windUpSound != null)
            audioSource.PlayOneShot(windUpSound);

        yield return new WaitForSeconds(windUpTime);

        if (!_engaged) { _isWindingUp = false; yield break; }

        for (int i = 0; i < burstCount; i++)
        {
            FireShot();
            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstDelay);
        }

        _isWindingUp = false;
    }

    private void FireShot()
    {
        if (muzzleFlash != null)
            muzzleFlash.Play();

        if (audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound);

        Vector3 origin    = muzzlePoint != null ? muzzlePoint.position : transform.position + Vector3.up;
        Vector3 targetDir = (_player.position + Vector3.up * 0.2f - origin).normalized;

        // Small spread so it's not perfectly accurate
        Vector3 spread = new Vector3(
            Random.Range(-accuracySpread, accuracySpread),
            Random.Range(-accuracySpread, accuracySpread),
            0f
        );
        Vector3 shootDir = (targetDir + spread).normalized;

        if (bulletPrefab == null)
        {
            Debug.LogWarning("[EnemyShooter] No bullet prefab assigned!");
            return;
        }

        // Spawn bullet and hand it everything it needs
        GameObject bulletGO = Instantiate(bulletPrefab, origin, Quaternion.LookRotation(shootDir));
        EnemyBullet bullet  = bulletGO.GetComponent<EnemyBullet>();

        if (bullet != null)
        {
            bullet.damage             = damage;
            bullet.hitLayers          = hitLayers;
            bullet.bulletHolePrefab   = bulletHolePrefab;
            bullet.impactFlashPrefab  = impactFlashPrefab;
            bullet.Init(shootDir);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_player == null || !_engaged) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + Vector3.up, _player.position + Vector3.up * 0.8f);
    }
}