using System.Collections;
using System.Xml.Serialization;
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

    [Header("Charge Shot")]
    [SerializeField] private float chargeTime = 1.2f;              // Seconds to reach full charge
    [SerializeField] private float chargeSpreadMultiplier = 0.15f; // Spread at full charge (fraction of normal)
    [SerializeField] private float chargeRangeMultiplier = 2.5f;   // Range multiplier at full charge
    [SerializeField] private float chargeDamageMultiplier = 2f;    // Damage per pellet at full charge
    [SerializeField] private int chargePellets = 12;               // More pellets in a tighter spread
    [SerializeField] private AudioClip chargeStartSound;           // Sound when starting to charge
    [SerializeField] private AudioClip chargeFullSound;            // Sound when fully charged
    [SerializeField] private AudioClip chargeReleaseSound;         // Sound when released (overrides fireSound)
    [SerializeField] private AudioClip chargeCancelSound;          // Sound when charge is cancelled

    [Header("Charge Visuals")]
    [SerializeField] private float chargeFlashMinScale = 0.45f;    // Bigger flash on charged shot
    [SerializeField] private float chargeFlashMaxScale = 0.65f;

    [Header("Recoil")]
    [SerializeField] private float recoilKick = 4f;
    [SerializeField] private float recoilRecoverySpeed = 8f;

    [Header("Camera Shake")]
    [SerializeField, Range(0f, 1f)] private float shakeTrauma = 0.7f;
    [SerializeField, Range(0f, 1f)] private float chargeShakeTrauma = 1f; // Heavier shake on full charge

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource reloadSource;
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
    [SerializeField] private GameObject damageNumbers; 
    [SerializeField] private GameObject blockedByShield; 

    [Header("Pellet Trails")]
    [SerializeField] private ShotgunPelletTrail pelletTrail;

    [SerializeField] private TMP_Text AmmoText;

    // State
    private int _currentAmmo;
    private bool _isReloading = false;
    private float _nextFireTime = 0f;
    private float _recoilOffset = 0f;
    private WeaponSway _weaponSway;

    // Charge state
    private bool _isCharging = false;
    private float _chargeProgress = 0f;         // 0..1
    private bool _chargeFullSoundPlayed = false;
    private bool _chargeStartSoundPlayed = false;

    // Expose charge progress for UI/VFX if needed
    public float ChargeProgress => _chargeProgress;
    public bool IsFullyCharged  => _chargeProgress >= 1f;


    [SerializeField] private Animator anim;
    [SerializeField] private AnimationClip reloadAnim; 
    [SerializeField] private AnimationClip shootAnim; 


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

        // --- Normal fire ---
        if (Input.GetButton("Fire1") && !_isCharging)
        {
            if (_currentAmmo <= 0)
            {
                PlaySound(emptyClickSound);
                return;
            }

            if (Time.time >= _nextFireTime)
                Shoot();
        }

        // --- Charge: hold right click ---
        if (Input.GetButton("Fire2"))
        {
            if (_currentAmmo > 0 && Time.time >= _nextFireTime && !_isCharging)
                BeginCharge();
        }

        if (Input.GetButton("Fire2") && _isCharging)
            UpdateCharge();

        if (Input.GetButtonUp("Fire2") && _isCharging)
            ReleaseCharge();

        // Cancel charge if we run out of ammo mid-charge
        if (_isCharging && _currentAmmo <= 0)
            CancelCharge();

        // --- Reload ---
        if (Input.GetKeyDown(KeyCode.R) && _currentAmmo < maxAmmo && !_isCharging)
            StartCoroutine(Reload());
    }

    // -------------------------------------------------------------------------
    // Charge logic
    // -------------------------------------------------------------------------
    private void BeginCharge()
    {
        
        _isCharging = true;
        _chargeProgress = 0f;
        _chargeFullSoundPlayed = false;
        _chargeStartSoundPlayed = false;
    }

    private void UpdateCharge()
    {
        _chargeProgress = Mathf.Min(_chargeProgress + Time.deltaTime / chargeTime, 1f);

        // Play charge start sound once on the first frame of charging
        if (!_chargeStartSoundPlayed)
        {
            PlaySound(chargeStartSound);
            _chargeStartSoundPlayed = true;
        }

        // Play full charge sound once when we hit 100%
        if (_chargeProgress >= 1f && !_chargeFullSoundPlayed)
        {
            StopCurrentSound();
            PlaySound(chargeFullSound, true);
            _chargeFullSoundPlayed = true;
        }
    }

    private void ReleaseCharge()
    {
        _isCharging = false;

        // Only fire if there's any meaningful charge — avoids accidental tap releases
        if (_chargeProgress > 0.05f)
            ShootCharged(_chargeProgress);

        _chargeProgress = 0f;
    }

    private void CancelCharge()
    {
        _isCharging = false;
        _chargeProgress = 0f;
        PlaySound(chargeCancelSound);
    }

    // -------------------------------------------------------------------------
    // Normal shot
    // -------------------------------------------------------------------------
    private void Shoot()
    {
        _nextFireTime = Time.time + fireRate;
        _currentAmmo--;

        PlaySound(fireSound);
        StartCoroutine(ShowMuzzleFlash(false));

        float speedMultiplier = shootAnim.length / fireRate;
        anim.SetFloat("FireRate", speedMultiplier);
        anim.SetTrigger("isShooting"); 

        _recoilOffset -= recoilKick;

        if (_weaponSway != null)
            _weaponSway.ApplyFirePunch();

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(shakeTrauma);

        for (int i = 0; i < pelletsPerShot; i++)
            FirePellet(spreadAngle, range, damage);

        if (_currentAmmo <= 0)
            StartCoroutine(Reload());

        UpdateUI();
    }

    // -------------------------------------------------------------------------
    // Charged shot — all stats interpolated from normal to full-charge values
    // -------------------------------------------------------------------------
    private void ShootCharged(float chargeT)
    {
        _nextFireTime = Time.time + fireRate;
        _currentAmmo--;

        float currentSpread  = Mathf.Lerp(spreadAngle, spreadAngle * chargeSpreadMultiplier, chargeT);
        float currentRange   = Mathf.Lerp(range,       range   * chargeRangeMultiplier,      chargeT);
        float currentDamage  = Mathf.Lerp(damage,      damage  * chargeDamageMultiplier,     chargeT);
        int   currentPellets = Mathf.RoundToInt(Mathf.Lerp(pelletsPerShot, chargePellets,    chargeT));

        // Full charge uses a distinct release sound; partial uses normal fire sound
        StopCurrentSound();
        PlaySound(chargeT >= 1f ? chargeReleaseSound : fireSound);
        StartCoroutine(ShowMuzzleFlash(chargeT >= 1f));

        // Heavier recoil at full charge
        _recoilOffset -= recoilKick * (1f + chargeT);

        if (_weaponSway != null)
            _weaponSway.ApplyChargedFirePunch(chargeT);

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(Mathf.Lerp(shakeTrauma, chargeShakeTrauma, chargeT));

        for (int i = 0; i < currentPellets; i++)
            FirePellet(currentSpread, currentRange, currentDamage);

        if (_currentAmmo <= 0)
            StartCoroutine(Reload());

        UpdateUI();
    }

    // -------------------------------------------------------------------------
    // Pellet — shared between normal and charged shot
    // -------------------------------------------------------------------------
    private void FirePellet(float spread, float shotRange, float pelletDamage)
    {
        Vector3 spreadDir = GetSpreadDirection(spread);
        Ray ray = new Ray(playerCamera.transform.position, spreadDir);

        Vector3 trailOrigin = muzzlePoint != null
            ? muzzlePoint.position
            : playerCamera.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit, shotRange, hitLayers))
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>();

            if (hit.transform.CompareTag("Player"))
            {
                Debug.Log("Player just hit himself"); 
                return;
            }

            if (target != null)
            {
                if (target.CanTakeDamage())
                {
                    target.TakeDamage(pelletDamage);

                    if (bloodHit != null)
                    {
                        GameObject blood = Instantiate(bloodHit,
                            hit.point + hit.normal * 0.01f,
                            Quaternion.LookRotation(-hit.normal));
                        Destroy(blood, 1f);

                        GameObject g = Instantiate(damageNumbers, hit.point + hit.normal * 0.01f, Quaternion.identity); 
                        g.GetComponentInChildren<TMP_Text>().text = $" -{damage}"; 
                        
                    }

                } else
                {
                   Instantiate(blockedByShield, hit.point + hit.normal * 0.01f, Quaternion.identity); 
                }
            }
            else if (bulletHolePrefab != null)
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
                pelletTrail.SpawnMissTrail(trailOrigin, spreadDir, shotRange);
        }
    }

    // spread is now a parameter so both normal and charged shots can call this
    private Vector3 GetSpreadDirection(float spread)
    {
        Vector2 circle = Random.insideUnitCircle * Mathf.Tan(spread * Mathf.Deg2Rad);
        Vector3 localDir = new Vector3(circle.x, circle.y, 1f).normalized;
        return playerCamera.transform.TransformDirection(localDir);
    }

    // -------------------------------------------------------------------------
    // Muzzle flash — charged version uses a larger scale
    // -------------------------------------------------------------------------
    private IEnumerator ShowMuzzleFlash(bool charged)
    {
        float minScale = charged ? chargeFlashMinScale : flashMinScale;
        float maxScale = charged ? chargeFlashMaxScale : flashMaxScale;

        muzzleFlashRenderer.transform.localScale =
            Vector3.one * Random.Range(minScale, maxScale);
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

    // -------------------------------------------------------------------------
    // Reload
    // -------------------------------------------------------------------------
    private IEnumerator Reload()
    {
        _isReloading = true;
        //PlaySound(reloadSound);

        float speedMultiplier = reloadAnim.length / reloadTime;
        anim.SetFloat("ReloadSpeed", speedMultiplier);
        anim.SetTrigger("isRejuicing"); 

        if (_weaponSway != null)
        {
            _weaponSway.reloadDipDuration = reloadTime;
            _weaponSway.ApplyReloadAnimation();
        }

        if (audioSource != null && reloadSound != null)
        {
            reloadSource.pitch = reloadSound.length / reloadTime;
            reloadSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);

        audioSource.pitch = 1; 

        _currentAmmo = maxAmmo;
        _isReloading = false;
        UpdateUI();
    }

    // -------------------------------------------------------------------------
    // Recoil
    // -------------------------------------------------------------------------
    private void RecoverRecoil()
    {
        if (Mathf.Abs(_recoilOffset) < 0.01f) { _recoilOffset = 0f; return; }

        float recovery = recoilRecoverySpeed * Time.deltaTime;
        float step = Mathf.Sign(_recoilOffset) * Mathf.Min(Mathf.Abs(_recoilOffset), recovery);
        _recoilOffset -= step;
        playerCamera.transform.localRotation *= Quaternion.Euler(step, 0f, 0f);
    }

    // -------------------------------------------------------------------------
    // Upgrades
    // -------------------------------------------------------------------------
    public void ApplyFireRateUpgrade(float newFireRate)    => fireRate   = fireRate   * (1f - newFireRate);
    public void AddPellets(int amount) => pelletsPerShot += amount; 
    public void AddRange(float amount) => range += amount; 
    public void LowerSpread(float amount) => spreadAngle = spreadAngle * (1f - amount); 
    public void ApplyReloadSpeedUpgrade(float newReload)
    {
        reloadTime = reloadTime * (1f - newReload);
        
        if (reloadTime < 0.1f)
        {
            reloadTime = 0.1f;
        }

        
    }   

    public void ApplyChargeUpgrade(float buff)
    {
        chargeTime = chargeTime * (1f - buff);
        if (chargeTime < 0.1f)
        {
            chargeTime = 0.1f;
        }
    }
    public void ApplyDamageBuff(float buff)                => damage     = damage * (1f + buff);   
    public void ApplyMagSizeUpgrade(float additionalShots)
    {
        maxAmmo   += Mathf.RoundToInt(additionalShots);
        UpdateUI();
    } 


    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------
    private void UpdateUI()
    {
        if (AmmoText != null)
            AmmoText.text = $"{_currentAmmo} / {maxAmmo}";
    }

    private void PlaySound(AudioClip clip, bool loop = false)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = 1; 
            audioSource.loop = loop; 
            if (!loop)
            {
                audioSource.PlayOneShot(clip);
                print("Playing sound: " + clip.name);
            } else if (loop)
            {
                audioSource.clip = clip; 
                audioSource.Play();
            }
        }
    }

    private void StopCurrentSound()
    {
        audioSource.Stop();
    }
}

public interface IDamageable
{
    void TakeDamage(float amount);
    bool CanTakeDamage(); 
}