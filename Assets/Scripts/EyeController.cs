using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class EyeController : MonoBehaviour
{
    [Header("Points of Interest")]
    public Transform[] pointsOfInterest;
    [SerializeField] private float maxRange = 100f;

    [Header("Spring Settings")]
    [SerializeField] private float springStrength = 10f;
    [SerializeField] private float damping = 0.75f;
    [SerializeField] private float maxAngularSpeed = 360f;

    [Header("Proximity Shake")]
    [SerializeField] private float vobbleStartDistance = 10f;  // Distance at which shake begins
    [SerializeField] private float vobbleMaxDistance = 2f;     // Distance at which shake is at full intensity
    [SerializeField] private float vobbleIntensity = 15f;      // Max shake angle in degrees
    [SerializeField] private float vobbleSpeed = 12f;          // How fast it shakes


    [Header("Shake Settings")]
    [SerializeField] private float shakeStartDistance = 10f;
    [SerializeField] private float shakeMaxDistance = 2f;
    [SerializeField] private float shakeIntensity = 0.05f;   // How far it shifts in units
    [SerializeField] private float maxRotationShake = 3f;      // How many degrees it rotates
    [SerializeField] private float shakeSpeed = 25f;           // Vibration frequency

   
    [Header("Idle Spin")]
    [SerializeField] private float idleSpinSpeed = 45f;        // Degrees per second base speed
    [SerializeField] private float idleWanderStrength = 30f;   // How much it wanders off axis
    [SerializeField] private float idleWanderSpeed = 0.5f;     // How slowly it wanders

    private float _idleSpinTime = 0f;
    private Vector3 _originLocalPosition;
    private Quaternion _originLocalRotation;
    private float _angularVelocity = 0f;
    private Quaternion _currentRotation;
    private float _shakeTime = 0f;


    void Start()
    {
        _currentRotation = transform.rotation;
        _originLocalPosition = transform.localPosition;


        var objects = GameObject.FindGameObjectsWithTag("Eye_PointOfInterest"); 
        Transform[] t = new Transform[objects.Length];

        for(int i = 0; i < objects.Length; i++)
        {
            t[i] = objects[i].transform; 
        }


        pointsOfInterest = t; 

    }

    void Update()
    {
        Debug.Log(pointsOfInterest.Length);
        if (pointsOfInterest.Length == 0) {
            DoIdleSpin();
            return;
        }

        var currentTransform = transform.position;
        var targetTransform = GetNearestPOI();
        if (targetTransform == null)
        {
            DoIdleSpin();
        }
        if (Vector3.Distance(currentTransform, targetTransform.position) > maxRange) {
            DoIdleSpin();
            return;
        }

        var targetAngle = targetTransform.position - currentTransform;
       

        Quaternion targetRotation = Quaternion.LookRotation(targetAngle);
       

        // Get target Y rotation
        float targetY = targetRotation.eulerAngles.y;
        float currentY = transform.rotation.eulerAngles.y;

        // Get shortest angle delta (-180 to 180)
        float delta = Mathf.DeltaAngle(currentY, targetY);

        // Spring force pulls toward target, damping resists velocity
        float springForce = delta * springStrength;
        float dampingForce = -_angularVelocity * (damping * 2f * Mathf.Sqrt(springStrength));

        // Accumulate and cap angular velocity
        _angularVelocity += (springForce + dampingForce) * Time.deltaTime;
        _angularVelocity = Mathf.Clamp(_angularVelocity, -maxAngularSpeed, maxAngularSpeed);

        // Apply rotation on Y axis only
        float newY = currentY + _angularVelocity * Time.deltaTime;

        // Proximity vobble
        float distance = targetAngle.magnitude;
        float shakeAmount = 0f;

        if (distance < shakeStartDistance)
        {
            // 0 at shakeStartDistance, 1 at shakeMaxDistance (clamped)
            float vobbleT = 1f - Mathf.Clamp01(
                (distance - shakeMaxDistance) / (shakeStartDistance - shakeMaxDistance)
            );

            // Ease in with a curve so it ramps up sharply when close
            vobbleT = vobbleT * vobbleT;

            _shakeTime += Time.deltaTime * vobbleSpeed;
            shakeAmount = Mathf.Sin(_shakeTime) * vobbleIntensity * vobbleT;
        }

        transform.rotation = Quaternion.Euler(0, newY + shakeAmount, 0);



        // Ball proximity shake
        if (distance >= shakeStartDistance)
        {
            // Snap back to rest when out of range
            transform.localPosition = _originLocalPosition;
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
        ) * (shakeIntensity / 1000) * shakeT;

        transform.localPosition = _originLocalPosition + posShake;
    }

    void DoIdleSpin()
    {
        _idleSpinTime += Time.deltaTime;
        _angularVelocity = 0f; // Reset spring so it doesnt lurch when a POI appears

        // Slowly drifting Y rotation
        float wanderY = Mathf.Sin(_idleSpinTime * idleWanderSpeed) * idleWanderStrength;
        float currentY = transform.rotation.eulerAngles.y;
        float newY = currentY + (idleSpinSpeed + wanderY) * Time.deltaTime;

        // Gentle tilt on X and Z for the aimless wandering feel
        float tiltX = Mathf.Sin(_idleSpinTime * idleWanderSpeed * 1.3f) * 15f;
        float tiltZ = Mathf.Sin(_idleSpinTime * idleWanderSpeed * 0.7f) * 10f;

        transform.rotation = Quaternion.Euler(tiltX, newY, tiltZ);
    }

    Transform GetNearestPOI()
    {
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Transform poi in pointsOfInterest)
        {
            if (poi == null) continue;
            float dist = Vector3.Distance(transform.position, poi.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = poi;
            }
        }

        return nearest;
    }
}
