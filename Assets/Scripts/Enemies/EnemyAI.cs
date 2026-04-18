using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Attach to enemy GameObject alongside a NavMeshAgent.
/// Strafes by moving in a raw direction for a set duration rather than
/// setting NavMesh destinations, which gives reliable, readable behaviour.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : BaseEnemyAI
{
    // References
    private NavMeshAgent _agent;

    public override void Awake()
    {
        base.Awake();
        _agent = GetComponent<NavMeshAgent>();
        // We drive movement manually — disable NavMesh auto-braking and steering
        _agent.updateRotation = false;  // We rotate manually toward player
        _agent.updateUpAxis = false;
        _agent.stoppingDistance = 0f;
        _agent.autoBraking = false;
    }
    // -------------------------------------------------------------------------
    // Idle
    // -------------------------------------------------------------------------
    public override void UpdateIdle()
    {
        Debug.Log(_state);
        if (Vector3.Distance(transform.position, _player.position) <= detectionRadius)
            EnterCombat();
    }

  
    public override void EnterCombat()
    {
        base.EnterCombat();
        
  
        
        if (_shooter != null)
            _shooter.SetEngaged(true);

        PickNewStrafe();
    }

    public override void ExitCombat()
    {
        base.ExitCombat();

        _agent.velocity = Vector3.zero;
        _agent.ResetPath();

        if (_shooter != null)
            _shooter.SetEngaged(false);
    }

    // -------------------------------------------------------------------------
    // Combat
    // -------------------------------------------------------------------------
    public override void UpdateCombat()
    {
        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist > losePlayerRadius) { ExitCombat(); return; }

        FacePlayer();
        DecideCombatMove(dist);
        ApplyMovement(dist);
    }

    // Choose whether to strafe, approach, or retreat based on distance
    public override void DecideCombatMove(float dist)
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

    public override void ApplyMovement(float dist)
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
}