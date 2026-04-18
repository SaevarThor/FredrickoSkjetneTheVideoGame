using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Attach to enemy GameObject alongside a NavMeshAgent.
/// Strafes by moving in a raw direction for a set duration rather than
/// setting NavMesh destinations, which gives reliable, readable behaviour.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius  = 18f;
    [SerializeField] private float losePlayerRadius = 24f;

    [Header("Combat Distance")]
    [SerializeField] private float preferredDistance = 10f;   // Sweet spot range
    [SerializeField] private float tooCloseDistance  = 5f;    // Back away inside this
    [SerializeField] private float tooFarDistance    = 14f;   // Close in beyond this

    [Header("Strafing")]
    [SerializeField] private float strafeSpeed      = 3f;
    [SerializeField] private float strafeMinTime    = 1f;     // Min seconds per strafe burst
    [SerializeField] private float strafeMaxTime    = 2.5f;   // Max seconds per strafe burst

    [Header("Approach / Retreat")]
    [SerializeField] private float chaseSpeed       = 4f;
    [SerializeField] private float retreatSpeed     = 3.5f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed    = 8f;     // Always faces player



    // State machine
    private enum CombatMove { Strafe, Approach, Retreat, StandStill }
    private enum State { Idle, Combat }

    private State      _state      = State.Idle;
    private CombatMove _combatMove = CombatMove.Strafe;

    // References
    private NavMeshAgent _agent;
    private Transform    _player;
    private EnemyShooter _shooter;

    // Strafe state
    private int   _strafeDir     = 1;       // 1 = right, -1 = left
    private float _strafeDuration = 0f;     // How long this strafe lasts
    private float _strafeElapsed  = 0f;     // How long we've been strafing


    public GameObject AlertedVisual; 

    private void Awake()
    {
        _agent   = GetComponent<NavMeshAgent>();
        _shooter = GetComponent<EnemyShooter>();

        // We drive movement manually — disable NavMesh auto-braking and steering
        _agent.updateRotation   = false;  // We rotate manually toward player
        _agent.updateUpAxis     = false;
        _agent.stoppingDistance = 0f;
        _agent.autoBraking      = false;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
    }

    private void Update()
    {
        if (_player == null) return;

        switch (_state)
        {
            case State.Idle:   UpdateIdle();   break;
            case State.Combat: UpdateCombat(); break;
        }
    }

    // -------------------------------------------------------------------------
    // Idle
    // -------------------------------------------------------------------------
    private void UpdateIdle()
    {
        if (Vector3.Distance(transform.position, _player.position) <= detectionRadius)
            EnterCombat();
    }

    private void EnterCombat()
    {
        AlertedVisual.SetActive(true); // Show visual indicator when enemy enters combat
        _state = State.Combat;
        PickNewStrafe();

        if (_shooter != null)
            _shooter.SetEngaged(true);
    }

    private void ExitCombat()
    {
        AlertedVisual.SetActive(false); // Hide visual indicator when enemy exits combat
        _state = State.Idle;
        _agent.velocity = Vector3.zero;
        _agent.ResetPath();

        if (_shooter != null)
            _shooter.SetEngaged(false);
    }

    // -------------------------------------------------------------------------
    // Combat
    // -------------------------------------------------------------------------
    private void UpdateCombat()
    {
        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist > losePlayerRadius) { ExitCombat(); return; }

        FacePlayer();
        DecideCombatMove(dist);
        ApplyMovement(dist);
    }

    // Choose whether to strafe, approach, or retreat based on distance
    private void DecideCombatMove(float dist)
    {
        if (_shooter._isWindingUp)
        {
            _combatMove = CombatMove.StandStill;
            return;
        }

        if (dist < tooCloseDistance)
        {
            _combatMove = CombatMove.Retreat;
        }
        else if (dist > tooFarDistance)
        {
            _combatMove = CombatMove.Approach;
        }
        else
        {
            // In sweet spot — strafe, cycling direction on a timer
            _combatMove = CombatMove.Strafe;

            _strafeElapsed += Time.deltaTime;
            if (_strafeElapsed >= _strafeDuration)
                PickNewStrafe();
        }
    }

    private void ApplyMovement(float dist)
    {
        Vector3 toPlayer    = (_player.position - transform.position).normalized;
        toPlayer.y          = 0f;
        Vector3 strafeRight = Vector3.Cross(Vector3.up, toPlayer); // Perpendicular on flat plane

        Vector3 moveDir = Vector3.zero;

        switch (_combatMove)
        {
            case CombatMove.Strafe:
                // Pure sideways — no forward/back component while in sweet spot
                moveDir = strafeRight * _strafeDir;
                _agent.speed = strafeSpeed;
                break;

            case CombatMove.Approach:
                // Move toward player, with a slight strafe blend so it doesn't bee-line
                moveDir = (toPlayer * 0.8f + strafeRight * _strafeDir * 0.2f).normalized;
                _agent.speed = chaseSpeed;
                break;

            case CombatMove.Retreat:
                // Directly away from player
                moveDir = -toPlayer;
                _agent.speed = retreatSpeed;
                break;

            case CombatMove.StandStill:
                moveDir = Vector3.zero;
                _agent.speed = 0f;
                break;
        }

        // Move via NavMeshAgent.Move — moves directly in world space each frame
        // while still respecting NavMesh boundaries and obstacles
        _agent.Move(moveDir * _agent.speed * Time.deltaTime);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------
    private void FacePlayer()
    {
        Vector3 dir = (_player.position - transform.position);
        dir.y = 0f;
        if (dir == Vector3.zero) return;

        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target,
            rotationSpeed * Time.deltaTime);
    }

    private void PickNewStrafe()
    {
        // Flip direction and pick a new random duration
        _strafeDir      = Random.value > 0.5f ? 1 : -1;
        _strafeDuration = Random.Range(strafeMinTime, strafeMaxTime);
        _strafeElapsed  = 0f;
    }

    // -------------------------------------------------------------------------
    // Gizmos
    // -------------------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, losePlayerRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, preferredDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, tooCloseDistance);
    }
}