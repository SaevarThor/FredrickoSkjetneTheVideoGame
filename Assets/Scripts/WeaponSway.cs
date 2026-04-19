using UnityEngine;

/// <summary>
/// Attach to the weapon model (child of Camera).
/// Handles idle sway, movement bob, fire punch, reload dip, and charge pose.
/// </summary>
public class WeaponSway : MonoBehaviour
{
    [Header("Idle Sway")]
    [SerializeField] private float swayAmount = 0.02f;
    [SerializeField] private float maxSwayAmount = 0.08f;
    [SerializeField] private float swaySmoothness = 8f;

    [Header("Movement Bob")]
    [SerializeField] private float bobFrequency = 8f;
    [SerializeField] private float bobAmplitudeX = 0.005f;
    [SerializeField] private float bobAmplitudeY = 0.008f;

    [Header("Fire Punch")]
    [SerializeField] private float punchKickback = 0.06f;
    [SerializeField] private float punchRise = 0.02f;
    [SerializeField] private float punchRecovery = 10f;

    [Header("Reload Dip")]
    [SerializeField] private float reloadDipAmount = 0.12f;
    public float reloadDipDuration = 0.4f;

    [Header("Charge Pose")]
    [SerializeField] private float chargePullback = 0.08f;          // Z pullback while charging
    [SerializeField] private float chargeLowerAmount = 0.03f;       // Y lower while charging
    [SerializeField] private float chargeTiltAngle = 12f;           // Forward tilt in degrees at full charge
    [SerializeField] private float chargeSwaySuppress = 0.2f;       // How much sway is reduced at full charge
    [SerializeField] private float chargePoseSmoothness = 6f;       // How fast weapon settles into charge pose
    [SerializeField] private float chargeReleasePunchScale = 1.8f;  // Punch multiplier on charged release

    // Internal
    private Vector3 _originPosition;
    private Quaternion _originRotation;

    private Vector3 _swayOffset;
    private Vector3 _bobOffset;
    private Vector3 _punchOffset;
    private Vector3 _reloadOffset;
    private Vector3 _chargeOffset;
    private Quaternion _chargeTiltOffset = Quaternion.identity;

    private float _bobTimer = 0f;
    private float _punchRecoveryT = 1f;
    private float _chargeT = 0f;
    private bool  _isCharging = false;

    private CharacterController _characterController;
    private ShotgunShooter _shooter;

    private void Awake()
    {
        _originPosition = transform.localPosition;
        _originRotation = transform.localRotation;
        _characterController = GetComponentInParent<CharacterController>();

        // Read ChargeProgress each frame rather than requiring manual calls
        _shooter = GetComponentInParent<ShotgunShooter>();
    }

