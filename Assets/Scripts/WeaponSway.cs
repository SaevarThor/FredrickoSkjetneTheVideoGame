using UnityEngine;

/// <summary>
/// Attach to the weapon model (child of Camera).
/// Handles idle sway, movement bob, fire punch, and reload dip.
/// </summary>
public class WeaponSway : MonoBehaviour
{
    [Header("Idle Sway")]
    [SerializeField] private float swayAmount = 0.02f;
    [SerializeField] private float maxSwayAmount = 0.08f;
    [SerializeField] private float swaySmoothness = 8f;

    [Header("Movement Bob")]
    [SerializeField] private float bobFrequency = 8f;
    [SerializeField] private float bobAmplitudeX = 0.005f;  // Horizontal
    [SerializeField] private float bobAmplitudeY = 0.008f;  // Vertical

    [Header("Fire Punch")]
    [SerializeField] private float punchKickback = 0.06f;   // Z pushback on fire
    [SerializeField] private float punchRise = 0.02f;       // Y rise on fire
    [SerializeField] private float punchRecovery = 10f;

    [Header("Reload Dip")]
    [SerializeField] private float reloadDipAmount = 0.12f;
    [SerializeField] private float reloadDipDuration = 0.4f;

    // Internal
    private Vector3 _originPosition;
    private Quaternion _originRotation;

    private Vector3 _swayOffset;
    private Vector3 _bobOffset;
    private Vector3 _punchOffset;
    private Vector3 _reloadOffset;

    private float _bobTimer = 0f;
    private float _punchRecoveryT = 1f;   // 1 = fully recovered
    private CharacterController _characterController;

    private void Awake()
    {
        _originPosition = transform.localPosition;
        _originRotation = transform.localRotation;
        _characterController = GetComponentInParent<CharacterController>();
    }

    private void Update()
    {
        UpdateSway();
        UpdateBob();
        RecoverPunch();

        // Combine all offsets and apply
        Vector3 targetPos = _originPosition + _swayOffset + _bobOffset + _punchOffset + _reloadOffset;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * swaySmoothness);
    }

    // -------------------------------------------------------------------------
    // Idle Sway — weapon drifts opposite to mouse movement
    // -------------------------------------------------------------------------
    private void UpdateSway()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        float swayX = Mathf.Clamp(-mouseX * swayAmount, -maxSwayAmount, maxSwayAmount);
        float swayY = Mathf.Clamp(-mouseY * swayAmount, -maxSwayAmount, maxSwayAmount);

        _swayOffset = Vector3.Lerp(_swayOffset, new Vector3(swayX, swayY, 0f), Time.deltaTime * swaySmoothness);

        // Subtle tilt rotation from sway
        Quaternion targetRot = Quaternion.Euler(-swayY * 10f, swayX * 10f, swayX * 5f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, _originRotation * targetRot,
            Time.deltaTime * swaySmoothness);
    }

    // -------------------------------------------------------------------------
    // Movement Bob — sine wave while walking/sprinting
    // -------------------------------------------------------------------------
    private void UpdateBob()
    {
        bool isMoving = _characterController != null && _characterController.velocity.magnitude > 0.1f
                        && _characterController.isGrounded;

        if (isMoving)
        {
            _bobTimer += Time.deltaTime * bobFrequency;
            _bobOffset = new Vector3(
                Mathf.Sin(_bobTimer) * bobAmplitudeX,
                Mathf.Abs(Mathf.Sin(_bobTimer)) * bobAmplitudeY,  // Abs keeps it always upward
                0f
            );
        }
        else
        {
            // Smoothly return to zero when not moving
            _bobOffset = Vector3.Lerp(_bobOffset, Vector3.zero, Time.deltaTime * swaySmoothness);
            _bobTimer = 0f;
        }
    }

    // -------------------------------------------------------------------------
    // Fire Punch — called by ShotgunShooter on each shot
    // -------------------------------------------------------------------------
    public void ApplyFirePunch()
    {
        // Instantly apply kickback and rise; RecoverPunch() lerps it back
        _punchOffset = new Vector3(0f, punchRise, -punchKickback);
        _punchRecoveryT = 0f;
    }

    private void RecoverPunch()
    {
        if (_punchRecoveryT >= 1f) return;

        _punchRecoveryT = Mathf.MoveTowards(_punchRecoveryT, 1f, Time.deltaTime * punchRecovery);
        _punchOffset = Vector3.Lerp(_punchOffset, Vector3.zero, _punchRecoveryT);
    }

    // -------------------------------------------------------------------------
    // Reload Dip — called by ShotgunShooter when reloading starts
    // -------------------------------------------------------------------------
    public void ApplyReloadAnimation()
    {
        StartCoroutine(ReloadDipRoutine());
    }

    private System.Collections.IEnumerator ReloadDipRoutine()
    {
        float elapsed = 0f;
        float halfDuration = reloadDipDuration / 2f;

        // Dip down
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            _reloadOffset = Vector3.Lerp(Vector3.zero, new Vector3(0f, -reloadDipAmount, 0f), t);
            yield return null;
        }

        // Rise back
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            _reloadOffset = Vector3.Lerp(new Vector3(0f, -reloadDipAmount, 0f), Vector3.zero, t);
            yield return null;
        }

        _reloadOffset = Vector3.zero;
    }
}
