using UnityEngine;

public class ProximityShake : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform poi;  // Assign your POI in the inspector

    [Header("Shake Settings")]
    [SerializeField] private float shakeStartDistance = 10f;
    [SerializeField] private float shakeMaxDistance = 2f;
    [SerializeField] private float maxPositionShake = 0.05f;   // How far it shifts in units
    [SerializeField] private float maxRotationShake = 3f;      // How many degrees it rotates
    [SerializeField] private float shakeSpeed = 25f;           // Vibration frequency

    private Vector3 _originLocalPosition;
    private Quaternion _originLocalRotation;

    void Start()
    {
        _originLocalPosition = transform.localPosition;
        _originLocalRotation = transform.localRotation;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, poi.position);

        if (distance >= shakeStartDistance)
        {
            // Snap back to rest when out of range
            transform.localPosition = _originLocalPosition;
            transform.localRotation = _originLocalRotation;
            return;
        }

        float shakeT = 1f - Mathf.Clamp01(
            (distance - shakeMaxDistance) / (shakeStartDistance - shakeMaxDistance)
        );
        shakeT = shakeT * shakeT; // Ease in

        float t = Time.time * shakeSpeed;

        // Layer a few sine waves at different frequencies for organic feel
        Vector3 posShake = new Vector3(
            (Mathf.Sin(t * 1.0f) + Mathf.Sin(t * 2.3f)) * 0.5f,
            (Mathf.Sin(t * 1.7f) + Mathf.Sin(t * 3.1f)) * 0.5f,
            (Mathf.Sin(t * 1.3f) + Mathf.Sin(t * 2.7f)) * 0.5f
        ) * maxPositionShake * shakeT;

        Vector3 rotShake = new Vector3(
            (Mathf.Sin(t * 1.1f) + Mathf.Sin(t * 2.9f)) * 0.5f,
            (Mathf.Sin(t * 1.6f) + Mathf.Sin(t * 3.3f)) * 0.5f,
            (Mathf.Sin(t * 1.4f) + Mathf.Sin(t * 2.1f)) * 0.5f
        ) * maxRotationShake * shakeT;

        transform.localPosition = _originLocalPosition + posShake;
        transform.localRotation = _originLocalRotation * Quaternion.Euler(rotShake);
    }
}
