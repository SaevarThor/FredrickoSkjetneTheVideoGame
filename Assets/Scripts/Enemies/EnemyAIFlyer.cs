using UnityEngine;

public class EnemyAIFlyer : EnemyAI
{
    [Header("Flight")]
    [SerializeField] private float flySpeed = 8f;
    [SerializeField] private float hoverHeight = 6f;          // Resting height above player
    [SerializeField] private float heightLerpSpeed = 3f;      // How smoothly it adjusts height
    [SerializeField] private float bobAmplitude = 0.6f;       // Idle hover bob amount
    [SerializeField] private float bobSpeed = 1.2f;           // Idle hover bob frequency

    [Header("Dive Bomb")]
    [SerializeField] private float diveWindupTime = 1.2f;     // Pause before diving
    [SerializeField] private float diveSpeed = 22f;           // Speed during dive
    [SerializeField] private float diveRecoveryHeight = 5f;   // Pulls up to this height after impact
    [SerializeField] private float diveRecoverySpeed = 10f;   // Speed pulling back up
    [SerializeField] private float diveCooldownMin = 3f;
    [SerializeField] private float diveCooldownMax = 6f;

    private enum FlyState { Circling, WindingUp, Diving, Recovering }
    private FlyState _flyState = FlyState.Circling;

    private float _diveCooldown = 0f;
    private float _windupElapsed = 0f;
    private Vector3 _diveTargetPos;       // Locked in when dive starts
    private float _bobTime = 0f;
    private float _orbitAngle = 0f;


    public override void UpdateIdle()
    {
        _bobTime += Time.deltaTime * bobSpeed;
        Vector3 pos = transform.position;
        pos.y += Mathf.Sin(_bobTime) * bobAmplitude * Time.deltaTime;
        transform.position = pos;

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


        if (_shooter != null)
            _shooter.SetEngaged(false);
    }

    // -------------------------------------------------------------------------
    // Combat
    // -------------------------------------------------------------------------
    public override void UpdateCombat()
    {
        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist > losePlayerRadius)
        {
            ExitCombat();
            return;
        }

        switch (_flyState)
        {
            case FlyState.Circling: UpdateCircling(dist); break;
            case FlyState.WindingUp: UpdateWindup(); break;
            case FlyState.Diving: UpdateDiving(); break;
            case FlyState.Recovering: UpdateRecovering(); break;
        }
    }

    // -------------------------------------------------------------------------
    // Circling - orbit above the player, counting down to next dive
    // -------------------------------------------------------------------------
    private void UpdateCircling(float dist)
    {
        _bobTime += Time.deltaTime * bobSpeed;

        // Hover above the player
        float targetY = _player.position.y + hoverHeight + Mathf.Sin(_bobTime) * bobAmplitude;
        Vector3 target = new Vector3(transform.position.x, targetY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, target, heightLerpSpeed * Time.deltaTime);

        FacePlayer();

        _diveCooldown -= Time.deltaTime;
        if (_diveCooldown <= 0f)
            EnterWindup();
    }

    // -------------------------------------------------------------------------
    // Windup - freeze and telegraph the dive
    // -------------------------------------------------------------------------
    private void EnterWindup()
    {
        _flyState = FlyState.WindingUp;
        _windupElapsed = 0f;

        // Lock in where the player is NOW - so they can dodge
        _diveTargetPos = _player.position;
    }

    private void UpdateWindup()
    {
        _windupElapsed += Time.deltaTime;

        // Hover in place, tilt nose down to telegraph
        FacePlayer();
        transform.rotation *= Quaternion.Euler(
            Mathf.Lerp(0f, 35f, _windupElapsed / diveWindupTime), 0f, 0f
        );

        if (_windupElapsed >= diveWindupTime)
            EnterDive();
    }

    // -------------------------------------------------------------------------
    // Dive - fly straight at locked target position
    // -------------------------------------------------------------------------
    private void EnterDive()
    {
        _flyState = FlyState.Diving;
    }

    private void UpdateDiving()
    {
        Vector3 dir = (_diveTargetPos - transform.position).normalized;
        transform.position += dir * diveSpeed * Time.deltaTime;

        // Face the direction of travel
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        // Close enough to target or hit ground - pull up
        float distToTarget = Vector3.Distance(transform.position, _diveTargetPos);
        if (distToTarget < 1.2f || transform.position.y <= 0.5f)
            EnterRecovery();
    }

    // -------------------------------------------------------------------------
    // Recovery - swoop back up to hover height
    // -------------------------------------------------------------------------
    private void EnterRecovery()
    {
        _flyState = FlyState.Recovering;
    }

    private void UpdateRecovering()
    {
        float recoveryY = _player.position.y + diveRecoveryHeight;
        Vector3 target = new Vector3(_player.position.x, recoveryY, _player.position.z);

        transform.position = Vector3.Lerp(
            transform.position, target, diveRecoverySpeed * Time.deltaTime
        );

        FacePlayer();

        // Back at hover height - return to circling
        if (Mathf.Abs(transform.position.y - recoveryY) < 0.5f)
        {
            _flyState = FlyState.Circling;
            _diveCooldown = Random.Range(diveCooldownMin, diveCooldownMax);
            PickNewStrafe(); // Randomise orbit direction after each dive
        }
    }

    


}
