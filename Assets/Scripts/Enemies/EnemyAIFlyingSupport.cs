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
    [SerializeField] private float switchAllyMinTime = 400f;     // Min seconds before switching ally
    [SerializeField] private float switchAllyMaxTime = 900f;     // Max seconds before switching ally

    [Header("Wandering")]
    [SerializeField] private float wanderRadius = 8f;          // How far wander points are picked
    [SerializeField] private float wanderArrivalDist = 1.5f;   // How close = arrived

    [Header("Protection bob")]
    [SerializeField] private float xAmplitude = 0.8f;   // Side to side range
    [SerializeField] private float yAmplitude = 0.4f;   // Up and down range
    [SerializeField] private float zAmplitude = 0.6f;   // Forward back range

    [SerializeField] private float xSpeed = 1.1f;       // Different speeds per axis
    [SerializeField] private float ySpeed = 2.1f;       // creates the figure-8 / lissajous
    [SerializeField] private float zSpeed = 1.7f;       // shape naturally

    [SerializeField] private float xPhase = 0f;         // Phase offsets shift where in
    [SerializeField] private float yPhase = 1.2f;       // the pattern it starts
    [SerializeField] private float zPhase = 2.4f;

    private SupportBeam _supportBeam;

    public enum FlyState { Protecting, Wandering, MoveToAlly }
    public FlyState _flyState = FlyState.Wandering;

    public Transform _currentAlly;
    private float _switchAllyTimer = 0f;

    private Vector3 Origin; //the original spawn position of this enemy
    private Vector3 _moveTarget;
    private float _bobTime = 0f;

    private EnemyAIFlyingSupport Ai;
    public ParticleSystem particle;

    // -------------------------------------------------------------------------
    public override void Awake()
    {
        base.Awake();
        Origin = transform.position;
        EnterCombat();
        PickWanderTarget();
        _switchAllyTimer = switchAllyMaxTime;

        _supportBeam = GetComponent<SupportBeam>();
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
        if (_supportBeam != null)
            _supportBeam.ClearTarget();

        base.ExitCombat();
        _currentAlly = null;
        _flyState = FlyState.Wandering;
    }

    // -------------------------------------------------------------------------
    // Combat
    // -------------------------------------------------------------------------
    public override void UpdateCombat()
    {
                        
        switch (_flyState)
        {
            case FlyState.MoveToAlly: UpdateMoveToAlly(); break;
            case FlyState.Wandering: UpdateWandering(); break;
            case FlyState.Protecting: UpdateProtecting(); break;
        }
    }

    // -------------------------------------------------------------------------
    // Hovering - float near chosen ally
    // -------------------------------------------------------------------------
    private void UpdateMoveToAlly()
    {
        // if the ally is dead we want to move on to the next ally
        if (_currentAlly == null) {
            TryPickAlly();
            return;
        }

        _moveTarget = _currentAlly.position;
        _moveTarget.y = _currentAlly.position.y + hoverHeight + Mathf.Sin(_bobTime) * bobAmplitude;

        // we move a bit faster when moving to an enemy
        transform.position = Vector3.Lerp(transform.position, _moveTarget, (flySpeed*1.2f) * Time.deltaTime * 2f);
        FacePlayer();

        // we reach the enemy and start protecting
        if (Vector3.Distance(transform.position, _moveTarget) < wanderArrivalDist)
        {
            _currentAlly.GetComponent<EnemyHealth>().MakeInvulnerable();

            if (_supportBeam != null)
                _supportBeam.SetTarget(_currentAlly);

            if (!particle.isPlaying)
            {
                particle.Play();
            }

            _flyState = FlyState.Protecting;
        }
    }

    private void UpdateProtecting() {
        if (_currentAlly == null) {
            TryPickAlly();
            return;
        }


        _bobTime += Time.deltaTime;

        Vector3 offset = new Vector3(
            Mathf.Sin(_bobTime * xSpeed + xPhase) * xAmplitude,
            Mathf.Sin(_bobTime * ySpeed + yPhase) * yAmplitude,
            Mathf.Sin(_bobTime * zSpeed + zPhase) * zAmplitude
        );

        transform.position = _moveTarget + offset;

        // Switch ally on a timer
        //_switchAllyTimer -= Time.deltaTime;
        //if (_switchAllyTimer <= 0f)
        //{
        //    _switchAllyTimer = UnityEngine.Random.Range(switchAllyMinTime, switchAllyMaxTime);
        //    _currentAlly.GetComponent<EnemyHealth>().MakeVulnerable();
        //    particle.Stop();
        //    TryPickAlly();
        //}
    }

    // -------------------------------------------------------------------------
    // Wandering - drift to random points when no ally available
    // -------------------------------------------------------------------------
    private void UpdateWandering()
    {
        float bob = Mathf.Sin(_bobTime) * bobAmplitude;

        // MoveTowards never overshoots, so arrival is clean
        transform.position = Vector3.MoveTowards(
            transform.position, _moveTarget, flySpeed * Time.deltaTime
        );

        FacePlayer();

        if (Vector3.Distance(transform.position, _moveTarget) < wanderArrivalDist)
        {
            PickWanderTarget();
        }
    }

    private void PickWanderTarget()
    {
        Vector2 rand = new Vector2(Origin.x, Origin.z) + Random.insideUnitCircle * wanderRadius;
        _moveTarget = new Vector3(
            rand.x,
            transform.position.y,   // Keep current height, bob handles the rest
            rand.y
        );
    }

    // -------------------------------------------------------------------------
    // Ally picking
    // -------------------------------------------------------------------------
    private void TryPickAlly()
    {
        //Clearing beam if exists
        if (_supportBeam != null)
            _supportBeam.ClearTarget();

        // Find all enemies in range, excluding self
        Collider[] hits = Physics.OverlapSphere(transform.position, allySearchRadius);
        List<Transform> candidates = new List<Transform>();

        foreach (Collider hit in hits)
        {
            BaseEnemyAI enemy = hit.GetComponent<BaseEnemyAI>();
            if (enemy != null && enemy != this )
                candidates.Add(hit.transform);
        }

        if (candidates.Count == 0)
        {
            //Debug.Log("no allies to support");
            _currentAlly = null;
            _flyState = FlyState.Wandering;
            return;
        }

        // Pick randomly, avoid re-picking the same one if possible
        if (candidates.Count > 1)
            candidates.RemoveAll(c => c == _currentAlly);

        _currentAlly = candidates[Random.Range(0, candidates.Count)];

        _flyState = FlyState.MoveToAlly;
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

    private void OnDestroy()
    {
        _currentAlly.GetComponent<EnemyHealth>().MakeVulnerable();

        if (_supportBeam != null)
            _supportBeam.ClearTarget();
    }


    internal void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_moveTarget, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Origin, wanderRadius);
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(Origin, allySearchRadius);

        switch (_flyState) { 
            case FlyState.Wandering:
                Gizmos.color = Color.red;
                break;
            case FlyState.Protecting:
                Gizmos.color = Color.yellow;
                break;
            case FlyState.MoveToAlly:
                Gizmos.color = Color.blue;
                break;
            default: break;
        }

        Gizmos.DrawWireSphere(this.transform.position + new Vector3(0,1,0), 0.2f);
    }
}
