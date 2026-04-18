using System.Collections;
using UnityEngine;

public class ShotgunShooter : MonoBehaviour
{
    [Header("Shotgun Stats")]
    [SerializeField] private int pelletsPerShot = 8;
    [SerializeField] private float spreadAngle = 6f;
    [SerializeField] private float range = 30f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float fireRate = 0.9f;
    [SerializeField] private int maxAmmo = 8;
    [SerializeField] private float reloadTime = 2.5f;

    [Header("Recoil")]
    [SerializeField] private float recoilKick = 4f;
    [SerializeField] private float recoilRecoverySpeed = 8f;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform muzzlePoint;         // Empty GO at barrel tip — also used as trail origin
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptyClickSound;

    [Header("Impact")]
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private LayerMask hitLayers = ~0;

    [Header("Pellet Trails")]
    [Tooltip("Assign the ShotgunPelletTrail component here (lives on any persistent GO)")]
    [SerializeField] private ShotgunPelletTrail pelletTrail;

    // State
    private int _currentAmmo;
    private bool _isReloading = false;
    private float _nextFireTime = 0f;
    private float _recoilOffset = 0f;
    private WeaponSway _weaponSway;

    private void Awake()
    {
        _currentAmmo = maxAmmo;
        _weaponSway = GetComponentInChildren<WeaponSway>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        // Auto-find trail component if not assigned in Inspector
        if (pelletTrail == null)
            pelletTrail = GetComponentInChildren<ShotgunPelletTrail>();
    }

    private void Update()
    {
        HandleInput();
        RecoverRecoil();
    }

    private void HandleInput()
    {
        if (_isReloading) return;

        if (Input.GetButtonDown("Fire1"))
        {
            if (_currentAmmo <= 0)
            {
                PlaySound(emptyClickSound);
                return;
            }

            if (Time.time >= _nextFireTime)
                Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && _currentAmmo < maxAmmo)
            StartCoroutine(Reload());
    }

    private void Shoot()
    {
        _nextFireTime = Time.time + fireRate;
        _currentAmmo--;

        if (muzzleFlash != null)
            muzzleFlash.Play();

        PlaySound(fireSound);
        _recoilOffset -= recoilKick;

        if (_weaponSway != null)
            _weaponSway.ApplyFirePunch();

        for (int i = 0; i < pelletsPerShot; i++)
            FirePellet();

        if (_currentAmmo <= 0)
            StartCoroutine(Reload());
    }

    private void FirePellet()
    {
        Vector3 spreadDir = GetSpreadDirection();
        Ray ray = new Ray(playerCamera.transform.position, spreadDir);

        // Determine the muzzle world position for trail start
        Vector3 trailOrigin = muzzlePoint != null
            ? muzzlePoint.position
            : playerCamera.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitLayers))
        {
            // Damage
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            target?.TakeDamage(damage);

            // Bullet hole decal
            if (bulletHolePrefab != null)
            {
                GameObject hole = Instantiate(bulletHolePrefab,
                    hit.point + hit.normal * 0.01f,
                    Quaternion.LookRotation(-hit.normal));
                Destroy(hole, 10f);
            }

            // Spawn trail from muzzle to impact point
            if (pelletTrail != null)
                pelletTrail.SpawnTrail(trailOrigin, hit.point);
        }
        else
        {
            // Miss — trail travels to max range
            if (pelletTrail != null)
                pelletTrail.SpawnMissTrail(trailOrigin, spreadDir, range);
        }
    }

    private Vector3 GetSpreadDirection()
    {
        Vector2 circle = Random.insideUnitCircle * Mathf.Tan(spreadAngle * Mathf.Deg2Rad);
        Vector3 localDir = new Vector3(circle.x, circle.y, 1f).normalized;
        return playerCamera.transform.TransformDirection(localDir);
    }

    private IEnumerator Reload()
    {
        _isReloading = true;
        PlaySound(reloadSound);

        if (_weaponSway != null)
            _weaponSway.ApplyReloadAnimation();

        yield return new WaitForSeconds(reloadTime);

        _currentAmmo = maxAmmo;
        _isReloading = false;
    }

    private void RecoverRecoil()
    {
        if (Mathf.Abs(_recoilOffset) < 0.01f) { _recoilOffset = 0f; return; }

        float recovery = recoilRecoverySpeed * Time.deltaTime;
        float step = Mathf.Sign(_recoilOffset) * Mathf.Min(Mathf.Abs(_recoilOffset), recovery);
        _recoilOffset -= step;
        playerCamera.transform.localRotation *= Quaternion.Euler(step, 0f, 0f);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    public int CurrentAmmo  => _currentAmmo;
    public int MaxAmmo      => maxAmmo;
    public bool IsReloading => _isReloading;
}

public interface IDamageable
{
    void TakeDamage(float amount);
}