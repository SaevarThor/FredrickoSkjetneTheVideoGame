using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemyAI: MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] internal float detectionRadius = 18f;
    [SerializeField] internal float losePlayerRadius = 24f;

    [Header("Combat Distance")]
    [SerializeField] internal float preferredDistance = 10f;   // Sweet spot range
    [SerializeField] internal float tooCloseDistance = 5f;    // Back away inside this
    [SerializeField] internal float tooFarDistance = 14f;   // Close in beyond this

    [Header("Strafing")]
    [SerializeField] internal float strafeSpeed = 3f;
    [SerializeField] internal float strafeMinTime = 1f;     // Min seconds per strafe burst
    [SerializeField] internal float strafeMaxTime = 2.5f;   // Max seconds per strafe burst

    [Header("Approach / Retreat")]
    [SerializeField] internal float chaseSpeed = 4f;
    [SerializeField] internal float retreatSpeed = 3.5f;

    [Header("Rotation")]
    [SerializeField] internal float rotationSpeed = 8f;     // Always faces player



    // State machine
    internal enum CombatMove { Strafe, Approach, Retreat, StandStill }
    internal enum State { Idle, Combat }

    internal State _state = State.Idle;
    internal CombatMove _combatMove = CombatMove.Strafe;

    
    internal Transform _player;
    internal EnemyShooter _shooter;

    // Strafe state
    internal int _strafeDir = 1;       // 1 = right, -1 = left
    internal float _strafeDuration = 0f;     // How long this strafe lasts
    internal float _strafeElapsed = 0f;     // How long we've been strafing


    public GameObject AlertedVisual;

    public virtual void Awake()
    {
       
        _shooter = GetComponent<EnemyShooter>();

        

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
    }

    private void Update()
    {
        if (_player == null) return;

        switch (_state)
        {
            case State.Idle: UpdateIdle(); break;
            case State.Combat: UpdateCombat(); break;
        }
    }

    // -------------------------------------------------------------------------
    // Idle
    // -------------------------------------------------------------------------
    public virtual void UpdateIdle()
    {
       
    }

    public virtual void EnterCombat()
    {
        AlertedVisual.SetActive(true); // Show visual indicator when enemy enters combat
        _state = State.Combat;
        PickNewStrafe();

        
    }

    public virtual void ExitCombat()
    {
        AlertedVisual.SetActive(false); // Hide visual indicator when enemy exits combat
        _state = State.Idle;
       
    }

    // -------------------------------------------------------------------------
    // Combat
    // -------------------------------------------------------------------------
    public virtual void UpdateCombat()
    {
        
    }

    // Choose whether to strafe, approach, or retreat based on distance
    public virtual void DecideCombatMove(float dist)
    {
        
    }

    public virtual void ApplyMovement(float dist)
    {
        
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------
    internal void FacePlayer()
    {
        Vector3 dir = (_player.position - transform.position);
        dir.y = 0f;
        if (dir == Vector3.zero) return;

        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target,
            rotationSpeed * Time.deltaTime);
    }

    internal void PickNewStrafe()
    {
        // Flip direction and pick a new random duration
        _strafeDir = Random.value > 0.5f ? 1 : -1;
        _strafeDuration = Random.Range(strafeMinTime, strafeMaxTime);
        _strafeElapsed = 0f;
    }

    // -------------------------------------------------------------------------
    // Gizmos
    // -------------------------------------------------------------------------
    internal void OnDrawGizmosSelected()
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
