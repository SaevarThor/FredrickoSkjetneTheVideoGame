using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;


public class EnemyAIFlyingSupport : BaseEnemyAI
{


    [Header("Flight")]
    [SerializeField] private float flySpeed = 5f;
    [SerializeField] private float hoverHeight = 4f;
    [SerializeField] private float bobAmplitude = 0.4f;
    [SerializeField] private float bobSpeed = 1.2f;

    [Header("Support Hovering")]
    [SerializeField] private float hoverRadius = 3f;           // How far from the ally it hovers
    [SerializeField] private float allySearchRadius = 20f;     // How far it looks for allies
    [SerializeField] private float switchAllyMinTime = 4f;     // Min seconds before switching ally
    [SerializeField] private float switchAllyMaxTime = 9f;     // Max seconds before switching ally

    [Header("Wandering")]
    [SerializeField] private float wanderRadius = 8f;          // How far wander points are picked
    [SerializeField] private float wanderArrivalDist = 1.5f;   // How close = arrived

    private enum FlyState { Protecting, Wandering }
    private FlyState _flyState = FlyState.Wandering;

    private Transform _currentAlly;
    private Vector3 _hoverOffset;          // Offset around ally we're aiming for
    private float _switchAllyTimer = 0f;

    private Vector3 _wanderTarget;
    private float _bobTime = 0f;

    // -------------------------------------------------------------------------
    public override void Awake()
    {
        base.Awake();
        EnterCombat();
    }

    // -------------------------------------------------------------------------
    // Idle - detect player to enter combat
    // -------------------------------------------------------------------------
    public override void UpdateIdle()
    {
        UpdateWandering();
        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist <= detectionRadius)
            EnterCombat();
    }

    public override void EnterCombat()
    {
        base.EnterCombat();
        TryPickAlly();
    }

    public override void ExitCombat()
    {
        Debug.Log("exit combat");
        base.ExitCombat();
        _currentAlly = null;
        _flyState = FlyState.Wandering;
        PickWanderTarget();
    }

    // -------------------------------------------------------------------------
    // Combat
    // -------------------------------------------------------------------------
    public override void UpdateCombat()
    {

        _bobTime += Time.deltaTime * bobSpeed;

        // Switch ally on a timer
        _switchAllyTimer -= Time.deltaTime;
        if (_switchAllyTimer <= 0f)
            TryPickAlly();

        // Clean up dead/missing ally
        if (_currentAlly == null)
        {
            TryPickAlly();
            if (_currentAlly == null)
                _flyState = FlyState.Wandering;
        }

        switch (_flyState)
        {
            case FlyState.Protecting: UpdateHovering(); break;
            case FlyState.Wandering: UpdateWandering(); break;
        }
    }

    // -------------------------------------------------------------------------
    // Hovering - float near chosen ally
    // -------------------------------------------------------------------------
    private void UpdateHovering()
    {
        Vector3 target = _currentAlly.position + _hoverOffset;
        target.y = _currentAlly.position.y + hoverHeight + Mathf.Sin(_bobTime) * bobAmplitude;

        transform.position = Vector3.Lerp(transform.position, target, flySpeed * Time.deltaTime);
        FacePlayer();
    }

    // -------------------------------------------------------------------------
    // Wandering - drift to random points when no ally available
    // -------------------------------------------------------------------------
    private void UpdateWandering()
    {
        float bob = Mathf.Sin(_bobTime) * bobAmplitude;

        Vector3 target = new Vector3(_wanderTarget.x, _wanderTarget.y + bob, _wanderTarget.z);

        // MoveTowards never overshoots, so arrival is clean
        transform.position = Vector3.MoveTowards(
            transform.position, target, flySpeed * Time.deltaTime
        );

        FacePlayer();

        if (Vector3.Distance(transform.position, _wanderTarget) < wanderArrivalDist)
        {
            Debug.Log("arrived, need new target");
            PickWanderTarget();
        }
    }

    private void PickWanderTarget()
    {
        Vector2 rand = Random.insideUnitCircle * wanderRadius;
        _wanderTarget = new Vector3(
            transform.position.x + rand.x,
            transform.position.y,   // Keep current height, bob handles the rest
            transform.position.z + rand.y
        );
    }

    // -------------------------------------------------------------------------
    // Ally picking
    // -------------------------------------------------------------------------
    private void TryPickAlly()
    {
        // Find all enemies in range, excluding self
        Collider[] hits = Physics.OverlapSphere(transform.position, allySearchRadius);
        List<Transform> candidates = new List<Transform>();

        foreach (Collider hit in hits)
        {
            BaseEnemyAI enemy = hit.GetComponent<BaseEnemyAI>();
            if (enemy != null && enemy != this && EvaluateAlly(enemy.transform))
                candidates.Add(hit.transform);
        }

        if (candidates.Count == 0)
        {
            _currentAlly = null;
            _flyState = FlyState.Wandering;
            ExitCombat();
            return;
        }

        // Pick randomly, avoid re-picking the same one if possible
        if (candidates.Count > 1)
            candidates.RemoveAll(c => c == _currentAlly);

        _currentAlly = candidates[Random.Range(0, candidates.Count)];
        _hoverOffset = new Vector3(
            Random.insideUnitCircle.x, 0f, Random.insideUnitCircle.y
        ).normalized * hoverRadius;

        _flyState = FlyState.Protecting;
        _switchAllyTimer = Random.Range(switchAllyMinTime, switchAllyMaxTime);
    }

    // Check if ally is withing line of sight
    private bool EvaluateAlly(Transform ally) {
        var allyDir = ally.position - this.transform.position;
        RaycastHit hit;
        Physics.Raycast(this.transform.position,allyDir, out hit);

        if (hit.transform.CompareTag("enemy"))
        {
            return true;
        }

        return false;
    }


    internal void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_wanderTarget, 0.5f);


        switch (_flyState) { 
            case FlyState.Wandering:
                Gizmos.color = Color.red;
                break;
            case FlyState.Protecting:
                Gizmos.color = Color.yellow;
                break;
            default: break;
        }

        Gizmos.DrawWireSphere(this.transform.position + new Vector3(0,1,0), 0.2f);
    }
}