    private void Update()
    {
        if (_shooter != null)
        {
            _chargeT   = _shooter.ChargeProgress;
            _isCharging = _chargeT > 0f;
        }

        UpdateSway();
        UpdateBob();
        RecoverPunch();
        UpdateChargePose();

        Vector3 targetPos = _originPosition
            + _swayOffset
            + _bobOffset
            + _punchOffset
            + _reloadOffset
            + _chargeOffset;

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos,
            Time.deltaTime * swaySmoothness);
    }

    // -------------------------------------------------------------------------
    // Idle Sway — suppressed while charging so aim feels deliberate
    // -------------------------------------------------------------------------
    private void UpdateSway()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        // Linearly reduce sway toward chargeSwaySuppress as charge fills
        float swayScale = _isCharging ? Mathf.Lerp(1f, chargeSwaySuppress, _chargeT) : 1f;

        float swayX = Mathf.Clamp(-mouseX * swayAmount * swayScale, -maxSwayAmount, maxSwayAmount);
        float swayY = Mathf.Clamp(-mouseY * swayAmount * swayScale, -maxSwayAmount, maxSwayAmount);

        _swayOffset = Vector3.Lerp(_swayOffset, new Vector3(swayX, swayY, 0f),
            Time.deltaTime * swaySmoothness);

        // Blend sway rotation with charge tilt
        Quaternion swayRot  = Quaternion.Euler(-swayY * 10f, swayX * 10f, swayX * 5f);
        Quaternion targetRot = _originRotation * swayRot * _chargeTiltOffset;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot,
            Time.deltaTime * swaySmoothness);
    }

    // -------------------------------------------------------------------------
    // Movement Bob — suppressed while charging
    // -------------------------------------------------------------------------
    private void UpdateBob()
    {
        bool isMoving = _characterController != null
            && _characterController.velocity.magnitude > 0.1f
            && _characterController.isGrounded;

        // Bob feels wrong when bracing for a big shot — fade it out as charge fills
        float bobScale = _isCharging ? Mathf.Lerp(1f, 0.1f, _chargeT) : 1f;

        if (isMoving)
        {
            _bobTimer += Time.deltaTime * bobFrequency;
            _bobOffset = new Vector3(
                Mathf.Sin(_bobTimer) * bobAmplitudeX * bobScale,
                Mathf.Abs(Mathf.Sin(_bobTimer)) * bobAmplitudeY * bobScale,
                0f
            );
        }
        else
        {
            _bobOffset = Vector3.Lerp(_bobOffset, Vector3.zero, Time.deltaTime * swaySmoothness);
            _bobTimer  = 0f;
        }
    }

    // -------------------------------------------------------------------------
    // Charge pose — weapon pulls back and tilts forward as charge builds,
    // communicating to the player that something powerful is loading
    // -------------------------------------------------------------------------
    private void UpdateChargePose()
    {
        if (_isCharging)
        {
            // Pull back and lower slightly — bracing for the shot
            Vector3 targetChargeOffset = new Vector3(
                0f,
                -chargeLowerAmount * _chargeT,
                -chargePullback    * _chargeT
            );
            _chargeOffset = Vector3.Lerp(_chargeOffset, targetChargeOffset,
                Time.deltaTime * chargePoseSmoothness);

            // Tilt barrel forward — negative X rotation tips the barrel downward/forward
            float tiltAngle = -chargeTiltAngle * _chargeT;
            _chargeTiltOffset = Quaternion.Slerp(_chargeTiltOffset,
                Quaternion.Euler(tiltAngle, 0f, 0f),
                Time.deltaTime * chargePoseSmoothness);
        }
        else
        {
            // Return to rest
            _chargeOffset = Vector3.Lerp(_chargeOffset, Vector3.zero,
                Time.deltaTime * chargePoseSmoothness);
            _chargeTiltOffset = Quaternion.Slerp(_chargeTiltOffset,
                Quaternion.identity,
                Time.deltaTime * chargePoseSmoothness);
        }
    }

    // -------------------------------------------------------------------------
    // Fire Punch — normal shot
    // -------------------------------------------------------------------------
    public void ApplyFirePunch()
    {
        _punchOffset    = new Vector3(0f, punchRise, -punchKickback);
        _punchRecoveryT = 0f;
    }

    /// <summary>
    /// Heavier punch on charged release.
    /// Call this from ShotgunShooter.ShootCharged() instead of ApplyFirePunch().
    /// </summary>
    public void ApplyChargedFirePunch(float chargeT)
    {
        float scale     = Mathf.Lerp(1f, chargeReleasePunchScale, chargeT);
        _punchOffset    = new Vector3(0f, punchRise * scale, -punchKickback * scale);
        _punchRecoveryT = 0f;
    }

    private void RecoverPunch()
    {
        if (_punchRecoveryT >= 1f) return;

        _punchRecoveryT = Mathf.MoveTowards(_punchRecoveryT, 1f, Time.deltaTime * punchRecovery);
        _punchOffset    = Vector3.Lerp(_punchOffset, Vector3.zero, _punchRecoveryT);
    }

    // -------------------------------------------------------------------------
    // Reload Dip
    // -------------------------------------------------------------------------
    public void ApplyReloadAnimation()
    {
        StartCoroutine(ReloadDipRoutine());
    }

    private System.Collections.IEnumerator ReloadDipRoutine()
    {
        float elapsed      = 0f;
        float fifthDuration = reloadDipDuration / 5f;
        float waitTimer    = fifthDuration * 3f;

        while (elapsed < fifthDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fifthDuration;
            _reloadOffset = Vector3.Lerp(Vector3.zero, new Vector3(0f, -reloadDipAmount, 0f), t);
            yield return null;
        }

        yield return new WaitForSeconds(waitTimer);

        elapsed = 0f;
        while (elapsed < fifthDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fifthDuration;
            _reloadOffset = Vector3.Lerp(new Vector3(0f, -reloadDipAmount, 0f), Vector3.zero, t);
            yield return null;
        }

        _reloadOffset = Vector3.zero;
    }
}