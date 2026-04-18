using System.Collections;
using TMPro;
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

    [Header("Camera Shake")]
    [SerializeField, Range(0f, 1f)] private float shakeTrauma = 0.7f;  // How hard the camera shakes (0..1)

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptyClickSound;
    
    [Header("Muzzle Flash")]
    [SerializeField] private SpriteRenderer muzzleFlashRenderer;
    [SerializeField] private Sprite[] flashFrames;
    [SerializeField] private float flashMinScale = 0.25f;
    [SerializeField] private float flashMaxScale = 0.45f;
    [SerializeField] private MuzleLight muzzleFlashLight;
    private Material _flashMat;

    [Header("Impact")]
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private GameObject bloodHit; 
    [SerializeField] private LayerMask hitLayers = ~0;

    [Header("Pellet Trails")]
    [SerializeField] private ShotgunPelletTrail pelletTrail;

    // State
    private int _currentAmmo;
    private bool _isReloading = false;
    private float _nextFireTime = 0f;
    private float _recoilOffset = 0f;
    private WeaponSway _weaponSway;

    [SerializeField] private TMP_Text AmmoText; 

    private void Awake()
    {
        _currentAmmo = maxAmmo;
        _weaponSway = GetComponentInChildren<WeaponSway>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (pelletTrail == null)
            pelletTrail = GetComponentInChildren<ShotgunPelletTrail>();

        if (muzzleFlashRenderer != null)
            _flashMat = muzzleFlashRenderer.material;


        UpdateUI();
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

    private void UpdateUI()
    {
        if (AmmoText != null)
            AmmoText.text = $"{_currentAmmo} / {maxAmmo}";
    }

    private void Shoot()
    {
        _nextFireTime = Time.time + fireRate;
        _currentAmmo--;

        PlaySound(fireSound);

        StartCoroutine(ShowMuzzleFlash());

        // Vertical recoil kick
        _recoilOffset -= recoilKick;

        // Weapon sway punch
        if (_weaponSway != null)
            _weaponSway.ApplyFirePunch();

        // Camera shake — via singleton, no reference needed
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(shakeTrauma);

        for (int i = 0; i < pelletsPerShot; i++)
            FirePellet();

        if (_currentAmmo <= 0)
            StartCoroutine(Reload());

        UpdateUI();
    }

    private IEnumerator ShowMuzzleFlash()
    {
        muzzleFlashRenderer.transform.localScale =
            Vector3.one * Random.Range(flashMinScale, flashMaxScale);
        muzzleFlashRenderer.transform.localRotation =
            Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        muzzleFlashLight.Flash();

        foreach (Sprite frame in flashFrames)
        {
            muzzleFlashRenderer.sprite = frame;
            muzzleFlashRenderer.enabled = true;
            yield return null;
        }

        muzzleFlashRenderer.enabled = false;
    }

    private void FirePellet()
    {
        Vector3 spreadDir = GetSpreadDirection();
        Ray ray = new Ray(playerCamera.transform.position, spreadDir);

        Vector3 trailOrigin = muzzlePoint != null
            ? muzzlePoint.position
            : playerCamera.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitLayers))
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>();

            if (target != null)
            {
                target.TakeDamage(damage);

                if (bloodHit != null)
                {
                    GameObject blood = Instantiate(bloodHit, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
                    Destroy(blood, 1f);
                }
            }else if (bulletHolePrefab != null)
            {
                GameObject hole = Instantiate(bulletHolePrefab,
                    hit.point + hit.normal * 0.01f,
                    Quaternion.LookRotation(-hit.normal));
                Destroy(hole, 10f);
            }

            if (pelletTrail != null)
                pelletTrail.SpawnTrail(trailOrigin, hit.point);
        }
        else
        {
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
        {
            audioSource.PlayOneShot(clip);
            print ("Playing sound: " + clip.name); 
        }
    }

    public int CurrentAmmo  => _currentAmmo;
    public int MaxAmmo      => maxAmmo;
    public bool IsReloading => _isReloading;
}

public interface IDamageable
{
    void TakeDamage(float amount);
}