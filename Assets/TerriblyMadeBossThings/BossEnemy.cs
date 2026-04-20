using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// BOSS ENEMY - FireSpinner
/// ========================
/// Attach this script to your Boss GameObject.
///
/// REQUIRED COMPONENTS ON THE BOSS GAMEOBJECT:
///   - NavMeshAgent
///   - Rigidbody (set to Kinematic = true)
///   - Collider (e.g. CapsuleCollider)
///
/// REQUIRED SETUP IN THE INSPECTOR:
///   - Assign a "Fireball Prefab" (a GameObject with a collider + rigidbody)
///   - Assign the Player Transform
///   - Optionally assign a Health Bar UI Slider
///
/// HOW FLIGHT WORKS:
///   The NavMeshAgent handles horizontal movement on the ground plane.
///   The boss's vertical position is controlled separately via a sine wave,
///   making it float up and down independently.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class BossEnemy : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // INSPECTOR SETTINGS
    // ─────────────────────────────────────────────

    public SpriteRenderer render; 
    public Sprite Phase2Sprite; 

    [Header("References")]
    [Tooltip("Drag your Player GameObject here.")]
    public Transform player;

    [Tooltip("The fireball prefab to spawn. Needs a Rigidbody and Collider.")]
    public GameObject fireballPrefab;

    [Header("Health")]
    [Tooltip("Total health points for the boss.")]
    public float maxHealth = 5000f;

    [Header("Movement")]
    [Tooltip("How fast the boss chases the player on the ground plane.")]
    public float moveSpeed = 4f;

    [Tooltip("How close the boss gets before it stops advancing.")]
    public float attackRange = 8f;

    [Header("Flight (Vertical Bobbing)")]
    [Tooltip("How high above its spawn point the boss floats.")]
    public float floatHeight = 3f;

    [Tooltip("How much it bobs up and down (amplitude).")]
    public float bobAmplitude = 1.2f;

    [Tooltip("How fast it bobs up and down.")]
    public float bobSpeed = 1.5f;

    [Header("Spin")]
    [Tooltip("Degrees per second the boss rotates.")]
    public float spinSpeed = 90f;

    [Header("Fireball Attack")]
    [Tooltip("How many fireballs are fired in each ring.")]
    public int fireballsPerRing = 8;

    [Tooltip("How fast each fireball travels outward.")]
    public float fireballSpeed = 10f;

    [Tooltip("Seconds between each fireball ring burst.")]
    public float fireRate = 1.5f;

    [Tooltip("Where fireballs spawn from (a child Transform on the boss). If empty, uses boss center.")]
    public Transform fireballSpawnPoint;

    [Header("Phase 2 (Low Health)")]
    [Tooltip("Health % (0–1) at which the boss enters Phase 2 and gets more aggressive.")]
    [Range(0f, 1f)]
    public float phase2Threshold = 0.4f;

    [Tooltip("Fire rate multiplier in Phase 2 (e.g. 1.5 = 50% faster).")]
    public float phase2FireRateMultiplier = 1.5f;

    [Tooltip("Spin speed multiplier in Phase 2.")]
    public float phase2SpinMultiplier = 2f;

    [Header("Optional UI")]
    [Tooltip("Drag a UI Slider here to use as a health bar. Optional.")]
    public UnityEngine.UI.Slider healthBarSlider;

    // ─────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────

    private NavMeshAgent agent;
    private float currentHealth;
    private float groundY;          // The Y position at spawn (used as base for floating)
    private float fireTimer;
    private bool isPhase2 = false;
    private bool isDead = false;
    private float currentFireRate;
    private float currentSpinSpeed;

    public BossLevel bossLevel; 

    private EnemyHealth enemyHealth; 

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyHealth = GetComponent<EnemyHealth>();
        // Store the spawn Y as our "ground level" for floating
        groundY = transform.position.y;

        maxHealth = enemyHealth.MaxHealth; 

        // Initialize health
        currentHealth = maxHealth;
        currentFireRate = fireRate;
        currentSpinSpeed = spinSpeed;

        // Set NavMeshAgent speed
        agent.speed = moveSpeed;

        // IMPORTANT: We control Y position manually, so disable agent's Y control
        agent.updateUpAxis = false;

        // Set health bar to full
        if (healthBarSlider != null)
        {
            healthBarSlider.minValue = 0f;
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = maxHealth;
        }

        // Start the fireball attack loop
        StartCoroutine(FireballLoop());

        if (player == null)
            Debug.LogWarning("[BossEnemy] No Player assigned! Drag your Player into the Inspector.");

        if (fireballPrefab == null)
            Debug.LogWarning("[BossEnemy] No Fireball Prefab assigned! Drag a fireball prefab into the Inspector.");
    }

    void Update()
    {
        if (isDead) return;

        HandleMovement();
        HandleFlying();
        HandleSpin();
        CheckPhaseTransition();
    }

    // ─────────────────────────────────────────────
    // MOVEMENT
    // ─────────────────────────────────────────────

    void HandleMovement()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            // Chase the player
            agent.SetDestination(player.position);
            agent.isStopped = false;
        }
        else
        {
            // In range — stop moving and face the player
            agent.isStopped = true;
            FacePlayer();
        }
    }

    void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position);
        direction.y = 0f; // Only rotate on Y axis
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    // ─────────────────────────────────────────────
    // FLYING (VERTICAL BOBBING)
    // ─────────────────────────────────────────────

    void HandleFlying()
    {
        // Calculate the target Y using a sine wave for smooth bobbing
        float targetY = groundY + floatHeight + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;

        // Apply it directly to the transform (NavMeshAgent handles X/Z)
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * 3f);
        transform.position = pos;
    }

    // ─────────────────────────────────────────────
    // SPINNING
    // ─────────────────────────────────────────────

    void HandleSpin()
    {
        transform.Rotate(Vector3.up, currentSpinSpeed * Time.deltaTime, Space.World);
    }

    // ─────────────────────────────────────────────
    // FIREBALL ATTACK
    // ─────────────────────────────────────────────

    IEnumerator FireballLoop()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(currentFireRate);
            ShootFireballRing();
        }
    }

    void ShootFireballRing()
    {
        if (fireballPrefab == null) return;

        Vector3 spawnPos = fireballSpawnPoint != null
            ? fireballSpawnPoint.position
            : transform.position;

        float angleStep = 360f / fireballsPerRing;

        for (int i = 0; i < fireballsPerRing; i++)
        {
            float angle = i * angleStep;
            // Calculate direction on the horizontal plane
            float rad = angle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));

            GameObject fireball = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
            Rigidbody rb = fireball.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = direction * fireballSpeed;
            }

            // Auto-destroy fireballs after 5 seconds to keep the scene clean
            Destroy(fireball, 5f);
        }
    }

    // ─────────────────────────────────────────────
    // PHASE TRANSITION
    // ─────────────────────────────────────────────

    void CheckPhaseTransition()
    {
        if (!isPhase2 && enemyHealth.CurrentHealth <= maxHealth * phase2Threshold)
        {
            EnterPhase2();
        }
    }

    void EnterPhase2()
    {
        isPhase2 = true;
        Debug.Log("[BossEnemy] ENTERING PHASE 2!");

        render.sprite = Phase2Sprite;

        // Speed up fire rate and spin
        currentFireRate = fireRate / phase2FireRateMultiplier;
        currentSpinSpeed = spinSpeed * phase2SpinMultiplier;
        agent.speed = moveSpeed * 1.5f;

        // Restart the fireball loop with the new fire rate
        StopAllCoroutines();
        StartCoroutine(FireballLoop());

        bossLevel.Phase2Pillars();

        // TODO: Trigger your Phase 2 VFX / animation here
        // e.g. animator.SetTrigger("Phase2");
    }

    // ─────────────────────────────────────────────
    // DAMAGE & DEATH
    // ─────────────────────────────────────────────

    /// <summary>
    /// Call this from your Fireball or Player attack script to deal damage.
    /// Example: bossEnemy.TakeDamage(25f);
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Update health bar UI
        if (healthBarSlider != null)
            healthBarSlider.value = currentHealth;

        Debug.Log($"[BossEnemy] Took {amount} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        isDead = true;
        agent.isStopped = true;
        StopAllCoroutines();

        Debug.Log("[BossEnemy] Boss defeated!");

        // TODO: Play death animation, spawn loot, trigger cutscene, etc.
        // e.g. animator.SetTrigger("Death");

        // Destroy after a short delay (gives time for death animation)
        Destroy(gameObject, 3f);
    }

    // ─────────────────────────────────────────────
    // EDITOR GIZMOS (visible in Scene view)
    // ─────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        // Show attack range as a red wire sphere
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Show float height as a yellow line
        Gizmos.color = Color.yellow;
        Vector3 floatPos = transform.position;
        floatPos.y = (Application.isPlaying ? groundY : transform.position.y) + floatHeight;
        Gizmos.DrawLine(transform.position, floatPos);
    }
}