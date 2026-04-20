using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BossEnemy : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // INSPECTOR SETTINGS
    // ─────────────────────────────────────────────

    public SpriteRenderer render;
    public Sprite Phase2Sprite;
    public Sprite Phase3Sprite;

    [Header("References")]
    public Transform  player;
    public GameObject fireballPrefab;

    [Header("Health")]
    public float maxHealth = 5000f;

    [Header("Movement")]
    public float moveSpeed   = 4f;
    public float attackRange = 8f;

    [Header("Flight")]
    public float floatHeight  = 3f;
    public float bobAmplitude = 1.2f;
    public float bobSpeed     = 1.5f;

    [Header("Spin")]
    public float spinSpeed = 90f;

    [Header("Fireball Attack")]
    public int       fireballsPerRing = 8;
    public float     fireballSpeed    = 10f;
    public float     fireRate         = 1.5f;
    public Transform fireballSpawnPoint;

    [Header("Phase 2")]
    public float phase2FireRateMultiplier = 1.5f;
    public float phase2SpinMultiplier     = 2f;

    [Header("Phase 3 — The Apocalypse")]
    public float phase3FloatHeight           = 14f;   // Towers over everything
    public float phase3SpinMultiplier        = 6f;    // Basically a helicopter blade
    public float phase3FireRateMultiplier    = 3f;
    public float phase3FireballSpeed         = 22f;
    public int   phase3HelixArms             = 5;     // 5-armed death spiral
    public float phase3HelixSpread           = 30f;

    [Header("Phase 3 — Shield Orbs")]
    public int   phase3ShieldCount       = 8;
    public float phase3ShieldOrbitRadius = 4f;
    public float phase3ShieldOrbitSpeed  = 180f;      // Two full rotations per second at max

    [Header("Phase 3 — Laser")]
    public float phase3LaserSweepAngle    = 270f;     // Almost a full circle
    public float phase3LaserSweepDuration = 2f;       // Fast sweep
    public float phase3LaserDamage        = 12f;
    public float phase3LaserWidth         = 0.2f;
    public float phase3LaserRange         = 35f;

    [Header("Phase 3 — Meteor Storm")]
    public int   meteorCount         = 24;            // Meteors raining down
    public float meteorFallSpeed     = 20f;
    public float meteorDamage        = 25f;
    public float meteorSpawnHeight   = 18f;           // How high they spawn
    public float meteorSpawnRadius   = 12f;           // Arena radius for spread
    public float meteorWarningTime   = 0.8f;          // Shadow warning before impact

    [Header("Phase 3 — Teleport Assault")]
    public float teleportInterval    = 4f;            // Teleports every N seconds
    public float teleportBlastRadius = 5f;            // Explosion at old position
    public float teleportBlastForce  = 14f;

    [Header("Phase 3 — Death Nova")]
    public float phase3DeathNovaForce  = 25f;
    public float phase3DeathNovaRadius = 25f;
    public int   deathNovaWaves        = 4;           // Multiple expanding rings
    public float deathNovaWaveDelay    = 0.2f;        // Seconds between each ring

    [Header("Phase 3 — Prefabs")]
    public GameObject shieldOrbPrefab;
    public GameObject laserImpactPrefab;
    public GameObject meteorPrefab;                   // Sphere or capsule with Rigidbody
    public GameObject meteorWarningShadowPrefab;      // Flat circle decal prefab
    public GameObject teleportBlastPrefab;            // Explosion particle prefab

    [Header("Optional UI")]
    public UnityEngine.UI.Slider healthBarSlider;

    // ─────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────

    private NavMeshAgent _agent;
    private float        _groundY;
    private float        _currentFireRate;
    private float        _currentSpinSpeed;
    private float        _currentFloatHeight;
    private float        _currentFireballSpeed;

    private bool _isDead          = false;
    private bool _isPhase2        = false;
    private bool _isPhase3        = false;
    private bool _isTransitioning = false;

    // Phase 3 systems
    private bool             _phase3Invulnerable = false;
    private List<GameObject> _shieldOrbs         = new List<GameObject>();
    private float            _shieldOrbitAngle   = 0f;
    private bool             _laserActive        = false;
    private LineRenderer     _laserLineA;         // Primary laser
    private LineRenderer     _laserLineB;         // Secondary laser — sweeps opposite direction
    private float            _helixAngleOffset   = 0f;
    private bool             _meteorStormActive  = false;

    // Screenshake line renderer for death shockwave rings
    private List<LineRenderer> _shockwaveRings = new List<LineRenderer>();

    public  BossLevel      bossLevel;
    private EnemyHealth    _enemyHealth;
    private PlayerMovement _playerMovement;

    // ─────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────

    void Start()
    {
        _agent       = GetComponent<NavMeshAgent>();
        _enemyHealth = GetComponent<EnemyHealth>();
        _groundY     = transform.position.y;

        maxHealth                  = _enemyHealth.MaxHealth;
        _enemyHealth.CurrentHealth = maxHealth;

        _currentFireRate      = fireRate;
        _currentSpinSpeed     = spinSpeed;
        _currentFloatHeight   = floatHeight;
        _currentFireballSpeed = fireballSpeed;

        _agent.speed        = moveSpeed;
        _agent.updateUpAxis = false;

        if (healthBarSlider != null)
        {
            healthBarSlider.minValue = 0f;
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value    = maxHealth;
        }

        if (player != null)
            _playerMovement = player.GetComponent<PlayerMovement>();

        SetupLaserRenderers();
        StartCoroutine(FireballLoop());
    }

    void Update()
    {
        if (healthBarSlider != null)
            healthBarSlider.value = _enemyHealth.CurrentHealth;

        if (_isDead || _isTransitioning) return;

        HandleMovement();
        HandleFlying();
        HandleSpin();

        if (_isPhase3)
            UpdateShieldOrbs();

        if (_enemyHealth.CurrentHealth <= 0f)
            Die();
    }

    // ─────────────────────────────────────────────
    // DEATH & PHASE GATING
    // ─────────────────────────────────────────────

    void Die()
    {
        if (_isDead || _isTransitioning) return;

        if (!_isPhase2) { StartCoroutine(EnterPhase2Sequence()); return; }
        if (!_isPhase3) { _isPhase3 = true; StartCoroutine(EnterPhase3Sequence()); return; }

        StartCoroutine(Phase3DeathSequence());
    }

    // ─────────────────────────────────────────────
    // MOVEMENT / FLIGHT / SPIN
    // ─────────────────────────────────────────────

    void HandleMovement()
    {
        if (player == null) return;
        float dist = Vector3.Distance(transform.position, player.position);
        float eff  = _isPhase3 ? attackRange * 1.5f : attackRange;

        if (dist > eff) { _agent.SetDestination(player.position); _agent.isStopped = false; }
        else            { _agent.isStopped = true; FacePlayer(); }
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position; dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 5f);
    }

    void HandleFlying()
    {
        float targetY = _groundY + _currentFloatHeight
            + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        Vector3 pos = transform.position;
        pos.y       = Mathf.Lerp(pos.y, targetY, Time.deltaTime * (_isPhase3 ? 8f : 3f));
        transform.position = pos;
    }

    void HandleSpin()
    {
        transform.Rotate(Vector3.up, _currentSpinSpeed * Time.deltaTime, Space.World);
    }

    // ─────────────────────────────────────────────
    // FIREBALL LOOP
    // ─────────────────────────────────────────────

    IEnumerator FireballLoop()
    {
        while (!_isDead)
        {
            yield return new WaitForSeconds(_currentFireRate);
            if (_isTransitioning) continue;
            if (_isPhase3) ShootHelixSpiral();
            else           ShootFireballRing();
        }
    }

    void ShootFireballRing()
    {
        if (fireballPrefab == null) return;
        Vector3 sp = fireballSpawnPoint != null ? fireballSpawnPoint.position : transform.position;
        for (int i = 0; i < fireballsPerRing; i++)
        {
            float rad = (i * (360f / fireballsPerRing)) * Mathf.Deg2Rad;
            SpawnFireball(sp, new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)), _currentFireballSpeed);
        }
    }

    void ShootHelixSpiral()
    {
        if (fireballPrefab == null || player == null) return;
        Vector3 sp      = fireballSpawnPoint != null ? fireballSpawnPoint.position : transform.position;
        float   armStep = 360f / phase3HelixArms;
        _helixAngleOffset += 15f; // Steady rotation between bursts

        for (int arm = 0; arm < phase3HelixArms; arm++)
        {
            float   rad        = (arm * armStep + _helixAngleOffset) * Mathf.Deg2Rad;
            Vector3 horizontal = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)).normalized;
            Vector3 toPlayer   = (player.position - sp).normalized;
            Vector3 shootDir   = Vector3.Lerp(horizontal, toPlayer, 0.4f).normalized;
            shootDir = Quaternion.AngleAxis(phase3HelixSpread * 0.5f,
                           Vector3.Cross(horizontal, Vector3.up)) * shootDir;
            SpawnFireball(sp, shootDir, phase3FireballSpeed);
        }
    }

    void SpawnFireball(Vector3 pos, Vector3 dir, float speed)
    {
        GameObject fb = Instantiate(fireballPrefab, pos, Quaternion.LookRotation(dir));
        Rigidbody  rb = fb.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = dir.normalized * speed;
        Destroy(fb, 6f);
    }

    // ─────────────────────────────────────────────
    // PHASE 2
    // ─────────────────────────────────────────────

    IEnumerator EnterPhase2Sequence()
    {
        _isTransitioning           = true;
        _isPhase2                  = true;
        _enemyHealth.CurrentHealth = 1f;
        _agent.isStopped           = true;

        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.8f, 0.8f);
        Debug.Log("[BossEnemy] PHASE 2!");
        yield return new WaitForSeconds(1f);

        _enemyHealth.CurrentHealth = _enemyHealth.MaxHealth;
        if (render != null && Phase2Sprite != null) render.sprite = Phase2Sprite;

        _currentFireRate  = fireRate / phase2FireRateMultiplier;
        _currentSpinSpeed = spinSpeed * phase2SpinMultiplier;
        _agent.speed      = moveSpeed * 1.5f;
        _agent.isStopped  = false;

        StopCoroutine(FireballLoop());
        StartCoroutine(FireballLoop());
        bossLevel.Phase2Pillars();

        _isTransitioning = false;
    }

    // ─────────────────────────────────────────────
    // PHASE 3 — ENTRY SEQUENCE
    // An overwhelming, multi-beat cinematic transition
    // ─────────────────────────────────────────────

    IEnumerator EnterPhase3Sequence()
    {
        _isTransitioning           = true;
        _enemyHealth.CurrentHealth = 1f;

        StopCoroutine(FireballLoop());
        _agent.isStopped   = true;
        _laserLineA.enabled = false;
        _laserLineB.enabled = false;


        // ── Beat 1: Silence. The boss stops dead. Max shake. ──
        _currentSpinSpeed = 0f;
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(1f, 2f);
        yield return new WaitForSeconds(0.6f);

        // ── Beat 2: Erupts into hyperspin — 20× normal speed ──
        Debug.Log("[BossEnemy] PHASE 3 — THE APOCALYPSE!");
        float elapsed    = 0f;
        float hyperSpin  = spinSpeed * 20f;
        while (elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            // Spin speed ramps up then holds
            float ramp = Mathf.Clamp01(elapsed / 0.4f);
            transform.Rotate(Vector3.up, hyperSpin * ramp * Time.deltaTime, Space.World);
            // Shake escalates with the spin
            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(ramp * 0.8f, 0.05f);
            yield return null;
        }


        // ── Beat 3: Fires a 360 ring in EVERY direction simultaneously ──
        if (fireballPrefab != null)
        {
            Vector3 blastOrigin = transform.position;
            // Two rings — one horizontal, one angled downward
            for (int ring = 0; ring < 2; ring++)
            {
                float yAngle = ring == 0 ? 0f : -20f;
                for (int i = 0; i < 16; i++)
                {
                    float   rad = (i * (360f / 16)) * Mathf.Deg2Rad;
                    Vector3 dir = Quaternion.Euler(yAngle, 0f, 0f)
                                  * new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
                    SpawnFireball(blastOrigin, dir.normalized, fireballSpeed * 2f);
                }
            }
        }

        if (CameraShake.Instance != null) CameraShake.Instance.Shake(1f, 0.6f);
        yield return new WaitForSeconds(0.4f);

        // ── Beat 4: Ascend — rockets to phase 3 float height ──
        _currentFloatHeight = phase3FloatHeight;
        yield return new WaitForSeconds(1.5f); // HandleFlying() lerps it up at 8× speed

        // ── Beat 5: Sprite swap + shockwave rings expand outward ──
        if (render != null && Phase3Sprite != null) render.sprite = Phase3Sprite;
        StartCoroutine(SpawnShockwaveRings(transform.position, 3));
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(1f, 0.8f);

        // ── Beat 6: Shield orbs erupt outward then snap into orbit ──
        SpawnShieldOrbs();

        // Full health restore — the player sees the bar slam back to full
        _enemyHealth.CurrentHealth = _enemyHealth.MaxHealth;

        bossLevel.Phase3Pillars();


        yield return new WaitForSeconds(0.5f);

        // ── Beat 7: Immediately launch all Phase 3 systems simultaneously ──
        _agent.isStopped      = false;
        _agent.speed          = moveSpeed * 2f;
        _currentSpinSpeed     = spinSpeed * phase3SpinMultiplier;
        _currentFireRate      = fireRate  / phase3FireRateMultiplier;
        _currentFireballSpeed = phase3FireballSpeed;

        _isTransitioning = false;

        // All systems go at once — this is the chaos
        StartCoroutine(FireballLoop());
        StartCoroutine(DualLaserSweepLoop());
        StartCoroutine(MeteorStormLoop());
        StartCoroutine(TeleportAssaultLoop());
    }

    // ─────────────────────────────────────────────
    // PHASE 3 — SHIELD ORBS
    // ─────────────────────────────────────────────

    void SpawnShieldOrbs()
    {
        _shieldOrbs.Clear();
        if (shieldOrbPrefab == null) return;

        float step = 360f / phase3ShieldCount;
        for (int i = 0; i < phase3ShieldCount; i++)
        {
            float   a      = i * step * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * phase3ShieldOrbitRadius;
            GameObject orb = Instantiate(shieldOrbPrefab, transform.position + offset, Quaternion.identity);
            orb.transform.SetParent(null);
            _shieldOrbs.Add(orb);
        }
        _phase3Invulnerable = true;
    }

    void UpdateShieldOrbs()
    {
        _shieldOrbs.RemoveAll(o => o == null);
        _shieldOrbitAngle += phase3ShieldOrbitSpeed * Time.deltaTime;
        float step = 360f / Mathf.Max(1, _shieldOrbs.Count);

        for (int i = 0; i < _shieldOrbs.Count; i++)
        {
            if (_shieldOrbs[i] == null) continue;
            float   a      = (_shieldOrbitAngle + i * step) * Mathf.Deg2Rad;
            float   bobY   = Mathf.Sin(Time.time * 3f + i * 1.3f) * 0.5f;
            Vector3 target = transform.position + new Vector3(
                Mathf.Cos(a) * phase3ShieldOrbitRadius, bobY,
                Mathf.Sin(a) * phase3ShieldOrbitRadius);
            _shieldOrbs[i].transform.position = Vector3.Lerp(
                _shieldOrbs[i].transform.position, target, Time.deltaTime * 12f);
        }

        if (_phase3Invulnerable && _shieldOrbs.Count == 0)
        {
            _phase3Invulnerable = false;
            Debug.Log("[BossEnemy] Shields down!");
            StartCoroutine(StaggerBoss(1.5f));
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.7f, 0.6f);
        }
    }

    IEnumerator StaggerBoss(float t)
    {
        _agent.isStopped = true;
        yield return new WaitForSeconds(t);
        if (!_isDead && !_isTransitioning) _agent.isStopped = false;
    }

    // ─────────────────────────────────────────────
    // PHASE 3 — DUAL LASER SWEEP
    // Two lasers sweep simultaneously in opposite directions
    // ─────────────────────────────────────────────

    IEnumerator DualLaserSweepLoop()
    {
        while (!_isDead && _isPhase3)
        {
            yield return new WaitForSeconds(Random.Range(4f, 7f));
            if (!_isDead && !_isTransitioning)
                StartCoroutine(DualLaserSweep());
        }
    }

    IEnumerator DualLaserSweep()
    {
        if (_laserActive || player == null) yield break;
        _laserActive = true;

        // Warning shake
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.5f, 0.5f);
        yield return new WaitForSeconds(0.5f);

        _laserLineA.enabled = true;
        _laserLineB.enabled = true;

        float   half      = phase3LaserSweepAngle * 0.5f;
        float   elapsed   = 0f;
        Vector3 toPlayer  = player.position - transform.position; toPlayer.y = 0f;
        float   baseAngle = Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg;

        while (elapsed < phase3LaserSweepDuration)
        {
            elapsed += Time.deltaTime;
            float t   = elapsed / phase3LaserSweepDuration;

            // Laser A sweeps left → right
            float angleA = (baseAngle + Mathf.Lerp(-half, half, t))    * Mathf.Deg2Rad;
            // Laser B sweeps right → left (opposite)
            float angleB = (baseAngle + Mathf.Lerp(half, -half, t))    * Mathf.Deg2Rad;

            Vector3 origin = transform.position;
            origin.y       = Mathf.Max(transform.position.y - 1f, _groundY + 0.3f);

            UpdateLaser(_laserLineA, origin, angleA);
            UpdateLaser(_laserLineB, origin, angleB);

            // Pulse widths out of phase for visual chaos
            _laserLineA.startWidth = phase3LaserWidth * (1f + Mathf.Sin(Time.time * 25f) * 0.3f);
            _laserLineB.startWidth = phase3LaserWidth * (1f + Mathf.Sin(Time.time * 25f + Mathf.PI) * 0.3f);

            yield return null;
        }

        _laserLineA.enabled = false;
        _laserLineB.enabled = false;
        _laserActive        = false;
    }

    void UpdateLaser(LineRenderer lr, Vector3 origin, float angleRad)
    {
        Vector3 dir = new Vector3(Mathf.Sin(angleRad), 0f, Mathf.Cos(angleRad));
        lr.SetPosition(0, origin);
        lr.SetPosition(1, origin + dir * phase3LaserRange);

        Ray ray = new Ray(origin, dir);
        if (Physics.Raycast(ray, out RaycastHit hit, phase3LaserRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<IDamageable>()?.TakeDamage(phase3LaserDamage * Time.deltaTime);
                if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.3f, 0.08f);
            }
            if (laserImpactPrefab != null)
                Instantiate(laserImpactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }

    // ─────────────────────────────────────────────
    // PHASE 3 — METEOR STORM
    // Meteors rain down from above, preceded by a shadow warning marker
    // ─────────────────────────────────────────────

    IEnumerator MeteorStormLoop()
    {
        // First storm fires shortly after Phase 3 starts for immediate overwhelm
        yield return new WaitForSeconds(3f);

        while (!_isDead && _isPhase3)
        {
            if (!_isTransitioning)
                yield return StartCoroutine(FireMeteorStorm());

            yield return new WaitForSeconds(Random.Range(8f, 14f));
        }
    }

    IEnumerator FireMeteorStorm()
    {
        if (_meteorStormActive) yield break;
        _meteorStormActive = true;
        Debug.Log("[BossEnemy] METEOR STORM!");

        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.5f, 0.4f);

        // Spawn all warning shadows first, then drop the meteors after the warning time
        List<Vector3>    impactPoints = new List<Vector3>();
        List<GameObject> shadows      = new List<GameObject>();

        // Aim some at the player's current position, rest are random across the arena
        for (int i = 0; i < meteorCount; i++)
        {
            Vector3 target;
            if (i < meteorCount / 3 && player != null)
            {
                // First third aimed at player + small offset so they're not all stacked
                Vector2 jitter = Random.insideUnitCircle * 2.5f;
                target = player.position + new Vector3(jitter.x, 0f, jitter.y);
            }
            else
            {
                // Rest scattered randomly across the arena
                Vector2 rand = Random.insideUnitCircle * meteorSpawnRadius;
                target = new Vector3(
                    transform.position.x + rand.x,
                    _groundY,
                    transform.position.z + rand.y
                );
            }

            impactPoints.Add(target);

            // Spawn shadow warning at impact point
            if (meteorWarningShadowPrefab != null)
            {
                GameObject shadow = Instantiate(meteorWarningShadowPrefab,
                    target + Vector3.up * 0.05f, Quaternion.identity);
                shadows.Add(shadow);
            }
        }

        // Warning phase — let player react
        yield return new WaitForSeconds(meteorWarningTime);

        // Drop all meteors
        for (int i = 0; i < impactPoints.Count; i++)
        {
            if (meteorPrefab != null)
            {
                Vector3    spawnPos = impactPoints[i] + Vector3.up * meteorSpawnHeight;
                GameObject meteor   = Instantiate(meteorPrefab, spawnPos, Random.rotation);
                Rigidbody  rb       = meteor.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.useGravity     = false;
                    rb.linearVelocity = Vector3.down * meteorFallSpeed;
                }

                // Attach a small script to handle meteor impact
                MeteorImpact impact = meteor.AddComponent<MeteorImpact>();
                impact.Init(meteorDamage, _groundY, player);

                Destroy(meteor, 4f);
            }

            // Stagger spawns slightly so they don't all land at once
            if (i % 4 == 3)
                yield return new WaitForSeconds(0.05f);
        }

        // Clean up shadows
        foreach (GameObject s in shadows)
            if (s != null) Destroy(s, 0.5f);

        _meteorStormActive = false;
    }

    // ─────────────────────────────────────────────
    // PHASE 3 — TELEPORT ASSAULT
    // Boss vanishes and reappears — explosion at old position
    // ─────────────────────────────────────────────

    IEnumerator TeleportAssaultLoop()
    {
        yield return new WaitForSeconds(teleportInterval * 1.5f); // Delay first teleport

        while (!_isDead && _isPhase3)
        {
            yield return new WaitForSeconds(teleportInterval);
            if (!_isDead && !_isTransitioning && !_laserActive)
                yield return StartCoroutine(TeleportAttack());
        }
    }

    IEnumerator TeleportAttack()
    {
        if (player == null) yield break;

        Vector3 oldPos = transform.position;

        // Flash warning — rapid spin burst before vanishing
        float elapsed   = 0f;
        float warnSpin  = spinSpeed * 15f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            transform.Rotate(Vector3.up, warnSpin * Time.deltaTime, Space.World);
            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(0.3f, 0.05f);
            yield return null;
        }

        // Hide renderer temporarily
        if (render != null) render.enabled = false;

        // Explosion at old position
        if (teleportBlastPrefab != null)
        {
            GameObject blast = Instantiate(teleportBlastPrefab, oldPos, Quaternion.identity);
            Destroy(blast, 2f);
        }

        // Blast any player nearby the old position
        if (player != null && _playerMovement != null)
        {
            float dist = Vector3.Distance(oldPos, player.position);
            if (dist <= teleportBlastRadius)
            {
                float   falloff   = 1f - (dist / teleportBlastRadius);
                Vector3 pushDir   = (player.position - oldPos).normalized;
                pushDir.y         = 0f;
                _playerMovement.ApplyKnockback(
                    pushDir * teleportBlastForce * falloff + Vector3.up * 5f, 0.35f);
                player.GetComponent<IDamageable>()?.TakeDamage(15f * falloff);
                if (CameraShake.Instance != null)
                    CameraShake.Instance.Shake(0.65f, 0.35f);
            }
        }

        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.7f, 0.3f);

        yield return new WaitForSeconds(0.15f);

        // Reappear behind or beside the player
        Vector3 behindPlayer = player.position - player.forward * 3f
                             + player.right    * Random.Range(-2f, 2f)
                             + Vector3.up      * (_currentFloatHeight * 0.6f);

        // Clamp to NavMesh
        if (NavMesh.SamplePosition(behindPlayer, out NavMeshHit navHit, 5f, NavMesh.AllAreas))
            transform.position = navHit.position + Vector3.up * _currentFloatHeight;
        else
            transform.position = behindPlayer;

        // Reappear — fire a point-blank burst immediately on arrival
        if (render != null) render.enabled = true;

        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.8f, 0.4f);
        StartCoroutine(SpawnShockwaveRings(transform.position, 1));

        // Point-blank fireball burst on reappear — aimed at player
        if (fireballPrefab != null && player != null)
        {
            Vector3 sp = transform.position;
            for (int i = 0; i < 6; i++)
            {
                float   spread  = Random.Range(-15f, 15f);
                Vector3 toPlayer = (player.position - sp).normalized;
                Vector3 dir      = Quaternion.Euler(0f, spread, 0f) * toPlayer;
                SpawnFireball(sp, dir, phase3FireballSpeed);
            }
        }
    }

    // ─────────────────────────────────────────────
    // PHASE 3 — SHOCKWAVE RINGS
    // Procedural expanding rings built from LineRenderers
    // ─────────────────────────────────────────────

    IEnumerator SpawnShockwaveRings(Vector3 center, int count)
    {
        for (int r = 0; r < count; r++)
        {
            yield return new WaitForSeconds(r * 0.15f);
            StartCoroutine(ExpandShockwaveRing(center, r));
        }
    }

    IEnumerator ExpandShockwaveRing(Vector3 center, int index)
    {
        GameObject ringGO = new GameObject($"ShockwaveRing_{index}");
        LineRenderer lr   = ringGO.AddComponent<LineRenderer>();

        int   segments = 48;
        float duration = 0.6f;

        lr.positionCount     = segments + 1;
        lr.useWorldSpace     = true;
        lr.loop              = true;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        Material mat = new Material(shader);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        lr.material = mat;

        float elapsed = 0f;
        float ringY   = center.y + 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t      = elapsed / duration;
            float radius = Mathf.Lerp(0f, phase3DeathNovaRadius * 0.7f, t);
            float alpha  = Mathf.Lerp(1f, 0f, t);
            float width  = Mathf.Lerp(0.4f, 0.05f, t);

            lr.startWidth = width;
            lr.endWidth   = width;

            Color c = Color.Lerp(new Color(1f, 0.6f, 0.1f), new Color(1f, 0.1f, 0f), t);
            c.a = alpha;
            lr.startColor = c;
            lr.endColor   = c;

            for (int i = 0; i <= segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(
                    center.x + Mathf.Cos(angle) * radius,
                    ringY,
                    center.z + Mathf.Sin(angle) * radius
                ));
            }
            yield return null;
        }

        Destroy(ringGO);
    }

    // ─────────────────────────────────────────────
    // LASER RENDERER SETUP — DUAL LASERS
    // ─────────────────────────────────────────────

    void SetupLaserRenderers()
    {
        _laserLineA = CreateLaser(new Color(1f, 0.95f, 0.7f), new Color(1f, 0.4f, 0.05f)); // Orange
        _laserLineB = CreateLaser(new Color(0.6f, 0.9f, 1f),  new Color(0.1f, 0.4f, 1f));  // Electric blue
    }

    LineRenderer CreateLaser(Color colorA, Color colorB)
    {
        GameObject go = new GameObject("BossLaser");
        go.transform.SetParent(transform);
        LineRenderer lr = go.AddComponent<LineRenderer>();

        lr.positionCount     = 2;
        lr.startWidth        = phase3LaserWidth;
        lr.endWidth          = phase3LaserWidth * 0.2f;
        lr.useWorldSpace     = true;
        lr.enabled           = false;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        Material mat = new Material(shader);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        lr.material = mat;

        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(colorA, 0f), new GradientColorKey(colorB, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.5f, 1f) }
        );
        lr.colorGradient = g;
        return lr;
    }

    // ─────────────────────────────────────────────
    // PHASE 3 DEATH — THE GRAND FINALE
    // ─────────────────────────────────────────────

    IEnumerator Phase3DeathSequence()
    {
        if (_isDead) yield break;
        _isDead          = true;
        _isTransitioning = true;

        _agent.isStopped    = true;
        _laserLineA.enabled = false;
        _laserLineB.enabled = false;

        foreach (GameObject orb in _shieldOrbs)
            if (orb != null) Destroy(orb);
        _shieldOrbs.Clear();

        Debug.Log("[BossEnemy] FINAL DEATH — THE GRAND FINALE!");

        // ── Step 1: All systems stop. Absolute silence for 0.4s ──
        _currentSpinSpeed = 0f;
        yield return new WaitForSeconds(0.4f);

        // ── Step 2: Violent seizure spin — fastest it's ever moved ──
        float elapsed    = 0f;
        float deathSpin  = spinSpeed * 25f;
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(1f, 3f);

        while (elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 2f;
            // Starts fast, slows to zero — like a top falling over
            transform.Rotate(Vector3.up, Mathf.Lerp(deathSpin, 0f,
                Mathf.SmoothStep(0f, 1f, t)) * Time.deltaTime, Space.World);

            // Descend toward the ground dramatically
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, _groundY + 0.5f, Time.deltaTime * 3f);
            transform.position = pos;

            yield return null;
        }

        // ── Step 3: Four expanding nova rings in sequence ──
        for (int wave = 0; wave < deathNovaWaves; wave++)
        {
            yield return new WaitForSeconds(deathNovaWaveDelay);

            // Each wave fires a denser fireball ring
            if (fireballPrefab != null)
            {
                int     count      = 8 + wave * 4; // 8, 12, 16, 20 fireballs
                float   speed      = phase3FireballSpeed * (0.8f + wave * 0.3f);
                Vector3 novaOrigin = transform.position;

                for (int i = 0; i < count; i++)
                {
                    float   a   = (i * (360f / count)) * Mathf.Deg2Rad;
                    // Each wave fires at a slightly different vertical angle
                    float   yOff = Mathf.Lerp(-10f, 20f, wave / (float)deathNovaWaves);
                    Vector3 dir  = Quaternion.Euler(yOff, 0f, 0f)
                                   * new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)).normalized;
                    SpawnFireball(novaOrigin, dir, speed);
                }
            }

            // Shockwave ring for each wave
            StartCoroutine(ExpandShockwaveRing(transform.position, wave));

            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(Mathf.Lerp(0.5f, 1f, wave / (float)deathNovaWaves),
                    0.4f);
        }

        yield return new WaitForSeconds(deathNovaWaveDelay * deathNovaWaves + 0.2f);

        // ── Step 4: Final shockwave — maximum everything ──
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(1f, 1.5f);
        StartCoroutine(SpawnShockwaveRings(transform.position, 5));

        // Knockback player — distance-based falloff
        if (player != null && _playerMovement != null)
        {
            float   dist    = Vector3.Distance(transform.position, player.position);
            if (dist <= phase3DeathNovaRadius)
            {
                float   falloff  = 1f - (dist / phase3DeathNovaRadius);
                Vector3 pushDir  = (player.position - transform.position).normalized;
                pushDir.y        = 0f;
                _playerMovement.ApplyKnockback(
                    pushDir * phase3DeathNovaForce * falloff + Vector3.up * 8f, 0.7f);
            }
        }

        Debug.Log("[BossEnemy] Boss fully defeated!");
        yield return new WaitForSeconds(1f);
        Destroy(gameObject, 1.5f);
    }

    // ─────────────────────────────────────────────
    // DAMAGE
    // ─────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (_isDead || _isTransitioning) return;
        if (_isPhase3 && _phase3Invulnerable)
        {
            Debug.Log("[BossEnemy] Invulnerable — destroy the shield orbs first!");
            return;
        }
        if (healthBarSlider != null)
            healthBarSlider.value = _enemyHealth.CurrentHealth;
    }

    // ─────────────────────────────────────────────
    // CLEANUP
    // ─────────────────────────────────────────────

    private void OnDestroy()
    {
        if (healthBarSlider != null)
            healthBarSlider.gameObject.SetActive(false);
        foreach (GameObject orb in _shieldOrbs)
            if (orb != null) Destroy(orb);
    }

    // ─────────────────────────────────────────────
    // GIZMOS
    // ─────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, phase3ShieldOrbitRadius);
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, phase3DeathNovaRadius);
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, meteorSpawnRadius);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// METEOR IMPACT — tiny helper attached to each meteor at spawn
// Handles ground hit detection and area damage
// ─────────────────────────────────────────────────────────────────────────────
public class MeteorImpact : MonoBehaviour
{
    private float     _damage;
    private float     _groundY;
    private Transform _player;

    public void Init(float damage, float groundY, Transform player)
    {
        _damage  = damage;
        _groundY = groundY;
        _player  = player;
    }

    private void Update()
    {
        // Detect ground impact by checking Y position against ground level
        if (transform.position.y <= _groundY + 0.5f)
            Impact();
    }

    private void OnCollisionEnter(Collision col)
    {
        Impact();
    }

    private void Impact()
    {
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.45f, 0.2f);

        // Damage player if close to impact
        if (_player != null)
        {
            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist <= 3f)
            {
                float falloff = 1f - (dist / 3f);
                _player.GetComponent<IDamageable>()?.TakeDamage(_damage * falloff);

                PlayerMovement pm = _player.GetComponent<PlayerMovement>();
                if (pm != null)
                {
                    Vector3 push = (_player.position - transform.position).normalized;
                    push.y = 0f;
                    pm.ApplyKnockback(push * 8f * falloff + Vector3.up * 4f, 0.25f);
                }
            }
        }

        Destroy(gameObject);
    }
}